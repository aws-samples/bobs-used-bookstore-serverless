using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using BookInventoryApiStack.Api;
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
        var bookInventory = new Table(this, "BookInventoryTable", new TableProps
        {
            TableName = "BookInventory",
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute { Name = "PK", Type = AttributeType.STRING },
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute { Name = "SK", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST
        });
        bookInventory.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps()
        {
            IndexName = "GSI1",
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute()
            {
                Name = "GSI1PK",
                Type = AttributeType.STRING
            },
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute()
            {
                Name = "GSI1SK",
                Type = AttributeType.STRING
            }, 
        });

        //Lambda Functions
        var bookInventoryServiceStackProps = new BookInventoryServiceStackProps();
        
        var getBooksApi = new GetBooksApi(
            this,
            "GetBooksEndpoint",
            bookInventoryServiceStackProps);
        
        var addBooksApi = new AddBooksApi(
            this,
            "AddBooksEndpoint",
            bookInventoryServiceStackProps);
        
        var listBooks = new ListBooksApi(
            this,
            "ListBooksEndpoint",
            bookInventoryServiceStackProps);

        var listBooksNew = new ListBooksNewApi(
            this,
            "ListBooksNewEndpoint",
            bookInventoryServiceStackProps);

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
                false)
            .WithEndpoint(
                "/books",
                HttpMethod.Get,
                listBooks.Function,
                false)
            .WithEndpoint(
                "/booksNew",
                HttpMethod.Get,
                listBooksNew.Function,
                false);

        //Grant DynamoDB Permission
        bookInventory.GrantReadData(getBooksApi.Function.Role!);
        bookInventory.GrantReadData(listBooks.Function.Role!);
        bookInventory.GrantReadData(listBooksNew.Function.Role!);
        bookInventory.GrantWriteData(addBooksApi.Function.Role!);

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