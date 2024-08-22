using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using BookInventory.Api.Utility;
using SharedConstructs;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BookInventory.Authorization;

/// <summary>
/// A collection of sample Lambda functions that provide a REST api for doing simple math calculations. 
/// </summary>
[JsonSerializable(typeof(APIGatewayCustomAuthorizerRequest))]
[JsonSerializable(typeof(APIGatewayCustomAuthorizerResponse))]
public class Functions
{
    private const string COGNITO_USER_POOL_ID = "COGNITO_USER_POOL_ID";
    private const string COGNITO_USER_POOL_CLIENT_ID = "COGNITO_USER_POOL_CLIENT_ID";
    private const string REGION = "REGION";

    private readonly Dictionary<string, List<string>> apiAuthMapping;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <remarks>
    /// The <see cref="ICalculatorService"/> implementation that we
    /// instantiated in <see cref="Startup"/> will be injected here.
    /// 
    /// As an alternative, a dependency could be injected into each 
    /// Lambda function handler via the [FromServices] attribute.
    /// </remarks>
    public Functions()
    {
        apiAuthMapping = new Dictionary<string, List<string>>
        {
            {@"^.*?/POST/books$", new List<string> {"Customer"}}, // Add book
            {@"^.*?/PUT/books/([a-zA-Z0-9\-]+)$", new List<string> {"Customer","Admin"}}, // Update Book
            {@"^.*?/GET/books/([a-zA-Z0-9\-]+)/?.*$", new List<string> {"Customer"}} // Upload Image 
        };
    }
    
    [LambdaFunction()]
    [Logging(LogEvent = true, CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
    [Metrics(CaptureColdStart = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    public async Task<APIGatewayCustomAuthorizerResponse> BookInventoryAuthorizer(APIGatewayCustomAuthorizerRequest request)
    {
        string token = request.AuthorizationToken;
        string? userPoolId = Environment.GetEnvironmentVariable(COGNITO_USER_POOL_ID);
        string? clientId = Environment.GetEnvironmentVariable(COGNITO_USER_POOL_CLIENT_ID);
        string? region = Environment.GetEnvironmentVariable(REGION);
        string method = request.MethodArn;
        try
        {
            // 1. Retrieve claim
            var claimPrincipal = await new CognitoJwtVerifier(userPoolId, clientId, region).ValidateTokenAsync(token);
            // 2. either claimPrincipal is received (not null) or an exception is thrown in case of invalid token
            if (claimPrincipal is null)
            {
                return ApiUtility.UnauthorizedResponse("Unable to retrieve the claim");
            }
            // Get cognito user name
            string cognitoUserId = claimPrincipal.Claims.First(t => t.Type == "cognito:username").Value;
            
            // Get groups from token
            var groups = claimPrincipal.Claims.Where(t => t.Type == "cognito:groups").Select(x=>x.Value).ToList();
            Logger.LogInformation($"User Logged in {cognitoUserId} groups {string.Join(",",groups)}");

            // Get matching apis from mapping
            var apiMapping = this.apiAuthMapping.Where(x => Regex.IsMatch(method,x.Key)).ToList();
            // Expected user groups to access the api
            var requiredGroups = apiMapping.Any()? apiMapping.First().Value : new List<string>(); // Every Api has only one entry in the dictionary. Get all matching roles
            Logger.LogInformation($"User groups allowed for the api {apiMapping.FirstOrDefault().Key} are {string.Join(",",requiredGroups)}");
            if (groups.Any(x => requiredGroups.Any(y => y.Equals(x, StringComparison.OrdinalIgnoreCase))))
            {
                return ApiUtility.AuthorizedResponse(cognitoUserId, request.MethodArn);
            }

            string unauthorizedMessage =
                $"User has groups {string.Join(",", groups)}, not meeting api rules";
            Logger.LogInformation($"User {cognitoUserId} not allowed to access api {apiMapping.FirstOrDefault().Key} - {unauthorizedMessage}");
            return ApiUtility.UnauthorizedResponse(unauthorizedMessage);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error occured in Lambda Custom Authorization");
            return ApiUtility.UnauthorizedResponse(e.Message);
        }
    }

}