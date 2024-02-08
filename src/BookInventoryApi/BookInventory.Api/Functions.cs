using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using BookInventory.Api.Extensions;
using BookInventory.Common;
using BookInventory.Models;
using BookInventory.Service;
using BookInventory.Service.Exceptions;
using FluentValidation;
using System.Net;

namespace BookInventory.Api;

public class Functions
{
    private readonly IBookInventoryService bookInventoryService;
    private readonly IValidator<CreateBookDto> createBookValidator;
    private readonly IValidator<UpdateBookDto> updateBookValidator;

    public Functions(IBookInventoryService bookInventoryService, IValidator<CreateBookDto> createBookValidator, IValidator<UpdateBookDto> updateBookValidator)
    {
        this.bookInventoryService = bookInventoryService;
        this.createBookValidator = createBookValidator;
        this.updateBookValidator = updateBookValidator;
    }

    [LambdaFunction()]
    [RestApi(LambdaHttpMethod.Get, "/books")]
    [Tracing]
    [Logging]
    public async Task<APIGatewayProxyResponse> GetBooks([FromQuery] int pageSize = 10, [FromQuery] string cursor = null)
    {
        var response = await this.bookInventoryService.ListAllBooksAsync(pageSize, cursor);
        return ApiGatewayResponseBuilder.Build(HttpStatusCode.OK, response);
    }

    [LambdaFunction()]
    [RestApi(LambdaHttpMethod.Get, "/books/{id}")]
    [Tracing]
    [Logging]
    public async Task<APIGatewayProxyResponse> GetBook(string id)
    {
        id.AddObservabilityTag("BookId");
        Logger.LogInformation($"Book search for id {id}");
        if (string.IsNullOrWhiteSpace(id))
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.BadRequest, "Id cannot be null");
        }

        var book = await this.bookInventoryService.GetBookByIdAsync(id);

        if (book == null)
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.NotFound, $"Book not found for id {id}");
        }

        return ApiGatewayResponseBuilder.Build(HttpStatusCode.OK, book);
    }

    [LambdaFunction()]
    [RestApi(LambdaHttpMethod.Post, "/books")]
    [Tracing]
    [Logging]
    [Metrics]
    public async Task<APIGatewayProxyResponse> AddBook([FromBody] CreateBookDto bookDto)
    {
        var validationResult = createBookValidator.Validate(bookDto);
        if (!validationResult.IsValid)
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.BadRequest, validationResult.GetErrorMessage());
        }

        var bookId = await this.bookInventoryService.AddBookAsync(bookDto);
        Metrics.AddMetric("Book_Created", 1, MetricUnit.Count);
        bookId.AddObservabilityTag("BookId");
        return ApiGatewayResponseBuilder.Build(HttpStatusCode.Created, bookId);
    }

    [LambdaFunction()]
    [RestApi(LambdaHttpMethod.Put, "/books/{id}")]
    [Tracing]
    [Logging]
    [Metrics]
    public async Task<APIGatewayProxyResponse> UpdateBook(string id, [FromBody] UpdateBookDto bookDto)
    {
        var validationResult = updateBookValidator.Validate(bookDto);
        if (!validationResult.IsValid)
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.BadRequest, validationResult.GetErrorMessage());
        }

        try
        {
            await this.bookInventoryService.UpdateBookAsync(id, bookDto);
        }
        catch (ProductNotFoundException ex)
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.NotFound, ex.Message);
        }
        return ApiGatewayResponseBuilder.Build(HttpStatusCode.NoContent);
    }
}