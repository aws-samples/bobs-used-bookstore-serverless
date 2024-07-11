using Amazon.Lambda.APIGatewayEvents;

namespace SharedConstructs;

public static class ApiUtility
{
    public static APIGatewayCustomAuthorizerResponse AuthorizedResponse(string principalId, string resource)
    {
        return new APIGatewayCustomAuthorizerResponse
        {
            PrincipalID = principalId,
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy()
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>()
                {
                    new()
                    {
                        Action = ["execute-api:Invoke"],
                        Effect = "Allow",
                        Resource = [resource]
                    }
                }
            }
        };
    }
    
    public static APIGatewayCustomAuthorizerResponse UnauthorizedResponse(string message) 
    {
        return new APIGatewayCustomAuthorizerResponse
        {
            PrincipalID = $"unauthorized - {message}",
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy()
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>()
                {
                    new()
                    {
                        Action = ["*"],
                        Effect = "Deny",
                        Resource = ["*"]
                    }
                }
            }
        };
    }
}