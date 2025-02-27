using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface IUserRepository
{
    Task<IReadOnlyCollection<User>> Query(UserQuery query, CancellationToken token);

    Task Insert(
        string nickname,
        string email,
        string passwordHash,
        DateTimeOffset createdAt,
        CancellationToken token);

    Task UpdateVerifiedMail(long userId, bool isMailVerified, CancellationToken token);
    Task UpdatePassword(long userId, string passwordHash, CancellationToken token);
    Task UpdateNickname(long userId, string newNickname, CancellationToken token);
}