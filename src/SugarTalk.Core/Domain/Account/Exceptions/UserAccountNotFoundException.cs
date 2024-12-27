using System;

namespace SugarTalk.Core.Domain.Account.Exceptions;

public class UserAccountNotFoundException : Exception
{
    public UserAccountNotFoundException() : base("Can not find the user account")
    {
    }
}