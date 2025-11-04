using System;

namespace MarkBartha.HarvestDemo.App.Maui.Exceptions;

public class TodoServiceException : Exception
{
    public TodoServiceException(string message) : base(message)
    {
    }

    public TodoServiceException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}
