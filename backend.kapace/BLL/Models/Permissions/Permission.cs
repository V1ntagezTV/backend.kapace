namespace backend.kapace.BLL.Models.Permissions;

public record Permission(
    long Id,
    string Alias,
    string Description,
    long CreatedBy,
    DateTimeOffset CreatedAt);