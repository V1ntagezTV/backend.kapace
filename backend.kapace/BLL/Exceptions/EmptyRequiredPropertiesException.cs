namespace backend.kapace.BLL.Exceptions;

public partial class ChangesHistoryService
{
    public class EmptyRequiredPropertiesException : Exception 
    {
        public string PropertyName { get; }
        
        public EmptyRequiredPropertiesException(string propertyName)
        {
            PropertyName = propertyName;
        }

        public EmptyRequiredPropertiesException() 
        {
            PropertyName = ""; 
        }
    }
}