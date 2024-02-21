using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotStartMeetingRecordingException : Exception
{
    public CannotStartMeetingRecordingException(int? userId) : base(
        $"The current user [userId: {userId}] does not have the permission to start meeting recording.")
    {
    }
}