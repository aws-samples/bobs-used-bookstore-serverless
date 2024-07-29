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
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using BookInventory.Api.Utility;
using SharedConstructs;
using Metrics = AWS.Lambda.Powertools.Metrics.Metrics;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace BookInventory.Api;

[JsonSerializable(typeof(CreateBookDto))]
[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSerializable(typeof(ApiWrapper<string>))]
[JsonSerializable(typeof(ApiWrapper<BookDto>))]
[JsonSerializable(typeof(ApiWrapper<List<BookDto>>))]
[JsonSerializable(typeof(ApiWrapper<BookQueryResponse>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(SQSEvent))]
[JsonSerializable(typeof(APIGatewayCustomAuthorizerRequest))]
[JsonSerializable(typeof(APIGatewayCustomAuthorizerResponse))]
public class Functions
{
    private readonly IBookInventoryService bookInventoryService;
    private readonly IValidator<CreateBookDto> createBookValidator;
    private readonly IValidator<UpdateBookDto> updateBookValidator;
    private readonly IAmazonS3 s3Client;
    private readonly string bucketName;
    private readonly double expiryDuration = 5;//minutes
    private readonly Dictionary<string, List<string>> apiAuthMapping;
    private const string REGION = "REGION";
    private const string COGNITO_USER_POOL_ID = "COGNITO_USER_POOL_ID";
    private const string COGNITO_USER_POOL_CLIENT_ID = "COGNITO_USER_POOL_CLIENT_ID";

    public Functions(IBookInventoryService bookInventoryService, IValidator<CreateBookDto> createBookValidator, IValidator<UpdateBookDto> updateBookValidator, IAmazonS3 s3Client)
    {
        this.bookInventoryService = bookInventoryService;
        this.createBookValidator = createBookValidator;
        this.updateBookValidator = updateBookValidator;
        this.s3Client = s3Client;
        this.bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME")!;
        this.expiryDuration = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("EXPIRY_DURATION"))? 0: double.Parse(Environment.GetEnvironmentVariable("EXPIRY_DURATION")!);
        this.apiAuthMapping = new Dictionary<string, List<string>>()
        {
            {@"^.*?/POST/books$", new List<string> {"Customer"}}, // Add book
            {@"^.*?/PUT/books/([a-zA-Z0-9\-]+)$", new List<string> {"Customer","Admin"}}, // Update Book
            {@"^.*?/GET/books/([a-zA-Z0-9\-]+)/?.*$", new List<string> {"Customer"}} // Upload Image 
        };
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
    [RestApi(LambdaHttpMethod.Get, "/books/{id}/{fileName}")]
    [Tracing]
    [Logging(LogEvent = true)]
    public async Task<APIGatewayProxyResponse> GetCoverPageUpload(string id, string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLower();
        if (!(extension == ".png" || extension == ".jpg"))
        {
            return ApiGatewayResponseBuilder.Build(HttpStatusCode.BadRequest, "Only .jpg and .png file is allowed");
        }

        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = $"{id}/{fileName}",
            Verb = HttpVerb.PUT,
            ContentType = "image/jpeg",
            Expires = DateTime.UtcNow.AddMinutes(expiryDuration)
        };

        var preSignedUrl = await this.s3Client.GetPreSignedURLAsync(request);
        return ApiGatewayResponseBuilder.Build(HttpStatusCode.Created, preSignedUrl);
    }

    [LambdaFunction()]
    [Logging(LogEvent = true, CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
    [Metrics(CaptureColdStart = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    public async Task<APIGatewayCustomAuthorizerResponse> BookInventoryAuthorizer(APIGatewayCustomAuthorizerRequest request)
    {
        string token = request.AuthorizationToken;
        string? userPoolId = Environment.GetEnvironmentVariable(COGNITO_USER_POOL_ID);
        string? clientId = Environment.GetEnvironmentVariable(COGNITO_USER_POOL_CLIENT_ID);
        string? region = Environment.GetEnvironmentVariable(REGION);
        string method = request.MethodArn;
        try
        {
            // 1. Retrieve claim
            var claimPrincipal = await new CognitoJwtVerifier(userPoolId, clientId, region).ValidateTokenAsync(token);
            // 2. either claimPrincipal is received (not null) or an exception is thrown in case of invalid token
            if (claimPrincipal is null)
            {
                return ApiUtility.UnauthorizedResponse("Unable to retrieve the claim");
            }
            // Get cognito user name
            string cogntioUserId = claimPrincipal.Claims.First(t => t.Type == "cognito:username").Value;
            
            // Get groups from token
            var groups = claimPrincipal.Claims.Where(t => t.Type == "cognito:groups").Select(x=>x.Value).ToList();
            Logger.LogInformation($"User Logged in {cogntioUserId} groups {string.Join(",",groups)}");

            // Get matching apis from mapping
            var apiMapping = this.apiAuthMapping.Where(x => Regex.IsMatch(method,x.Key)).ToList();
            // Expected user groups to access the api
            var requiredGroups = apiMapping.Any()? apiMapping.First().Value : new List<string>(); // Every Api has only one entry in the dictionary. Get all matching roles
            Logger.LogInformation($"User groups allowed for the api {apiMapping.FirstOrDefault().Key} are {string.Join(",",requiredGroups)}");
            if (groups.Any(x => requiredGroups.Any(y => y.Equals(x, StringComparison.OrdinalIgnoreCase))))
            {
                return ApiUtility.AuthorizedResponse(cogntioUserId, request.MethodArn);
            }

            string unauthorizedMessage =
                $"User has groups {string.Join(",", groups)}, not meeting api rules";
            Logger.LogInformation($"User {cogntioUserId} not allowed to access api {apiMapping.FirstOrDefault().Key} - {unauthorizedMessage}");
            return ApiUtility.UnauthorizedResponse(unauthorizedMessage);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error occured in Lambda Custom Authorization");
            return ApiUtility.UnauthorizedResponse(e.Message);
        }
    }
}