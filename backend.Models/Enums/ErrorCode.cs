namespace backend.Models.Enums;

public enum ErrorCode
{
    UserNotFound,
    NicknameAlreadyExists,
    
    PasswordResetForbidden,
    PasswordMustBeDifferentThanOld,
    PasswordResetError,
    
    VerificationCodeExpired,
    VerificationCodeNotFound,
    VerificationCodeAttemptsLimitExceeded,
    VerificationCodeError,
    VerificationCodeDayLimitExceeded,
    
    EmailAlreadyVerified,
    EmailVerificationError,
    EmailUpdateError,
    EmailAlreadyUsed,
    
    WrongInputCode,
    CallWithoutForceFlag,
}