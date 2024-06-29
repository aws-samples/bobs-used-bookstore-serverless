using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using BookInventory.Models;

namespace BookInventory.Repository;

public class BookInventoryRepository : IBookInventoryRepository
{
    private readonly IDynamoDBContext context;
    private readonly IAmazonDynamoDB client;
    private const int MAX_MONTHS_TO_CHECK_WITHOUT_DATA = 2;
    private readonly bool isPostfix = false;
    private readonly string tableName;

    public BookInventoryRepository(IDynamoDBContext context, IAmazonDynamoDB client)
    {
        this.context = context;
        this.client = client;
        isPostfix = IsPostFix();
        tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
        if (isPostfix)
        {
            Logger.LogInformation($"Postfix environment to query postfix table {tableName}");
        }
    }

    [Tracing]
    public async Task<Book?> GetByIdAsync(string bookId)
    {
        Logger.LogInformation($"Postfix environment {(isPostfix?"true" :"false")} Table Name to override {tableName}" );
        return await context.LoadAsync<Book>(bookId, 
            isPostfix ? 
                new DynamoDBOperationConfig
                {
                    OverrideTableName = tableName
                }
            : null);
    }

    [Tracing]
    public async Task SaveAsync(Book book)
    {
        await context.SaveAsync(book,
            isPostfix ? 
                new DynamoDBOperationConfig
                {
                    OverrideTableName = tableName
                }
                : null);
    }

    [Tracing]
    public async Task<ListResponse> List(int pageSize = 10, string cursor = null)
    {
        // Extract where to start information from the cursor
        ListResponse bookResponse = new ListResponse(cursor);
        
        // Execute the initial query.
        Logger.LogInformation($"ExecuteQuery for the partition {bookResponse.Metadata.LastDate.ToString("yyyyMM")}");
        var queryResponse = await this.ExecuteQuery(bookResponse, pageSize);
        Logger.LogInformation($"ExecutedQuery Response for the partition {bookResponse.Metadata.LastGsiPartition}. Next Page: {JsonSerializer.Serialize(queryResponse.LastEvaluatedKey)}");
        
        // Construct output list. Update meta data based on the next page key
        this.ProcessResults(ref bookResponse, queryResponse, pageSize);
        Logger.LogInformation($"ProcessResults Response: Count - {bookResponse.Books.Count} MetaData: {JsonSerializer.Serialize(bookResponse.Metadata)} New Cursor: {bookResponse.Cursor} PageSize: {pageSize}");
        
        if (bookResponse.Books.Count == pageSize)
        {
            Logger.LogInformation("Page size or end of partition is reached, returning from List Api");
            return bookResponse;
        }
        
        // For more data, update Key to look for previous partitions 
        int noDataInPreviousMonth = 0;
        while (bookResponse.Books.Count < pageSize && noDataInPreviousMonth < MAX_MONTHS_TO_CHECK_WITHOUT_DATA) // Check 2 past partitions, if no data, end the search to avoid infinite loop
        {
            Logger.LogInformation($"Check data in previous month - {bookResponse.Metadata.LastDate.ToString()}");
            bookResponse = await this.ListBooksInPreviousMonths(
                pageSize,
                bookResponse);
            if (string.IsNullOrEmpty(bookResponse.Cursor))
            {
                noDataInPreviousMonth++;
            }
            if (bookResponse.Books.Count == pageSize)
            {
                return bookResponse;
            }
        }
        return bookResponse;
    }

    [Tracing]
    private async Task<ListResponse> ListBooksInPreviousMonths(
        int page,
        ListResponse bookResponse)
    {
        QueryResponse queryResponse;

        queryResponse = await this.ExecuteQuery(
            bookResponse,
            page);

        // Build the list of books.
        this.ProcessResults(
            ref bookResponse,
            queryResponse,
            page);

        return bookResponse;
    }

