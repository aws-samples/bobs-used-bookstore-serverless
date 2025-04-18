using System.Security.Claims;

namespace BookInventory.Authorization.Utility;

public interface ICognitoJwtVerifier
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
    public Task<ClaimsPrincipal> ValidateTokenAsync(string jwtToken, string userPoolId, string clientId, string region);

    /// <summary>
    /// Get User info from the token, no token validation
    /// </summary>
    /// <param name="jwtToken">Jwt token</param>
    /// <returns>User Name</returns>
    /// <exception cref="Exception">Invalid token</exception>
    public string GetUsernameFromToken(string jwtToken);
}