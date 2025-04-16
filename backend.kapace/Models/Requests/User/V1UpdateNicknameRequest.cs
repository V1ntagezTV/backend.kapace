namespace backend.kapace.Models.Requests.User;

public record V1UpdateNicknameRequest(string NewNickname, bool IsForce);