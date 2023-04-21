using System;

namespace SugarTalk.Core.Domain.Account.Exceptions;

public class CannotRegisterWhenExistTheSameUserAccountException : Exception
{
    public CannotRegisterWhenExistTheSameUserAccountException() : base("Can not Register Because Existing The Same User Account Exception")
    {
    }
}