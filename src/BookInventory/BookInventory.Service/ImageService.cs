using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.S3.Model;
using AWS.Lambda.Powertools.Logging;
using S3Object = Amazon.Rekognition.Model.S3Object;

namespace BookInventory.Service;

public class ImageService : IImageService
{
    private readonly IAmazonRekognition rekognitionClient;
    private readonly IAmazonS3 amazonS3Client;

    private readonly string[] BannedCategories = 
    {
        "Explicit Nudity", 
        "Suggestive", 
        "Violence", 
        "Visually Disturbing", 
        "Rude Gestures", 
        "Drugs", 
        "Tobacco", 
        "Alcohol", 
        "Gambling", 
        "Hate Symbols"
    };

    public ImageService(IAmazonRekognition rekognitionClient, IAmazonS3 amazonS3Client)
    {
        this.rekognitionClient = rekognitionClient;
        this.amazonS3Client = amazonS3Client;
    }

    [Logging]
    public async Task<bool> IsSafeAsync(string bucket, string image)
    {
        Logger.LogInformation($"Image Recognition {bucket} Image {image}");
        var result = await rekognitionClient.DetectModerationLabelsAsync(new DetectModerationLabelsRequest()
        {
            Image = new Image()
            {
                S3Object = new S3Object()
                {
                    Bucket = bucket,
                    Name = image
                }
            }
        });

        return !result.ModerationLabels.Any(x => BannedCategories.Contains(x.Name, StringComparer.OrdinalIgnoreCase));
    }
    
    public async Task MoveImageToPublish(string bucket, string image)
    {
        string destinationBucket = Environment.GetEnvironmentVariable("PUBLISH_IMAGE_BUCKET");
        Logger.LogInformation($"Image Copy to Publish Folder Source Bucket {bucket} SourceKey {image} Destination {destinationBucket}");
        await this.amazonS3Client.CopyObjectAsync(new CopyObjectRequest()
        {
            DestinationBucket = destinationBucket,
            SourceBucket = bucket,
            SourceKey = image,
            DestinationKey = image
        });
    }
}