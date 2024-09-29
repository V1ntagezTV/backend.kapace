using backend.kapace.BLL.Models.Episode;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Controllers;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using Episode = backend.kapace.BLL.Models.Episode.Episode;

namespace backend.kapace.BLL.Services;

public class EpisodeService : IEpisodeService
{
    private readonly IEpisodeRepository _episodeRepository;

    public EpisodeService(IEpisodeRepository episodeRepository)
    {
        _episodeRepository = episodeRepository;
    }
    public async Task<Episode[]> QueryAsync(EpisodeQuery query, CancellationToken token)
    {
        var episodes = await _episodeRepository.QueryAsync(new QueryEpisode
        {
            EpisodeIds = query.EpisodeIds,
            ContentIds = query.ContentIds,
            Limit = query.Limit,
            Offset = query.Offset,
        }, token);

        return episodes
            .Select(x => new Episode(
                x.Id,
                x.ContentId,
                x.Title,
                x.Image,
                x.Number,
                x.Views,
                x.Stars,
                x.CreatedAt,
                x.CreatedBy))
            .ToArray();
    }

    public async Task IncrementViews(long episodeId, CancellationToken token)
    {
        await _episodeRepository.IncrementViews(episodeId, token);
    }
}