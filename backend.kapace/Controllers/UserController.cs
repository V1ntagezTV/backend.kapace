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
public class UserController(IUserService userService) : ApiController
{
    private readonly DateTimeOffset _loginDuration = DateTimeOffset.UtcNow.AddDays(1);
    private readonly TimeSpan _temporaryTokensExpiration = TimeSpan.FromMinutes(5);

    [Authorize]
    [HttpPost("current")]
    public async Task<ActionResult> V1GetCurrent(CancellationToken token)
    {
        var userId = User.GetUserId();
        var (user, roles) = await userService.GetCurrent(userId, token);

        var response = new
        {
            User = new
            {
                Id = user.Id,
                Nickname = user.Nickname,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                IsMailVerified = user.IsMailVerified,
                ImageUrl = user.ImageUrl,
            },
            Roles = roles.Select(r => new
            {
                Id = r.Id,
                Alias = r.Alias,
            }).ToArray()
        };

        return Ok(response);
    }

    [Authorize(Roles = "admin")]
    [HttpPost("query")]
    public async Task<ActionResult<V1QueryResponse>> V1Query(V1QueryRequest request, CancellationToken token)
    {
        var users = await userService.Query(new UserQuery
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
        await userService.Register(
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
        
        await userService.UpdatePassword(userId, request.OldPassword, request.NewPassword, token);

        return Ok();
    }

    [Authorize]
    [HttpPost("update-nickname")]
    public async Task<ActionResult> UpdateNickname(V1UpdateNicknameRequest request, CancellationToken token)
    {
        var userId = User.GetUserId();

        await userService.UpdateNickname(userId, request.NewNickname, request.IsForce, token);

        return Ok();
    }

    [Authorize]
    [HttpPost("verify-mail")]
    public async Task<ActionResult> VerifyMail(V1VerifyMailRequest request, CancellationToken token)
    {
        var userId = User.GetUserId();
        await userService.TryVerifyMail(userId, request.VerificationCode, token);
        return Ok();
    }
    
    [Authorize]
    [HttpPost("send-mail-verify-code")]
    public async Task<ActionResult> SendApproveMailVerificationCode(CancellationToken token)
    {
        var userId = User.GetUserId();

        await userService.SendMailVerification(userId, token);

        return Ok();
    }
    
    [Authorize]
    [HttpPost("update-mail/old-email-send-code")]
    public async Task<ActionResult> UpdateMailSendCode(CancellationToken token)
    {
        var userId = User.GetUserId();

        await userService.UpdateMailSendCode(userId, token);

        return Ok();
    }
    
    [Authorize]
    [HttpPost("update-mail/old-email-verify-code")]
    public async Task<ActionResult> UpdateMailVerifyCode(UpdateMailVerifyCodeRequest request, CancellationToken token)
    {
        var userId = User.GetUserId();

        var user = await userService.UpdateMailVerifyCode(userId, request.Code, token);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Sid, user.Id.ToString()),
            new(ClaimTypes.WindowsDeviceClaim, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "no_ip"),
            new(Constants.ClaimTypeTokenScope, Constants.ForbiddenAuthorizeClaimScope),
            new(Constants.ClaimTypeTokenType, TokenTypes.MailUpdateToken.ToString())
        };
        
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.Issuer,
            audience: AuthOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.ToUniversalTime().AddMinutes(5),
            signingCredentials: new SigningCredentials(
                AuthOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256)
        );

        var bearerToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        
        return Ok(new { Token = bearerToken });
    }
    
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("update-mail/new-email-send-code")]
    public async Task<ActionResult> UpdateMail(UpdateMailRequest request, CancellationToken token)
    {
        var userIdStr =
            User.FindFirst(ClaimTypes.Sid)?.Value
            ?? throw new ServiceException(ErrorCode.EmailUpdateError);
        if (!long.TryParse(userIdStr, out var userId))
        {
            throw new ServiceException(ErrorCode.EmailUpdateError);
        }
        
        ValidateTokenType(TokenTypes.MailUpdateToken, ErrorCode.EmailUpdateError);
        ValidateTokenIpAddress(ErrorCode.EmailUpdateError);
        ValidateTokenScopeOrThrow(ErrorCode.EmailUpdateError);

        await userService.SendVerifyCodeToNewEmail(userId, request.NewMail, token);
        
        return Ok();
    }
    
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("update-mail/new-email-verify-code")]
    public async Task<ActionResult> UpdateMailVerify(UpdateMailVerifyNewMailRequest request, CancellationToken token)
    {
        var userIdStr =
            User.FindFirst(ClaimTypes.Sid)?.Value
            ?? throw new ServiceException(ErrorCode.EmailUpdateError);
        if (!long.TryParse(userIdStr, out var userId))
        {
            throw new ServiceException(ErrorCode.EmailUpdateError);
        }
        
        ValidateTokenType(TokenTypes.MailUpdateToken, ErrorCode.EmailUpdateError);
        ValidateTokenIpAddress(ErrorCode.EmailUpdateError);
        ValidateTokenScopeOrThrow(ErrorCode.EmailUpdateError);

        await userService.VerifyNewEmail(userId, request.NewMail, request.Code, token);

        return Ok();
    }

    [HttpPost("login-by-bearer")]
    public async Task<ActionResult<V1LogInResponse>> Login(V1LogInRequest request, CancellationToken token)
    {
        var (user, permissions) = await userService.AuthorizeUser(
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
        var (user, permissions) = await userService.AuthorizeUser(
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
            // Устанавливаем время отсчета срока действия куки
            IssuedUtc = DateTime.UtcNow,
            // Устанавливаем параметры обновления куки
            AllowRefresh = true
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
    
    [HttpPost("reset-password/send-code")]
    public async Task<ActionResult> SendPasswordResetCode(V1SendPasswordResetCodeRequest request, CancellationToken token)
    {
        await userService.PasswordResetSendCode(request.Email, token);

        return Ok();
    }

    [HttpPost("reset-password/verify-code")]
    public async Task<IActionResult> VerifyPasswordReset(V1VerifyPasswordResetRequest request, CancellationToken token)
    {
        var user = await userService.PasswordResetVerifyCode(
            request.Email,
            request.Code.ToString(),
            token);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Sid, user.Id.ToString()),
            new(ClaimTypes.WindowsDeviceClaim, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "no_ip"),
            new(Constants.ClaimTypeTokenScope, Constants.ForbiddenAuthorizeClaimScope),
            new(Constants.ClaimTypeTokenType, TokenTypes.ResetPasswordToken.ToString())
        };
        
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.Issuer,
            audience: AuthOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.ToUniversalTime().Add(_temporaryTokensExpiration),
            signingCredentials: new SigningCredentials(
                AuthOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256)
        );

        var bearerToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        
        return Ok(new { Token = bearerToken });
    }
    
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("reset-password/finish")]
    public async Task<ActionResult> ResetPassword(
        V1ResetPasswordRequest request,
        CancellationToken token)
    {
        var userIdStr =
            User.FindFirst(ClaimTypes.Sid)?.Value
            ?? throw new ServiceException(ErrorCode.PasswordResetError);
        
        if (!long.TryParse(userIdStr, out var userId))
        {
            throw new ServiceException(ErrorCode.PasswordResetError);
        }

        ValidateTokenType(TokenTypes.ResetPasswordToken, ErrorCode.PasswordResetError);
        ValidateTokenIpAddress(ErrorCode.PasswordResetError);
        ValidateTokenScopeOrThrow(ErrorCode.PasswordResetError);

        await userService.PasswordReset(userId, request.Email, request.NewPassword, token);

        return Ok();
    }
}