namespace backend.kapace.BLL.Exceptions;

public class UserNotFoundException : Exception
{
    private readonly long _currentUserId;

    public UserNotFoundException(long currentUserId)
    {
        _currentUserId = currentUserId;
    }
}