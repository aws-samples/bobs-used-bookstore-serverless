namespace BookInventory.Repository;

using BookInventory.Models;

public interface IBookInventoryRepository
{
    Task<IList<Book>> GetAllBooks();

    Task<Book> GetBookById(string id);

    Task<int> AddBook(Book newBook);
}