using BookInventory.Models;
using BookInventory.Repository;

namespace BookInventory.Service;

public class BookInventoryService : IBookInventoryService
{
    private IBookInventoryRepository bookInventoryRepository;

    public BookInventoryService(IBookInventoryRepository bookInventoryRepository)
    {
        this.bookInventoryRepository = bookInventoryRepository;
    }

    public async Task<BookDto?> GetBookById(string id)
    {
        var book = await this.bookInventoryRepository.GetByIdAsync(id);
        return book == null ? null : new BookDto()
        {
            BookId = book.SK,
            Name = book.Name,
            Author = book.Author,
            BookType = book.BookType,
            Condition = book.Condition,
            Genre = book.Genre,
            Publisher = book.Publisher,
            ISBN = book.ISBN,
            Summary = book.Summary,
            Price = book.Price,
            Quantity = book.Quantity,
            Year = book.Year,
            CoverImage = book.CoverImageUrl
        };
    }

    public async Task AddBookAsync(CreateBookDto dto)
    {
        var book = new Book(
            dto.Name,
            dto.Author,
            dto.ISBN,
            dto.Publisher,
            dto.BookType,
            dto.Genre,
            dto.Condition,
            dto.Price,
            dto.Quantity,
            dto.Summary,
            dto.Year);
        await this.bookInventoryRepository.SaveAsync(book);
    }
}