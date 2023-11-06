using BookInventory.Models;
using BookInventory.Repository;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace BookInventory.Service.UnitTests;

public class BookInventoryServiceTests
{
    private readonly IBookInventoryRepository bookInventoryRepository;
    private readonly IBookInventoryService sut;

    public BookInventoryServiceTests()
    {
        this.bookInventoryRepository = A.Fake<IBookInventoryRepository>();
        this.sut = new BookInventoryService(this.bookInventoryRepository);
    }

    [Fact]
    public async Task GetBookById_WhenRequestIsValid_ShouldRespondSearchResult()
    {
        // Arrange
        A.CallTo(() => this.bookInventoryRepository.GetByIdAsync("8274dcb1-e651-41b4-98c6-d416e8b59fab"))
            .Returns(new Book() { PK = BookInventoryConstants.BOOK, SK = "8274dcb1-e651-41b4-98c6-d416e8b59fab", Name = "History" });

        // Act
        var response = await this.sut.GetBookById("8274dcb1-e651-41b4-98c6-d416e8b59fab");

        // Assert
        response.Should().NotBeNull();
        response.Name.Should().Be("History");
        A.CallTo(() => this.bookInventoryRepository.GetByIdAsync("8274dcb1-e651-41b4-98c6-d416e8b59fab")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetBookById_WhenRequestIsInvalid_ShouldRespondNullResult()
    {
        // Arrange
        Book? book = null;
        A.CallTo(() => this.bookInventoryRepository.GetByIdAsync("8274dcb1-e651-41b4-98c6-d416e8b59fab"))
            .Returns(book);

        // Act
        var response = await this.sut.GetBookById("8274dcb1-e651-41b4-98c6-d416e8b59fab");

        // Assert
        response.Should().BeNull();
        A.CallTo(() => this.bookInventoryRepository.GetByIdAsync("8274dcb1-e651-41b4-98c6-d416e8b59fab")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AddBook_WhenRequestIsValid_ShouldSaveData()
    {
        // Arrange
        CreateBookDto book = new()
        {
            Name = "2020: The Apocalypse",
            Author = "Li Juan",
            ISBN = "6556784356",
            BookType = "Hardcover",
            Condition = "Like New",
            Genre = "Mystery, Thriller & Suspense",
            Publisher = "Arcadia Books",
            Price = 10,
            Quantity = 1,
            Summary = "Sample book"
        };
        A.CallTo(() => this.bookInventoryRepository.SaveAsync(A<Book>._))
            .Returns(Task.CompletedTask);
        // Act
        await this.sut.AddBookAsync(book);

        // Assert
        A.CallTo(() => this.bookInventoryRepository.SaveAsync(
            A<Book>.That.Matches(x => x.Name == book.Name
            && x.Author == book.Author
            && x.ISBN == book.ISBN
            && x.BookType == book.BookType
            && x.Condition == book.Condition
            && x.Genre == book.Genre
            && x.Publisher == book.Publisher
            && x.Price == book.Price
            && x.Quantity == book.Quantity
            && x.Summary == book.Summary
            ))).MustHaveHappenedOnceExactly();
    }
}