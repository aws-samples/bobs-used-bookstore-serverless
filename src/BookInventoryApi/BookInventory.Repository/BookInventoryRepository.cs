using Amazon.DynamoDBv2.DataModel;
using BookInventory.Models;

namespace BookInventory.Repository;

public class BookInventoryRepository : IBookInventoryRepository
{
    private readonly IDynamoDBContext context;

    public BookInventoryRepository(IDynamoDBContext context)
    {
        this.context = context;
    }

    public async Task<Book?> GetByIdAsync(string bookId)
    {
        return await context.LoadAsync<Book>(BookInventoryConstants.BOOK, bookId);
    }

    public async Task SaveAsync(Book book)
    {
        await context.SaveAsync(book);
    }
}