using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.S3;
using Amazon.S3.Model;
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
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AWS.Lambda.Powertools.BatchProcessing;
using AWS.Lambda.Powertools.BatchProcessing.Sqs;
using BookInventory.Api.Utility;
using Metrics = AWS.Lambda.Powertools.Metrics.Metrics;

namespace BookInventory.Api;

public class Functions
{
    private readonly IBookInventoryService bookInventoryService;
    private readonly IValidator<CreateBookDto> createBookValidator;
    private readonly IValidator<UpdateBookDto> updateBookValidator;
    private readonly IAmazonS3 s3Client;
    private readonly string bucketName;
    private readonly double expiryDuration = 5;//minutes

    public Functions(IBookInventoryService bookInventoryService, IValidator<CreateBookDto> createBookValidator, IValidator<UpdateBookDto> updateBookValidator, IAmazonS3 s3Client)
    {
        this.bookInventoryService = bookInventoryService;
        this.createBookValidator = createBookValidator;
        this.updateBookValidator = updateBookValidator;
        this.s3Client = s3Client;
        this.bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME")!;
        this.expiryDuration = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("EXPIRY_DURATION"))? 0: double.Parse(Environment.GetEnvironmentVariable("EXPIRY_DURATION")!);
    }

    [LambdaFunction]
    [RestApi(LambdaHttpMethod.Get, "/books")]
    [Tracing(CaptureMode = TracingCaptureMode.Error)]
    [Logging(ClearState = true)]
    public async Task<APIGatewayProxyResponse> ListBooks([FromQuery] int pageSize = 10, [FromQuery] string cursor = null)
    {
        cursor.AddObservabilityTag("ListBooks");
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
    [Logging(ClearState = true, LogEvent = true)]
    public async Task<APIGatewayProxyResponse> GetBook(string id)
    {
        Logger.LogInformation($"Book search for id {id}");
        if (string.IsNullOrWhiteSpace(id))
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.BadRequest, "Id cannot be null");
        }
        id.AddObservabilityTag("BookId");
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
    [Logging(ClearState = true, LogEvent = true)]
    [Metrics(Namespace = "BookInventory")]
    public async Task<APIGatewayProxyResponse> AddBook([FromBody] CreateBookDto bookDto)
    {
        var validationResult = createBookValidator.Validate(bookDto);
        if (!validationResult.IsValid)
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.BadRequest, validationResult.GetErrorMessage());
        }
        bookDto.Name.AddObservabilityTag("CreateBook");
        var bookId = await this.bookInventoryService.AddBookAsync(bookDto);
        Metrics.AddMetric("BookCreated", 1, MetricUnit.Count);
        return ApiGatewayResponseBuilder.Build(HttpStatusCode.Created, bookId);
    }

    [LambdaFunction]
    [RestApi(LambdaHttpMethod.Put, "/books/{id}")]
    [Tracing(CaptureMode = TracingCaptureMode.Error)]
    [Logging(ClearState = true, LogEvent = true)]
    public async Task<APIGatewayProxyResponse> UpdateBook(string id, [FromBody] UpdateBookDto bookDto)
    {
        var validationResult = updateBookValidator.Validate(bookDto);
        if (!validationResult.IsValid)
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.BadRequest, validationResult.GetErrorMessage());
        }
        id.AddObservabilityTag("UpdateBook");
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

    [LambdaFunction()]
    [RestApi(LambdaHttpMethod.Get, "/books/cover-page-upload/{fileName}")]
    [Tracing]
    [Logging(LogEvent = true)]
    public async Task<APIGatewayProxyResponse> GetCoverPageUpload(string fileName)
    {
        if (!fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.BadRequest, "Only .jpg file is allowed");
        }

        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = fileName,
            Verb = HttpVerb.PUT,
            ContentType = "image/jpeg",
            Expires = DateTime.UtcNow.AddMinutes(expiryDuration)
        };

        var preSignedUrl = await this.s3Client.GetPreSignedURLAsync(request);
        return ApiGatewayResponseBuilder.Build(HttpStatusCode.Created, preSignedUrl);
    }
    
    /// <summary>
    /// Image validation
    /// </summary>
    /// <param name="evnt">SQS Event</param>
    /// <param name="context">Lambda context</param>
    /// <returns>Image Validation Response</returns>
    [LambdaFunction()]
    [Logging(LogEvent = true, CorrelationIdPath = CorrelationIdPaths.EventBridge)]
    [Metrics(CaptureColdStart = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    [BatchProcessor(RecordHandler = typeof(BatchProcessorHandler), BatchParallelProcessingEnabled = true, MaxDegreeOfParallelism = -1)]
    public BatchItemFailuresResponse ImageValidation(SQSEvent evnt, ILambdaContext context)
    {
        return SqsBatchProcessor.Result.BatchItemFailuresResponse;
    }
}