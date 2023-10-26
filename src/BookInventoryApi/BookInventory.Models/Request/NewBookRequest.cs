namespace BookInventory.Models.Request;

using System.Text.Json.Serialization;

public class NewBookRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}