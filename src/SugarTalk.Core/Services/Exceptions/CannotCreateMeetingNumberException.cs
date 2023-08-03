using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotCreateMeetingNumberException : Exception
{
    public CannotCreateMeetingNumberException() : base("Cannot to create meeting, need to expand meeting number capacity")
    {
    }
}