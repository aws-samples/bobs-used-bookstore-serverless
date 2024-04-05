using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.S3.Model;
using AWS.Lambda.Powertools.Logging;
using ImageMagick;
using S3Object = Amazon.Rekognition.Model.S3Object;

namespace BookInventory.Service;

public class ImageService : IImageService
{
    private readonly IAmazonRekognition rekognitionClient;
    private readonly IAmazonS3 amazonS3Client;
    private readonly string destinationBucket;
    private const int BookCoverImageWidth = 400;
    private const int BookCoverImageHeight = 600;

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
        destinationBucket = Environment.GetEnvironmentVariable("PUBLISH_IMAGE_BUCKET");
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

    [Logging]
    public async Task SaveImageAsync(string bucket, string image)
    {
        // Download the original image from S3. Resize the image and upload it to the destination bucket.
        using (var responseStream = await amazonS3Client.GetObjectStreamAsync(bucket, image, null))
        {
            var resizedImage = await ResizeImageAsync(responseStream);

            var putObjectRequest = new PutObjectRequest
            {
                BucketName = destinationBucket,
                Key = image,
                InputStream = resizedImage
            };
            await amazonS3Client.PutObjectAsync(putObjectRequest);
        }
    }

    [Logging]
    private async Task<Stream> ResizeImageAsync(Stream image)
    {
        using var magickImage = new MagickImage(image);

        if (magickImage.BaseWidth == BookCoverImageWidth && magickImage.BaseHeight == BookCoverImageHeight) return image;

        var size = new MagickGeometry(BookCoverImageWidth, BookCoverImageHeight) { IgnoreAspectRatio = false };

        magickImage.Resize(size);

        var result = new MemoryStream();

        await magickImage.WriteAsync(result);

        result.Position = 0;

        return result;
    }
}