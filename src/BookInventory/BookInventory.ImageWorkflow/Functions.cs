using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.S3;
using Amazon.S3.Model;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using BookInventory.ImageWorkflow.Requests;
using BookInventory.ImageWorkflow.Responses;
using BookInventory.Service;
using ImageMagick;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BookInventory.ImageWorkflow;

/// <summary>
/// A collection of sample Lambda functions that provide a REST api for doing simple math calculations. 
/// </summary>
public class Functions
{
    private readonly IImageService imageService;
    private readonly IAmazonS3 amazonS3Client;
    private const int BookCoverImageWidth = 400;
    private const int BookCoverImageHeight = 600;
    
    /// <summary>
    /// Default constructor.
    /// </summary>
    public Functions(IImageService imageService, IAmazonS3 amazonS3Client)
    {
        this.imageService = imageService;
        this.amazonS3Client = amazonS3Client;
    }

    /// <summary>
    /// Image validation
    /// </summary>
    /// <param name="imageValidationRequest">Image to validate</param>
    /// <returns>Image Validation Response</returns>
    [LambdaFunction()]
    [Logging(LogEvent = true, CorrelationIdPath = CorrelationIdPaths.EventBridge)]
    [Metrics(CaptureColdStart = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    public async Task<ImageValidationResponse> ImageValidation(ImageValidationRequest imageValidationRequest)
    {
        Logger.LogInformation(
            $"Image to validate {imageValidationRequest.BucketName} - {imageValidationRequest.ObjectKey}");
        // Validate image
        var isImageSafe = await this.imageService.IsSafeAsync(imageValidationRequest.BucketName,
            imageValidationRequest.ObjectKey);
        Logger.LogInformation(
            $"Image {imageValidationRequest.ObjectKey} in bucket {imageValidationRequest.BucketName} is {(isImageSafe ? "Safe" : "UnSafe")}");

        return new ImageValidationResponse()
        {
            IsImageSafe = isImageSafe,
            ObjectKey = imageValidationRequest.ObjectKey
        };
    }
    
    /// <summary>
    /// Image resize and save in destination bucket
    /// </summary>
    /// <param name="imageValidationRequest">Image to validate</param>
    /// <returns>Image Validation Response</returns>
    [LambdaFunction()]
    [Logging(LogEvent = true, CorrelationIdPath = CorrelationIdPaths.EventBridge)]
    [Metrics(CaptureColdStart = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    public async Task<ImageResizeResponse> SaveResizedImage(ImageValidationRequest imageValidationRequest)
    {
        bool isImageSuccessfullyPublished = true;
        Logger.LogInformation(
            $"Image to Resize {imageValidationRequest.BucketName} - {imageValidationRequest.ObjectKey}");
        string destinationBucket = Environment.GetEnvironmentVariable("DESTINATION_BUCKET");
        // Download the original image from S3. Resize the image and upload it to the destination bucket.
        try
        {
            Logger.LogInformation("Streaming image");
            using (var responseStream = await amazonS3Client.GetObjectStreamAsync(imageValidationRequest.BucketName,
                       imageValidationRequest.ObjectKey, null))
            {
                var resizedImage = await ResizeImageAsync(responseStream);
                resizedImage.Seek(0, SeekOrigin.Begin);
                Logger.LogInformation("Construct PutObject to save image in destination");
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = destinationBucket,
                    Key = imageValidationRequest.ObjectKey,
                    InputStream = resizedImage
                };
                await amazonS3Client.PutObjectAsync(putObjectRequest);
            }
        }
        catch (Exception ex)
        {
            isImageSuccessfullyPublished = false;
            Logger.LogError($"Error in publishing image {imageValidationRequest.ObjectKey} from bucket {imageValidationRequest.BucketName}. Error Message {ex.Message}. Error");
        }

        return new ImageResizeResponse()
        {
            DestinationBucket = destinationBucket,
            ObjectKey = imageValidationRequest.ObjectKey,
            IsPublishedInDestination = isImageSuccessfullyPublished
        };
    }
    
    
    
    [Logging(LogEvent = true, CorrelationIdPath = CorrelationIdPaths.EventBridge)]
    [Metrics(CaptureColdStart = true)]
    [Tracing(CaptureMode = TracingCaptureMode.Error)]
    private async Task<Stream> ResizeImageAsync(Stream image)
    {
        using var magickImage = new MagickImage(image);
        Logger.LogInformation("Image Resize started");
        if (magickImage.BaseWidth == BookCoverImageWidth && magickImage.BaseHeight == BookCoverImageHeight) return image;

        var size = new MagickGeometry(BookCoverImageWidth, BookCoverImageHeight) { IgnoreAspectRatio = false };

        magickImage.Resize(size);

        var result = new MemoryStream();

        await magickImage.WriteAsync(result);

        result.Position = 0;
        Logger.LogInformation("Image Resize successful");
        return result;
    }
}