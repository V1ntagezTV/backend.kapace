using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface ITranslatorsRepository
{
    Task<IReadOnlyCollection<Translator>> QueryAsync(TranslatorQuery query, CancellationToken token);
    Task<IReadOnlyCollection<Translator>> QueryAsync(long[] ids, CancellationToken token);
    Task<long> InsertAsync(Translator translator, CancellationToken token); 
}