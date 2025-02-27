using System.Security.Cryptography;
using backend.kapace.BLL.Exceptions;
using backend.kapace.BLL.Models.Cache;
using backend.kapace.BLL.Models.User;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models.Query;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Options;
using backend.Models.Enums;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MimeKit;
using User = backend.kapace.BLL.Models.User.User;

namespace backend.kapace.BLL.Services;

public class UserService : IUserService
{
    const string MailVerificationCodePrefix = "mail_verification_code";
    const string RestorePasswordCodePrefix = "restore_password_code";

    private readonly SmtpMailOptions _smtpMailOptions;
    private readonly IMemoryCache _memoryCache;
    private readonly IUserRepository _userRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserPermissionRepository _userPermissionRepository;

    public UserService(
        IOptions<SmtpMailOptions> smtpMailOptions,
        IMemoryCache memoryCache,
        IUserRepository userRepository,
        IPermissionRepository permissionRepository,
        IUserPermissionRepository userPermissionRepository)
    {
        _smtpMailOptions = smtpMailOptions.Value;
        _memoryCache = memoryCache;
        _userRepository = userRepository;
        _permissionRepository = permissionRepository;
        _userPermissionRepository = userPermissionRepository;
    }

    public async Task<(User, UserPermission[])> GetCurrent(long currentUserId, CancellationToken token)
    {
        var users = await Query(new UserQuery
        {
            UserIds = new[] { currentUserId }
        }, token);

        if (users.Length == 0)
        {
            throw new UserNotFoundException(currentUserId);
        }

        var user = users.Single();
        var userRoles = await GetUserPermissions(user.Id, token);
        return (user, userRoles);
    }

    public async Task<User[]> Query(UserQuery query, CancellationToken token)
    {
        var users = await _userRepository.Query(new UserQuery
        {
            UserIds = query.UserIds,
            Nicknames = query.Nicknames,
            Emails = query.Emails
        }, token);

        return users
            .Select(user => new User(
                user.Id,
                user.Nickname,
                user.PasswordHash,
                user.Email,
                user.IsMailVerified,
                user.CreatedAt))
            .ToArray();
    }

    public async Task<(User, UserPermission[])> AuthorizeUser(
        string email,
        string password,
        CancellationToken token)
    {
        var user = await GetUserByEmailOrThrow(email, token);
        ValidateUserPassword(password, user.HashedPassword);
        var userRoles = await GetUserPermissions(user.Id, token);
        return (user, userRoles);
    }

    public async Task UpdatePassword(long userId, string oldPassword, string newPassword, CancellationToken token)
    {
        var user = await GetUserByIdOrThrow(userId, token);
        ValidateUserPassword(oldPassword, user.HashedPassword);
        var hasher = new PasswordHasher<User>();
        var passwordHash = hasher.HashPassword(null!, newPassword);
        await _userRepository.UpdatePassword(userId, passwordHash, token);
    }

    public async Task Register(string nickname, string email, string password, CancellationToken token)
    {
        await ValidateNickname(nickname, token);
        var hasher = new PasswordHasher<User>();
        var passwordHash = hasher.HashPassword(null!, password);

        await _userRepository.Insert(nickname, email, passwordHash, DateTimeOffset.UtcNow, token);
    }
    
    public async Task<bool> TryVerifyMail(long userId, int verificationCode, CancellationToken token)
    {
        var user = await GetUserByIdOrThrow(userId, token);
        if (user.IsMailVerified)
        {
            throw new ServiceException(ErrorCode.UserMailAlreadyVerified);
        }

        var cacheKey = GetMailVerificationCodeCacheKey(userId, user.Email);
        if (_memoryCache.TryGetValue(cacheKey, out MailVerificationCacheValue? value) && value is not null)
        {
            if (DateTimeOffset.UtcNow > value.CreatedAt.AddMinutes(Constants.MailVerificationCodeExpiration))
            {
                throw new ServiceException(ErrorCode.MailVerificationError);
            }

            if (value.AttemptsLeft == 0)
            {
                throw new ServiceException(ErrorCode.MailVerificationError);
            }

            if (value.Code == verificationCode)
            {
                await _userRepository.UpdateVerifiedMail(userId, true, token);
                return true;
            }

            _memoryCache.Remove(cacheKey);
            _memoryCache.Set(cacheKey, value with { AttemptsLeft = value.AttemptsLeft - 1 });
            return false;
        }

        return false;
    }

