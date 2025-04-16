namespace backend.kapace.DAL.Models;

public class Favorite 
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long ContentId { get; set; }
    public int? Status { get; set; }
    public long? EpisodeId { get; set; }
    public int Stars { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}