namespace BookInventoryApiStack.Api;

using Amazon.CDK.AWS.Lambda;

using Constructs;

using SharedConstructs;

public class ApiSpecDocs : Construct
{
    public Function Function { get; }

    public ApiSpecDocs(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,            
            $"GetApiDocs",
            new LambdaFunctionProps("./src/BookInventoryApi/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_GetApiDocs_Generated::GetApiDocs",
                Environment = new Dictionary<string, string>(1)
                {
                    { "POWERTOOLS_SERVICE_NAME", "Books" },
                },                
                IsNativeAot = false //dotnet 6 runtime
            }).Function;
    }
}