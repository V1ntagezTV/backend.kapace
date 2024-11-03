namespace backend.kapace.Models.Response;

public record V1GetByIdResponse(
    QueryContent Content,
    V1GetByIdResponse.V1GetFullContentEpisode[] Episodes,
    V1GetByIdResponse.V1GetFullContentGenre[] Genres,
    V1GetByIdResponse.V1GetFullContentUserInfo? UserInfo)
{
    public record V1GetFullContentGenre(long GenreId, string Name);
    
    public record V1GetFullContentEpisode(
        long Id,
        string Title,
        long? Image,
        int Number);

    public record V1GetFullContentUserInfo(
        long ContentId,
        long UserId,
        bool IsInFavourite,
        int LastViewedSeries,
        decimal Rating);
};