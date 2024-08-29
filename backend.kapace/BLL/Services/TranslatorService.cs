using backend.kapace.BLL.Models.Translators;
using backend.kapace.BLL.Services.Interfaces;
using DALModel = backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;

namespace backend.kapace.BLL.Services;

public class TranslatorService : ITranslatorService
{
    private readonly ITranslatorsRepository _translatorsRepository;

    public TranslatorService(ITranslatorsRepository translatorsRepository)
    {
        _translatorsRepository = translatorsRepository;
    }

    public async Task<Translator[]> Query(TranslatorQuery query, CancellationToken token)
    {
        var translator = await _translatorsRepository.QueryAsync((DALModel.Query.TranslatorQuery)query, token);

        return translator
            .Select(translator => new Translator(translator.Id, translator.Name, translator.Link))
            .ToArray();
    }
}
