using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using BookInventoryApiStack.Api;
using BookInventoryApiStack.Database;
using Construct = Constructs.Construct;

namespace BookInventoryApiStack;

public record BookInventoryServiceStackProps();

public class BookInventoryServiceStack : Stack
{
    internal BookInventoryServiceStack(
        Construct scope,
        string id,
        BookInventoryServiceStackProps apiProps,
        IStackProps props = null) : base(
        scope,
        id,
        props)
    {
        //Database
        var bookInventoryDB = new BookInventoryDB(
            this, "BookInventoryTable", new BookInventoryServiceStackProps());

        //Lambda Functions
        var searchBooksApi = new GetBooksApi(
            this,
            "SearchBooksEndpoint",
            new BookInventoryServiceStackProps());
        var addBooksApi = new AddBooksApi(
            this,
            "AddBooksEndpoint",
            new BookInventoryServiceStackProps());

        //Api
        var api = new SharedConstructs.Api(
            this,
            "BookInventoryApi",
            new RestApiProps { RestApiName = "BookInventoryApi" })
            .WithEndpoint(
                "/books/{id}",
                HttpMethod.Get,
                searchBooksApi.Function,
                false)
            .WithEndpoint(
                "/books",
                HttpMethod.Post,
                addBooksApi.Function,
                false);

        //Grant DynamoDB Permission
        bookInventoryDB.Table.GrantReadData(searchBooksApi.Function.Role);
        bookInventoryDB.Table.GrantReadWriteData(addBooksApi.Function.Role);

        var apiEndpointOutput = new CfnOutput(
            this,
            $"APIEndpointOutput",
            new CfnOutputProps
            {
                Value = api.Url,
                ExportName = $"ApiEndpoint",
                Description = "Endpoint of the Book Inventory API"
            });
    }
}