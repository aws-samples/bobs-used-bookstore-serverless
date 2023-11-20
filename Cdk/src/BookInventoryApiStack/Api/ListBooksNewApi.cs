namespace BookInventoryApiStack.Api;

using Amazon.CDK.AWS.Lambda;

using Constructs;

using SharedConstructs;

public class ListBooksNewApi : Construct
{
    public Function Function { get; }

    public ListBooksNewApi(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            $"ListBooksNewApi",
            new LambdaFunctionProps("./src/BookInventoryApi/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_GetBooksNew_Generated::GetBooksNew",
                Environment = new Dictionary<string, string>(1)
                {
                    { "POWERTOOLS_SERVICE_NAME", "Books" },
                },
                IsNativeAot = false //dotnet 6 runtime
            }).Function;
    }
}