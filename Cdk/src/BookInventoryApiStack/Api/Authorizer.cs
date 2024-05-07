using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Constructs;
using SharedConstructs;

namespace BookInventoryApiStack.Api;

public class Authorizer : Construct
{
    public Function Function { get; }

    public Authorizer(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            $"{Constants.AUTHORIZER}",
            new LambdaFunctionProps("./src/BookInventory/BookInventory.Api")
            {
                
                Handler = "BookInventory.Api::BookInventory.Api.Functions_BookInventoryAuthorizer_Generated::BookInventoryAuthorizer",
                Environment = new Dictionary<string, string>
                {
                    { "POWERTOOLS_SERVICE_NAME", Constants.AUTHORIZER },
                    { "POWERTOOLS_METRICS_NAMESPACE", Constants.AUTHORIZER },
                    { "POWERTOOLS_LOGGER_LOG_EVENT", "true" },
                    { "REGION",  Stack.Of(this).Region},
                    { "COGNITO_USER_POOL_ID", props.UserPoolId },
                    { "COGNITO_USER_POOL_CLIENT_ID", props.UserPoolClientId}
                },
                IsNativeAot = false
            }).Function;
    }
}