using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using BookInventory.Common;
using BookInventory.Models;
using BookInventory.Service;
using System.Net;

namespace BookInventory.Api;

public class Functions
{
    private readonly IBookInventoryService bookInventoryService;

    public Functions(IBookInventoryService bookInventoryService)
    {
        this.bookInventoryService = bookInventoryService;
    }

    [LambdaFunction()]
    [Tracing]
    [RestApi(LambdaHttpMethod.Get, "/books/{id}")]
    public async Task<APIGatewayProxyResponse> GetBook(string id)
    {
        Logger.LogInformation($"Book search for id {id}");

        if (string.IsNullOrWhiteSpace(id))
        {
            return ApiGatewayResponseBuilder.Build(
                HttpStatusCode.BadRequest,
                "Id cannot be null");
        }

        var book = await this.bookInventoryService.GetBookById(id);
        return ApiGatewayResponseBuilder.Build(HttpStatusCode.OK, book);
    }

    [LambdaFunction()]
    [RestApi(LambdaHttpMethod.Post, "/books")]
    public async Task<APIGatewayProxyResponse> AddBook([FromBody] CreateBookDto createBookDto)
    {
        if (createBookDto is null)
        {
            return ApiGatewayResponseBuilder.Build(
                HttpStatusCode.BadRequest,
                "Book cannot be null");
        }

        await this.bookInventoryService.AddBookAsync(createBookDto);
        return ApiGatewayResponseBuilder.Build(HttpStatusCode.OK);
    }
}