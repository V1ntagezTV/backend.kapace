using backend.kapace.BLL.Enums;

namespace backend.kapace.BLL.Models;

public record HistoryListUnit(
    long HistoryId,
    long? TargetId,
    string? Title,
    HistoryType HistoryType,
    string Text,
    long? CreatedBy,
    DateTimeOffset CreatedAt,
    long? ApprovedBy,
    DateTimeOffset? ApprovedAt);