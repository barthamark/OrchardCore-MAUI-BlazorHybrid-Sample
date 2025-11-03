using System;
using System.Net;

namespace MarkBartha.HarvestDemo.App.Maui.Services.UserProfiles;

public class UserProfileServiceException : Exception
{
    public UserProfileServiceException(string message)
        : base(message)
    {
    }

    public UserProfileServiceException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public HttpStatusCode? StatusCode { get; init; }
}
