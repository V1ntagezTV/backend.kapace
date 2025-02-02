using System.Security.Claims;

namespace backend.kapace.Middlewares;

public static class ClaimsPrinciapalExtension
{
    public static long GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var userIdStr = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid);
        if (userIdStr?.Value != null && long.TryParse(userIdStr.Value, out var userId))
        {
            return userId;
        }

        throw new ArgumentException("Not authorized");
    }
}