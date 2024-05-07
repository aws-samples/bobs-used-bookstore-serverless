using System.Text.Json.Nodes;
using Amazon.CDK;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.KMS;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Notifications;
using Amazon.CDK.AWS.SES.Actions;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.StepFunctions;
using Amazon.CDK.AWS.StepFunctions.Tasks;
using BookInventoryApiStack.ImageValidation;
using Constructs;
using EventBus = Amazon.CDK.AWS.Events.EventBus;
using EventBusProps = Amazon.CDK.AWS.Events.EventBusProps;
using HttpMethods = Amazon.CDK.AWS.S3.HttpMethods;

namespace BookInventoryApiStack;

public record ImageValidationConstructProps
{
    public ImageValidationConstructProps(string account, string servicePrefix, Bucket imageBucket, Table bookInventoryTable)
    {
        Account = account;
        ServicePrefix = servicePrefix;
        ImageBucket = imageBucket;
        BookInventoryTable = bookInventoryTable;
    }

    public string Account { get; set; }
    public string ServicePrefix { get; set; }
    public Bucket ImageBucket { get; set; }
    public Table BookInventoryTable { get; set; }
}

internal class ImageValidationConstruct : Construct
{
    internal ImageValidationConstruct(Stack scope, string id,
        ImageValidationConstructProps props) : base(scope, id)
    {
        // S3 bucket to publish Image
        var bookInventoryPublishBucket = new Bucket(this, "BookInventoryBucket-PublishedImage", new BucketProps
        {
            BucketName = $"{props.Account}-{props.ServicePrefix.ToLower()}-book-inventory-image-publish",
            BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
            AccessControl = BucketAccessControl.PRIVATE,
            Versioned = true,
            Cors =
            [
                new CorsRule()
                {
                    AllowedHeaders = ["*"],
                    AllowedOrigins = ["*"],
                    AllowedMethods = [HttpMethods.GET, HttpMethods.HEAD],
                    MaxAge = 300
                }
            ]
        });
        
        // CloudFront Distribution to use images in published bucket
        var oai = new OriginAccessIdentity(this, "BookInventory-OAI");
        bookInventoryPublishBucket.GrantRead(oai);

        var distribution = new CloudFrontWebDistribution(this, "BookInventory-Image-Distribution",
            new CloudFrontWebDistributionProps()
            {
                OriginConfigs =
                [
                    new SourceConfiguration()
                    {
                        S3OriginSource = new S3OriginConfig()
                        {
                            S3BucketSource = bookInventoryPublishBucket,
                            OriginAccessIdentity = oai
                        },
                        Behaviors =
                        [
                            new Behavior
                            {
                                IsDefaultBehavior = true,
                                PathPattern = "/*",
                                AllowedMethods = CloudFrontAllowedMethods.GET_HEAD
                            }
                        ]
                    }
                ]
            });
        #region EventToStepFunction
        // Enable S3 event notifications through Event Bridge
        props.ImageBucket.EnableEventBridgeNotification();

        var bookInventoryServiceStackProps = new BookInventoryServiceStackProps();
        
        var validateImageLambda = new ValidateImage(
            this,
            $"{Constants.VALIDATE_IMAGE}-Step",
            bookInventoryServiceStackProps).Function;
        validateImageLambda.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps()
        {
            Effect = Effect.ALLOW,
            Resources = ["*"],
            Actions = ["rekognition:DetectModerationLabels"]
        }));
        props.ImageBucket.GrantReadWrite(validateImageLambda.Role!);
        bookInventoryPublishBucket.GrantReadWrite(validateImageLambda.Role!);
        var validateImageLambdaInvoke = new LambdaInvoke(this, Constants.VALIDATE_IMAGE, new LambdaInvokeProps()
        {
            LambdaFunction = validateImageLambda,
            IntegrationPattern = IntegrationPattern.REQUEST_RESPONSE,
            Payload = TaskInput.FromObject(
                new Dictionary<string, object>
                {
                    {
                        "bucketName.$","$.detail.bucket.name" 
                    },
                    {
                        "objectKey.$","$.detail.object.key" 
                    }
                }),
            InputPath = "$",
            ResultPath = "$.imageValidationResponse"
        });
        var successStep = new Succeed(this, "Image-validation-workflow-Successful");
        var chain = Chain
            .Start(
                new Choice(this, "Image-Size-Check", new ChoiceProps
                    {
                        InputPath = "$"
                    })
                    .When(Condition.NumberLessThanEquals("$.detail.object.size", 0), successStep)
                    .Otherwise(validateImageLambdaInvoke
                        .Next(
                            new Choice(this, "Image-Safe-Check", new ChoiceProps
                                {
                                    InputPath = "$"
                                })
                                .When(Condition.BooleanEquals("$.imageValidationResponse.Payload.isImageSafe", false),
                                    successStep)
                                .Otherwise(new Pass(this, "Image-Resize")
                                    .Next(new Pass(this, "Delete-Image-From-Source"))
                                    .Next(successStep)
                                )
                        )));
        
        var imageValidationWorkflow = new StateMachine(this, "ImageValidationStateMachine", new StateMachineProps()
        {
            DefinitionBody = DefinitionBody.FromChainable(chain),
            StateMachineName = "BookInventory-ImageValidation",
            TracingEnabled = true
        });
        
        // Create Event Rule in Default - Event Bus (Only Default Bus can receive events from AWS Services) 
        var eventRule = new Rule(this, "BookInventoryImageUpload-Rule", new RuleProps()
        {
            Description = "Image upload event for validation",
            RuleName = "BookInventoryImageUpload-Rule",
            Targets = [new SfnStateMachine(imageValidationWorkflow)],
            EventPattern = new EventPattern()
            {
                Source = ["aws.s3"],
                DetailType = ["Object Created"],
                Detail =  new Dictionary<string, object>()
                {
                    {
                        "bucket", new Dictionary<string, object>()
                        {
                            {
                                "name", new[]
                                {
                                    props.ImageBucket.BucketName
                                }
                            }
                        }
                    },
                    {
                        "object", new Dictionary<string, object>()
                        {
                            {
                                "key", new Dictionary<string, object>[]
                                {
                                    new()
                                    {
                                        {"suffix",".png"}
                                    },
                                    new()
                                    {
                                        {"suffix",".PNG"}
                                    },
                                    new()
                                    {
                                        {"suffix",".jpg"}
                                    },
                                    new()
                                    {
                                        {"suffix",".JPG"}
                                    }
                                }
                            }
                        }
                    }
                }
            }
        });
        
        #endregion EventToStepFunction
        #region EventToLambda
        // Queue for event notification from S3 Upload
        var imageNotificationQueue = new Queue(this, $"{id}-queue", new QueueProps()
        {
            QueueName = "cover-page-image-upload-queue",
            DeadLetterQueue = new DeadLetterQueue()
            {
                MaxReceiveCount = 3,
                Queue = new Queue(this, $"{id}-queue-failure", new QueueProps()
                {
                    QueueName = "cover-page-image-upload-queue-failure"
                })
            }
        });
        
        // Publish Image uploaded/created event in SQS - Trigger notification only for .jpg and .png
        props.ImageBucket.AddObjectCreatedNotification(new SqsDestination(imageNotificationQueue),
            filters:
            [
                new NotificationKeyFilter
                {
                    Suffix = ".jpg"
                }
            ]);
        
        props.ImageBucket.AddObjectCreatedNotification(new SqsDestination(imageNotificationQueue),
            filters:
            [
                new NotificationKeyFilter
                {
                    Suffix = ".png"
                }
            ]);
        
        // Lambda to validate image and move to Published folder
        var imageValidationLambda = new SharedConstructs.LambdaFunction(
            this,
            Constants.VALIDATE_BOOK_IMAGE_API,
            new SharedConstructs.LambdaFunctionProps("./src/BookInventory/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_ImageValidation_Generated::ImageValidation",
                Environment = new Dictionary<string, string>
                {
                    { "POWERTOOLS_SERVICE_NAME", Constants.VALIDATE_BOOK_IMAGE_API },
                    { "POWERTOOLS_METRICS_NAMESPACE", Constants.VALIDATE_BOOK_IMAGE_API },
                    { "POWERTOOLS_LOGGER_LOG_EVENT", "true" },
                    { "QUEUE_URL", imageNotificationQueue.QueueUrl },
                    { "PUBLISH_IMAGE_BUCKET", bookInventoryPublishBucket.BucketName }
                }
            }).Function;
        // Lambda roles
        imageNotificationQueue.GrantConsumeMessages(imageValidationLambda.Role!);
        imageValidationLambda.AddEventSource(new SqsEventSource(imageNotificationQueue, new SqsEventSourceProps()
        {
            BatchSize = 5,
            ReportBatchItemFailures = true
        }));
        props.BookInventoryTable.GrantReadWriteData(imageValidationLambda.Role!);
        imageValidationLambda.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps()
        {
            Effect = Effect.ALLOW,
            Resources = ["*"],
            Actions = ["rekognition:DetectModerationLabels"]
        }));
        props.ImageBucket.GrantReadWrite(imageValidationLambda.Role!);
        bookInventoryPublishBucket.GrantReadWrite(imageValidationLambda.Role!);
        #endregion EventToLambda
    }
}