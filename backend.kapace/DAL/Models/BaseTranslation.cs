using backend.kapace.BLL.Enums;
using backend.kapace.DAL.Repository.Interfaces.Models;

namespace backend.kapace.DAL.Models;

public class BaseTranslation : ITranslation, ITranslator
{
    public long TranslationId { get; init; }
    public long ContentId { get; init; }
    public long EpisodeId { get; init; }
    public Language Language { get; init; }
    public string Link { get; init; }
    public TranslationType TranslationType { get; init; }
    public int Quality { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public long CreatedBy { get; init; }
    
    public long TranslatorId { get; init; }
    public string? TranslatorName { get; init; }
    public string? TranslatorLink { get; init; }
}