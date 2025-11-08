using System;
using System.Net;

namespace MarkBartha.HarvestDemo.App.Maui.Exceptions;

public class UserProfileServiceException : Exception
{
    public UserProfileServiceException(string message)
        : base(message)
    {
    }

    public UserProfileServiceException(string message, Exception innerException = null)
        : base(message, innerException)
    {
    }

    public Nullable<HttpStatusCode> StatusCode { get; init; }
}
