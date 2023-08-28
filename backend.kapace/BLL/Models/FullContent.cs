using backend.Models.Enums;

namespace backend.kapace.BLL.Models;

public record FullContent(
    long ContentId,
    string Title,
    string Description,
    ContentType Type,
    ContentStatus Status,
    long ImageId,
    decimal ImportStars,
    int OutSeries,
    int PlannedSeries,
    int Views,
    string Country,
    DateTimeOffset ReleasedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastUpdateAt,
    int MinAgeLimit,
    int? Duration,
    FullContent.FullContentEpisode[] Episodes,
    FullContent.FullContentGenre[] Genres,
    FullContent.FullContentUserInfo? UserInfo)
{
    public record FullContentGenre(long GenreId, string Name);
    
    public record FullContentEpisode(
        long Id,
        string Title,
        string Image,
        int Number);

    public record FullContentUserInfo(
        long ContentId,
        long UserId,
        bool IsInFavourite,
        int LastViewedSeries,
        decimal Rating);
};