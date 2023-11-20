namespace BookInventory.Service;

using BookInventory.Models;

public interface IBookInventoryService
{
    Task<BookQueryResponse> ListAllBooks(int pageCount = 10, string cursor = null);

    Task<PaginatedResult<BookDto>> GetBooksAsync(int pageSize, string? pageKey);

    Task<BookDto?> GetBookById(string id);

    Task<string> AddBookAsync(CreateBookDto dto);
}