namespace backend.kapace.Models.Requests.User;

public record V1ResetPasswordRequest(string Email, string NewPassword);