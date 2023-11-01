namespace BookInventoryApiStack.Api;

using Amazon.CDK.AWS.Lambda;

using Constructs;

using SharedConstructs;

public class GetBooksApi : Construct
{
    public Function Function { get; }

    public GetBooksApi(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            $"GetBooksApi",
            new LambdaFunctionProps("./src/BookInventoryApi/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_GetBook_Generated::GetBook",
                Environment = new Dictionary<string, string>(1)
                {
                    { "POWERTOOLS_SERVICE_NAME", "GetBooksApi" },
                },
                IsNativeAot = false //dotnet 6 runtime
            }).Function;
    }
}