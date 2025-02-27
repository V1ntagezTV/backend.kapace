namespace backend.kapace.DAL.Models;

public class User
{
    public long Id { get; set; }
    public string Nickname { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
    public bool IsMailVerified { get; set; }
}
