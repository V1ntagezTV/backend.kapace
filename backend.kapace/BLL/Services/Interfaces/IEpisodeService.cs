using backend.kapace.BLL.Models.Episode;
using backend.kapace.Controllers;

namespace backend.kapace.BLL.Services.Interfaces;

public interface IEpisodeService
{
    Task<Episode[]> QueryAsync(EpisodeQuery query, CancellationToken token);
}