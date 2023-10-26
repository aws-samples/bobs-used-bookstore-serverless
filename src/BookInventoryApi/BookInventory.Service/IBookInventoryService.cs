namespace BookInventory.Service;

using BookInventory.Models;

public interface IBookInventoryService
{
    Task<IList<Book>> GetAllBooks();

    Task<Book> GetBookById(string id);

    Task<string> AddBook(Book newBook); 
}