using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotUpdateMeetingWhenMasterUserIdMismatchException : Exception
{
    public CannotUpdateMeetingWhenMasterUserIdMismatchException() : 
        base("The meeting master user is mismatched with current user")
    {
    }
}