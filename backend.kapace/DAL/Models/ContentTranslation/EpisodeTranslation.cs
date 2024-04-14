namespace backend.kapace.DAL.Models.ContentTranslation;

public class EpisodeTranslation
{
    public long Id { get; set; }
    public long ContentId { get; set; }
    public long EpisodeId { get; set; }
    public string Link { get; set; }
    public int TranslationType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public int Quality { get; set; }
    public long TranslatorId { get; set; }
    public int Lang { get; set; }
}
