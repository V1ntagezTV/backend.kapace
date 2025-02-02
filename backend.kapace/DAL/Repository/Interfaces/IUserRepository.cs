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
}