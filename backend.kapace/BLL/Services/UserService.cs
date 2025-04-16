using System.Security.Cryptography;
using backend.kapace.BLL.Exceptions;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Options;
using backend.Models.Enums;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using User = backend.kapace.BLL.Models.User.User;
using UserPermission = backend.kapace.BLL.Models.User.UserPermission;

namespace backend.kapace.BLL.Services;

public class UserService(
    IOptions<SmtpMailOptions> smtpMailOptions,
    IVerificationCodeRepository verificationCodeRepository,
    IUserRepository userRepository,
    IPermissionRepository permissionRepository,
    IUserPermissionRepository userPermissionRepository,
    ILoggingRepository loggingRepository)
    : IUserService
{
    private readonly SmtpMailOptions _smtpMailOptions = smtpMailOptions.Value;

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
        var users = await userRepository.Query(new UserQuery
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
                user.CreatedAt,
                user.ImageUrl))
            .ToArray();
    }

    public async Task<(User, UserPermission[])> AuthorizeUser(
        string email,
        string password,
        CancellationToken token)
    {
        var user = await GetUserByEmailOrThrow(email, token);
        if (!IsValidHashedInput(password, user.HashedPassword))
        {
            throw new ServiceException(ErrorCode.WrongInputCode);
        }
        
        var userRoles = await GetUserPermissions(user.Id, token);
        return (user, userRoles);
    }

    public async Task UpdatePassword(
        long userId,
        string oldPassword,
        string newPassword,
        CancellationToken token)
    {
        var user = await GetUserByIdOrThrow(userId, token);
        if (IsValidHashedInput(oldPassword, user.HashedPassword))
        {
            throw new ServiceException(ErrorCode.WrongInputCode);
        }
        var hasher = new PasswordHasher<User>();
        var passwordHash = hasher.HashPassword(null!, newPassword);
        await userRepository.UpdatePassword(userId, passwordHash, token);

        var logModel = new LogModel
        {
            UserId = userId,
            Changes = [new LogModel.Value(user.HashedPassword, passwordHash)],
            CreatedAt = DateTimeOffset.UtcNow,
            Metadata = ["PasswordUpdate"]
        };
        
        await loggingRepository.Insert(logModel, token);
    }

    public async Task Register(string nickname, string email, string password, CancellationToken token)
    {
        await ValidateNickname(nickname, token);
        var hasher = new PasswordHasher<User>();
        var passwordHash = hasher.HashPassword(null!, password);

        var userId = await userRepository.Insert(nickname, email, passwordHash, DateTimeOffset.UtcNow, token);
        
        var logModel = new LogModel
        {
            UserId = userId,
            Changes = [
                new LogModel.Value(null, nickname),
                new LogModel.Value(null, email),
                new LogModel.Value(null, passwordHash)
            ],
            CreatedAt = DateTimeOffset.UtcNow,
            Metadata = ["Registration"]
        };
        
        await loggingRepository.Insert(logModel, token);
    }
    
    public async Task<bool> TryVerifyMail(long userId, int verificationCode, CancellationToken token)
    {
        var user = await GetUserByIdOrThrow(userId, token);
        if (user.IsMailVerified)
        {
            throw new ServiceException(ErrorCode.EmailAlreadyVerified);
        }

        var codeModels = await verificationCodeRepository.Query(new VerificationCodeQuery
        {
            UserId = userId,
            Type = VerificationCodeType.MailApprove,
            IsUsed = false
        }, token);

        if (codeModels is null || codeModels.Count == 0)
        {
            return false;
        }

        var lastCode = codeModels.MaxBy(x => x.CreatedAt);
        if (DateTimeOffset.UtcNow > lastCode.ExpiresAt.ToUniversalTime())
        {
            throw new ServiceException(ErrorCode.EmailVerificationError);
        }

        if (lastCode.Attempts <= 0)
        {
            throw new ServiceException(ErrorCode.EmailVerificationError);
        }

        if (!IsValidHashedInput(verificationCode.ToString(), lastCode.CodeHash))
        {
            lastCode.Attempts -= 1;
            await verificationCodeRepository.Update(lastCode, token);
            return false;
        }

        await userRepository.UpdateVerifiedMail(userId, true, token);

        var logModel = new LogModel
        {
            UserId = userId,
            Changes = [new LogModel.Value(null, user.Email)],
            CreatedAt = DateTimeOffset.UtcNow,
            Metadata = ["VerifiedMail"]
        };
        
        await loggingRepository.Insert(logModel, token);
        return true;
    }

    public async Task SendMailVerification(long userId, CancellationToken token)
    {
        var user = await GetUserByIdOrThrow(userId, token);
        if (user.IsMailVerified)
        {
            throw new ServiceException(ErrorCode.EmailAlreadyVerified);
        }

        var oldCodesQuery = new VerificationCodeQuery
        {
            UserId = userId,
            CreatedAfter = DateTimeOffset.UtcNow - TimeSpan.FromDays(1),
            Type = VerificationCodeType.MailApprove,
        };

        var oldCodes = await verificationCodeRepository.Query(oldCodesQuery, token);
        if (oldCodes.Count > 0)
        {
            const long dayLimit = 5;
            
            var todaySentCodes = oldCodes
                .Where(x => x.CreatedAt > DateTimeOffset.UtcNow - TimeSpan.FromDays(1))
                .ToArray();
            
            // 5 сообщений на почту - лимит в день
            if (todaySentCodes.Length > dayLimit)
            {
                throw new ServiceException(ErrorCode.VerificationCodeDayLimitExceeded)
                    .SetData(nameof(dayLimit), 5);
            }
            
            // Проверяем кулдаун
            var cooldownInMin = TimeSpan.FromMinutes(2);
            var isExistCodeOnCooldown = oldCodes
                .Any(code => code.CreatedAt.ToUniversalTime() > DateTimeOffset.UtcNow - cooldownInMin);
            
            if (isExistCodeOnCooldown)
            {
                throw new ServiceException(ErrorCode.VerificationCodeError)
                    .SetData(nameof(cooldownInMin), cooldownInMin.Minutes);
            }
        }

        var code = GetRandomNumber();
        
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("insurka.ru", _smtpMailOptions.Email));
        message.To.Add(new MailboxAddress("", user.Email));
        message.Subject = "Подтвердите ваш email";
        message.Body = new TextPart("plain")
        {
            Text = $"Your verification code is: {code}",
        };

        await SendEmailMessage(message, token);

        await verificationCodeRepository.ClearTable(
            userId,
            TimeSpan.FromDays(1),
            VerificationCodeType.MailApprove,
            token);

        await verificationCodeRepository.Insert([
            new VerificationCode
            {
                UserId = userId,
                CodeHash = GetHash(code.ToString()),
                ExpiresAt = DateTime.Now.AddMinutes(Constants.MailVerificationCodeExpirationInMinutes),
                Attempts = 3,
                Type = (int)VerificationCodeType.MailApprove,
                Email = user.Email,
            }
        ], token);
    }

    public async Task UpdateNickname(long userId, string newNickname, bool isForce, CancellationToken token)
    {
        var user = await GetUserByIdOrThrow(userId, token);
        await ValidateNickname(newNickname, token);

        if (!isForce)
            throw new ServiceException(ErrorCode.CallWithoutForceFlag);
        
        await userRepository.UpdateNickname(user.Id, newNickname, token);

        var logModel = new LogModel
        {
            UserId = userId,
            Changes = [new LogModel.Value(user.Nickname, newNickname)],
            CreatedAt = DateTimeOffset.UtcNow,
            Metadata = ["UpdateNickname"]
        };
        
        await loggingRepository.Insert(logModel, token);
    }

    public async Task<User> PasswordResetSendCode(string email, CancellationToken token)
    {
        var user = await GetUserByEmailOrThrow(email, token);
        var code = GetRandomNumber();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("insurka.ru", _smtpMailOptions.Email));
        message.To.Add(new MailboxAddress("", user.Email));
        message.Subject = "Код для восстановления пароля";
        message.Body = new TextPart("plain")
        {
            Text = $"""
                    Никому его не показывайте!
                    Ваш код для восстановления пароля: {code}. 
                    Код действителен в течении {Constants.RestorePasswordCodeExpirationInMinutes} минут.
                    """
        };

        await verificationCodeRepository.Insert([
            new VerificationCode
            {
                UserId = user.Id,
                CodeHash = GetHash(code.ToString()),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(Constants.RestorePasswordCodeExpirationInMinutes),
                Attempts = 3,
                Type = (int)VerificationCodeType.PasswordReset,
                Email = user.Email
            }
        ], token);
        await SendEmailMessage(message, token);

        return user;
    }
    
    public async Task<User> PasswordResetVerifyCode(string email, string code, CancellationToken token)
    {
        var user = await GetUserByEmailOrThrow(email, token);

        await InternalVerifyCode(
            user.Id,
            user.Email,
            code,
            VerificationCodeType.PasswordReset,
            token: token);
        
        return user;
    }

    public async Task PasswordReset(long userId, string email, string newPassword, CancellationToken token)
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
            throw new ServiceException(ErrorCode.PasswordMustBeDifferentThanOld);
        }
        
        await userRepository.UpdatePassword(userId, GetHash(newPassword), token);
        
        var logModel = new LogModel
        {
            UserId = userId,
            Changes = [new LogModel.Value(emailUser.HashedPassword, passwordHash)],
            CreatedAt = DateTimeOffset.UtcNow,
            Metadata = ["PasswordReset"]
        };
        
        await loggingRepository.Insert(logModel, token);
    }
    
    public async Task UpdateMailSendCode(long userId, CancellationToken token)
    {
        var user = await GetUserByIdOrThrow(userId, token);
        var code = GetRandomNumber();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("insurka.ru", _smtpMailOptions.Email));
        message.To.Add(new MailboxAddress("", user.Email));
        message.Subject = "Код для изменения почтового адреса";
        message.Body = new TextPart("plain")
        {
            Text = $"""
                    Ваш код для изменения почтового адреса: {code}. Никому его не показывайте!\n
                    Код действителен в течении {Constants.MailUpdateCodeExpirationInMinutes} минут.
                    """
        };
        
        await verificationCodeRepository.Insert([
            new VerificationCode
            {
                UserId = user.Id,
                CodeHash = GetHash(code.ToString()),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(Constants.MailUpdateCodeExpirationInMinutes),
                Attempts = 3,
                Type = (int)VerificationCodeType.MailUpdate,
                Email = user.Email
            }
        ], token);
        
        await SendEmailMessage(message, token);
    }

    public async Task<User> UpdateMailVerifyCode(long userId, string code, CancellationToken token)
    {
        var user = await GetUserByIdOrThrow(userId, token);
        await InternalVerifyCode(
            user.Id,
            user.Email,
            code,
            VerificationCodeType.MailUpdate,
            dayLimit: 1,
            token: token);
        
        return user;
    }

    public async Task SendVerifyCodeToNewEmail(long userId, string newEmail, CancellationToken token)
    {
        var usersByEmail = await userRepository.Query(new UserQuery { Emails = [newEmail] }, token);
        if (usersByEmail.Count != 0)
        {
            throw new ServiceException(ErrorCode.EmailAlreadyUsed);
        }
        
        var user = await GetUserByIdOrThrow(userId, token);
        
        var code = GetRandomNumber();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("insurka.ru", _smtpMailOptions.Email));
        message.To.Add(new MailboxAddress("", newEmail));
        message.Subject = "Код для подтверждения новой электронной почты";
        message.Body = new TextPart("plain")
        {
            Text = $"""
                    Ваш код: {code}. Никому его не показывайте!\n
                    Код действителен в течении {Constants.MailUpdateCodeExpirationInMinutes} минут.
                    """
        };

        await verificationCodeRepository.Insert([
            new VerificationCode
            {
                UserId = user.Id,
                CodeHash = GetHash(code.ToString()),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(Constants.MailUpdateCodeExpirationInMinutes),
                Attempts = 3,
                Type = (int)VerificationCodeType.NewMailApprove,
                Email = newEmail
            }
        ], token);

        await SendEmailMessage(message, token);
    }

    public async Task VerifyNewEmail(long userId, string newEmail, string code, CancellationToken token)
    {
        var usersByEmail = await userRepository.Query(new UserQuery { Emails = [newEmail] }, token);
        if (usersByEmail.Count != 0)
        {
            throw new ServiceException(ErrorCode.EmailAlreadyUsed);
        }

        var currentUser = await GetUserByIdOrThrow(userId, token);
        
        await InternalVerifyCode(
            currentUser.Id,
            newEmail,
            code,
            VerificationCodeType.NewMailApprove,
            dayLimit: 1,
            token: token);

        await userRepository.EmailUpdate(currentUser.Id, newEmail, token);
        
        var logModel = new LogModel
        {
            UserId = userId,
            Changes = [new LogModel.Value(currentUser.Email, newEmail)],
            CreatedAt = DateTimeOffset.UtcNow,
            Metadata = ["EmailUpdate"]
        };
        
        await loggingRepository.Insert(logModel, token);
    }
    
    private async Task InternalVerifyCode(
        long userId,
        string verificationEmail,
        string verificationCode,
        VerificationCodeType verificationCodeType,
        int dayLimit = 5,
        CancellationToken token = default)
    {
        var oldCodesQuery = new VerificationCodeQuery
        {
            UserId = userId,
            CreatedAfter = DateTimeOffset.UtcNow - TimeSpan.FromDays(1),
            Type = verificationCodeType,
        };

        var verificationCodes = await verificationCodeRepository.Query(oldCodesQuery, token);
        if (verificationCodes.IsNullOrEmpty())
        {
            throw new ServiceException(ErrorCode.VerificationCodeNotFound);
        }

        if (verificationCodes.Count(x => x.IsUsed) >= dayLimit)
        {
            throw new ServiceException(ErrorCode.VerificationCodeDayLimitExceeded);
        }
        
        var lastCode = verificationCodes.MaxBy(x => x.CreatedAt);
        var timeoutDate = lastCode.ExpiresAt.ToUniversalTime();

        if (lastCode.Email != verificationEmail)
        {
            throw new ServiceException(ErrorCode.VerificationCodeError);
        }
        
        if (DateTimeOffset.UtcNow >= timeoutDate)
        {
            throw new ServiceException(ErrorCode.VerificationCodeExpired);
        }

        if (lastCode.Attempts <= 0)
        {
            throw new ServiceException(ErrorCode.VerificationCodeAttemptsLimitExceeded);
        }
        
        if (!IsValidHashedInput(verificationCode, lastCode.CodeHash))
        {
            lastCode.Attempts -= 1;
            await verificationCodeRepository.Update(lastCode, token);

            throw new ServiceException(ErrorCode.VerificationCodeNotFound)
                .SetData(nameof(lastCode.Attempts), lastCode.Attempts.ToString());
        }

        lastCode.Attempts -= 1;
        lastCode.IsUsed = true;
        await verificationCodeRepository.Update(lastCode, token);
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
            Nicknames = [nickname]
        }, token);
        
        if (usersQuery is {Length: >0})
        {
            throw new ServiceException(ErrorCode.NicknameAlreadyExists);
        }
    }

    private async Task<UserPermission[]> GetUserPermissions(long userId, CancellationToken token)
    {
        var userPermissions = await userPermissionRepository.Query(
            new UserPermissionsQuery { UserIds = new[] { userId } },
            token);
        
        if (userPermissions.Count == 0)
        {
            return Array.Empty<UserPermission>();
        }

        var userPermissionIds = userPermissions.Select(ur => ur.PermissionId).ToArray();
        var permissions = await permissionRepository.Query(
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
    
    private static string GetHash(string input)
    {
        var hasher = new PasswordHasher<User>();

        return hasher.HashPassword(null, input);
    }
    
    private static bool IsValidHashedInput(
        string input,
        string hashValue)
    {
        var hasher = new PasswordHasher<User>();
        var passwordVerificationResult = hasher.VerifyHashedPassword(null!, hashValue, input);
        return passwordVerificationResult != PasswordVerificationResult.Failed;
    }
}