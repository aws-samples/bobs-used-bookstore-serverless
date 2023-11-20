namespace BookInventory.Service;

using BookInventory.Models;

public interface IBookInventoryService
{
    Task<BookQueryResponse> ListAllBooks(int pageCount = 10, string cursor = null);
    
    Task<BookDto?> GetBookById(string id);

    Task<string> AddBookAsync(CreateUpdateBookDto dto);

    Task UpdateBookAsync(string bookId, CreateUpdateBookDto dto);
}