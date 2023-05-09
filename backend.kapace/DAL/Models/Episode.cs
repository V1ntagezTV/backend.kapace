namespace backend.kapace.DAL.Models;

public record Episode
{
    public long Id {get; set; }
    public long ContentId {get; set; }
    public string Title {get; set; }
    public string Image {get; set; }
    public int Number {get; set; }
};