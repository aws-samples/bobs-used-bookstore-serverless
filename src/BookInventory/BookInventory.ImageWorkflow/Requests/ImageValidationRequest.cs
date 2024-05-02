using System.Text.Json.Serialization;

namespace BookInventory.ImageWorkflow.Requests;

public class ImageValidationRequest
{
    [JsonPropertyName("bucketName")]
    public string BucketName { get; set; }
    
    [JsonPropertyName("objectKey")]
    public string ObjectKey { get; set; }
}