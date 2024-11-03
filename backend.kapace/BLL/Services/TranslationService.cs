using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Exceptions;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Models.EpisodeTranslations;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.ContentTranslation.Query;
using backend.kapace.DAL.Repository.Interfaces;
using backend.Models.Enums;
using InsertTranslation = backend.kapace.BLL.Models.InsertTranslation;

namespace backend.kapace.BLL.Services;

public class TranslationService : ITranslationService
{
    private readonly ITranslationRepository _translationRepository;
    private readonly IEpisodeRepository _episodeRepository;
    private readonly IContentRepository _contentRepository;
    private readonly ITranslatorsRepository _translatorsRepository;

    public TranslationService(
        ITranslationRepository translationRepository,
        IEpisodeRepository episodeRepository,
        IContentRepository contentRepository,
        ITranslatorsRepository translatorsRepository)
    {
        _translationRepository = translationRepository;
        _episodeRepository = episodeRepository;
        _contentRepository = contentRepository;
        _translatorsRepository = translatorsRepository;
    }
    
    public async Task<TranslatesView> GetByEpisodeAsync(
        long contentId,
        long? episodeId,
        long? translatorId,
        EpisodeOrderType? orderBy,
        CancellationToken token)
    {
        var episodes = await _episodeRepository.QueryAsync(new QueryEpisode()
        {
            ContentIds = new[] { contentId },
            EpisodeIds = episodeId is not null ? new[] { episodeId.Value } : null,
            OrderBy = orderBy
        }, token);

        if (episodes.Length == 0)
        {
            return new TranslatesView(
                Array.Empty<TranslatesView.Translator>(),
                Array.Empty<TranslatesView.Episode>()
            );
        }

        var allEpisodeTranslators = await _translationRepository.QueryAsync(
            new[] { contentId },
            episodeId.HasValue ? new[] { episodeId.Value } : null,
            null,
            token);

        var translationsMap = allEpisodeTranslators
            .GroupBy(t => t.EpisodeId)
            .ToDictionary(t => t.Key);

        var episodeUnits = episodes.Select(model =>
        {
            translationsMap.TryGetValue(model.Id, out var episodeTranslations);

            var episodeTranslation =
                (episodeTranslations?.ToArray() ?? Array.Empty<BaseTranslation>())
                .Where(t => translatorId == null || t.TranslatorId == translatorId)
                .Select(t => new TranslatesView.EpisodeTranslation(
                    t.TranslationId,
                    t.EpisodeId,
                    t.Language,
                    t.Link,
                    t.TranslationType,
                    t.CreatedAt,
                    t.CreatedBy,
                    t.Quality))
                .ToArray();

            return new TranslatesView.Episode(
                model.Id,
                model.Title,
                model.Number,
                model.Views,
                model.Stars,
                episodeTranslation);
        }).ToArray();
        
        var translators = allEpisodeTranslators
            .GroupBy(t => t.TranslatorId)
            .Select(x => new TranslatesView.Translator(x.First().TranslatorId, x.First().TranslatorName))
            .ToArray();

        return new TranslatesView(translators, episodeUnits);
    }

    public async Task<EpisodeTranslation[]> QueryAsync(EpisodeTranslationQuery request, CancellationToken token)
    {
        var query = await _translationRepository.Select(new Select
        {
            EpisodeTranslationIds = request.EpisodeTranslationIds,
            TranslatorIds = request.TranslatorIds,
            ContentIds = request.ContentIds,
            EpisodeIds = request.EpisodeIds,
            Limit = request.Limit,
            Offset = request.Offset
        }, token);

        return query
            .Select(x =>
                new EpisodeTranslation(
                    x.Id,
                    x.ContentId,
                    x.EpisodeId,
                    x.Link,
                    (TranslationType)x.TranslationType,
                    x.CreatedAt,
                    x.CreatedBy,
                    x.Quality,
                    x.TranslatorId,
                    (Language)x.Lang))
            .ToArray();
    }

    public async Task<long> InsertAsync(InsertTranslation model, CancellationToken token)
    {
        var episodes = await _episodeRepository.QueryAsync(
            new DAL.Models.QueryEpisode() { EpisodeIds = new[] { model.EpisodeId } },
            token);
        if (episodes?.Any() is false)
        {
            throw new EpisodeNotFoundException(model.EpisodeId);
        }

        var contents = await _contentRepository.QueryAsync(
            new DAL.Models.QueryContent() { Ids = new[] { model.ContentId } },
            token);
        if (contents?.Any() is false){
            throw new ContentNotFoundException(model.ContentId);
        }

        var translators = await _translatorsRepository.QueryAsync(new[] {model.TranslatorId}, token);
        if (translators?.Any() is false) {
            throw new TranslatorNotFoundException(model.TranslatorId);
        }

        var translationId = await _translationRepository.InsertAsync(new DAL.Models.InsertTranslation(
                model.ContentId,
                model.EpisodeId,
                model.TranslatorId,
                model.Lang,
                model.Link,
                model.TranslationType,
                model.CreatedAt,
                model.CreatedBy,
                model.Quality
        ), token);

        return translationId;
    }
}