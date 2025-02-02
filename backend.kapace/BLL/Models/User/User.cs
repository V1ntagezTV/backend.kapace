namespace backend.kapace.BLL.Models.User;

public record User(
    long Id,
    string Nickname,
    string HashedPassword,
    string Email,
    DateTimeOffset CreatedAt);