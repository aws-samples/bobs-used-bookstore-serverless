using BookInventory.Models;
using BookInventory.Repository;
using BookInventory.Service.Exceptions;

namespace BookInventory.Service;

public class BookInventoryService : IBookInventoryService
{
    private IBookInventoryRepository bookInventoryRepository;

    public BookInventoryService(IBookInventoryRepository bookInventoryRepository)
    {
        this.bookInventoryRepository = bookInventoryRepository;
    }

    /// <inheritdoc />
    public async Task<BookQueryResponse> ListAllBooksAsync(int pageCount = 10, string cursor = null)
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

    public async Task<BookDto?> GetBookByIdAsync(string id)
    {
        var book = await this.bookInventoryRepository.GetByIdAsync(id);
        return book == null ? null : new BookDto()
        {
            BookId = book.BookId,
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

    public async Task<string> AddBookAsync(CreateBookDto dto)
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
        return book.BookId;
    }

    public async Task UpdateBookAsync(string bookId, UpdateBookDto dto)
    {
        var book = await this.bookInventoryRepository.GetByIdAsync(bookId) ?? throw new ProductNotFoundException($"Book not found.", bookId);
        book.Name = dto.Name;
        book.Author = dto.Author;
        book.ISBN = dto.ISBN;
        book.Publisher = dto.Publisher;
        book.Quantity = dto.Quantity;
        book.Summary = dto.Summary;
        book.Genre = dto.Genre;
        book.Condition = dto.Condition;
        book.Price = dto.Price;
        book.Year = dto.Year;
        book.BookType = dto.BookType;
        await this.bookInventoryRepository.SaveAsync(book);
    }
}