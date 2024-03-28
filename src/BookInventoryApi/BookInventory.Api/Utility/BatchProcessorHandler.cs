using Amazon.Lambda.SQSEvents;
using AWS.Lambda.Powertools.BatchProcessing;
using AWS.Lambda.Powertools.BatchProcessing.Sqs;
using AWS.Lambda.Powertools.Logging;

namespace BookInventory.Api.Utility;

public class BatchProcessorHandler : ISqsRecordHandler
{
    [Logging]
    public async Task<RecordHandlerResult> HandleAsync(SQSEvent.SQSMessage record, CancellationToken cancellationToken)
    {
        Logger.LogInformation($"Image Uploaded {record.Body}");

        if (string.IsNullOrWhiteSpace(record.Body))
        {
            var exception = new Exception($"Error on Image {record.Body}");
            Logger.LogError(exception);
            throw exception;
        }

        return await Task.FromResult(RecordHandlerResult.None);
    }
}