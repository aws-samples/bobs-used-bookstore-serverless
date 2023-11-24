namespace SharedConstructs;

using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Lambda;

using Constructs;

using HttpMethod = System.Net.Http.HttpMethod;

public class Api : RestApi
{
    public CognitoUserPoolsAuthorizer Authorizer { get; private set; }
    
    public Api(
        Construct scope,
        string id,
        RestApiProps props) : base(
        scope,
        id,
        props)
    {
        
    }

    public Api WithCognito(IUserPool cognitoUserPool)
    {
        this.Authorizer = new CognitoUserPoolsAuthorizer(
            this,
            "CognitoAuthorizer",
            new CognitoUserPoolsAuthorizerProps
            {
                CognitoUserPools = new IUserPool[]
                {
                    cognitoUserPool
                },
                AuthorizerName = "cognitoauthorizer",
                IdentitySource = "method.request.header.Authorization"
            });

        return this;
    }
    
    public Api WithEndpoint(string path, HttpMethod method, Function function, bool authorizeApi = true)
    {
        IResource? lastResource = null;

        foreach (var pathSegment in path.Split('/'))
        {
            var sanitisedPathSegment = pathSegment.Replace(
                "/",
                "");

            if (string.IsNullOrEmpty(sanitisedPathSegment))
            {
                continue;
            }

            if (lastResource == null)
            {
                lastResource = this.Root.GetResource(sanitisedPathSegment) ?? this.Root.AddResource(sanitisedPathSegment);
                continue;
            }

            lastResource = lastResource.GetResource(sanitisedPathSegment) ??
                           lastResource.AddResource(sanitisedPathSegment);
        }

        lastResource?.AddMethod(
            method.ToString().ToUpper(),
            new LambdaIntegration(function),
            new MethodOptions
            {
                MethodResponses = new IMethodResponse[]
                {
                    new MethodResponse { StatusCode = "200" },
                    new MethodResponse { StatusCode = "400" },
                    new MethodResponse { StatusCode = "500" }
                },
                AuthorizationType = authorizeApi ? AuthorizationType.COGNITO : AuthorizationType.NONE,
                Authorizer = authorizeApi ? this.Authorizer : null
            });

        return this;
    }
}