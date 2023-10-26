namespace BookInventory.Service.UnitTests;

using System.Net;

using BookInventory.Models;
using BookInventory.Repository;

using FakeItEasy;

using FluentAssertions;

using Xunit;

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
    public async Task GetAllBooks_WhenRequestIsValid_ShouldRespondSearchResult()
    {
        // Arrange
        A.CallTo(() => this.bookInventoryRepository.GetAllBooks())
            .Returns(new List<Book>() { new Book() { Id = "100", Name = "History" } });

        // Act
        var response = await this.sut.GetAllBooks();

        // Assert
        response.Should().NotBeNull();
        response.Count.Should().Be(1);
        A.CallTo(() => this.bookInventoryRepository.GetAllBooks()).MustHaveHappened(
            1,
            Times.Exactly);
    }
    
    [Fact]
    public async Task GetBookById_WhenRequestIsValid_ShouldRespondSearchResult()
    {
        // Arrange
        A.CallTo(() => this.bookInventoryRepository.GetBookById("100"))
            .Returns(new Book() { Id = "100", Name = "History" });

        // Act
        var response = await this.sut.GetBookById("100");

        // Assert
        response.Should().NotBeNull();
        response.Name.Should().Be("History");
        A.CallTo(() => this.bookInventoryRepository.GetBookById("100")).MustHaveHappened(
            1,
            Times.Exactly);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetBookById_WhenRequestIsInvalid_ShouldRespondError(string id)
    {
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await this.sut.GetBookById(id));
    }
    
    [Fact]
    public async Task GetBookById_WhenBookNotFound_ShouldRespondError()
    {
        // Arrange
        Book book = null;
        A.CallTo(() => this.bookInventoryRepository.GetBookById("100"))!
            .Returns(book);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(async () => await this.sut.GetBookById("100"));
        exception.Message.Should().Be("No book found");
    }
    
    [Fact]
    public async Task AddBook_WhenRequestIsInvalid_ShouldRespondError()
    {
        // Arrange
        Book newBook = null;
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await this.sut.AddBook(newBook));

        newBook = new Book(); // Check for Name is empty
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await this.sut.AddBook(newBook));
    }
    
    [Fact]
    public async Task AddBook_WhenBookIsAlreadyExisting_ShouldRespondError()
    {
        // Arrange
        Book book = new Book() { Name = "History" };
        A.CallTo(() => this.bookInventoryRepository.GetBookById("100"))!
            .Returns(book);
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await this.sut.AddBook(book));
        A.CallTo(() => this.bookInventoryRepository.AddBook(A.Dummy<Book>())).MustNotHaveHappened();
    }
    
    [Fact]
    public async Task AddBook_WhenBookIsNotExisting_ShouldAddBook()
    {
        // Arrange
        Book book = null;
        A.CallTo(() => this.bookInventoryRepository.GetBookById("100"))!
            .Returns(book);
        A.CallTo(() => this.bookInventoryRepository.AddBook(A<Book>.Ignored)).Returns(1);
            
        
        // Act 
        var response = await this.sut.AddBook(new Book() { Name = "History" });
        
        // Assert
        response.Should().NotBeNull();
        A.CallTo(() => this.bookInventoryRepository.AddBook(A<Book>._)).MustHaveHappened(
            1,
            Times.Exactly);
    }
    
    [Fact]
    public async Task AddBook_WhenBookIsNotExistingButAddBookErrored_ShouldThrowError()
    {
        // Arrange
        Book book = null;
        A.CallTo(() => this.bookInventoryRepository.GetBookById("100"))!
            .Returns(book);
        A.CallTo(() => this.bookInventoryRepository.AddBook(A<Book>._)).Returns(0);
            
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await this.sut.AddBook(new Book() { Name = "History" }));
        A.CallTo(() => this.bookInventoryRepository.AddBook(A<Book>._)).MustHaveHappened(
            1,
            Times.Exactly);
    }
}