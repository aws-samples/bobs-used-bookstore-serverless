namespace BookInventory.Models;

using System.Text.Json.Serialization;

public class Book
{
    /// <summary>
    ///  Gets or sets Id of the book
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets Name of the book
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
}