namespace BookInventoryApiStack.Api;

using Amazon.CDK.AWS.Lambda;

using Constructs;

using SharedConstructs;

public class UpdateBooksApi : Construct
{
    public Function Function { get; }

    public UpdateBooksApi(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            $"UpdateBooksApi",
            new LambdaFunctionProps("./src/BookInventoryApi/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_UpdateBook_Generated::UpdateBook",
                Environment = new Dictionary<string, string>(3)
                {
                    { "POWERTOOLS_SERVICE_NAME", "BookInventory" },
                    { "POWERTOOLS_METRICS_NAMESPACE", "BookInventoryMetrics"},
                    { "POWERTOOLS_LOGGER_LOG_EVENT", "true"}
                },
                IsNativeAot = false //dotnet 6 runtime
            }).Function;
    }
}