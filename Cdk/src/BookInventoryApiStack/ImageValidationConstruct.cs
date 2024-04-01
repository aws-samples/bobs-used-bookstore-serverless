using Amazon.CDK;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Notifications;
using Amazon.CDK.AWS.SES.Actions;
using Amazon.CDK.AWS.SQS;
using Constructs;

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
        
        // Queue for event notification from S3 Upload
        Queue imageNotificationQueue = new Queue(this, $"{id}-queue", new QueueProps()
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
            new SharedConstructs.LambdaFunctionProps("./src/BookInventoryApi/BookInventory.Api")
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
        /*
        imageValidationLambda.Role.AttachInlinePolicy(new Policy(this, "RecognitionPermissionPolicy", new PolicyProps()
        {
            Document = new PolicyDocument(new PolicyDocumentProps()
            {
                Statements = [new PolicyStatement()
                {
                    Effect = Effect.ALLOW
                }]
            })
        }));
        imageValidationLambda.AddPermission("RecognitionPermission", new Permission()
        {
            Action = "rekognition:*",

        });
        */
    }
}