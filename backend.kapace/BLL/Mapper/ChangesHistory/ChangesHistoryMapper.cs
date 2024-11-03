using backend.kapace.BLL.Models;
using backend.kapace.BLL.Models.HistoryChanges;
using backend.kapace.BLL.Models.VideoService;

namespace backend.kapace.BLL.Mapper.ChangesHistory;

internal static class ChangesHistoryMapper
{
    internal static HistoryChangesComparisons MapEpisodeUnitToChangesComparison(
        this HistoryUnit unit, 
        IReadOnlyDictionary<long, Models.Episode.Episode> episodesMap)
    {
        var model = unit.Changes as HistoryUnit.JsonEpisodeChanges;
        var fieldsComparisons = new Dictionary<string, (string oldValue, string newValue)>();

        Models.Episode.Episode? episode = null;
        _ = unit.TargetId.HasValue && episodesMap.TryGetValue(unit.TargetId.Value, out episode);

        // Эпизод
        AddChange(fieldsComparisons, "Название", model?.Title, episode?.Title);
        AddChange(fieldsComparisons, "Серия", model?.Number, episode?.Number);
        AddChange(fieldsComparisons, "Изображение", model?.ImageId, episode?.ImageId);
        // Перевод
        AddChange(fieldsComparisons, "Тип перевода", model?.TranslationType);
        AddChange(fieldsComparisons, "Язык", model?.Language);
        AddChange(fieldsComparisons, "Качество", model?.Quality);
        AddChange(fieldsComparisons, "Видео", model?.VideoScript);

        return new HistoryChangesComparisons
        {
            HistoryId = unit.Id,
            TargetId = unit.TargetId,
            Title = model?.Title ?? episode?.Title ?? default,
            ImageId = model?.ImageId,
            HistoryType = unit.HistoryType,
            FieldsComparisons = fieldsComparisons,
            CreatedBy = unit.CreatedBy,
            CreatedAt = unit.CreatedAt,
            ApprovedBy = unit.ApprovedBy,
            ApprovedAt = unit.ApprovedAt
        };
    }

    internal static HistoryChangesComparisons MapContentUnitToChangesComparison(
        this HistoryUnit unit, 
        IReadOnlyDictionary<long, Content> contentsMap)
    {
        var model = unit.Changes as HistoryUnit.JsonContentChanges;
        var fieldsComparisons = new Dictionary<string, (string newValue, string oldValue)>();

        Content? content = null;
        _ = unit.TargetId.HasValue && !contentsMap.TryGetValue(unit.TargetId.Value, out content);

        AddChange(fieldsComparisons, "Изображение", model?.ImageId, content?.ImageId);
        AddChange(fieldsComparisons, "Название", model?.Title, content?.Title);
        AddChange(fieldsComparisons, "EN-название", model?.EngTitle, content?.EngTitle);
        AddChange(fieldsComparisons, "Оригинальное имя", model?.OriginTitle, content?.OriginTitle);
        AddChange(fieldsComparisons, "Канал", model?.Channel, content?.Channel);
        AddChange(fieldsComparisons, "Статус", model?.Status, content?.Status);
        AddChange(fieldsComparisons, "Страна", model?.Country, content?.Country );
        AddChange(fieldsComparisons, "Тип", model?.ContentType, content?.ContentType);
        AddChange(fieldsComparisons, "Длительность", model?.Duration, content?.Duration);
        AddChange(fieldsComparisons, "Дата выпуска", model?.ReleasedAt, content?.ReleasedAt);
        AddChange(fieldsComparisons, "Запланировано серий", model?.PlannedSeries, content?.PlannedSeries);
        AddChange(fieldsComparisons, "Возраст", model?.MinAge, content?.MinAge);
        AddChange(fieldsComparisons, "Описание", model?.Description, content?.Description);

        return new HistoryChangesComparisons
        {
            HistoryId = unit.Id,
            TargetId = unit.TargetId,
            Title = model?.Title ?? content?.Title ?? default,
            ImageId = model?.ImageId,
            HistoryType = unit.HistoryType,
            FieldsComparisons = fieldsComparisons,
            CreatedBy = unit.CreatedBy,
            CreatedAt = unit.CreatedAt,
            ApprovedBy = unit.ApprovedBy,
            ApprovedAt = unit.ApprovedAt
        };
    }
    
    private static void AddChange<TType>(
        IDictionary<string, (string newValue, string oldValue)> changes, 
        string name, 
        TType? newValue,
        TType? oldValue = default)
    {
        var newValueStr = newValue?.ToString() ?? "";
        var oldValueStr = oldValue?.ToString() ?? "";
        
        if (newValueStr != oldValueStr)
            changes.Add(name, (newValueStr, oldValueStr));
    }
}