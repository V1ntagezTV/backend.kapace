namespace backend.kapace.BLL.Models.Episode;

public record Episode(
    long Id,
    long ContentId,
    string Title,
    string Image,
    int Number,
    int Views,
    int Stars,
    DateTimeOffset CreatedAt,
    long CreatedBy);