using backend.kapace.BLL.Enums;

namespace backend.kapace.DAL.Models;

public class Translation 
{
    public required long Id { get; init; }
    public required long ContentId { get; init; }
    public required long EpisodeId { get; init; }
    public required long TranslatorId { get; init; }
    public required Language Lang { get; init; }
    public required string Link { get; init; }
    public required TranslationType TranslationType { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required long CreatedBy { get; init; }
    public required int Quality { get; init; }
    public required WithEpisode Episode { get; init; }
    public required WithTranslator Translator { get; init; }

    public record WithTranslator(long TranslatorId, string Name, string TranslatorLink);
    public record WithEpisode(string Title, int Number, int Views, double Stars);
}