    [Tracing]
    private void ProcessResults(ref ListResponse bookResponse, QueryResponse queryResponse, int pageSize)
    {
        Logger.LogInformation($"Processing results, QueryResponse contains {queryResponse.Items.Count} item(s)");

        // Build the list of books.
        foreach (var item in queryResponse.Items)
        {
            bookResponse.Books.Add(
                new Book
                {
                    BookId = item.AsString("BookId"),
                    Name = item.AsString("Name"),
                    Author = item.AsString("Author"),
                    ISBN = item.AsString("ISBN"),
                    Publisher = item.AsString("Publisher"),
                    BookType = item.AsString("BookType"),
                    Genre = item.AsString("Genre"),
                    Condition = item.AsString("Condition"),
                    Price = item.AsDecimal("Price"),
                    Quantity = item.AsInt("Quantity"),
                    Summary = item.AsString("Summary"),
                    Year = item.AsInt("Year"),
                    CoverImageUrl = item.AsString("CoverImageUrl")
                });
        }
        // If the next page is not available, Reset stored key. Otherwise update the key
        if (queryResponse.LastEvaluatedKey is null || !queryResponse.LastEvaluatedKey.Any())
        {
            Logger.LogInformation("End of the partition is reached, Reset cursor");
            bookResponse.Metadata.ResetPartitions(1);
        }
        else
        {
            Logger.LogInformation($"Update cursor: {JsonSerializer.Serialize(queryResponse.LastEvaluatedKey)}");
            bookResponse.Metadata.AddPartitions(queryResponse);
        }
    }

    [Tracing]
    private async Task<QueryResponse> ExecuteQuery(ListResponse bookResponse, int pageSize)
    {
        Logger.LogInformation($"Executing query for listing books, for {bookResponse.Metadata.LastGsiPartition} and page size of {pageSize}");
        
        Dictionary<string, AttributeValue> startKey = null;
        string keyConditionExpression = "#gsi1pk = :gsi1pk";
        Dictionary<string, string> listAttributeNames = new(1) { { "#gsi1pk", "GSI1PK" }};
        var attributeValues = new Dictionary<string, AttributeValue>(1);
        
        // Query first page. Skip startkey, GSI1SK and PK (BookId) 
        if (string.IsNullOrWhiteSpace(bookResponse.Metadata.LastGsiPartition))
        {
            attributeValues.Add(":gsi1pk", new AttributeValue(bookResponse.Metadata.LastDate.ToString("yyyyMM")));
        }
        else
        {
            attributeValues.Add(":gsi1pk", new AttributeValue(bookResponse.Metadata.LastGsiPartition));
            startKey = new Dictionary<string, AttributeValue>()
            {
                { "GSI1PK", new AttributeValue(bookResponse.Metadata.LastGsiPartition) },
                { "GSI1SK", new AttributeValue(bookResponse.Metadata.LastGsiKey) },
                { "BookId", new AttributeValue(bookResponse.Metadata.LastGsiKey) } // for Paritition Key
            };
        }
        
        var queryRequest = new QueryRequest
        {
            TableName = tableName, // Always takes table name from environment variable irrespective of postfix
            KeyConditionExpression = keyConditionExpression,
            ExpressionAttributeNames = listAttributeNames,
            ExpressionAttributeValues = attributeValues,
            IndexName = "GSI1",
            Limit = (pageSize-bookResponse.Books.Count),
            ExclusiveStartKey = startKey
        };
        
        var queryResponse = await this.client.QueryAsync(queryRequest);
        Logger.LogInformation($"Executing query for listing books, for {bookResponse.Metadata.LastGsiPartition} and {bookResponse.Metadata.LastGsiKey}. Next key page {JsonSerializer.Serialize(queryResponse.LastEvaluatedKey)}");
        return queryResponse;
    }
    
    private bool IsPostFix()
    {
        string isPostFixString = Environment.GetEnvironmentVariable("IS_POSTFIX");
        if (string.IsNullOrEmpty(isPostFixString) || isPostFixString.ToLower() == "false")
        {
            return false;
        }

        return true;
    }
}