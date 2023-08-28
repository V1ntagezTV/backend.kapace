using backend.kapace.BLL.Enums;

namespace backend.kapace.DAL.Models;

public sealed class HistoryUnit
{
    public long Id { get; set; }
    public long? TargetId { get; set; }
    public HistoryType HistoryType { get; set; }
    public string Text { get; set; }
    public long CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}
