namespace backend.kapace.DAL.Models;

public class Genre
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long CreatedBy { get; set; }
}