namespace backend.kapace.DAL.Repository.Interfaces;

public interface IVerificationCodeRepository
{
    Task<IReadOnlyCollection<VerificationCode>> Query(VerificationCodeQuery query, CancellationToken token);
    Task<long[]> Insert(IReadOnlyCollection<VerificationCode> insertModels, CancellationToken token);
    Task Update(VerificationCode updateModel, CancellationToken token);
    Task ClearTable(long userId, TimeSpan olderThan, VerificationCodeType codeType, CancellationToken token);
}

public enum VerificationCodeType
{
    MailApprove = 1,
    PasswordReset = 2,
    MailUpdate = 3,
    NewMailApprove = 4
}

public class VerificationCodeQuery
{
    public long? Id { get; set; }
    public long? UserId { get; init; }
    public VerificationCodeType? Type { get; init; }
    public bool? IsUsed { get; init; }
    public DateTimeOffset? CreatedAfter { get; init; }
}

public class VerificationCode
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string CodeHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public int Attempts { get; set; }
    public int Type { get; set; }
    public string Email { get; set; }
}