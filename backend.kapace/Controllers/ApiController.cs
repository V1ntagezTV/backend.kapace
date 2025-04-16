using System.Security.Claims;
using backend.kapace.BLL.Exceptions;
using backend.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace backend.kapace.Controllers;

public abstract class ApiController : Controller
{
    protected void ValidateTokenScopeOrThrow(ErrorCode errorCode)
    {
        var scope = 
            User.FindFirst("token_scope")?.Value 
            ?? throw new ServiceException(errorCode);
        
        if (scope != Constants.ForbiddenAuthorizeClaimScope)
        {
            throw new ServiceException(errorCode);
        }
    }

    protected void ValidateTokenIpAddress(ErrorCode errorCode)
    {
        var tokenIp = User.FindFirst(ClaimTypes.WindowsDeviceClaim)?.Value
                      ?? throw new ServiceException(errorCode);
        var currentIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        if (tokenIp != currentIp)
        {
            // Запрещаем выполнение запроса, если IP не совпадает
            throw new ServiceException(errorCode);
        }
    }

    protected void ValidateTokenType(TokenTypes expected, ErrorCode errorCode)
    {
        var isMailUpdateTokenType = User.FindFirst(Constants.ClaimTypeTokenType)?.Value
                                    ?? throw new ServiceException(errorCode);
        
        if (Enum.TryParse<TokenTypes>(isMailUpdateTokenType, out var value) && value != expected)
        {
            throw new ServiceException(errorCode);
        }
    }
}