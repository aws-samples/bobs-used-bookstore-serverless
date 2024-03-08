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

    [LambdaFunction]
    [RestApi(LambdaHttpMethod.Get, "/books")]
    [Tracing(CaptureMode = TracingCaptureMode.Error)]
    [Logging(ClearState = true)]
    public async Task<APIGatewayProxyResponse> ListBooks([FromQuery] int pageSize = 10, [FromQuery] string cursor = null)
    {
        Tracing.AddAnnotation("ListBooks",cursor);
        Logger.AppendKey("ListBooks", cursor);
        try
        {
            var response = await this.bookInventoryService.ListAllBooksAsync(
                pageSize,
                cursor);
            return ApiGatewayResponseBuilder.Build(
                HttpStatusCode.OK,
                response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,$"Error occured while searching books for criteria {cursor}");
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.InternalServerError, $"Error occured while searching books for criteria {cursor}");
        }
    }

    [LambdaFunction]
    [RestApi(LambdaHttpMethod.Get, "/books/{id}")]
    [Tracing(CaptureMode = TracingCaptureMode.Error)]
    [Logging(ClearState = true)]
    public async Task<APIGatewayProxyResponse> GetBook(string id)
    {
        Logger.AppendKey("BookId", id);
        Tracing.AddAnnotation("BookId", id);
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

    [LambdaFunction]
    [RestApi(LambdaHttpMethod.Post, "/books")]
    [Tracing(CaptureMode = TracingCaptureMode.Error)]
    [Logging(ClearState = true)]
    [Metrics(Namespace = "BookInventory")]
    public async Task<APIGatewayProxyResponse> AddBook([FromBody] CreateBookDto bookDto)
    {
        Tracing.AddAnnotation("CreateBook",bookDto.Name);
        Logger.AppendKey("CreateBook", bookDto.Name);
        var validationResult = createBookValidator.Validate(bookDto);
        if (!validationResult.IsValid)
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.BadRequest, validationResult.GetErrorMessage());
        }

        var bookId = await this.bookInventoryService.AddBookAsync(bookDto);
        Metrics.AddMetric("BookCreated", 1, MetricUnit.Count);
        return ApiGatewayResponseBuilder.Build(HttpStatusCode.Created, bookId);
    }

    [LambdaFunction]
    [RestApi(LambdaHttpMethod.Put, "/books/{id}")]
    [Tracing(CaptureMode = TracingCaptureMode.Error)]
    [Logging(ClearState = true)]
    public async Task<APIGatewayProxyResponse> UpdateBook(string id, [FromBody] UpdateBookDto bookDto)
    {
        Tracing.AddAnnotation("UpdateBook",id);
        Logger.AppendKey("UpdateBook", id);
        
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