namespace BookInventory.Repository;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;

using BookInventory.Models;

public class BookInventoryRepository : BaseRepository<Book>,
    IBookInventoryRepository
{
    private readonly IAmazonDynamoDB client;
    private readonly Dictionary<string, string> listAttributeNames = new(1) { { "#gsi1pk", "GSI1PK" } };
    private const int MAX_MONTHS_TO_CHECK = 3;

    public BookInventoryRepository(IDynamoDBContext context, IAmazonDynamoDB client)
        : base(context)
    {
        this.client = client;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> List(int page = 10, string cursor = null)
    {
        var bookResponse = new ListResponse(cursor);

        // Execute the initial query.
        var queryResponse = await this.ExecuteQuery(bookResponse, page);
        bookResponse.Metadata.LastEvaluatedKey = queryResponse.LastEvaluatedKey;

        this.ProcessResults(ref bookResponse, queryResponse, page);

        if (bookResponse.Books.Count == page)
        {
            Logger.LogInformation("Page size reached, returning");
            
            // If they are equal, and there is no last evaluated key, increment the months before to allow for queries into the next month.
            // For example, if the page size is 1 and there are no other items in that month then move the cursor to the next month
            if (queryResponse.Count == page && queryResponse.LastEvaluatedKey == null)
            {
                bookResponse.Metadata.LastCheckedMonth++;
            }
            
            return bookResponse;
        }

        for (var currentMonth = bookResponse.Metadata.LastCheckedMonth; currentMonth <= MAX_MONTHS_TO_CHECK; currentMonth++)
        {
            Logger.LogInformation($"Page size not met for current query, attempting query in {currentMonth}(s) previous. Page size is currently at {bookResponse.Books.Count}.");
            
            bookResponse.Metadata.LastCheckedMonth = currentMonth;
            
            bookResponse.Metadata.LastDate = bookResponse.Metadata.LastDate.AddMonths(-currentMonth);

            queryResponse = await this.ExecuteQuery(bookResponse, page);
            bookResponse.Metadata.LastEvaluatedKey = queryResponse.LastEvaluatedKey;

            // Build the list of books.
            this.ProcessResults(ref bookResponse, queryResponse, page);

            if (bookResponse.Books.Count == page)
            {
                return bookResponse;
            }
        }

        return bookResponse;
    }

    [Tracing]
    private void ProcessResults(ref ListResponse bookResponse, QueryResponse queryResponse, int pageSize)
    {
        Logger.LogInformation($"Processing results, QueryResponse contains {pageSize} item(s)");
        
        // Build the list of books.
        foreach (var item in queryResponse.Items)
        {
            bookResponse.Books.Add(
                new Book(
                    item.AsString("Name"),
                    item.AsString("Author"),
                    item.AsString("ISBN"),
                    item.AsString("Publisher"),
                    item.AsString("BookType"),
                    item.AsString("Genre"),
                    item.AsString("Condition"),
                    item.AsDecimal("Price"),
                    item.AsInt("Quantity"),
                    item.AsString("Summary"),
                    item.AsInt("Year"),
                    item.AsString("CoverImageUrl")));

            if (bookResponse.Books.Count != pageSize) continue;
            
            Logger.LogInformation("Page size reached, returning");
                
            break;
        }
    }

    [Tracing]
    private async Task<QueryResponse> ExecuteQuery(ListResponse bookResponse, int pageSize)
    {
        Logger.LogInformation($"Executing query for listing books, using date of {(bookResponse.Metadata.LastDate.ToString("yyyyMM"))} and page size of {pageSize}");
        
        var attributeValues = new Dictionary<string, AttributeValue>(1);
        attributeValues.Add(
            ":gsi1pk",
            new AttributeValue(bookResponse.Metadata.LastDate.ToString("yyyyMM")));

        var queryRequest = new QueryRequest
        {
            TableName = BookInventoryConstants.TABLE_NAME,
            KeyConditionExpression = "#gsi1pk = :gsi1pk",
            ExpressionAttributeNames = this.listAttributeNames,
            ExpressionAttributeValues = attributeValues,
            IndexName = "GSI1",
            Limit = pageSize
        };

        var queryResponse = await this.client.QueryAsync(queryRequest);

        return queryResponse;
    }
}