using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotJoinMeetingWhenKickedOutMeetingException : Exception
{
    public CannotJoinMeetingWhenKickedOutMeetingException(string userId) : base(
        $"The current user [userId:{userId}] has been kicked out of the meeting by the creator and cannot join the meeting again")
    {
    }
}