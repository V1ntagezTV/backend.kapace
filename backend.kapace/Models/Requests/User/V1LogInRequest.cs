namespace backend.kapace.Models.Requests.User;

public record V1LogInRequest(
    string Email,
    string Password,
    bool? IsRememberMe);

public record UpdateMailVerifyCodeRequest(string Code);

public record UpdateMailRequest(string NewMail);

public record UpdateMailVerifyNewMailRequest(string NewMail, string Code);