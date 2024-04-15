using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotUpdateMeetingWhenStatusNotInProgressException : Exception
{
    public CannotUpdateMeetingWhenStatusNotInProgressException() : base("The meeting status is not inprogress when update meeting.")
    {
    }
}