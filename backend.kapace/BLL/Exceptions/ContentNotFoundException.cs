namespace backend.kapace.BLL.Exceptions;

public class ContentNotFoundException : Exception
{
    public long ContentId { get; }

    public ContentNotFoundException(long contentId)
    {
        ContentId = contentId;
    }
}