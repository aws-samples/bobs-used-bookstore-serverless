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
    private readonly Dictionary<string, string> listAttributeNames = new(1) { { "#gsi1pk", "GSI1PK" } };
    private const int MAX_MONTHS_TO_CHECK = 1;

    public BookInventoryRepository(IDynamoDBContext context, IAmazonDynamoDB client)
    {
        this.context = context;
        this.client = client;
    }

    [Tracing]
    public async Task<Book?> GetByIdAsync(string bookId)
    {
        return await context.LoadAsync<Book>(bookId);
    }

    [Tracing]
    public async Task SaveAsync(Book book)
    {
        await context.SaveAsync(book);
    }

    [Tracing]
    public async Task<ListResponse> List(int pageSize = 10, string cursor = null)
    {
        var bookResponse = new ListResponse(cursor);

        // Execute the initial query.
        var queryResponse = await this.ExecuteQuery(bookResponse, pageSize);

        bookResponse.Metadata.AddPartitions(queryResponse);

        this.ProcessResults(ref bookResponse, queryResponse, pageSize);

        if (bookResponse.Books.Count == pageSize)
        {
            Logger.LogInformation("Page size reached, returning");

            // If they are equal, and there is no last evaluated key, increment the months before to allow for queries into the next month.
            // For example, if the page size is 1 and there are no other items in that month then move the cursor to the next month
            if (queryResponse.Count == pageSize && queryResponse.LastEvaluatedKey == null)
            {
                bookResponse.Metadata.LastCheckedMonth++;
            }

            return bookResponse;
        }

        for (var currentMonth = bookResponse.Metadata.LastCheckedMonth; currentMonth <= MAX_MONTHS_TO_CHECK; currentMonth++)
        {
            bookResponse = await this.ListBooksInPreviousMonths(
                pageSize,
                currentMonth,
                bookResponse);

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
        int currentMonth,
        ListResponse bookResponse)
    {
        QueryResponse queryResponse;

        Logger.LogInformation($"Page size not met for current query, attempting query in {currentMonth}(s) previous. Page size is currently at {bookResponse.Books.Count}.");

        // When moving to a different partition (in this case by looking at a different month), the last evaluated key needs to be reset.
        bookResponse.Metadata.ResetPartitions(currentMonth);

        queryResponse = await this.ExecuteQuery(
            bookResponse,
            page);

        bookResponse.Metadata.AddPartitions(queryResponse);

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
        Logger.LogInformation($"Processing results, QueryResponse contains {pageSize} item(s)");

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

        Dictionary<string, AttributeValue> startKey = null;

        if (!string.IsNullOrEmpty(bookResponse.Metadata.LastPartition))
        {
            startKey = new Dictionary<string, AttributeValue>()
            {
                { "PK", new AttributeValue(bookResponse.Metadata.LastPartition) },
                { "SK", new AttributeValue(bookResponse.Metadata.LastKey) },
                { "GSI1PK", new AttributeValue(bookResponse.Metadata.LastGsiPartition) },
                { "GSI1SK", new AttributeValue(bookResponse.Metadata.LastGsiKey) }
            };
        }

        var queryRequest = new QueryRequest
        {
            TableName = BookInventoryConstants.TABLE_NAME,
            KeyConditionExpression = "#gsi1pk = :gsi1pk",
            ExpressionAttributeNames = this.listAttributeNames,
            ExpressionAttributeValues = attributeValues,
            IndexName = "GSI1",
            Limit = pageSize,
            ExclusiveStartKey = startKey
        };

        var queryResponse = await this.client.QueryAsync(queryRequest);

        return queryResponse;
    }
}