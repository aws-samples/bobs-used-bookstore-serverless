using BookInventory.Models;

namespace BookInventory.Service;
public interface IBookInventoryService
{
    Task<BookQueryResponse> ListAllBooks(int pageCount = 10, string cursor = null);

    Task<BookDto?> GetBookById(string id);

    Task<string> AddBookAsync(CreateBookDto dto);
}