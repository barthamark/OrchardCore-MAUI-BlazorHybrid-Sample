namespace MarkBartha.HarvestDemo.App.Maui.Services.Todos;

public class TodoServiceException : Exception
{
    public TodoServiceException(string message) : base(message)
    {
    }

    public TodoServiceException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}
