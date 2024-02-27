using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using BookInventoryApiStack.Api;
using Construct = Constructs.Construct;

namespace BookInventoryApiStack;

using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SSM;

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
        // S3 bucket
        var bookInventoryBucket = new Bucket(this, "BookInventoryBucket", new BucketProps
        {
            BucketName = $"{this.Account}-book-inventory-bucket"//TODO: we can add pre/post fix name to create multiple S3 bucket in same account
        });

        //Database
        var bookInventory = new Table(this, "BookInventoryTable", new TableProps
        {
            TableName = "BookInventory",
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute { Name = "BookId", Type = AttributeType.STRING },
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
        
        // Retrieve user pool info from ssm
        var userPoolParameterValue =
            StringParameter.ValueForStringParameter(this, $"/bookstore/authentication/user-pool-id");

        var userPool = UserPool.FromUserPoolArn(this, "UserPool", userPoolParameterValue);
        
        new CfnOutput(
            this,
            $"User Pool Id",
            new CfnOutputProps
            {
                Value = userPool.UserPoolId,
                ExportName = "UserPool",
                Description = "UserPool"
            });

        //Lambda Functions
        var bookInventoryServiceStackProps = new BookInventoryServiceStackProps
        {
            BucketName = bookInventoryBucket.BucketName
        };
        
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

        var updateBooksApi = new UpdateBooksApi(
            this,
            "UpdateBooksEndpoint",
            bookInventoryServiceStackProps);

        var getCoverPageUploadApi = new GetCoverPageUploadApi(
            this,
            "GeneratePreSignedURLEndpoint",
            bookInventoryServiceStackProps);

        //Api

        var api = new SharedConstructs.Api(
                this,
                "BookInventoryApi",
                new RestApiProps
                {
                    RestApiName = "BookInventoryApi",
                    DeployOptions = new StageOptions
                    {
                        AccessLogDestination = new LogGroupLogDestination(new LogGroup(this, "BookInventoryLogGroup")),
                        AccessLogFormat = AccessLogFormat.JsonWithStandardFields(),
                        TracingEnabled = true,
                        LoggingLevel = MethodLoggingLevel.ERROR
                    }
                })
            .WithCognito(userPool)
            .WithEndpoint(
                "/books/{id}",
                HttpMethod.Get,
                getBooksApi.Function,
                false)
            .WithEndpoint(
                "/books",
                HttpMethod.Post,
                addBooksApi.Function)
            .WithEndpoint(
                "/books",
                HttpMethod.Get,
                listBooks.Function,
                false)
            .WithEndpoint(
                "/books/{id}",
                HttpMethod.Put,
                updateBooksApi.Function,
                false)
            .WithEndpoint(
                "books/cover-page-upload-url/{fileName}",
                HttpMethod.Get,
                getCoverPageUploadApi.Function);

        //Grant DynamoDB Permission
        bookInventory.GrantReadData(getBooksApi.Function.Role!);
        bookInventory.GrantReadData(listBooks.Function.Role!);
        bookInventory.GrantWriteData(addBooksApi.Function.Role!);
        bookInventory.GrantReadWriteData(updateBooksApi.Function.Role!);
        bookInventoryBucket.GrantPut(getCoverPageUploadApi.Function.Role!);

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