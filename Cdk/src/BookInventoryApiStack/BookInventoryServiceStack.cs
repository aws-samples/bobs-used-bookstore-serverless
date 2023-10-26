namespace BookInventoryApiStack;

using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;

using BookInventoryApiStack.Api;

using Construct = Constructs.Construct;

public record BookInventoryServiceStackProps(
    string Postfix);

public class BookInventoryServiceStack : Stack
{
    internal BookInventoryServiceStack(
        Construct scope,
        string id,
        BookInventoryServiceStackProps apiProps,
        IStackProps props = null) : base(
        scope,
        id,
        props)
    {
        var searchBooksApi = new SearchBooksApi(
            this,
            "SearchBooksEndpoint",
            new BookInventoryServiceStackProps(""));

        var api = new SharedConstructs.Api(
            this,
            "BookInventoryApi",
            new RestApiProps { RestApiName = "BookInventoryApi" }).WithEndpoint(
            "/search/{id}",
            HttpMethod.Get,
            searchBooksApi.Function,
            false);
        
        var apiEndpointOutput = new CfnOutput(
            this,
            $"APIEndpointOutput{apiProps.Postfix}",
            new CfnOutputProps
            {
                Value = api.Url,
                ExportName = $"ApiEndpoint{apiProps.Postfix}",
                Description = "Endpoint of the Book Inventory API"
            });
    }
}