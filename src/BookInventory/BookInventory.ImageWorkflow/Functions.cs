using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using BookInventory.ImageWorkflow.Requests;
using BookInventory.ImageWorkflow.Responses;
using BookInventory.Service;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BookInventory.ImageWorkflow;

/// <summary>
/// A collection of sample Lambda functions that provide a REST api for doing simple math calculations. 
/// </summary>
public class Functions
{
    private readonly IImageService imageService;
    
    /// <summary>
    /// Default constructor.
    /// </summary>
    public Functions(IImageService imageService)
    {
        this.imageService = imageService;
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
}