    public async Task InitMailVerification(long userId, CancellationToken token)
    {
        var users = await _userRepository.Query(new UserQuery()
        {
            UserIds = new[] { userId },
        }, token);

        if (users.Count == 0)
        {
            throw new ServiceException(ErrorCode.UserNotFound)
                .SetData(nameof(userId), userId.ToString());
        }

        var user = users.Single();
        if (user.IsMailVerified)
        {
            throw new ServiceException(ErrorCode.UserMailAlreadyVerified);
        }

        var code = GetRandomNumber();
        
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Подтвердите ваш email", _smtpMailOptions.Email));
        message.To.Add(new MailboxAddress("", user.Email));
        message.Subject = "Подтвердите ваш email";
        message.Body = new TextPart("plain")
        {
            Text = $"Your verification code is: {code}",
        };

        await SendEmailMessage(message, token);
        var cacheKey = GetMailVerificationCodeCacheKey(userId, user.Email);
        var value = new MailVerificationCacheValue(code, DateTimeOffset.UtcNow);

        _memoryCache.Remove(cacheKey);
        _memoryCache.Set(cacheKey, value, TimeSpan.FromMinutes(Constants.MailVerificationCodeExpiration));
    }

    public async Task UpdateNickname(long userId, string newNickname, CancellationToken token)
    {
        var user = await GetUserByIdOrThrow(userId, token);
        await ValidateNickname(newNickname, token);
        await _userRepository.UpdateNickname(user.Id, newNickname, token);
    }

    public async Task<User> SendPasswordResetCode(string email, CancellationToken token)
    {
        var user = await GetUserByEmailOrThrow(email, token);
        var code = GetRandomNumber();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Восстановление пароля на insurka.ru", _smtpMailOptions.Email));
        message.To.Add(new MailboxAddress("", user.Email));
        message.Subject = "Код для восстановления пароля";
        message.Body = new TextPart("plain")
        {
            Text = $"Your verification code is: {code}",
        };
        
        var cacheKey = GetPasswordResetCacheKey(user.Id, user.Email);
        var value = new PasswordResetCacheValue(code, DateTimeOffset.UtcNow);
        _memoryCache.Remove(cacheKey);
        _memoryCache.Set(cacheKey, value, TimeSpan.FromMinutes(Constants.RestorePasswordCodeExpiration));
        await SendEmailMessage(message, token);

        return user;
    }
    
    public async Task<User> VerifyPasswordResetCode(string email, int code, CancellationToken token)
    {
        var user = await GetUserByEmailOrThrow(email, token);
        var cacheKey = GetPasswordResetCacheKey(user.Id, user.Email);
        if (_memoryCache.TryGetValue(cacheKey, out PasswordResetCacheValue? value) && value is not null)
        {
            if (DateTimeOffset.UtcNow > value.CreatedAt.AddMinutes(Constants.MailVerificationCodeExpiration))
            {
                throw new ServiceException(ErrorCode.PasswordResetError);
            }

            if (value.AttemptsLeft == 0)
            {
                throw new ServiceException(ErrorCode.PasswordResetError);
            }

            if (value.Code == code)
            {
                _memoryCache.Remove(cacheKey);
                return user;
            }

            var nextValue = value with { AttemptsLeft = value.AttemptsLeft - 1 };
            _memoryCache.Remove(cacheKey);
            _memoryCache.Set(cacheKey, nextValue);
            throw new ServiceException(ErrorCode.PasswordResetError)
                .SetData(nameof(nextValue.AttemptsLeft), nextValue.AttemptsLeft.ToString());
        }

        throw new ServiceException(ErrorCode.PasswordResetForbidden);
    }

