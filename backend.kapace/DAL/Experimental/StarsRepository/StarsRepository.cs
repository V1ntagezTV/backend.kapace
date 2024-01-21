using Npgsql;

namespace backend.kapace.DAL.Experimental.StarsRepository;

public class StarsDataColumns : BaseDataColumns
{
    public long UserId { get; set; }
    public long TargetId { get; set; }
    public int Value { get; set; }

    internal override string[] GetColumnNames()
    {
        return new[] { nameof(Id), nameof(UserId), nameof(TargetId), nameof(Value), nameof(CreatedAt) };
    }
}

public class StarsRepository : BaseRepository<StarsDataColumns>
{
    public StarsRepository(NpgsqlDataSource npgsqlDataSource)
        : base("stars",npgsqlDataSource)
    {
        
    }
}