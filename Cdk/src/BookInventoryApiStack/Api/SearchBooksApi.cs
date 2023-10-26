namespace BookInventoryApiStack.Api;

using Amazon.CDK.AWS.Lambda;

using Constructs;

using SharedConstructs;

public class SearchBooksApi : Construct
{
    public Function Function { get; }

    public SearchBooksApi(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            $"SearchBooksApi{props.Postfix}",
            new LambdaFunctionProps("./src/BookInventoryApi/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_Search_Generated::Search",
                Environment = new Dictionary<string, string>(1)
                {
                    { "ENV", props.Postfix },
                    { "POWERTOOLS_SERVICE_NAME", $"SearchBooksApi{props.Postfix}" },
                },
                IsNativeAot = false //dotnet 6 runtime
            }).Function;
    }
}