using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.VerifiedPermissions;
using Amazon.VerifiedPermissions.Model;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using BookInventory.Authorization.Utility;
using BookInventory.Common;

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
    private const string AWS_REGION = "AWS_REGION";
    private const string POLICY_STORE_ID = "POLICY_STORE_ID";
    private readonly IAmazonVerifiedPermissions verifiedPermissions;
    private readonly ICognitoJwtVerifier jwtVerifier;
    private string? policyStoreId;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <remarks>
    /// The implementation that we
    /// instantiated in <see cref="Startup"/> will be injected here.
    /// 
    /// As an alternative, a dependency could be injected into each 
    /// Lambda function handler via the [FromServices] attribute.
    /// </remarks>
    public Functions(IAmazonVerifiedPermissions verifiedPermissions, ICognitoJwtVerifier jwtVerifier)
    {
        this.verifiedPermissions = verifiedPermissions;
        this.jwtVerifier = jwtVerifier;
        policyStoreId = Environment.GetEnvironmentVariable(POLICY_STORE_ID);
    }

    [LambdaFunction()]
    [Logging(LogEvent = true, CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
    [Metrics(CaptureColdStart = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    public async Task<APIGatewayCustomAuthorizerResponse> BookInventoryAuthorizer(APIGatewayCustomAuthorizerRequest request)
    {
        string token = request.Headers["Authorization"];
        string cognitoUserName = String.Empty;
        try
        {
            // Extract userName from the claim (AVP does validate token as well, if explicit token validation is needed use method  this.ValidateAndGetUserName
            cognitoUserName = jwtVerifier.GetUsernameFromToken(token);
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error occured in Lambda Custom Authorization - Invalid token");
            return ApiGatewayResponseBuilder.UnauthorizedResponse(e.Message);
        }
        try
        {
            // AVP - Validate token and policy
            string appNamespace = "BookInventoryApi";
            string resourceType = $"{appNamespace}::Application";
            string resourceId = appNamespace;
            string actionType = $"{appNamespace}::Action";
            var actionId = $"{request.RequestContext.HttpMethod.ToLower()} {request.RequestContext.ResourcePath}";

            // Create IsAuthorizedWithToken request
            var authRequest = new IsAuthorizedWithTokenRequest
            {
                PolicyStoreId = policyStoreId,
                AccessToken = token,
                Action = new ActionIdentifier
                {
                    ActionType = actionType,
                    ActionId = actionId
                },
                Resource = new EntityIdentifier
                {
                    EntityType = resourceType,
                    EntityId = resourceId
                },
                Context = new ContextDefinition
                {
                    ContextMap = GetContextMap(request)
                }
            };
            Logger.LogInformation($"Authorization Request for action: {actionId}, resource: {resourceId}, policy store: {policyStoreId}, context: {JsonSerializer.Serialize(authRequest.Context)}");
            
            // Call Verified Permissions
            var authResponse = await verifiedPermissions.IsAuthorizedWithTokenAsync(authRequest);

            Logger.LogInformation($"Authorization decision for user {cognitoUserName}: {authResponse.Decision} for action {actionId}");

            if (authResponse.Decision == Decision.ALLOW)
            {
                return ApiGatewayResponseBuilder.AuthorizedResponse(authResponse.Principal.EntityId, request.MethodArn);
            }

            return ApiGatewayResponseBuilder.UnauthorizedResponse("User not authorized to access this resource");
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error occured in Lambda Custom Authorization");
            return ApiGatewayResponseBuilder.UnauthorizedResponse(e.Message);
        }
    }
    
    private Dictionary<string, AttributeValue>? GetContextMap(APIGatewayCustomAuthorizerRequest request)
    {
        var hasPathParameters = request.PathParameters?.Any() == true;
        var hasQueryParameters = request.QueryStringParameters?.Any() == true;

        if (!hasPathParameters && !hasQueryParameters)
        {
            return null;
        }

        var contextMap = new Dictionary<string, AttributeValue>();

        if (hasPathParameters)
        {
            var pathParamsRecord = new Dictionary<string, AttributeValue>();
            foreach (var param in request.PathParameters!)
            {
                pathParamsRecord[param.Key] = new AttributeValue 
                { 
                    String = param.Value 
                };
            }

            contextMap["pathParameters"] = new AttributeValue
            {
                Record = new Dictionary<string, AttributeValue>
                {
                    ["record"] = new AttributeValue { Record = pathParamsRecord }
                }
            };
        }

        if (hasQueryParameters)
        {
            var queryParamsRecord = new Dictionary<string, AttributeValue>();
            foreach (var param in request.QueryStringParameters!)
            {
                queryParamsRecord[param.Key] = new AttributeValue 
                { 
                    String = param.Value 
                };
            }

            contextMap["queryStringParameters"] = new AttributeValue
            {
                Record = new Dictionary<string, AttributeValue>
                {
                    ["record"] = new AttributeValue { Record = queryParamsRecord }
                }
            };
        }

        return new Dictionary<string, AttributeValue>
        {
            ["contextMap"] = new AttributeValue { Record = contextMap }
        };
    }

    /// <summary>
    /// Use this method to validate the token explicitly
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>Indicator to check if the token is valid and userName if exists for valid token</returns>
    private async Task<(bool isValidToken, string userName)> ValidateAndGetUserName(string token)
    {
        try
        {
            string? userPoolId = Environment.GetEnvironmentVariable(COGNITO_USER_POOL_ID);
            string? clientId = Environment.GetEnvironmentVariable(COGNITO_USER_POOL_CLIENT_ID);
            string? region = Environment.GetEnvironmentVariable(AWS_REGION);

            var claimPrincipal = await jwtVerifier.ValidateTokenAsync(token, userPoolId, clientId, region);

            // 2. either claimPrincipal is received (not null) or an exception is thrown in case of invalid token
            if (claimPrincipal is null)
            {
                Logger.LogError("Error occured in Lambda Custom Authorization - Invalid token");
                return (false, string.Empty);
            }

            var userName = claimPrincipal.Claims.FirstOrDefault(c =>
                c.Type == "preferred_username" ||
                c.Type == "email" ||
                c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                Logger.LogError("Username not found in token claims");
                return (false, string.Empty);
            }

            return (true, userName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating token");
            return (false, string.Empty);
        }
    }
}