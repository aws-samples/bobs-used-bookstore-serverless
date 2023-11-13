using BookInventory.Models;

namespace BookInventory.Repository;

public interface IBookInventoryRepository
{
    Task<Book?> GetByIdAsync(string bookId);

    Task SaveAsync(Book book);
}