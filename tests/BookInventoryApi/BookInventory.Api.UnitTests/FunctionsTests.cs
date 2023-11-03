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
    private readonly IBookInventoryService bookInventoryServiceFake;
    private readonly IValidator<CreateBookDto> createBookValidator;
    private readonly Functions sut;

    public FunctionsTests()
    {
        this.bookInventoryServiceFake = A.Fake<IBookInventoryService>();
        this.createBookValidator = new CreateBookDtoValidator();
        this.sut = new Functions(this.bookInventoryServiceFake, this.createBookValidator);
    }

    [Fact]
    public async Task GetBook_WhenRequestIsValid_ShouldRespondSearchResult()
    {
        // Arrange
        A.CallTo(() => this.bookInventoryServiceFake.GetBookById("8274dcb1-e651-41b4-98c6-d416e8b59fab"))
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
        A.CallTo(() => this.bookInventoryServiceFake.GetBookById("8274dcb1-e651-41b4-98c6-d416e8b59fab")).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => this.bookInventoryServiceFake.GetBookById(A.Dummy<string>())).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetBook_WhenRequestIsInvalid_ShouldRespondNotFound()
    {
        // Arrange
        BookDto? bookDto = null;
        A.CallTo(() => this.bookInventoryServiceFake.GetBookById("8274dcb1-e651-41b4-98c6-d416e8b59fab"))
            .Returns(bookDto);

        // Act
        var response = await this.sut.GetBook("8274dcb1-e651-41b4-98c6-d416e8b59fab");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        A.CallTo(() => this.bookInventoryServiceFake.GetBookById("8274dcb1-e651-41b4-98c6-d416e8b59fab")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AddBook_WhenRequestIsValid_ShouldCreateBook()
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

        // Act
        var response = await this.sut.AddBook(book);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);
        A.CallTo(() => this.bookInventoryServiceFake.AddBookAsync(
            A<CreateBookDto>.That.Matches(x => x.Name == book.Name
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
        CreateBookDto createBookDto = new CreateBookDto();

        // Act
        var response = await this.sut.AddBook(createBookDto);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        A.CallTo(() => this.bookInventoryServiceFake.AddBookAsync(A<CreateBookDto>.Ignored)).MustNotHaveHappened();
    }
}