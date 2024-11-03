namespace backend.kapace.DAL.Models;

public class ContentGenreV1
{
    public long ContentId { get; init; }
    
    public long GenreId { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    
    public long CreatedBy { get; init; }
    
    public class WithName : ContentGenreV1
    {
        public string Name { get; set; }
    }
}