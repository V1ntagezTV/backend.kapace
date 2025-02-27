namespace backend.kapace.Models.Requests.User;

public record V1UpdatePasswordRequest(string OldPassword, string NewPassword);