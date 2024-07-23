using BookInventory.Models;

namespace BookInventory.Repository.IntegrationTests.Builders;

public class BookBuilder
{
    public string BookId { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public int? Year { get; set; }
    public string ISBN { get; set; }
    public string Publisher { get; set; }
    public string BookType { get; set; }
    public string Genre { get; set; }
    public string Condition { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Summary { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string CreatedBy { get; set; }
    
    public BookBuilder()
    {
        BookId = Guid.NewGuid().ToString();
        Name = "Test Book";
        Author = "Test Author";
        Year = 2023;
        ISBN = "1234567890";
        Publisher = "Test Publisher";
        BookType = "Hardcover";
        Genre = "Fiction";
        Condition = "New";
        CoverImageUrl = "https://example.com/cover.jpg";
        Summary = "This is a test book";
        Price = 19.99m;
        CreatedBy = "Test User";
        Quantity = 10;
    }
    
    public Book Build()
    {
        var book = new Book(
            Name, Author, ISBN, Publisher, BookType, Genre, Condition, Price, Quantity, Summary, Year, CoverImageUrl)
            {
                BookId = BookId,
                CreatedBy = CreatedBy
            };

        return book;
    }
    
    public BookBuilder WithBookId(string bookId)
    {
        BookId = bookId;
        return this;
    }

    public BookBuilder WithCreatedBy(string createdBy)
    {
        CreatedBy = createdBy;
        return this;
    }

    public BookBuilder WithName(string name)
    {
        Name = name;
        return this;
    }

    public BookBuilder WithAuthor(string author)
    {
        Author = author;
        return this;
    }

    public BookBuilder WithYear(int? year)
    {
        Year = year;
        return this;
    }

    public BookBuilder WithISBN(string isbn)
    {
        ISBN = isbn;
        return this;
    }

    public BookBuilder WithPublisher(string publisher)
    {
        Publisher = publisher;
        return this;
    }

    public BookBuilder WithBookType(string bookType)
    {
        BookType = bookType;
        return this;
    }

    public BookBuilder WithGenre(string genre)
    {
        Genre = genre;
        return this;
    }

    public BookBuilder WithCondition(string condition)
    {
        Condition = condition;
        return this;
    }

    public BookBuilder WithCoverImageUrl(string coverImageUrl)
    {
        CoverImageUrl = coverImageUrl;
        return this;
    }

    public BookBuilder WithSummary(string summary)
    {
        Summary = summary;
        return this;
    }

    public BookBuilder WithPrice(decimal price)
    {
        Price = price;
        return this;
    }

    public BookBuilder WithQuantity(int quantity)
    {
        Quantity = quantity;
        return this;
    }
}