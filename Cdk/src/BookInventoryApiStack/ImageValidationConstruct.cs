using Amazon.CDK;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Notifications;
using Amazon.CDK.AWS.SQS;
using Constructs;

namespace BookInventoryApiStack;

public record ImageValidationConstructProps
{
    public ImageValidationConstructProps(Bucket imageBucket, Bucket imageBucketPublish)
    {
        ImageBucket = imageBucket;
        ImageBucketPublish = imageBucketPublish;
    }
    
    public Bucket ImageBucket { get; set; }
    public Bucket ImageBucketPublish { get; set; }
}

internal class ImageValidationConstruct : Construct
{
    internal ImageValidationConstruct(Stack scope, string id,
        ImageValidationConstructProps props) : base(scope, id)
    {
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
        
        // Publish Image uploaded/created event in SQS
        props.ImageBucket.AddObjectCreatedNotification(new SqsDestination(imageNotificationQueue));
        
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
                    { "QUEUE_URL", imageNotificationQueue.QueueUrl }
                }
            }).Function;
        
        imageNotificationQueue.GrantConsumeMessages(imageValidationLambda.Role!);
        imageValidationLambda.AddEventSource(new SqsEventSource(imageNotificationQueue, new SqsEventSourceProps()
        {
            BatchSize = 5,
            ReportBatchItemFailures = true
        }));
        
    }
}