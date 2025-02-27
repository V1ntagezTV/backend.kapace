using backend.Models.Enums;

namespace backend.kapace.BLL.Exceptions;

internal sealed class ServiceException : Exception
{
    public ErrorCode ErrorCode { get; }

    public Dictionary<string, string> ErrorDetails { get; } = new();

    internal ServiceException(ErrorCode errorCode, string message = "") : base(message)
    {
        ErrorCode = errorCode;
    }

    internal ServiceException SetData(string key, string value)
    {
        ErrorDetails.TryAdd(key, value);

        return this;
    }
}