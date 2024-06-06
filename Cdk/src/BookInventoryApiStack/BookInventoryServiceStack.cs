using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SSM;
using BookInventoryApiStack.Api;
using Authorizer = BookInventoryApiStack.Api.Authorizer;
using Construct = Constructs.Construct;


namespace BookInventoryApiStack;

public sealed class BookInventoryServiceStack : Stack
{
    internal BookInventoryServiceStack(
        Construct scope,
        string id,
        BookInventoryServiceStackProps apiProps,
        IStackProps? props = null) : base(
        scope,
        id,
        props)
    {
        string servicePrefix = "BookInventoryService";

        // S3 bucket
        var bookInventoryBucket = new Bucket(this, $"BookInventoryBucket{apiProps.PostFix}", new BucketProps
        {
            BucketName = $"{this.Account}-{servicePrefix.ToLower()}-coverpage-images{apiProps.PostFix}",
            Versioned = true
        });
        
        //Database
        var bookInventory = new Table(this, $"{servicePrefix}-BookInventoryTable{apiProps.PostFix}", new TableProps
        {
            TableName = $"BookInventory{apiProps.PostFix}",
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
        
        // Image Validation Stack
        var imageValidationConstruct = new ImageValidationConstruct(this, $"ImageValidationConstruct{apiProps.PostFix}",
            new ImageValidationConstructProps(this.Account, servicePrefix, apiProps.PostFix, bookInventoryBucket, bookInventory));
        
        // Retrieve user pool info from ssm
        var userPoolParameterValue =
            StringParameter.ValueForStringParameter(this, $"/bookstore/authentication/user-pool-id{apiProps.PostFix}");

        var userPool = UserPool.FromUserPoolArn(this, $"{servicePrefix}-UserPool{apiProps.PostFix}", userPoolParameterValue);
        
        _ = new CfnOutput(
            this,
            $"{servicePrefix}-User-Pool-Id{apiProps.PostFix}",
            new CfnOutputProps
            {
                Value = userPool.UserPoolId,
                ExportName = $"{servicePrefix}-UserPool{apiProps.PostFix}",
                Description = "UserPool"
            });
        var userPoolClientParameterValue =
            StringParameter.ValueForStringParameter(this, $"/bookstore/authentication/user-pool-client-id{apiProps.PostFix}");
        
        _ = new CfnOutput(
            this,
            $"{servicePrefix}-User-Pool-Client-Id{apiProps.PostFix}",
            new CfnOutputProps
            {
                Value = userPoolClientParameterValue,
                ExportName = $"{servicePrefix}-UserPool-Client{apiProps.PostFix}",
                Description = "UserPoolClientId"
            });

        var bookInventoryServiceStackProps = new BookInventoryServiceStackProps(apiProps.PostFix)
        {
            BucketName = bookInventoryBucket.BucketName,
            UserPoolId = userPool.UserPoolId,
            UserPoolClientId = userPoolClientParameterValue,
            Table = bookInventory.TableName
        };
        
        //Lambda Functions
        var getBookApi = new GetBookApi(
            this,
            $"GetBookEndpoint{apiProps.PostFix}",
            bookInventoryServiceStackProps);

        var addBooksApi = new AddBookApi(
            this,
            $"AddBooksEndpoint{apiProps.PostFix}",
            bookInventoryServiceStackProps);

        var listBooks = new ListBooksApi(
            this,
            $"ListBooksEndpoint{apiProps.PostFix}",
            bookInventoryServiceStackProps);

        var updateBooksApi = new UpdateBookApi(
            this,
            $"UpdateBooksEndpoint{apiProps.PostFix}",
            bookInventoryServiceStackProps);

        var getCoverPageUploadApi = new GetCoverPageUploadApi(
            this,
            $"GeneratePreSignedURLEndpoint{apiProps.PostFix}",
            bookInventoryServiceStackProps);

        var authorizer = new Authorizer(this, $"BookInventoryAuthorizer{apiProps.PostFix}", bookInventoryServiceStackProps);

        //Api

        var api = new SharedConstructs.Api(
                this,
                $"BookInventoryApi{apiProps.PostFix}",
                new RestApiProps
                {
                    RestApiName = $"BookInventoryApi{apiProps.PostFix}",
                    DeployOptions = new StageOptions
                    {
                        AccessLogDestination = new LogGroupLogDestination(new LogGroup(this, $"BookInventoryLogGroup{apiProps.PostFix}")),
                        AccessLogFormat = AccessLogFormat.JsonWithStandardFields(),
                        TracingEnabled = true,
                        LoggingLevel = MethodLoggingLevel.INFO
                    }
                })
            .WithCognito(authorizer.Function)
            .WithEndpoint(
                "/books/{id}",
                HttpMethod.Get,
                getBookApi.Function,
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
        bookInventory.GrantReadData(getBookApi.Function.Role!);
        bookInventory.GrantReadData(listBooks.Function.Role!);
        bookInventory.GrantWriteData(addBooksApi.Function.Role!);
        bookInventory.GrantReadWriteData(updateBooksApi.Function.Role!);
        bookInventoryBucket.GrantPut(getCoverPageUploadApi.Function.Role!);
        

        _ = new CfnOutput(
            this,
            $"{servicePrefix}-APIEndpointOutput{apiProps.PostFix}",
            new CfnOutputProps
            {
                Value = api.Url,
                ExportName = $"{servicePrefix}-ApiEndpoint{apiProps.PostFix}",
                Description = "Endpoint of the Book Inventory API"
            });
    }
}