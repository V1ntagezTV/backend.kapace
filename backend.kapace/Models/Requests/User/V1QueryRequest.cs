namespace backend.kapace.Models.Requests.User;

public record V1QueryRequest(
    long[]? UserIds,
    string[]? Nicknames,
    string[]? Emails);