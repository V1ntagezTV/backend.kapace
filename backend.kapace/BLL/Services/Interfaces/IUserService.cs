using backend.kapace.DAL.Models.Query;
using User = backend.kapace.BLL.Models.User.User;
using UserPermission = backend.kapace.BLL.Models.User.UserPermission;

namespace backend.kapace.BLL.Services.Interfaces;

public interface IUserService
{
    Task<(User, UserPermission[])> GetCurrent(long currentUserId, CancellationToken token);
    Task<User[]> Query(UserQuery query, CancellationToken token);
    Task<(User, UserPermission[])> AuthorizeUser(string email, string password, CancellationToken token);
    Task UpdatePassword(string email, string oldPassword, string newPassword);
    Task Register(string nickname, string email, string password, CancellationToken token);
}