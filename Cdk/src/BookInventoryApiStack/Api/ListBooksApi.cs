namespace BookInventoryApiStack.Api;

using Amazon.CDK.AWS.Lambda;

using Constructs;

using SharedConstructs;

public class ListBooksApi : Construct
{
    public Function Function { get; }

    public ListBooksApi(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            $"ListBooksApi",
            new LambdaFunctionProps("./src/BookInventoryApi/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_GetBooks_Generated::GetBooks",
                Environment = new Dictionary<string, string>(2)
                {
                    { "POWERTOOLS_SERVICE_NAME", Constants.SERVICE_NAME },
                    { "POWERTOOLS_METRICS_NAMESPACE", Constants.METRICS_NAMESPACE},
                },
                IsNativeAot = false //dotnet 8 runtime
            }).Function;
    }
}