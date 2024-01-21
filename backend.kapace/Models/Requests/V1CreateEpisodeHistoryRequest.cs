namespace backend.kapace.Models.Requests;

public record V1CreateEpisodeHistoryRequest(
    long ContentId,
    V1CreateEpisodeHistoryRequest.ChangeableEpisodeFields ChangeableFields,
    long CreatedBy)
{
    public record ChangeableEpisodeFields(
        long? EpisodeId,
        int? Number,
        string? Title,
        string? Image
    );
}