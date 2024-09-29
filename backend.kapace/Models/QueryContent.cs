using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Models.VideoService;
using backend.Models.Enums;

namespace backend.kapace.Models;

/// <summary>
/// Базовая REST-модель для возвращения контента. 
/// Создана из-за наличия огромного количества полей в модели.
/// </summary>
/// <param name="Id">Идентификатор</param>
/// <param name="Title">Название (преимущественно на русском)</param>
/// <param name="EngTitle">Название на английском</param>
/// <param name="OriginTitle">Название на языке контента</param>
/// <param name="Description">Описание фильмографии</param>
/// <param name="Type">Тип контента</param>
/// <param name="Status">Статус</param>
/// <param name="ImageId">Карточка контента</param>
/// <param name="ImportStars">Оценка на внешнем сайте</param>
/// <param name="OutSeries">Вышло серий</param>
/// <param name="PlannedSeries">Запланнированно серий</param>
/// <param name="Views">Просмотров</param>
/// <param name="Country">Страна выпуска</param>
/// <param name="ReleasedAt">Дата выпуска</param>
/// <param name="CreatedAt">Дата создания</param>
/// <param name="LastUpdateAt">Дата последнего обновления</param>
/// <param name="MinAgeLimit">Минимально-допустимый возраст</param>
/// <param name="Duration">Продолжительность просмотра</param>
/// <param name="Channel">Канал выпуска</param>
public record QueryContent(
    long Id,
    string Title,
    string EngTitle,
    string OriginTitle,
    string Description,
    ContentType Type,
    ContentStatus? Status,
    long ImageId,
    decimal ImportStars,
    int OutSeries,
    int PlannedSeries,
    int Views,
    Country Country,
    DateTimeOffset? ReleasedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastUpdateAt,
    int MinAgeLimit,
    int? Duration,
    string? Channel)
{
    public static explicit operator QueryContent(GetByQueryResult content)
    {
        return new QueryContent(
            content.Id,
            content.Title,
            content.EngTitle,
            content.OriginTitle,
            content.Description,
            content.ContentType,
            content.Status,
            content.ImageId,
            content.ImportStars,
            content.OutSeries,
            content.PlannedSeries,
            content.Views,
            content.Country,
            content.ReleasedAt,
            content.CreatedAt,
            content.LastUpdateAt,
            content.MinAge,
            content.Duration,
            content.Channel);
    }
    
    public static explicit operator QueryContent(Content content)
    {
        return new QueryContent(
            content.Id,
            content.Title,
            content.EngTitle,
            content.OriginTitle,
            content.Description,
            content.ContentType,
            content.Status,
            content.ImageId,
            content.ImportStars,
            content.OutSeries,
            content.PlannedSeries,
            content.Views,
            content.Country,
            content.ReleasedAt,
            content.CreatedAt,
            content.LastUpdateAt,
            content.MinAge,
            content.Duration,
            content.Channel);
    }
    
    public static IEnumerable<QueryContent> MapArray(params Content[] content)
    {
        return content.Select(c => (QueryContent)c);
    }
}