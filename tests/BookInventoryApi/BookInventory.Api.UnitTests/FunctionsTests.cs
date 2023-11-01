using BookInventory.Common;
using BookInventory.Models;
using BookInventory.Service;
using FakeItEasy;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace BookInventory.Api.UnitTests;

public class FunctionsTests
{
    private readonly IBookInventoryService bookInventoryServiceFake;
    private readonly Functions sut;

    public FunctionsTests()
    {
        this.bookInventoryServiceFake = A.Fake<IBookInventoryService>();
        this.sut = new Functions(this.bookInventoryServiceFake);
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
        A.CallTo(() => this.bookInventoryServiceFake.GetBookById("8274dcb1-e651-41b4-98c6-d416e8b59fab")).MustHaveHappened(1, Times.Exactly);
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
}