﻿using backend.kapace.DAL.Models;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface IEpisodeRepository
{
    Task<Episode[]> QueryAsync(QueryEpisode contentId, CancellationToken token);

    Task<long> InsertAsync(Episode model, CancellationToken token);
    
    Task UpdateAsync(Episode episode, CancellationToken token);
}