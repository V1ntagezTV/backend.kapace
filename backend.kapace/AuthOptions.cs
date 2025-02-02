using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace backend.kapace;

internal static class AuthOptions
{
    public const string Issuer = "Kapace.Backend"; // издатель токена
    public const string Audience = "Kapace.Frontend"; // потребитель токен!а
    private const string Key = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";   // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(Key));
}