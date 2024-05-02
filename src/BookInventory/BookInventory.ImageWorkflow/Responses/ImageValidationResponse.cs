using System.Text.Json.Serialization;

namespace BookInventory.ImageWorkflow.Responses;

public class ImageValidationResponse
{
    [JsonPropertyName("objectKey")]
    public string ObjectKey { get; set; }
    
    [JsonPropertyName("isImageSafe")]
    public bool IsImageSafe { get; set; }
}