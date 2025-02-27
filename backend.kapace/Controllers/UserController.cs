using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using backend.kapace.BLL.Exceptions;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models.Query;
using backend.kapace.Middlewares;
using backend.kapace.Models.Requests.User;
using backend.kapace.Models.Response;
using backend.Models.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using V1QueryRequest = backend.kapace.Models.Requests.User.V1QueryRequest;
using V1QueryResponse = backend.kapace.Models.Requests.User.V1QueryResponse;

namespace backend.kapace.Controllers;

[ApiController]
[Route("v1/user")]
public class UserController : Controller
{
    private readonly DateTimeOffset _loginDuration = DateTimeOffset.UtcNow.AddDays(1);
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpPost("current")]
    public async Task<ActionResult> V1GetCurrent(CancellationToken token)
    {
        var userId = User.GetUserId();
        var (user, roles) = await _userService.GetCurrent(userId, token);

        var response = new
        {
            User = new
            {
                Id = user.Id,
                Nickname = user.Nickname,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            },
            Roles = roles.Select(r => new
            {
                Id = r.Id,
                Alias = r.Alias,
            }).ToArray()
        };

        return Ok(response);
    }
    
    [HttpPost("test-except")]
    public async Task<ActionResult> Teest(CancellationToken token)
    {
        throw new ServiceException(ErrorCode.MailVerificationError, "Что-то пошло не так")
            .SetData("Да?", "Да!");
    }

    [Authorize(Roles = "admin")]
    [HttpPost("query")]
    public async Task<ActionResult<V1QueryResponse>> V1Query(V1QueryRequest request, CancellationToken token)
    {
        var users = await _userService.Query(new UserQuery
        {
            UserIds = request.UserIds ?? [],
            Nicknames = request.Nicknames ?? [],
            Emails = request.Emails ?? []
        }, token);

        var units = users
            .Select(user => new V1QueryResponse.User(
                user.Id,
                user.Nickname,
                user.Email,
                user.CreatedAt))
            .ToArray();
        
        return Ok(new V1QueryResponse(units));
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(V1RegisterRequest request, CancellationToken token)
    {
        await _userService.Register(
            request.Nickname,
            request.Email,
            request.Password,
            token);

        return Ok();
    }

    [Authorize]
    [HttpPost("update-password")]
    public async Task<ActionResult> UpdatePassword(V1UpdatePasswordRequest request, CancellationToken token)
    {
        var userId = User.GetUserId();

        await _userService.UpdatePassword(userId, request.OldPassword, request.NewPassword, token);

        return Ok();
    }

    [HttpPost("update-nickname")]
    public async Task<ActionResult> UpdateNickname(V1UpdateNicknameRequest request, CancellationToken token)
    {
        var userId = User.GetUserId();

        await _userService.UpdateNickname(userId, request.NewNickname, token);

        return Ok();
    }

    [Authorize]
    [HttpPost("verify-mail")]
    public async Task<ActionResult> VerifyMail(V1VerifyMailRequest request, CancellationToken token)
    {
        var userId = User.GetUserId();
        await _userService.TryVerifyMail(userId, request.VerificationCode, token);

        return Ok();
    }
    
    [Authorize]
    [HttpPost("send-mail-verify-code")]
    public async Task<ActionResult> SendMailVerificationCode(CancellationToken token)
    {
        var userId = User.GetUserId();

        await _userService.InitMailVerification(userId, token);

        return Ok();
    }

    [HttpPost("login-by-bearer")]
    public async Task<ActionResult<V1LogInResponse>> Login(V1LogInRequest request, CancellationToken token)
    {
        var (user, permissions) = await _userService.AuthorizeUser(
            request.Email,
            request.Password,
            token);

        var userRoleClaims = permissions
            .Select(r => new Claim(ClaimTypes.Role, r.Alias))
            .ToArray();

        var userInfoClaims = new List<Claim>
        {
            new(ClaimTypes.Sid, user.Id.ToString()),
        };

        var claims = userInfoClaims.Concat(userRoleClaims);

        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.Issuer,
            audience: AuthOptions.Audience,
            claims: claims,
            expires: _loginDuration.DateTime,
            signingCredentials: new SigningCredentials(
                AuthOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256)
        );

        var bearerToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        return Ok(new V1LogInResponse(bearerToken, user.Id));
    }

    [HttpPost("login-by-cookie")]
    public async Task<ActionResult> LogInByCookie(V1LogInRequest request, CancellationToken token)
    {
        var (user, permissions) = await _userService.AuthorizeUser(
            request.Email,
            request.Password,
            token);

        var userRoleClaims = permissions
            .Select(r => new Claim(ClaimTypes.Role, r.Alias))
            .ToArray();

        var userInfoClaims = new List<Claim> { new(ClaimTypes.Sid, user.Id.ToString()) };
        var claims = userInfoClaims.Concat(userRoleClaims);
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            // Кука будет "persistent", т.е. сохраняется после закрытия браузера
            IsPersistent = request.IsRememberMe ?? false,
            // Устанавливаем срок действия куки
            ExpiresUtc = _loginDuration,
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Ok();
    }
    
    [HttpPost("cookie-logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }
    
    [HttpPost("send-password-reset-code")]
    public async Task<ActionResult> SendPasswordResetCode(V1SendPasswordResetCodeRequest request, CancellationToken token)
    {
        await _userService.SendPasswordResetCode(request.Email, token);

        return Ok();
    }

    [HttpPost("verify-password-reset-code")]
    public async Task<IActionResult> VerifyPasswordReset(V1VerifyPasswordResetRequest request, CancellationToken token)
    {
        var user = await _userService.VerifyPasswordResetCode(request.Email, request.Code, token);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Sid, user.Id.ToString()),
            new(ClaimTypes.WindowsDeviceClaim, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "no_ip"),
            new("scope", Constants.ForbiddenAuthorizeClaimScope)
        };
        
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.Issuer,
            audience: AuthOptions.Audience,
            claims: claims,
            expires: _loginDuration.DateTime,
            signingCredentials: new SigningCredentials(
                AuthOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256)
        );

        var bearerToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        
        return Ok(new { Token = bearerToken });
    }
    
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(
        V1ResetPasswordRequest request,
        CancellationToken token)
    {
        var userIdStr =
            User.FindFirst(ClaimTypes.Sid)?.Value
            ?? throw new ServiceException(ErrorCode.PasswordResetForbidden);
        
        if (!long.TryParse(userIdStr, out var userId))
        {
            throw new ServiceException(ErrorCode.PasswordResetForbidden);
        }
        
        var tokenIp = User.FindFirst(ClaimTypes.WindowsDeviceClaim)?.Value;
        var currentIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (tokenIp != currentIp)
        {
            // Запрещаем выполнение запроса, если IP не совпадает
            throw new ServiceException(ErrorCode.PasswordResetForbidden);
        }
        
        var scope =
            User.FindFirst("ip_address")?.Value
            ?? throw new ServiceException(ErrorCode.PasswordResetForbidden);
        if (scope != Constants.ForbiddenAuthorizeClaimScope)
        {
            throw new ServiceException(ErrorCode.PasswordResetForbidden);
        }

        await _userService.ResetPassword(userId, request.Email, request.NewPassword, token);

        return Ok();
    }
}