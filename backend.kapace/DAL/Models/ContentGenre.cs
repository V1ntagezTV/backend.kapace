namespace backend.kapace.DAL.Models;

public class ContentGenre
{
    public long ContentId {get; set;}
    
    public long GenreId {get; set;}
    
    public DateTimeOffset CreatedAt {get; set;}
    
    public long? CreatedBy {get; set;}
    
    public class WithName : ContentGenre
    {
        public string Name { get; set; }
    }
}