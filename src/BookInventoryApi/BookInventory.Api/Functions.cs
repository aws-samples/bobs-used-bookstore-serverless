using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using BookInventory.Api.Extensions;
using BookInventory.Common;
using BookInventory.Models;
using BookInventory.Service;
using FluentValidation;
using System.Net;
namespace BookInventory.Api;

public class Functions
{
    private readonly IBookInventoryService bookInventoryService;
    private readonly IValidator<CreateBookDto> bookValidator;

    public Functions(IBookInventoryService bookInventoryService, IValidator<CreateBookDto> bookValidator)
    {
        this.bookInventoryService = bookInventoryService;
        this.bookValidator = bookValidator;
    }

    [LambdaFunction()]
    [Tracing]
    [RestApi(LambdaHttpMethod.Get, "/books")]
    public async Task<APIGatewayProxyResponse> GetBooks([FromQuery] int pageSize = 10, [FromQuery] string cursor = null)
    {
        var books = await this.bookInventoryService.ListAllBooks(pageSize, cursor);
        return ApiGatewayResponseBuilder.Build(HttpStatusCode.OK, books);
    }

    [LambdaFunction()]
    [Tracing]
    [RestApi(LambdaHttpMethod.Get, "/books/{id}")]
    public async Task<APIGatewayProxyResponse> GetBook(string id)
    {
        Logger.LogInformation($"Book search for id {id}");

        if (string.IsNullOrWhiteSpace(id))
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.BadRequest, "Id cannot be null");
        }

        var book = await this.bookInventoryService.GetBookById(id);

        if (book == null)
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.NotFound, $"Book not found for id {id}");
        }

        return ApiGatewayResponseBuilder.Build(HttpStatusCode.OK, book);
    }

    [LambdaFunction()]
    [RestApi(LambdaHttpMethod.Post, "/books")]
    public async Task<APIGatewayProxyResponse> AddBook([FromBody] CreateBookDto createBookDto)
    {
        var validationResult = bookValidator.Validate(createBookDto);
        if (!validationResult.IsValid)
        {
            return ApiGatewayResponseBuilder.Build(
                HttpStatusCode.BadRequest,
                validationResult.GetErrorMessage());
        }

        var bookId = await this.bookInventoryService.AddBookAsync(createBookDto);
        return ApiGatewayResponseBuilder.Build(HttpStatusCode.Created, bookId);
    }

    [LambdaFunction()]
    [RestApi(LambdaHttpMethod.Get, "/api-docs")]
    public APIGatewayProxyResponse GetApiDocs()
    {
        string spec = GetInlineApiSpecs(Environment.GetEnvironmentVariable("BookInventoryApiUrl")!);
        string html = File.ReadAllText("./ApiSpecs/swaggerui.html");
       
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>(1)
            {
                { "Content-Type", "text/html" },
                { "Access-Control-Allow-Origin", "*" }
            },
            Body = html.Replace("#spec", spec)
        };
    }

    private string GetInlineApiSpecs(string apiUrl)
    {
        string spec = File.ReadAllText("./ApiSpecs/openapi.json");
        return spec.Replace("BookInventoryApiUrl", apiUrl);
    }
}