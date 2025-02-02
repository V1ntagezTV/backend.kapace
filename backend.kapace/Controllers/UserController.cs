using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models.Query;
using backend.kapace.Middlewares;
using backend.kapace.Models.Requests.User;
using backend.kapace.Models.Response;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using V1QueryRequest = backend.kapace.Models.Requests.User.V1QueryRequest;
using V1QueryResponse = backend.kapace.Models.Requests.User.V1QueryResponse;

namespace backend.kapace.Controllers;

[ApiController]
[Route("v1/user")]
// TODO: добавить валидаторы
// TODO: need update password handler
// TODO: mail verification method with message code
public class UserController : Controller
{
    private readonly DateTimeOffset _loginDuration = DateTimeOffset.UtcNow.AddDays(1);
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize(Roles = "admin")]
    [HttpPost("current")]
    public async Task<ActionResult> V1GetCurrent(CancellationToken token)
    {
        var userId = User.GetUserId();
        var (user, roles) = await _userService.GetCurrent(userId, CancellationToken.None);

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

    [Authorize(Roles = "admin")]
    [HttpPost("query")]
    public async Task<ActionResult<V1QueryResponse>> V1Query(V1QueryRequest request, CancellationToken token)
    {
        var users = await _userService.Query(new UserQuery
        {
            UserIds = request.UserIds,
            Nicknames = request.Nicknames,
            Emails = request.Emails
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
    public async Task<IActionResult> LogInByCookie(V1LogInRequest request, CancellationToken token)
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
            IsPersistent = true,
            // Устанавливаем срок действия куки
            ExpiresUtc = _loginDuration,
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Ok(new { message = "Login successful" });
    }

    [Authorize(Roles = "admin")]
    [HttpPost("test-admin")]
    public Task<IActionResult> Test()
    {
        return Task.FromResult<IActionResult>(Ok(new { message = "successful" }));
    }
    
    [HttpPost("cookie-logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }
}