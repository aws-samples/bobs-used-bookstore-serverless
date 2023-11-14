using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using BookInventoryApiStack.Api;
using Construct = Constructs.Construct;

namespace BookInventoryApiStack;

using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SSM;

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
                new RestApiProps { RestApiName = "BookInventoryApi", DeployOptions = new StageOptions {
                    AccessLogDestination = new LogGroupLogDestination(new LogGroup(this, "BookInventoryLogGroup")),
                    AccessLogFormat = AccessLogFormat.JsonWithStandardFields(),
                    TracingEnabled = true,
                    LoggingLevel = MethodLoggingLevel.ERROR
                }})
            .WithCognito(userPool)
            .WithEndpoint(
                "/books/{id}",
                HttpMethod.Get,
                getBooksApi.Function,
                false)
            .WithEndpoint(
                "/books",
                HttpMethod.Post,
                addBooksApi.Function);

        //Grant DynamoDB Permission
        bookInventory.GrantReadData(getBooksApi.Function.Role!);
        bookInventory.GrantReadWriteData(addBooksApi.Function.Role!);

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