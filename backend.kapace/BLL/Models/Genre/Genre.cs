namespace backend.kapace.BLL.Models.Genre;

public record Genre(
    long Id,
    string Name,
    DateTimeOffset CreatedAt,
    long CreatedBy
);