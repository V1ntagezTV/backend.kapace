using backend.kapace.BLL.Exceptions;
using backend.kapace.BLL.Models.User;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models.Query;
using backend.kapace.DAL.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using User = backend.kapace.BLL.Models.User.User;

namespace backend.kapace.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserPermissionRepository _userPermissionRepository;

    public UserService(
        IUserRepository userRepository,
        IPermissionRepository permissionRepository,
        IUserPermissionRepository userPermissionRepository)
    {
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

    public Task UpdatePassword(string email, string oldPassword, string newPassword)
    {
        throw new NotImplementedException();
    }

    public async Task Register(string nickname, string email, string password, CancellationToken token)
    {
        var usersQuery = await Query(new UserQuery
        {
            Nicknames = new[] { nickname }
        }, token);
        
        if (usersQuery is {Length: >0})
        {
            throw new ArgumentException();
        }

        var hasher = new PasswordHasher<User>();
        var passwordHash = hasher.HashPassword(null!, password);

        await _userRepository.Insert(nickname, email, passwordHash, DateTimeOffset.UtcNow, token);
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
            Emails = new[] { email }
        }, token);
        
        if (usersQuery is null or {Length: 0})
        {
            throw new ArgumentException();
        }

        return usersQuery.Single();
    }
}