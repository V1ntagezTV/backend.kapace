namespace backend.kapace.Models.Response;

public record TranslatorQueryResponse(TranslatorQueryResponse.Translator[] Translators)
{
    public record Translator(long TranslatorId, string Name, string Link);
}