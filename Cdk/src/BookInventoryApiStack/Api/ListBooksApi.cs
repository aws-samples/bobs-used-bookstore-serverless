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
            $"{Constants.LIST_BOOK_API}{props.PostFix}",
            new LambdaFunctionProps("./src/BookInventory/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_ListBooks_Generated::ListBooks",
                Environment = new Dictionary<string, string>
                {
                    { "POWERTOOLS_SERVICE_NAME", Constants.LIST_BOOK_API },
                    { "POWERTOOLS_METRICS_NAMESPACE", Constants.LIST_BOOK_API },
                    { "POWERTOOLS_LOGGER_LOG_EVENT", "true" },
                    { "IS_POSTFIX", string.IsNullOrWhiteSpace(props.PostFix)?"false":"true" },
                    { "TABLE_NAME", props.Table }
                }
            }).Function;
    }
}