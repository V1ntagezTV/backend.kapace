namespace backend.kapace.DAL.Repository.Interfaces.Models;

public interface ITranslator
{
    public long TranslatorId { get; init; }
    public string? TranslatorName { get; init; }
    public string? TranslatorLink { get; init; }
}