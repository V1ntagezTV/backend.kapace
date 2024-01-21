namespace backend.kapace.BLL.Exceptions;

public class TranslatorNotFoundException : Exception
{
    public long TranslatorId { get; }

    public TranslatorNotFoundException(long translatorId)
    {
        TranslatorId = translatorId;
    }
}
