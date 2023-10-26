namespace BookInventory.Service;

using System.Text.Json;

using AWS.Lambda.Powertools.Logging;

using BookInventory.Models;
using BookInventory.Repository;

public class BookInventoryService : IBookInventoryService
{
    private IBookInventoryRepository bookInventoryRepository;

    public BookInventoryService(IBookInventoryRepository bookInventoryRepository)
    {
        this.bookInventoryRepository = bookInventoryRepository;
    }
    
    public async Task<IList<Book>> GetAllBooks()
    {
        return await this.bookInventoryRepository.GetAllBooks();
    }
    
    public async Task<Book> GetBookById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Logger.LogError($"Book id {id} is not valid");
            throw new Exception("No book found");
        }

        var book = await this.bookInventoryRepository.GetBookById(id);
        if (book is null)
        {
            Logger.LogError($"Book id {id} is not found");
            throw new Exception("No book found");
        }
        Logger.LogInformation($"Searched book {JsonSerializer.Serialize(book)}");
        return book;
    }

    public async Task<string> AddBook(Book newBook)
    {
        if (newBook is null || string.IsNullOrWhiteSpace(newBook.Name))
        {
            Logger.LogError($"Book is null, cannot add new Book");
            throw new Exception("New book cannot be added"); 
        }
        
        if (string.IsNullOrWhiteSpace(newBook.Id))
        {
            newBook.Id = Guid.NewGuid().ToString();
        }
        else
        {
            var existingBook = await this.bookInventoryRepository.GetBookById(newBook.Id);
            if (!string.IsNullOrWhiteSpace(existingBook?.Id))
            {
                Logger.LogError($"New book id {newBook.Id} already exists");
                throw new Exception("New book already exists, cannot be added");
            }
        }

        if (await this.bookInventoryRepository.AddBook(newBook) > 0)
        {
            return newBook.Id;
        }
        else
        {
            Logger.LogError($"Error in adding new Book id {newBook.Id}");
            throw new Exception($"Error in adding new Book id {newBook.Id}");
        }
    }
}