namespace backend.kapace.Models.Requests;

public record V1GetByEpisodeRequest(
    long ContentId,
    long? EpisodeId,
    long? TranslatorId);
