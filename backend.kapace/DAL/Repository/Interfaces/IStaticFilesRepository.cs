using backend.kapace.BLL.Enums;
using backend.kapace.DAL.Models;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface IStaticFilesRepository
{
    Task<long> InsertAsync(string fileName, long linkId, StaticFileLinkType linkType, DateTimeOffset createdAt,
        CancellationToken token);

    Task<StaticFile> GetByIdAsync(long imageId, CancellationToken token);
}