namespace backend.kapace.BLL.Models.Cache;

internal record MailVerificationCacheValue(int Code, DateTimeOffset CreatedAt)
{
    public int AttemptsLeft { get; set; } = 3;
}