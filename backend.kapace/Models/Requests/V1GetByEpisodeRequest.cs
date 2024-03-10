namespace backend.kapace.Models.Requests;

public class V1GetByEpisodeRequest 
{
    public required long ContentId { get; init; }
    public long? EpisodeId { get; init; }
    public long? TranslatorId { get; init; }
}
