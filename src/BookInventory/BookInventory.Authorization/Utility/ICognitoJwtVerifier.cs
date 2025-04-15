using System.Security.Claims;

namespace BookInventory.Authorization.Utility;

public interface ICognitoJwtVerifier
{
    public Task<ClaimsPrincipal> ValidateTokenAsync(string jwtToken, string userPoolId, string clientId, string region);
}