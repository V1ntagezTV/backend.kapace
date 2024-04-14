using backend.kapace.BLL.Enums;

namespace backend.kapace.BLL.Models.EpisodeTranslations;

public record EpisodeTranslation(
    long Id, 
    long ContentId, 
    long EpisodeId, 
    string Link, 
    TranslationType TranslationType, 
    DateTimeOffset CreatedAt, 
    long CreatedBy, 
    int Quality, 
    long TranslatorId, 
    Language Language);