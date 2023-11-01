namespace BookInventoryApiStack.Api;

using Amazon.CDK.AWS.Lambda;

using Constructs;

using SharedConstructs;

public class AddBooksApi : Construct
{
    public Function Function { get; }

    public AddBooksApi(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            $"AddBooksApi",
            new LambdaFunctionProps("./src/BookInventoryApi/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_AddBook_Generated::AddBook",
                Environment = new Dictionary<string, string>(1)
                {
                    { "POWERTOOLS_SERVICE_NAME", "AddBooksApi" },
                },
                IsNativeAot = false //dotnet 6 runtime
            }).Function;
    }
}