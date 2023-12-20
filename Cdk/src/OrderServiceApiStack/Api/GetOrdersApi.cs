namespace OrderServiceApiStack.Api;

using Amazon.CDK.AWS.Lambda;

using Constructs;

using SharedConstructs;

public class GetOrdersApi : Construct
{
    public Function Function { get; }

    public GetOrdersApi(Construct scope, string id, OrderServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            $"GetOrdersApi",
            new LambdaFunctionProps(".src/OrderServiceApi/OrderService.Api/bin/Release/net8.0/OrderService.Api.zip")
            {
                Environment = new Dictionary<string, string>(1)
                {
                    { "POWERTOOLS_SERVICE_NAME", "Orders" },
                    { "ANNOTATIONS_HANDLER", "GetOrders" }
                },
                IsNativeAot = true //dotnet 8 runtime
            }).Function;
    }
}