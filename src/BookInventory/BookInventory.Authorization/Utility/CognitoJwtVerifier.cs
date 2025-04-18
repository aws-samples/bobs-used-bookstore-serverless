using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace BookInventory.Authorization.Utility;

public class CognitoJwtVerifier : ICognitoJwtVerifier
{
    /// <summary>
    /// Validate jwt token and extract claims
    /// </summary>
    /// <param name="jwtToken">Token to validate </param>
    /// <param name="userPoolId">Cognito User Pool</param>
    /// <param name="clientId">Cognito client id</param>
    /// <param name="region">Region</param>
    /// <returns>Validated token claims</returns>
    /// <exception cref="Exception">Token is empty</exception>
    public async Task<ClaimsPrincipal> ValidateTokenAsync(string jwtToken, string userPoolId, string clientId, string region)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            throw new Exception("Missing identity bearer token");
        }

        jwtToken = jwtToken.Replace("Bearer", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

        var issuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
        var metadataEndpoint = $"{issuer}/.well-known/openid-configuration";

        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataEndpoint, new OpenIdConnectConfigurationRetriever(), new HttpClient());
        var discoveryDocument = await configurationManager.GetConfigurationAsync();
        var signingKeys = discoveryDocument.SigningKeys;

        var tokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = true,
            AudienceValidator = (audiences, securityToken, validationParameters) =>
            {
                var jwtToken = securityToken as JwtSecurityToken;
                var _clientId = jwtToken?.Claims.FirstOrDefault(x => x.Type == "client_id")?.Value;
                return _clientId == clientId;
            },
            ValidateIssuer = true,
            ValidIssuer = issuer,
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = signingKeys,
            RoleClaimType = "cognito:groups",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
        SecurityToken validatedToken = new JwtSecurityToken();

        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out validatedToken);
    }

    /// <summary>
    /// Get User info from the token, no token validation
    /// </summary>
    /// <param name="jwtToken">Jwt token</param>
    /// <returns>User Name</returns>
    /// <exception cref="Exception">Invalid token</exception>
    public string GetUsernameFromToken(string jwtToken)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            throw new Exception("Missing identity bearer token");
        }

        jwtToken = jwtToken.Replace("Bearer", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(jwtToken))
        {
            throw new Exception("Invalid JWT token format");
        }
        var token = tokenHandler.ReadJwtToken(jwtToken);
        string userName = token.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ??
                          token.Claims.FirstOrDefault(t => t.Type == "username")?.Value ??
                          token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(userName))
        {
            throw new Exception("Username not found in the token");
        }

        return userName;
    }
}
