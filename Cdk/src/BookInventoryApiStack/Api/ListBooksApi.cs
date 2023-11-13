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
                Environment = new Dictionary<string, string>(1)
                {
                    { "POWERTOOLS_SERVICE_NAME", "Books" },
                },
                IsNativeAot = false //dotnet 6 runtime
            }).Function;
    }
}