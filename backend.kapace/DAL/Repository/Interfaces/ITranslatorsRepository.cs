using backend.kapace.DAL.Models;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface ITranslatorsRepository
{
    Task<IReadOnlyCollection<Translator>> QueryAsync(long[] ids, CancellationToken token);
    Task<long> InsertAsync(Translator translator, CancellationToken token); 
}