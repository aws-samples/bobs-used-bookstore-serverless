using System.Text.Json.Serialization;

namespace BookInventory.ImageWorkflow.Responses;

public class ImageResizeResponse
{
    [JsonPropertyName("objectKey")]
    public string ObjectKey { get; set; }
    
    [JsonPropertyName("isPublishedInDestination")]
    public bool IsPublishedInDestination { get; set; }
    
    [JsonPropertyName("destinationBucket")]
    public string DestinationBucket { get; set; }
}