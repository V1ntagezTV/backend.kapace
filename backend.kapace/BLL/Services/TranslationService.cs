using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Exceptions;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Repository.Interfaces;

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
    
    public async Task<IReadOnlyCollection<EpisodeTranslation>> GetByEpisodeAsync(
        long episodeId,
        long? translationId,
        CancellationToken token)
    {
        var episodes = await _translationRepository.QueryAsync(
            new[] { episodeId },
            translationId.HasValue ? new[] { translationId.Value } : Array.Empty<long>(),
            token);
        
        if (!episodes.Any())
        {
            return Array.Empty<EpisodeTranslation>();
        }

        return episodes
            ?.Select(x => new EpisodeTranslation(
                x.Id,
                x.EpisodeId,
                (Language)x.Lang,
                x.Link,
                x.TranslationType,
                x.CreatedAt,
                x.CreatedBy,
                x.Quality,
                x.Translator != null
                    ? new EpisodeTranslation.WithTranslator(
                        x.Translator.TranslatorId,
                        x.Translator.Name,
                        x.Translator.TranslatorLink)
                    : null,
                x.Episode != null
                    ? new EpisodeTranslation.WithEpisode(
                        x.EpisodeId,
                        x.Episode.Title,
                        x.Episode.Number,
                        x.Episode.Views,
                        x.Episode.Stars)
                    : null
                ))
        .ToArray()
        ?? Array.Empty<EpisodeTranslation>();
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