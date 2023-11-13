namespace BookInventory.Repository;

using System.Text;
using System.Text.Json;

using Amazon.DynamoDBv2.Model;

using AWS.Lambda.Powertools.Logging;

using BookInventory.Models;

public record ListResponse
{
    public ListResponse(string cursor)
    {
        if (string.IsNullOrEmpty(cursor))
        {
            this.Metadata = new QueryMetadata();
            return;
        }
        
        var base64EncodedBytes = Convert.FromBase64String(cursor);
        this.Metadata = JsonSerializer.Deserialize<QueryMetadata>(Encoding.UTF8.GetString(base64EncodedBytes));
        
        Logger.LogInformation("Input metadata:");
        Logger.LogInformation(this.Metadata);
    }
    
    public List<Book> Books { get; set; } = new();

    // This cursor based approach is taken from an article by Serverless Hero Yan Cui, on implementing pagination. 
    public string Cursor
    {
        get
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Metadata)));
        }
    }

    internal QueryMetadata Metadata  { get; set; } = new();
}

public record QueryMetadata()
{
    public DateTime LastDate { get; set; } = DateTime.Now;

    public Dictionary<string, AttributeValue> LastEvaluatedKey { get; set; } = new();
    
    public int LastCheckedMonth { get; set; } = 1;
}