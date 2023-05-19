using System;

namespace SugarTalk.Core.Services.Exceptions;

public class UserMismatchException : Exception
{
    public UserMismatchException() : base("The user is not master of meeting")
    {
    }
}