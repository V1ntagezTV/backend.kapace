using backend.kapace.BLL.Enums;

namespace backend.kapace.DAL.Repository.Interfaces.Models;

public interface ITranslation
{
    public long TranslationId { get; init; }
    public long ContentId { get; init; }
    public long EpisodeId { get; init; }
    public long TranslatorId { get; init; }
    
    public Language Language { get; init; }
    public string Link { get; init; }
    public TranslationType TranslationType { get; init; }
    public int Quality { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    public long CreatedBy { get; init; }
}
