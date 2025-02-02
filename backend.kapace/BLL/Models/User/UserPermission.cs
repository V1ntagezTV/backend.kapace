namespace backend.kapace.BLL.Models.User;

public record UserPermission(
    long Id,
    string Alias,
    string Description,
    long CreatedBy,
    DateTimeOffset CreatedAt);