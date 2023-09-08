using backend.Models.Enums;

namespace backend.kapace.Models.Response;

public record V1GetFullContentResponse(
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
    DateTimeOffset? ReleasedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastUpdateAt,
    int MinAgeLimit,
    int? Duration,
    V1GetFullContentResponse.V1GetFullContentEpisode[] Episodes,
    V1GetFullContentResponse.V1GetFullContentGenre[] Genres,
    V1GetFullContentResponse.V1GetFullContentUserInfo? UserInfo)
{
    public record V1GetFullContentGenre(long GenreId, string Name);
    
    public record V1GetFullContentEpisode(
        long Id,
        string Title,
        string Image,
        int Number);

    public record V1GetFullContentUserInfo(
        long ContentId,
        long UserId,
        bool IsInFavourite,
        int LastViewedSeries,
        decimal Rating);
};