namespace backend.kapace.BLL.Exceptions;

public class ChangesAlreadyApprovedException : Exception
{
    public long ApprovedBy { get; }

    public ChangesAlreadyApprovedException(long approvedBy)
    {
        ApprovedBy = approvedBy;
    }
}