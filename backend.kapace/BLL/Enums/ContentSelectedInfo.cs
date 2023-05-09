namespace backend.kapace.BLL.Enums;

[Flags]
public enum ContentSelectedInfo
{
    None = 0,
    Episodes = 1,
    ContentGenres = 2,
    UserInfo = 4,
    Translations = 8,
}