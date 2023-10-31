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
        A.CallTo(() => this.bookInventoryRepository.GetByPrimaryKeyAsync("8274dcb1-e651-41b4-98c6-d416e8b59fab"))
            .Returns(new Book() { PK = BookInventoryConstants.BOOK, SK = "8274dcb1-e651-41b4-98c6-d416e8b59fab", Name = "History" });

        // Act
        var response = await this.sut.GetBookById("8274dcb1-e651-41b4-98c6-d416e8b59fab");

        // Assert
        response.Should().NotBeNull();
        response.Name.Should().Be("History");
        A.CallTo(() => this.bookInventoryRepository.GetByPrimaryKeyAsync("8274dcb1-e651-41b4-98c6-d416e8b59fab")).MustHaveHappened(1, Times.Exactly);
    }

    [Fact]
    public async Task AddBook_WhenRequestIsValid_ShouldSaveData()
    {
        // Arrange
        A.CallTo(() => this.bookInventoryRepository.SaveAsync(A<Book>._))
            .Returns(Task.CompletedTask);
        var book = new CreateBookDto() { Author = "Bob", BookType = "Old", Condition = "Like New", ISBN = "42343222", Name = "History", Quantity = 10, Year = 2023 };
        // Act
        await this.sut.AddBookAsync(book);

        // Assert
        A.CallTo(() => this.bookInventoryRepository.SaveAsync(A<Book>._)).MustHaveHappened(1, Times.Exactly);
    }
}