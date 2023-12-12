using BookInventory.Models;
using BookInventory.Repository;
using BookInventory.Service.Exceptions;
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
            .Returns(new Book() { BookId = "8274dcb1-e651-41b4-98c6-d416e8b59fab", Name = "History" });

        // Act
        var response = await this.sut.GetBookByIdAsync("8274dcb1-e651-41b4-98c6-d416e8b59fab");

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
        var response = await this.sut.GetBookByIdAsync("8274dcb1-e651-41b4-98c6-d416e8b59fab");

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
        var response = await this.sut.AddBookAsync(book);

        // Assert
        response.Should().NotBeNull();
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

    [Fact]
    public async Task UpdateBook_WhenRequestIsValid_ShouldSaveData()
    {
        // Arrange
        string bookId = Guid.NewGuid().ToString();
        Book book = new Book()
        {
            BookId = bookId,
            Name = "The Apocalypse",
        };
        A.CallTo(() => this.bookInventoryRepository.GetByIdAsync(bookId)).Returns(book);
        UpdateBookDto updateBook = new()
        {
            Name = "2022: The Apocalypse",
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
        A.CallTo(() => this.bookInventoryRepository.SaveAsync(A<Book>._)).Returns(Task.CompletedTask);

        // Act
        await this.sut.UpdateBookAsync(bookId, updateBook);

        // Assert
        A.CallTo(() => this.bookInventoryRepository.SaveAsync(
            A<Book>.That.Matches(x => 
            //x.BookId == bookId
             x.Name == updateBook.Name
            && x.Author == updateBook.Author
            && x.ISBN == updateBook.ISBN
            && x.BookType == updateBook.BookType
            && x.Condition == updateBook.Condition
            && x.Genre == updateBook.Genre
            && x.Publisher == updateBook.Publisher
            && x.Price == updateBook.Price
            && x.Quantity == updateBook.Quantity
            && x.Summary == updateBook.Summary
            ))).MustHaveHappenedOnceExactly();
        A.CallTo(() => this.bookInventoryRepository.GetByIdAsync(bookId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void UpdateBook_WhenRequestIsInvalid_ShouldThrowException()
    {
        // Arrange
        string bookId = Guid.NewGuid().ToString();
        Book? book = null;
        A.CallTo(() => this.bookInventoryRepository.GetByIdAsync(bookId)).Returns(book);
        UpdateBookDto updateBook = new()
        {
            Name = "2022: The Apocalypse",
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

        // Act Assert
        var response = Assert.ThrowsAsync<ProductNotFoundException>(async () => await this.sut.UpdateBookAsync(bookId, updateBook));
        A.CallTo(() => this.bookInventoryRepository.SaveAsync(A<Book>._)).MustNotHaveHappened();
    }
}