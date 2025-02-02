namespace backend.kapace.DAL.Models.Query;

public class UserQuery
{
    public string[] Nicknames { get; init; }
    public string[] Emails { get; init; }
    public long[] UserIds { get; init; }
}