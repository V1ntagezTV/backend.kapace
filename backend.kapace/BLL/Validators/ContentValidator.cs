using backend.kapace.BLL.Services.Interfaces;

namespace backend.kapace.BLL.Validators;

internal static class ContentValidator
{
    internal static void ValidateContentRequiredProperties(HistoryUnit.JsonContentChanges jsonContentChanges)
    {
        MustBeNotNull(
            jsonContentChanges.Image,
            jsonContentChanges.Description,
            jsonContentChanges.Title
        );
        
        MustBeNotNull(jsonContentChanges.Country);
        MustBeNotNull(jsonContentChanges.ContentType);
    }
    
    private static void MustBeNotNull<T>(params T?[] properties)
    {
        if (properties is null)
            throw new ArgumentException($"Property must be initialized: {nameof(properties)}");

        foreach (var property in properties)
        {
            if (property is null)
                throw new ArgumentException($"Property must be initialized: {nameof(property)}");
        }
    }
}