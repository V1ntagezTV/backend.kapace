using System.Security.Claims;

namespace backend.kapace.Middlewares;

public static class ClaimsPrinciapalExtension
{
    public static long GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var claims = claimsPrincipal.Claims.ToArray();
        var isContainForbbidenClaim = claims.Any(x => x.Value == Constants.ForbiddenAuthorizeClaimScope);
        if (isContainForbbidenClaim)
        {
            throw new ArgumentException("Not authorized");
        }
        
        var userIdStr = claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid);
        if (userIdStr?.Value != null && long.TryParse(userIdStr.Value, out var userId))
        {
            return userId;
        }

        throw new ArgumentException("Not authorized");
    }
}