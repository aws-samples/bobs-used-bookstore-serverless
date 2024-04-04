using Amazon.Lambda.SQSEvents;
using Amazon.S3.Util;
using AWS.Lambda.Powertools.BatchProcessing;
using AWS.Lambda.Powertools.BatchProcessing.Sqs;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Service.Utility;

public class BatchProcessorHandler : ISqsRecordHandler
{
    private readonly IImageService imageService;

    public BatchProcessorHandler()
    {
        imageService = Services.Provider.GetRequiredService<IImageService>();
    }

    [Logging(LogEvent = true, LogLevel = Microsoft.Extensions.Logging.LogLevel.Debug)]
    [Metrics]
    public async Task<RecordHandlerResult> HandleAsync(SQSEvent.SQSMessage record, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(record.Body))
        {
            var exception = new Exception($"Error on Image {record.Body}");
            Logger.LogError(exception);
            throw exception;
        }

        var s3EventNotification = S3EventNotification.ParseJson(record.Body);
        foreach (var eventNotificationRecord in s3EventNotification.Records)
        {
            string bucket = eventNotificationRecord.S3.Bucket.Name;
            string image = eventNotificationRecord.S3.Object.Key;

            // check if the object is not empty and validate
            if (eventNotificationRecord.S3.Object.Size == 0)
            {
                Logger.LogInformation($"Image {image} in bucket {bucket} is empty");
                Metrics.AddMetric("Empty Image", 1, MetricUnit.Count);
            }

            // Validate image
            await ValidateImage(bucket, image);

            // Save valid image to another bucket
            await imageService.SaveImageAsync(bucket, image);
        }
        return await Task.FromResult(RecordHandlerResult.None);
    }

    private async Task ValidateImage(string bucket, string image)
    {
        var isImageSafe = await imageService.IsSafeAsync(bucket,
            image);
        Logger.LogDebug(
            $"Image {image} in bucket {bucket} is {(isImageSafe ? "Safe" : "UnSafe")}");

        if (!isImageSafe)
        {
            throw new Exception($"Image {image} is not safe");
        }
    }
}