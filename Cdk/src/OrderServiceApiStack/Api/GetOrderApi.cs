namespace OrderServiceApiStack.Api;

using Amazon.CDK.AWS.Lambda;

using Constructs;

using SharedConstructs;

public class GetOrderApi : Construct
{
    public Function Function { get; }

    public GetOrderApi(Construct scope, string id, OrderServiceApiStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            $"GetOrdersApi",
            new LambdaFunctionProps(".src/OrderServiceApi/OrderService.Api/bin/Release/net8.0/OrderService.Api")
            {
                Handler = "OrderService.Api",
                Environment = new Dictionary<string, string>(1)
                {
                    { "POWERTOOLS_SERVICE_NAME", "Orders" },
                    { "ANNOTATIONS_HANDLER", "GetOrder" }
                },
                IsNativeAot = true
            }).Function;
    }
}