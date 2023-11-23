namespace BookInventory.Service;

using BookInventory.Models;

public interface IBookInventoryService
{
    Task<BookQueryResponse> ListAllBooksAsync(int pageCount = 10, string cursor = null);
    
    Task<BookDto?> GetBookByIdAsync(string id);

    Task<string> AddBookAsync(CreateBookDto dto);

    Task UpdateBookAsync(string bookId, UpdateBookDto dto);
}