namespace backend.kapace;

internal static class Constants
{
    internal const int MailVerificationCodeExpirationInMinutes = 5;
    internal const int MailUpdateCodeExpirationInMinutes = 5;
    internal const int RestorePasswordCodeExpirationInMinutes = 5;
    internal const string ForbiddenAuthorizeClaimScope = "forbidden_authorize_claim";
    internal const string ClaimTypeTokenType = "token_type";
    internal const string ClaimTypeTokenScope = "token_scope";
}

public enum TokenTypes
{
    MailUpdateToken,
    ResetPasswordToken
}