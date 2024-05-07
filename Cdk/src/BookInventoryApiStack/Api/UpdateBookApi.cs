namespace BookInventoryApiStack.Api;

using Amazon.CDK.AWS.Lambda;

using Constructs;

using SharedConstructs;

public class UpdateBookApi : Construct
{
    public Function Function { get; }

    public UpdateBookApi(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            Constants.UPDATE_BOOK_API,
            new LambdaFunctionProps("./src/BookInventory/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_UpdateBook_Generated::UpdateBook",
                Environment = new Dictionary<string, string>(3)
                {
                    { "POWERTOOLS_SERVICE_NAME", Constants.UPDATE_BOOK_API },
                    { "POWERTOOLS_METRICS_NAMESPACE", Constants.UPDATE_BOOK_API },
                    { "POWERTOOLS_LOGGER_LOG_EVENT", "true" }
                },
                IsNativeAot = false
            }).Function;
    }
}