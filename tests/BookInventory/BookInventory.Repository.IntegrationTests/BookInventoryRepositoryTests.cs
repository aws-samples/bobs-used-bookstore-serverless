using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using BookInventory.Models;
using BookInventory.Repository.IntegrationTests.Builders;
using BookInventory.Repository.IntegrationTests.Fixtures;
using FluentAssertions;

namespace BookInventory.Repository.IntegrationTests;

public abstract class BookInventoryRepositoryTestBase : IClassFixture<DynamoDbFixture>, IDisposable
{
    protected string TableName => "test-book-inventory";

    protected readonly DynamoDbFixture fixture;

    protected BookInventoryRepositoryTestBase(DynamoDbFixture fixture)
    {
        this.fixture = fixture;
        
        CreateDynamoDbTable(fixture.DynamoDbClient);
    }

    private void CreateDynamoDbTable(IAmazonDynamoDB dynamoDbClient)
    {
        var createTableRequest = new CreateTableRequest
        {
            TableName = TableName,
            ProvisionedThroughput = new ProvisionedThroughput(1, 1),
            AttributeDefinitions =
            [
                new("BookId", ScalarAttributeType.S),
                new("GSI1PK", ScalarAttributeType.S),
                new("GSI1SK", ScalarAttributeType.S)
            ],
            KeySchema = [new KeySchemaElement("BookId", KeyType.HASH)],
            GlobalSecondaryIndexes =
            [
                new()
                {
                    IndexName = "GSI1",
                    ProvisionedThroughput = new ProvisionedThroughput(1, 1),
                    KeySchema =
                    [
                        new("GSI1PK", KeyType.HASH),
                        new("GSI1SK", KeyType.RANGE)
                    ],
                    Projection = new Projection { ProjectionType = ProjectionType.ALL }
                }
            ]
        };

        dynamoDbClient.CreateTableAsync(createTableRequest).Wait();
    }

    /// <summary>
    /// Clean up between tests
    /// </summary>
    public void Dispose()
    {
        fixture.DynamoDbClient.DeleteTableAsync(TableName).Wait();
    }
}

public class BookInventoryRepositoryTests(DynamoDbFixture fixture) : BookInventoryRepositoryTestBase(fixture)
{
    private BookInventoryRepository CreateBookInventoryRepository()
    {
        return new BookInventoryRepository(
            fixture.DynamoDbContext, 
            fixture.DynamoDbClient, 
            new BookInventoryRepositoryTestOptions(TableName, true));
    }

    [Fact]
    public async Task GetByIdAsync_EmptyDB_ShouldReturnNull()
    {
        var target = CreateBookInventoryRepository();

        var result = await target.GetByIdAsync("book-1");

        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetByIdAsync_BookExists_ShouldReturnBook()
    {
        var book = new BookBuilder()
            .WithBookId("book-1")
            .Build();
        
        var target = CreateBookInventoryRepository();

        await target.SaveAsync(book);

        var result = await target.GetByIdAsync("book-1");
        result.Should().BeEquivalentTo(book, 
            options => options.Excluding(b => b.LastUpdated));
    }   
    
    [Fact]
    public async Task GetByIdAsync_BookExistsWithOtherBooks_ShouldReturnBook()
    {
        var book1 = new BookBuilder()
            .WithBookId("book-1")
            .Build();  
        
        var book2 = new BookBuilder()
            .WithBookId("book-2")
            .Build();  
        
        var book3 = new BookBuilder()
            .WithBookId("book-3")
            .Build();
        
        var target = CreateBookInventoryRepository();

        await target.SaveAsync(book1);
        await target.SaveAsync(book2);
        await target.SaveAsync(book3);

        var result = await target.GetByIdAsync("book-2");
        result.Should().BeEquivalentTo(book2, 
            options => options.Excluding(b => b.LastUpdated));
    }
    
    [Fact]
    public async Task GetByIdAsync_NoBooksInRepository_ShouldReturnNull()
    {
        var target = CreateBookInventoryRepository();

        var result = await target.GetByIdAsync("book-1");

        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetByIdAsync_BookNOtFound_ShouldReturnNull()
    {
        var otherBook = new BookBuilder()
            .WithBookId("book-other")
            .Build();  

        var target = CreateBookInventoryRepository();

        var result = await target.GetByIdAsync("book-1");

        result.Should().BeNull();
    }
    
    [Fact]
    public async Task List_NoItemsInDb_ShouldReturnEmptyList()
    {
        var target = CreateBookInventoryRepository();

        var result = await target.List();

        result.Books.Should().BeEmpty();
    }
    
    [Fact]
    public async Task List_BooksInDb_ShouldReturnAllBooks()
    {
        var book1 = new BookBuilder()
            .WithBookId("book-1")
            .WithCreatedBy("user-1")
            .Build();

        var book2 = new BookBuilder()
            .WithBookId("book-2")
            .WithCreatedBy("user-2")

            .Build();

        var book3 = new BookBuilder()
            .WithBookId("book-3")
            .WithCreatedBy("user-3")

            .Build();

        var target = CreateBookInventoryRepository();

        await target.SaveAsync(book1);
        await target.SaveAsync(book2);
        await target.SaveAsync(book3);

        var result = await target.List();

        result.Books.Should().BeEquivalentTo(new[]{book1, book2, book3}, 
            options => options
                .Excluding(b => b.LastUpdated)
                .Excluding(b => b.LastUpdatedString));
    }
    
    [Fact]
    public async Task List_BooksInDbUsePaginationCursorStart_ShouldReturnFirstPage()
    {
        var book1 = new BookBuilder()
            .WithBookId("book-1")
            .WithCreatedBy("user-1")
            .Build();

        var book2 = new BookBuilder()
            .WithBookId("book-2")
            .WithCreatedBy("user-2")

            .Build();

        var book3 = new BookBuilder()
            .WithBookId("book-3")
            .WithCreatedBy("user-3")

            .Build();

        var target = CreateBookInventoryRepository();

        await target.SaveAsync(book1);
        await target.SaveAsync(book2);
        await target.SaveAsync(book3);

        var result = await target.List(2);

        result.Books.Should().BeEquivalentTo(new[]{book1, book2}, 
            options => options
                .Excluding(b => b.LastUpdated)
                .Excluding(b => b.LastUpdatedString));
    }
    
    [Fact]
    public async Task List_BooksInDbUsePaginationCursorEndOffirstPage_ShouldReturnSecondPage()
    {
        var book1 = new BookBuilder()
            .WithBookId("book-1")
            .WithCreatedBy("user-1")
            .Build();

        var book2 = new BookBuilder()
            .WithBookId("book-2")
            .WithCreatedBy("user-2")

            .Build();

        var book3 = new BookBuilder()
            .WithBookId("book-3")
            .WithCreatedBy("user-3")

            .Build();

        var target = CreateBookInventoryRepository();

        await target.SaveAsync(book1);
        await target.SaveAsync(book2);
        await target.SaveAsync(book3);

        var result1 = await target.List(2);
        var result2 = await target.List(2, result1.Cursor);

        result2.Books.Should().BeEquivalentTo(new[]{book3}, 
            options => options
                .Excluding(b => b.LastUpdated)
                .Excluding(b => b.LastUpdatedString));
    }
}