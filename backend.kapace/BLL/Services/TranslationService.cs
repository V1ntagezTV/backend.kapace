using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Exceptions;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Models.EpisodeTranslations;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.ContentTranslation.Query;
using backend.kapace.DAL.Repository.Interfaces;
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
        long? translationId,
        CancellationToken token)
    {
        var episodeTranslations = await _translationRepository.QueryAsync(
            new[] { contentId },
             episodeId.HasValue ? new[] { episodeId.Value } : Array.Empty<long>(),
            translationId.HasValue ? new[] { translationId.Value } : Array.Empty<long>(),
            token);
        
        if (!episodeTranslations.Any())
        {
            return new TranslatesView(
                Array.Empty<TranslatesView.Translator>(),
                Array.Empty<TranslatesView.Episode>()
            );
        }

        var translators = new Dictionary<long, TranslatesView.Translator>();
        var translationsByEpisodeNumber = new Dictionary<long, List<Translation>>();
        foreach (var translation in episodeTranslations)
        {
            translators.TryAdd(
                translation.TranslatorId,
                new TranslatesView.Translator(
                    translation.TranslatorId, 
                    translation.Name)
                );

            if (!translationsByEpisodeNumber.TryAdd(translation.Number, new() {translation}))
            {
                translationsByEpisodeNumber[translation.Number].Add(translation);
            }
        }
        
        var episodesByNumber = new List<TranslatesView.Episode>();
        foreach (var (_, translations) in translationsByEpisodeNumber)
        {
            var episodeTranslation = translations
                .Select(x =>
                    new TranslatesView.EpisodeTranslation(
                        x.Id,
                        x.EpisodeId,
                        x.Lang,
                        x.Link,
                        x.TranslationType,
                        x.CreatedAt,
                        x.CreatedBy,
                        x.Quality))
                .ToArray();

            var episodeInfoSource = translations.First();
            var episode = new TranslatesView.Episode(
                episodeInfoSource.EpisodeId,
                episodeInfoSource.EpisodeTitle,
                episodeInfoSource.Number,
                episodeInfoSource.EpisodeViews,
                episodeInfoSource.EpisodeStars,
                episodeTranslation);

            episodesByNumber.Add(episode);
        }

        return new TranslatesView(translators.Values.ToArray(), episodesByNumber.ToArray());
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