using BookInventory.Models;

namespace BookInventory.Repository;

public interface IBookInventoryRepository
{
    Task<ListResponse> List(int pageSize = 10, string cursor = null);
    Task<Book?> GetByIdAsync(string bookId);
    Task SaveAsync(Book book);
}