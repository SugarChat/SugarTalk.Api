using System;

namespace SugarTalk.Core.Services.Exceptions;

public class MeetingSecurityCodeException : Exception
{
    public MeetingSecurityCodeException() : base(
        "The current meeting password is empty or not matched, please input correct meeting password.")
    {
    }
}