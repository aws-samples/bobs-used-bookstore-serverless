using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;


namespace BookInventory.Api;

using System.Net;

using Amazon.Lambda.APIGatewayEvents;

using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;

using BookInventory.Common;
using BookInventory.Models;
using BookInventory.Models.Request;
using BookInventory.Service;

/// <summary>
/// A collection of sample Lambda functions that provide a REST api for doing simple math calculations. 
/// </summary>
public class Functions
{
    private readonly IBookInventoryService bookInventoryService;
    
    /// <summary>
    /// Default constructor.
    /// </summary>
    public Functions(IBookInventoryService bookInventoryService)
    {
        this.bookInventoryService = bookInventoryService;
    }

    /// <summary>
    /// Search books
    /// </summary>
    /// <param name="id">Book id</param>
    /// <returns>Sum of x and y.</returns>
    [LambdaFunction()]
    [Tracing]
    [RestApi(LambdaHttpMethod.Get,"/search/{id}")]
    public async Task<APIGatewayProxyResponse> Search(string id)
    {
        Logger.LogInformation($"Book search for id {id}");
        
        if (string.IsNullOrWhiteSpace(id))
        {
            return ApiGatewayResponseBuilder.Build(
                HttpStatusCode.BadRequest,
                "Id cannot be null");
        }

        try
        {
            var book = await this.bookInventoryService.GetBookById(id);
            return ApiGatewayResponseBuilder.Build(
                HttpStatusCode.OK,
                book);
        }
        catch (Exception ex)
        {
            return ApiGatewayResponseBuilder.Build(
                HttpStatusCode.InternalServerError,
                ex.Message);
        }
    }
    
    /// <summary>
    /// Add book
    /// </summary>
    /// <param name="newBook">Book to be added</param>
    /// <returns>Id of the new book</returns>
    [LambdaFunction()]
    [RestApi(LambdaHttpMethod.Post,"/add")]
    public async Task<APIGatewayProxyResponse> AddBook([FromBody] NewBookRequest newBook)
    {
        if (newBook is null)
        {
            return ApiGatewayResponseBuilder.Build(
                HttpStatusCode.BadRequest,
                "Id cannot be null");
        }

        try
        {
            var bookId = await this.bookInventoryService.AddBook(new Book() { Name = newBook.Name });
            return ApiGatewayResponseBuilder.Build(
                HttpStatusCode.OK,
                bookId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }    
}