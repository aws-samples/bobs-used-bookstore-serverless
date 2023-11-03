using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using BookInventoryApiStack.Api;
using SharedConstructs;
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
        var bookInventory = new DynamoDBTable(
            this, "BookInventoryTable", new TableProps()
            {
                TableName = "BookInventory"
            });

        //Lambda Functions
        var getBooksApi = new GetBooksApi(
            this,
            "GetBooksEndpoint",
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
                getBooksApi.Function,
                false)
            .WithEndpoint(
                "/books",
                HttpMethod.Post,
                addBooksApi.Function,
                false);

        //Grant DynamoDB Permission
        bookInventory.Table.GrantReadData(getBooksApi.Function.Role!);
        bookInventory.Table.GrantReadWriteData(addBooksApi.Function.Role!);

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