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

        try
        {
            var base64EncodedBytes = Convert.FromBase64String(cursor);
            this.Metadata = JsonSerializer.Deserialize<QueryMetadata>(Encoding.UTF8.GetString(base64EncodedBytes));
        }
        catch (Exception)
        {
            Logger.AppendKey("cursor", cursor);
            Logger.LogError("Failure parsing input cursor.");
            this.Metadata = new QueryMetadata();
        }
        
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

    public string LastGsiPartition { get; set; } = "";

    public string LastGsiKey { get; set; } = "";

    public string LastPartition { get; set; } = "";

    public string LastKey { get; set; } = "";
    
    public int LastCheckedMonth { get; set; } = 1;

    public void AddPartitions(QueryResponse queryResponse)
    {
        if (queryResponse.LastEvaluatedKey != null && queryResponse.LastEvaluatedKey.ContainsKey("PK"))
        {
            Logger.LogInformation(queryResponse);

            this.LastPartition = queryResponse.LastEvaluatedKey["PK"].S;
            this.LastKey = queryResponse.LastEvaluatedKey["SK"].S;
            this.LastGsiPartition = queryResponse.LastEvaluatedKey["GSI1PK"].S;
            this.LastGsiKey = queryResponse.LastEvaluatedKey["GSI1SK"].S;
        }
    }

    public void ResetPartitions(int resetToMonth)
    {
        this.LastCheckedMonth = resetToMonth;
        this.LastDate = this.LastDate.AddMonths(-resetToMonth);
        this.LastGsiPartition = string.Empty;
        this.LastGsiKey = string.Empty;
        this.LastPartition = string.Empty;
        this.LastKey = string.Empty;
    }
}