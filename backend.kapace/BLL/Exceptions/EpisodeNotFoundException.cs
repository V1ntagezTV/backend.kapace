namespace backend.kapace.BLL.Exceptions;

public class EpisodeNotFoundException : Exception
{
    public long EpisodeId { get; }

    public EpisodeNotFoundException(long episodeId)
    {
        EpisodeId = episodeId;
    }
}
