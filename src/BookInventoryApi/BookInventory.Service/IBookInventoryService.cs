namespace BookInventory.Service;

using BookInventory.Models;

public interface IBookInventoryService
{
    Task<BookDto> GetBookById(string id);

    Task AddBookAsync(CreateBookDto dto);
}