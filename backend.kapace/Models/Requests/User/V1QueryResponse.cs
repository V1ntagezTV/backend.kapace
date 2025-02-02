namespace backend.kapace.Models.Requests.User;

public record V1QueryResponse(V1QueryResponse.User[] Users)
{
    public record User(
        long UserId,
        string Nickname,
        string Email,
        DateTimeOffset CreatedAt);
}