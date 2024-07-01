namespace BookInventory.Repository;

using System.Text;
using System.Text.Json;

using Amazon.DynamoDBv2.Model;

using AWS.Lambda.Powertools.Logging;

using Models;

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
        catch (Exception exception)
        {
            Logger.LogError($"Failure parsing input cursor. Cursor: {cursor} Error Message: {exception.Message}");
            this.Metadata = new QueryMetadata(); // If cursor is altered, search will start from the beginning. Update this logic as per the use case.
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
            return string.IsNullOrWhiteSpace(Metadata.LastGsiPartition)? String.Empty : Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Metadata)));
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

    public void AddPartitions(QueryResponse queryResponse)
    {
        if (queryResponse.LastEvaluatedKey != null && queryResponse.LastEvaluatedKey.ContainsKey("GSI1PK"))
        {
            Logger.LogInformation($"Adding Partition for the next query: {JsonSerializer.Serialize(queryResponse.LastEvaluatedKey)}");

            this.LastPartition = queryResponse.LastEvaluatedKey["BookId"].S; // for PK
            this.LastGsiPartition = queryResponse.LastEvaluatedKey["GSI1PK"].S;
            this.LastGsiKey = queryResponse.LastEvaluatedKey["GSI1SK"].S;
        }
    }

    public void ResetPartitions(int resetToMonth)
    {
        this.LastDate = this.LastDate.AddMonths(-resetToMonth);
        this.LastGsiPartition = string.Empty;
        this.LastGsiKey = string.Empty;
        this.LastPartition = string.Empty;
    }
}