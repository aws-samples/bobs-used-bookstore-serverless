using System.Text.Json.Serialization;

namespace BookInventory.Models
{
    public class CreateBookDto
    {
        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("bookType")]
        public string BookType { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("coverImage")]
        public string? CoverImage { get; set; }

        [JsonPropertyName("coverImageFileName")]
        public string? CoverImageFileName { get; set; }

        [JsonPropertyName("genre")]
        public string Genre { get; set; }

        [JsonPropertyName("ISBN")]
        public string ISBN { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("publisher")]
        public string Publisher { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("year")]
        public int? Year { get; set; }
    }
}