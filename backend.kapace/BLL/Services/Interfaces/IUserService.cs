using backend.kapace.DAL.Models.Query;
using User = backend.kapace.BLL.Models.User.User;
using UserPermission = backend.kapace.BLL.Models.User.UserPermission;

namespace backend.kapace.BLL.Services.Interfaces;

public interface IUserService
{
    Task<(User, UserPermission[])> GetCurrent(long currentUserId, CancellationToken token);
    Task<User[]> Query(UserQuery query, CancellationToken token);
    Task<(User, UserPermission[])> AuthorizeUser(string email, string password, CancellationToken token);
    Task UpdatePassword(long userId, string oldPassword, string newPassword, CancellationToken token);
    Task Register(string nickname, string email, string password, CancellationToken token);
    Task<bool> TryVerifyMail(long userId, int verificationCode, CancellationToken token);
    Task SendMailVerification(long userId, CancellationToken token);
    Task UpdateNickname(long userId, string newNickname, bool isForce, CancellationToken token);
    Task<User> PasswordResetSendCode(string email, CancellationToken token);
    Task PasswordReset(long userId, string email, string newPassword, CancellationToken token);
    Task<User> PasswordResetVerifyCode(string email, string code, CancellationToken token);
    Task UpdateMailSendCode(long userId, CancellationToken token);
    Task<User> UpdateMailVerifyCode(long userId, string code, CancellationToken token);
    Task VerifyNewEmail(long userId, string newEmail, string code, CancellationToken token);
    Task SendVerifyCodeToNewEmail(long userId, string newEmail, CancellationToken token);
}