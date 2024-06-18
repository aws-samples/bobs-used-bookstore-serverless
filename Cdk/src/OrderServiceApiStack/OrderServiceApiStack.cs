namespace OrderServiceApiStack;

using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SSM;

using Constructs;

using global::OrderServiceApiStack.Api;

public record OrderServiceApiStackProps(string PostFix);

public class OrderServiceApiStack : Stack
{
    internal OrderServiceApiStack(
        Construct scope,
        string id,
        OrderServiceApiStackProps apiProps,
        IStackProps props = null) : base(
        scope,
        id,
        props)
    {
        string servicePrefix = "OrderService";
        
        // Retrieve user pool info from ssm
        var userPoolParameterValue =
            StringParameter.ValueForStringParameter(this, $"/bookstore/authentication/user-pool-id");

        var userPool = UserPool.FromUserPoolArn(this, $"{servicePrefix}-UserPool{apiProps.PostFix}", userPoolParameterValue);
        
        new CfnOutput(
            this,
            $"{servicePrefix}-User-Pool-Id{apiProps.PostFix}",
            new CfnOutputProps
            {
                Value = userPool.UserPoolId,
                ExportName = $"{servicePrefix}-UserPool",
                Description = "UserPool"
            });

        //Lambda Functions
        var orderServiceStackProps = new OrderServiceApiStackProps(apiProps.PostFix);
        
        var getOrderApi = new GetOrderApi(
            this,
            $"GetOrderEndpoint{apiProps.PostFix}",
            orderServiceStackProps);
        
        var listOrdersApi = new GetOrdersApi(
            this,
            $"ListOrdersEndpoint{apiProps.PostFix}",
            orderServiceStackProps);

        //Api
        var api = new SharedConstructs.Api(
                this,
                $"OrderServiceApi{apiProps.PostFix}",
                new RestApiProps { RestApiName = $"OrderServiceApi{apiProps.PostFix}", DeployOptions = new StageOptions {
                    AccessLogDestination = new LogGroupLogDestination(new LogGroup(this, $"OrderServiceLogGroup{apiProps.PostFix}")),
                    AccessLogFormat = AccessLogFormat.JsonWithStandardFields(),
                    TracingEnabled = true,
                    LoggingLevel = MethodLoggingLevel.ERROR
                }})
            //.WithCognito(userPool)
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
            $"{servicePrefix}-APIEndpointOutput{apiProps.PostFix}",
            new CfnOutputProps
            {
                Value = api.Url,
                ExportName = $"{servicePrefix}-ApiEndpoint",
                Description = "Endpoint of the Order Service API"
            });
    }
}