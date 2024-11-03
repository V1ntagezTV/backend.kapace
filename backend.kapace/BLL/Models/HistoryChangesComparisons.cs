using backend.kapace.BLL.Enums;

namespace backend.kapace.BLL.Models;

public class HistoryChangesComparisons
{
    public long HistoryId { get; init; }
    public long? TargetId { get; init; }
    public string? Title { get; init; }
    public long? ImageId { get; init; }
    public HistoryType HistoryType { get; init; }
    public Dictionary<string, (string newValue, string oldValue)> FieldsComparisons { get; init; }
    public long? CreatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public long? ApprovedBy { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
};