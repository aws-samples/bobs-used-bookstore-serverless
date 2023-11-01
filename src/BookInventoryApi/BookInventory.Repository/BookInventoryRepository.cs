using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using BookInventory.Models;

namespace BookInventory.Repository;

public class BookInventoryRepository : BaseRepository<Book>, IBookInventoryRepository
{
    private readonly IAmazonDynamoDB client;

    public BookInventoryRepository(IDynamoDBContext context, IAmazonDynamoDB client)
        : base(context)
    {
        this.client = client;
    }
}