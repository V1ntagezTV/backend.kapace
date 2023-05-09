namespace backend.kapace.DAL.Models;

public record QueryContentGenre
{
    public long[] GenreIds {get; set;} = Array.Empty<long>();
    public long[] ContentIds { get; set; } = Array.Empty<long>();
}