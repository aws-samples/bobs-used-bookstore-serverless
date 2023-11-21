using BookInventory.Api.Validators;
using BookInventory.Common;
using BookInventory.Models;
using BookInventory.Service;
using FakeItEasy;
using FluentAssertions;
using FluentValidation;
using System.Net;
using System.Text.Json;
using Xunit;

namespace BookInventory.Api.UnitTests;

public class FunctionsTests
{
    private readonly IBookInventoryService bookInventoryService;
    private readonly IValidator<CreateUpdateBookDto> bookValidator;
    private readonly Functions sut;

    public FunctionsTests()
    {
        this.bookInventoryService = A.Fake<IBookInventoryService>();
        this.bookValidator = new CreateBookDtoValidator();
        this.sut = new Functions(this.bookInventoryService, this.bookValidator);
    }

    [Fact]
    public async Task GetBook_WhenRequestIsValid_ShouldRespondSearchResult()
    {
        // Arrange
        A.CallTo(() => this.bookInventoryService.GetBookById("8274dcb1-e651-41b4-98c6-d416e8b59fab"))
            .Returns(new BookDto() { BookId = "8274dcb1-e651-41b4-98c6-d416e8b59fab", Name = "History", Author = "Bob", BookType = "Old", Condition = "Like New" });

        // Act
        var response = await this.sut.GetBook("8274dcb1-e651-41b4-98c6-d416e8b59fab");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Body.Should().NotBeNull();
        var apiWrapperResponse = JsonSerializer.Deserialize<ApiWrapper<Book>>(response.Body);
        apiWrapperResponse.Should().NotBeNull();
        apiWrapperResponse!.Data.Should().NotBeNull();
        apiWrapperResponse.Data.Name.Should().Be("History");
        A.CallTo(() => this.bookInventoryService.GetBookById("8274dcb1-e651-41b4-98c6-d416e8b59fab")).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetBook_WhenRequestIsInvalid_ShouldRespondBadRequest(string id)
    {
        // Act
        var response = await this.sut.GetBook(id);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        A.CallTo(() => this.bookInventoryService.GetBookById(A.Dummy<string>())).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetBook_WhenRequestIsInvalid_ShouldRespondNotFound()
    {
        // Arrange
        BookDto? bookDto = null;
        A.CallTo(() => this.bookInventoryService.GetBookById("8274dcb1-e651-41b4-98c6-d416e8b59fab"))
            .Returns(bookDto);

        // Act
        var response = await this.sut.GetBook("8274dcb1-e651-41b4-98c6-d416e8b59fab");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        A.CallTo(() => this.bookInventoryService.GetBookById("8274dcb1-e651-41b4-98c6-d416e8b59fab")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AddBook_WhenRequestIsValid_ShouldCreateBook()
    {
        // Arrange
        string bookId = Guid.NewGuid().ToString();
        CreateUpdateBookDto book = new()
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
        A.CallTo(() => this.bookInventoryService.AddBookAsync(book)).Returns(bookId);
        
        // Act
        var response = await this.sut.AddBook(book);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);
        response.Body.Should().NotBeNull();
        var apiWrapperResponse = JsonSerializer.Deserialize<ApiWrapper<string>>(response.Body);
        apiWrapperResponse.Should().NotBeNull();
        apiWrapperResponse!.Data.Should().NotBeNull();
        apiWrapperResponse.Data.Should().Be(bookId);
        A.CallTo(() => this.bookInventoryService.AddBookAsync(
            A<CreateUpdateBookDto>.That.Matches(x => x.Name == book.Name
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
    public async Task AddBook_WhenRequestIsInvalid_ShouldRespondBadRequest()
    {
        // Arrange
        CreateUpdateBookDto createBookDto = new();

        // Act
        var response = await this.sut.AddBook(createBookDto);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        A.CallTo(() => this.bookInventoryService.AddBookAsync(A<CreateUpdateBookDto>.Ignored)).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateBook_WhenRequestIsValid_ShouldUpdateBook()
    {
        // Arrange
        string bookId = Guid.NewGuid().ToString();
        CreateUpdateBookDto book = new()
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
        A.CallTo(() => this.bookInventoryService.UpdateBookAsync(bookId, book)).Returns(Task.CompletedTask);

        // Act
        var response = await this.sut.UpdateBook(bookId, book);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        A.CallTo(() => this.bookInventoryService.UpdateBookAsync(bookId,
            A<CreateUpdateBookDto>.That.Matches(x => x.Name == book.Name
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
    public async Task UpdateBook_WhenRequestIsInvalid_ShouldRespondBadRequest()
    {
        // Arrange
        string bookId = Guid.NewGuid().ToString();
        CreateUpdateBookDto book = new();

        // Act
        var response = await this.sut.UpdateBook(bookId, book);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        A.CallTo(() => this.bookInventoryService.UpdateBookAsync(A<string>._, A<CreateUpdateBookDto>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateBook_WhenRequestIsInvalid_ShouldRespondNotFound()
    {
        // Arrange
        string bookId = Guid.NewGuid().ToString();        
        CreateUpdateBookDto book = new()
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
        A.CallTo(() => this.bookInventoryService.UpdateBookAsync(bookId, book)).Throws(new ArgumentException($"Book not found for id {bookId}"));

        // Act
        var response = await this.sut.UpdateBook(bookId, book);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        A.CallTo(() => this.bookInventoryService.UpdateBookAsync(A<string>._, A<CreateUpdateBookDto>._)).MustHaveHappenedOnceExactly();
    }
}