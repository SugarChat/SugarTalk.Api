using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotUpdateMeetingWhenStatusNotPendingException : Exception
{
    public CannotUpdateMeetingWhenStatusNotPendingException() : base("The meeting status is not pending when update meeting.")
    {
    }
}