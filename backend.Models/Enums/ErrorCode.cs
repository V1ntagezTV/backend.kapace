namespace backend.Models.Enums;

public enum ErrorCode
{
    UserNotFound = 1,
    UserMailAlreadyVerified = 2,
    MailVerificationError = 3,
    NicknameAlreadyExists = 4,
    PasswordResetForbidden = 5,
    UserPasswordMustBeDifferentThanOld = 6,
    PasswordResetError = 7
}