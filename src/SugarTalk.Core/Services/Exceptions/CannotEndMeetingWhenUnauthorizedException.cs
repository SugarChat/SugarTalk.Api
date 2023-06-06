using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotEndMeetingWhenUnauthorizedException : Exception
{
    public CannotEndMeetingWhenUnauthorizedException() : base("The user is not master of meeting")
    {
    }
}