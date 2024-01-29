using System;

namespace SugarTalk.Core.Services.Exceptions;

public class MeetingSecurityCodeNotMatchException : Exception
{
    public MeetingSecurityCodeNotMatchException() : base("The correct meeting password is not matched")
    {
    }
}