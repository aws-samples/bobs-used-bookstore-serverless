namespace BookInventory.Api.UnitTests;

using System.Net;
using System.Text.Json;

using BookInventory.Common;
using BookInventory.Models;
using BookInventory.Service;

using FakeItEasy;

using FluentAssertions;

using Xunit;

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
    public async Task Search_WhenRequestIsValid_ShouldRespondSearchResult()
    {
        // Arrange
        A.CallTo(() => this.bookInventoryServiceFake.GetBookById("100"))
            .Returns(new Book() { Id = "100", Name = "History" });

        // Act
        var response = await this.sut.Search("100");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Body.Should().NotBeNull();
        var apiWrapperResponse = JsonSerializer.Deserialize<ApiWrapper<Book>>(response.Body);
        apiWrapperResponse.Should().NotBeNull();
        apiWrapperResponse!.Data.Should().NotBeNull();
        apiWrapperResponse.Data.Name.Should().Be("History");
        A.CallTo(() => this.bookInventoryServiceFake.GetBookById("100")).MustHaveHappened(
            1,
            Times.Exactly);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Search_WhenRequestIsInvalid_ShouldRespondBadRequest(string id)
    {
        // Act
        var response = await this.sut.Search(id);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        A.CallTo(() => this.bookInventoryServiceFake.GetBookById(A.Dummy<string>())).MustNotHaveHappened();
    }
    
    [Fact]
    public async Task Search_WhenRequestFailedInternally_ShouldRespondInteranlServerError()
    {
        // Arrange
        A.CallTo(() => this.bookInventoryServiceFake.GetBookById("100")).Throws<Exception>();

        // Act
        var response = await this.sut.Search("100");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        A.CallTo(() => this.bookInventoryServiceFake.GetBookById("100")).MustHaveHappened(
            1,
            Times.Exactly);
    }
}