    public async Task ResetPassword(long userId, string email, string newPassword, CancellationToken token)
    {
        var emailUser = await GetUserByEmailOrThrow(email, token);
        var userByUserId = await GetUserByIdOrThrow(userId, token);
        if (emailUser.Id != userByUserId.Id)
        {
            throw new ServiceException(ErrorCode.PasswordResetForbidden);
        }

        var hasher = new PasswordHasher<User>();
        var passwordHash = hasher.HashPassword(null!, newPassword);
        if (userByUserId.HashedPassword == passwordHash)
        {
            throw new ServiceException(ErrorCode.UserPasswordMustBeDifferentThanOld);
        }
        
        await _userRepository.UpdatePassword(userId, newPassword, token);
    }

    private async Task SendEmailMessage(MimeMessage message, CancellationToken token)
    {
        using var client = new SmtpClient();
        // Подключение к SMTP-серверу
        await client.ConnectAsync(_smtpMailOptions.SmtpServer, _smtpMailOptions.Port, SecureSocketOptions.None, token);
        
        // Вход в систему
        await client.AuthenticateAsync(_smtpMailOptions.Email, _smtpMailOptions.Password, token);

        // Отправка сообщения
        await client.SendAsync(message, token);
        
        // Отключение
        await client.DisconnectAsync(true, token);
    }
    
    private static int GetRandomNumber()
    {
        var codeBytes = new byte[4];
        RandomNumberGenerator.Fill(codeBytes);
        var code = BitConverter.ToInt32(codeBytes) % 1000000;
        if (code < 0)
        {
            code *= -1;
        }
        
        return code;
    }

    private async Task ValidateNickname(string nickname, CancellationToken token)
    {
        var usersQuery = await Query(new UserQuery
        {
            Nicknames = new[] { nickname }
        }, token);
        
        if (usersQuery is {Length: >0})
        {
            throw new ServiceException(ErrorCode.NicknameAlreadyExists);
        }
    }

    private async Task<UserPermission[]> GetUserPermissions(long userId, CancellationToken token)
    {
        var userPermissions = await _userPermissionRepository.Query(
            new UserPermissionsQuery { UserIds = new[] { userId } },
            token);
        
        if (userPermissions.Count == 0)
        {
            return Array.Empty<UserPermission>();
        }

        var userPermissionIds = userPermissions.Select(ur => ur.PermissionId).ToArray();
        var permissions = await _permissionRepository.Query(
            new() { Ids = userPermissionIds },
            token);
        
        var permissionsMap = permissions.ToDictionary(x => x.Id);
        
        var result = userPermissions.Select(
                userPermission =>
                {
                    var permission = permissionsMap[userPermission.PermissionId];

                    return new UserPermission(
                        userPermission.PermissionId,
                        permission.Alias,
                        permission.Description,
                        userPermission.GivedBy,
                        userPermission.CreatedAt);
                })
            .ToArray();

        return result;
    }

    private static void ValidateUserPassword(
        string inputPassword,
        string hashedPassword)
    {
        var hasher = new PasswordHasher<User>();
        var passwordVerificationResult = hasher.VerifyHashedPassword(null!, hashedPassword, inputPassword);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            throw new ArgumentException();
        }
    }
    
    private async Task<User> GetUserByEmailOrThrow(string email, CancellationToken token)
    {
        var usersQuery = await Query(new UserQuery
        {
            Emails = [email]
        }, token);
        
        if (usersQuery is null or {Length: 0})
        {
            throw new ServiceException(ErrorCode.UserNotFound)
                .SetData(nameof(email), email);
        }

        return usersQuery.Single();
    }
    
    private async Task<User> GetUserByIdOrThrow(long userId, CancellationToken token)
    {
        var usersQuery = await Query(new UserQuery
        {
            UserIds = [userId]
        }, token);
        
        if (usersQuery is null or {Length: 0})
        {
            throw new ServiceException(ErrorCode.UserNotFound);
        }

        return usersQuery.Single();
    }
    
    private static string GetMailVerificationCodeCacheKey(long userId, string userEmail)
    {
        return $"{MailVerificationCodePrefix}-{userId}-{userEmail}";
    }
    
    private static string GetPasswordResetCacheKey(long userId, string userEmail)
    {
        return $"{RestorePasswordCodePrefix}-{userId}-{userEmail}";
    }
}