using System;

namespace SugarTalk.Core.Services.Exceptions;

public class NoFoundMeetingUserSettingForCurrentUserException : Exception
{
    public NoFoundMeetingUserSettingForCurrentUserException(int userId):
        base($"No found meeting user setting for current user: {userId}")
    {
    }
}