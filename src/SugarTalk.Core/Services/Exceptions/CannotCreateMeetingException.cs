using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotCreateMeetingException : Exception
{
    public CannotCreateMeetingException() : base("Meeting cannot be created")
    {
    }
}
