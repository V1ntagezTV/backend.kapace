using System.ComponentModel.DataAnnotations;
using backend.kapace.BLL.Enums;

namespace backend.kapace.Models.Requests;

public record V1CreateEpisodeHistoryRequest(
    [Required]long ContentId,
    [Required]V1CreateEpisodeHistoryRequest.ChangeableEpisodeFields ChangeableFields,
    long CreatedBy)
{
    public record ChangeableEpisodeFields(
        [Required]int Number,
        [Required]string VideoScript,
        [Required]Language Language,
        [Required]TranslationType TranslationType,
        long? EpisodeId,
        long? TranslatorId,
        string? Title,
        long? Image,
        int? Quality
    );
}