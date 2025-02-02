namespace backend.kapace.Models.Requests.User;

public record V1RegisterRequest(string Nickname, string Email, string Password);