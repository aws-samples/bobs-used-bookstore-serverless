namespace OrderServiceApiStack;

using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SSM;

using Constructs;

using OrderServiceApiStack.Api;

public record OrderServiceStackProps();

public class OrderServiceStack : Stack
{
    internal OrderServiceStack(
        Construct scope,
        string id,
        OrderServiceStackProps apiProps,
        IStackProps props = null) : base(
        scope,
        id,
        props)
    {
        //Database
        
        
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
        var orderServiceStackProps = new OrderServiceStackProps();
        
        var getOrderApi = new GetOrderApi(
            this,
            "GetOrderEndpoint",
            orderServiceStackProps);
        
        var listOrdersApi = new GetOrdersApi(
            this,
            "ListOrdersEndpoint",
            orderServiceStackProps);

        //Api
        var api = new SharedConstructs.Api(
                this,
                "OrderServiceApi",
                new RestApiProps { RestApiName = "OrderServiceApi", DeployOptions = new StageOptions {
                    AccessLogDestination = new LogGroupLogDestination(new LogGroup(this, "OrderServiceLogGroup")),
                    AccessLogFormat = AccessLogFormat.JsonWithStandardFields(),
                    TracingEnabled = true,
                    LoggingLevel = MethodLoggingLevel.ERROR
                }})
            .WithCognito(userPool)
            .WithEndpoint(
                "/orders/{id}",
                HttpMethod.Get,
                getOrderApi.Function)
            .WithEndpoint(
                "/orders",
                HttpMethod.Get,
                listOrdersApi.Function);

        //Grant DynamoDB Permission
        

        var apiEndpointOutput = new CfnOutput(
            this,
            $"APIEndpointOutput",
            new CfnOutputProps
            {
                Value = api.Url,
                ExportName = $"ApiEndpoint",
                Description = "Endpoint of the Order Service API"
            });
    }
}