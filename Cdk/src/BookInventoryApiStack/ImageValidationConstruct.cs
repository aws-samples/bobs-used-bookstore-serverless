using Amazon.CDK;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.StepFunctions;
using Amazon.CDK.AWS.StepFunctions.Tasks;
using BookInventoryApiStack.ImageValidation;
using Constructs;
using HttpMethods = Amazon.CDK.AWS.S3.HttpMethods;

namespace BookInventoryApiStack;

public record ImageValidationConstructProps
{
    public ImageValidationConstructProps(string servicePrefix, string postfix, Bucket imageBucket, Table bookInventoryTable)
    {
        PostFix = postfix;
        ServicePrefix = servicePrefix;
        ImageBucket = imageBucket;
        BookInventoryTable = bookInventoryTable;
    }

    public string PostFix { get; set; }
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
        var bookInventoryPublishBucket = new Bucket(this, $"{props.ServicePrefix.ToLower()}-published-image{props.PostFix}", new BucketProps
        {
            BucketName = $"{props.ServicePrefix.ToLower()}-published-image{props.PostFix}",
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
            ],
            RemovalPolicy = string.IsNullOrWhiteSpace(props.PostFix)? RemovalPolicy.RETAIN : RemovalPolicy.DESTROY // Destroy in postfix environment
        });
        
        // CloudFront Distribution to use images in published bucket
        var oai = new OriginAccessIdentity(this, $"BookInventory-OAI{props.PostFix}");
        bookInventoryPublishBucket.GrantRead(oai);

        var distribution = new CloudFrontWebDistribution(this, $"BookInventory-Image-Distribution{props.PostFix}",
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

        var bookInventoryServiceStackProps = new BookInventoryServiceStackProps(props.PostFix);
        bookInventoryServiceStackProps.PublishBucketName = bookInventoryPublishBucket.BucketName;
        
        var validateImageLambda = new ValidateImage(
            this,
            $"{Constants.VALIDATE_IMAGE}-Step{props.PostFix}",
            bookInventoryServiceStackProps).Function;
        validateImageLambda.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps()
        {
            Effect = Effect.ALLOW,
            Resources = ["*"],
            Actions = ["rekognition:DetectModerationLabels"]
        }));
        props.ImageBucket.GrantRead(validateImageLambda.Role!);

        var resizeImageLambda = new ResizeImage(this, $"{Constants.RESIZE_IMAGE}-Step{props.PostFix}", bookInventoryServiceStackProps)
            .Function;
        props.ImageBucket.GrantRead(resizeImageLambda.Role!);
        bookInventoryPublishBucket.GrantReadWrite(resizeImageLambda.Role!);
        
        var validateImageLambdaInvoke = new LambdaInvoke(this, $"{Constants.VALIDATE_IMAGE}{props.PostFix}", new LambdaInvokeProps()
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
        
        var imageResizeLambdaInvoke = new LambdaInvoke(this, $"{Constants.RESIZE_IMAGE}{props.PostFix}", new LambdaInvokeProps()
        {
            LambdaFunction = resizeImageLambda,
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
            ResultPath = "$.imageResizeResponse"
        });
        
        var successStep = new Succeed(this, $"Image-validation-workflow-Successful{props.PostFix}");
        var failureStep = new Fail(this, $"Image-validation-workflow-Fails{props.PostFix}");
        var chain = Chain
            .Start(
                new Choice(this, $"Image-Size-Check{props.PostFix}", new ChoiceProps
                    {
                        InputPath = "$"
                    })
                    .When(Condition.NumberLessThanEquals("$.detail.object.size", 0), successStep)
                    .Otherwise(validateImageLambdaInvoke
                        .Next(
                            new Choice(this, $"Image-Safe-Check{props.PostFix}", new ChoiceProps
                                {
                                    InputPath = "$"
                                })
                                .When(Condition.BooleanEquals("$.imageValidationResponse.Payload.isImageSafe", false),
                                    failureStep)
                                .Otherwise(imageResizeLambdaInvoke
                                    .Next(new Choice(this, $"Image-Resize-Check{props.PostFix}", new ChoiceProps
                                        {
                                            InputPath = "$"
                                        }).When(
                                            Condition.BooleanEquals(
                                                "$.imageResizeResponse.Payload.isPublishedInDestination", false),
                                            failureStep)
                                        .Otherwise(new CallAwsService(this, $"Delete-Image{props.PostFix}", new CallAwsServiceProps
                                        {
                                            Service = "s3",
                                            Action = "deleteObject",
                                            InputPath = "$",
                                            Parameters = new Dictionary<string, object>
                                            {
                                                {"Bucket", JsonPath.StringAt("$.detail.bucket.name")},
                                                {"Key", JsonPath.StringAt("$.detail.object.key")}
                                            },
                                            IamResources = [props.ImageBucket.ArnForObjects("*")],
                                            ResultPath = "$.DeleteImageResponse"
                                        }).Next(successStep))
                                    )
                                ))));
        
        var imageValidationWorkflow = new StateMachine(this, $"ImageValidationStateMachine{props.PostFix}", new StateMachineProps()
        {
            DefinitionBody = DefinitionBody.FromChainable(chain),
            StateMachineName = $"BookInventory-ImageValidation{props.PostFix}",
            TracingEnabled = true,
            RemovalPolicy = string.IsNullOrWhiteSpace(props.PostFix)? RemovalPolicy.RETAIN : RemovalPolicy.DESTROY // Destroy in postfix environment
        });
        
        // Create Event Rule in Default - Event Bus (Only Default Bus can receive events from AWS Services) 
        var eventRule = new Rule(this, $"BookInventoryImageUpload-Rule{props.PostFix}", new RuleProps()
        {
            Description = "Image upload event for validation",
            RuleName = $"BookInventoryImageUpload-Rule{props.PostFix}",
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
    }
}