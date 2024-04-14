
namespace backend.kapace.DAL.Models;

public record Episode
{
    public long Id { get; init; }
    public long ContentId { get; init;}
    public string Title { get; init;}
    public string Image { get; init; }
    public int Number { get; init; }
    public int Views { get; init; }
    public int Stars { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public long CreatedBy { get; init; }

    public static Episode CreateInsertModel(
        long contentId,
        int number,
        string title,
        string image,
        long createdBy)
    {
        return new Episode {
            Id = 0,
            ContentId = contentId,
            Number = number,
            Title = title,
            Image = image,
            Views = 0,
            Stars = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}