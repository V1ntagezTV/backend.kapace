namespace backend.kapace.BLL.Models.Cache;

internal record PasswordResetCacheValue(int Code, DateTimeOffset CreatedAt)
{
    public int AttemptsLeft { get; set; } = 3;
}