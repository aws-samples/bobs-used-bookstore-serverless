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

    /// <inheritdoc />
    public async Task<BookQueryResponse> ListAllBooks(int pageCount = 10, string cursor = null)
    {
        var books = await this.bookInventoryRepository.List(pageCount, cursor);

        var bookResponse = new List<BookDto>();

        if (!books.Books.Any())
        {
            return new BookQueryResponse(bookResponse, books.Cursor);
        }

        foreach (var book in books.Books)
        {
            bookResponse.Add(new BookDto(book));
        }

        return new BookQueryResponse(bookResponse, books.Cursor);
    }

    public async Task<BookDto> GetBookById(string id)
    {
        var book = await this.bookInventoryRepository.GetByPrimaryKeyAsync(BookInventoryConstants.BOOK, id);
        var bookDto = new BookDto(book);
        
        return bookDto;
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