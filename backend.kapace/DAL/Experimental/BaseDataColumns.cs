namespace backend.kapace.DAL.Experimental;

public abstract class BaseDataColumns
{
    internal long Id { get; set; }
    internal DateTimeOffset CreatedAt { get; set; }
    
    internal abstract string[] GetColumnNames();
}