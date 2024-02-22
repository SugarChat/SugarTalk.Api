using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotCreateRepeatMeetingWhenUtilDateIsBeforeNowException : Exception
{
    public CannotCreateRepeatMeetingWhenUtilDateIsBeforeNowException(): base(
        "Cannot create repeat meeting when util date is before now.")
    {
    }
}