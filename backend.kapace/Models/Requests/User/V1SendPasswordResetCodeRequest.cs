namespace backend.kapace.Models.Requests.User;

public record V1SendPasswordResetCodeRequest(string Email);

public record V1VerifyPasswordResetRequest(string Email, int Code);