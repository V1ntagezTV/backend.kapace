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
    Task InitMailVerification(long userId, CancellationToken token);
    Task UpdateNickname(long userId, string newNickname, CancellationToken token);
    Task<User> SendPasswordResetCode(string email, CancellationToken token);
    Task ResetPassword(long userId, string email, string newPassword, CancellationToken token);
    Task<User> VerifyPasswordResetCode(string email, int code, CancellationToken token);
}