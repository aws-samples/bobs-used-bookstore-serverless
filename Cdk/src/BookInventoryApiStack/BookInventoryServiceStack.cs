using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Batch;
using Amazon.CDK.AWS.DynamoDB;
using AppStack.Models;
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

        // Endpoint to access API specs on Swagger-UI
        var apiDocs = new ApiSpecDocs(
            this,
            "ApiDocsEndpoint",
            bookInventoryServiceStackProps);

        //Api
        var api = new SharedConstructs.Api(
                this,
                "BookInventoryApi",
                new RestApiProps
                {
                    RestApiName = "BookInventoryApi"
                });

        var endpoints = api.WithEndpoint(
             "/books/{id}",
             HttpMethod.Get,
             getBooksApi.Function,
             false)
         .WithEndpoint(
             "/books",
             HttpMethod.Get,
             listBooks.Function,
             false)
         .WithEndpoint(
             "/api-docs",
             HttpMethod.Get,
             apiDocs.Function,
             false)
         .WithEndpoint(
             "/books",
             HttpMethod.Post,
             addBooksApi.Function,
             false,
             new CreateBookDtoModel(this, api));

        apiDocs.Function.AddEnvironment("BookInventoryApiUrl", $"https://{api.RestApiId}.execute-api.{this.Region}.amazonaws.com");

        //Grant DynamoDB Permission
        bookInventory.GrantReadData(getBooksApi.Function.Role!);
        bookInventory.GrantReadData(listBooks.Function.Role!);
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

        string script = $"-------------------------------\n aws apigateway get-export --parameters extensions='apigateway' --rest-api-id {api.RestApiId} --stage-name prod --export-type oas30 swagger-ui/openapi.json \n-------------------------------";

        var scriptToExportSwagger = new CfnOutput(
           this,
           $"SwaggerExportCommand",
           new CfnOutputProps
           {
               Value = script,
               ExportName = $"Script",
               Description = "Update API specs in API project"
           });
    }
}