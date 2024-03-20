namespace BookInventoryApiStack.Api;

using Amazon.CDK.AWS.Lambda;

using Constructs;

using SharedConstructs;

public class GetBookApi : Construct
{
    public Function Function { get; }

    public GetBookApi(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            Constants.SEARCH_BOOK_API,
            new LambdaFunctionProps("./src/BookInventoryApi/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_GetBook_Generated::GetBook",
                Environment = new Dictionary<string, string>(2)
                {
                    { "POWERTOOLS_SERVICE_NAME", Constants.SEARCH_BOOK_API },
                    { "POWERTOOLS_METRICS_NAMESPACE", Constants.SEARCH_BOOK_API },
                    { "POWERTOOLS_LOGGER_LOG_EVENT", "true" }
                },
                IsNativeAot = false
            }).Function;
    }
}