using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotShareWhenConfirmRequiredException : Exception
{
    public CannotShareWhenConfirmRequiredException() : base("The current user is not a sharer and needs to confirm")
    {
    }
}