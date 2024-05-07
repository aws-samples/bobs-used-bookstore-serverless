namespace BookInventoryApiStack.Api;

using Amazon.CDK.AWS.Lambda;

using Constructs;

using SharedConstructs;

public class AddBookApi : Construct
{
    public Function Function { get; }

    public AddBookApi(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            Constants.ADD_BOOK_API,
            new LambdaFunctionProps("./src/BookInventory/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_AddBook_Generated::AddBook",
                Environment = new Dictionary<string, string>
                {
                    { "POWERTOOLS_SERVICE_NAME", Constants.ADD_BOOK_API },
                    { "POWERTOOLS_METRICS_NAMESPACE", Constants.ADD_BOOK_API },
                    { "POWERTOOLS_LOGGER_LOG_EVENT", "true" }
                },
                IsNativeAot = false
            }).Function;
    }
}