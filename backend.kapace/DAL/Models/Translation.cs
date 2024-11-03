using backend.kapace.BLL.Enums;

namespace backend.kapace.DAL.Models;

public class Translation 
{
    public required long Id { get; init; }
    public required long ContentId { get; init; }
    public required long EpisodeId { get; init; }
    public required long TranslatorId { get; init; }
    public required Language Language { get; init; }
    public required string Link { get; init; }
    public required TranslationType TranslationType { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required long CreatedBy { get; init; }
    public required int Quality { get; init; }

    #region Translator
    public required string? Name { get; init; }
    public required string? TranslatorLink { get; init; }
    #endregion

    #region Episode 
    public required int Number { get; init; }
    public required string EpisodeTitle { get; init; }
    public required int EpisodeViews { get; init; }
    public required double EpisodeStars { get; init; }
    #endregion
}

