using backend.kapace.BLL.Models.VideoService;

namespace backend.kapace.BLL.Models;

public record FullContent(
    Content Content,
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