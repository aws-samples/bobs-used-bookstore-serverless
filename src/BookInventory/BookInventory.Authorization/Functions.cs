using System.Security.Claims;
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
using Microsoft.AspNetCore.WebUtilities;

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
    private readonly IAmazonVerifiedPermissions _verifiedPermissions;
    private readonly ICognitoJwtVerifier _jwtVerifier;

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
        _verifiedPermissions = verifiedPermissions;
        _jwtVerifier = jwtVerifier;
    }

    [LambdaFunction()]
    [Logging(LogEvent = true, CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
    [Metrics(CaptureColdStart = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    public async Task<APIGatewayCustomAuthorizerResponse> BookInventoryAuthorizer(APIGatewayCustomAuthorizerRequest request)
    {
        string token = request.Headers["Authorization"];
        string? userPoolId = Environment.GetEnvironmentVariable(COGNITO_USER_POOL_ID);
        string? clientId = Environment.GetEnvironmentVariable(COGNITO_USER_POOL_CLIENT_ID);
        string? region = Environment.GetEnvironmentVariable(AWS_REGION);
        string? policyStoreId = Environment.GetEnvironmentVariable(POLICY_STORE_ID);
        ClaimsPrincipal claimPrincipal;
        try
        {
            // Validate Token (AVP does validate token as well, so this step can be skipped 
            // 1. Retrieve claim
            claimPrincipal = await _jwtVerifier.ValidateTokenAsync(token, userPoolId, clientId, region);

            // 2. either claimPrincipal is received (not null) or an exception is thrown in case of invalid token
            if (claimPrincipal is null)
            {
                Logger.LogError($"Error occured in Lambda Custom Authorization - Invalid token - {JsonSerializer.Serialize(request)}");
                return ApiGatewayResponseBuilder.UnauthorizedResponse("Unable to retrieve the claim");
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error occured in Lambda Custom Authorization - Invalid token - {JsonSerializer.Serialize(request)}");
            return ApiGatewayResponseBuilder.UnauthorizedResponse(e.Message);
        }
        try
        {
            // AVP - Validate token and policy
            // Get cognito user name
            string cognitoUserId = claimPrincipal.Claims.First(t => t.Type == "username").Value;
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
            Logger.LogInformation($"authRequest: {JsonSerializer.Serialize(authRequest)}");
            
            // Call Verified Permissions
            var authResponse = await _verifiedPermissions.IsAuthorizedWithTokenAsync(authRequest);

            Logger.LogInformation($"Authorization decision for user {cognitoUserId}: {authResponse.Decision} for action {actionId}");

            if (authResponse.Decision == Decision.ALLOW)
            {
                return ApiGatewayResponseBuilder.AuthorizedResponse(cognitoUserId, request.MethodArn);
            }

            return ApiGatewayResponseBuilder.UnauthorizedResponse("User not authorized to access this resource");
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error occured in Lambda Custom Authorization - {JsonSerializer.Serialize(request)}");
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
}