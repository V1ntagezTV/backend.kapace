using backend.kapace.BLL.Models.Translators;

namespace backend.kapace.BLL.Services.Interfaces;

public interface ITranslatorService {
    Task<Translator[]> Query(TranslatorQuery query, CancellationToken token);
}
