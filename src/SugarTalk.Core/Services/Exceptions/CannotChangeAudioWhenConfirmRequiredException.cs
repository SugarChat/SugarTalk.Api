using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotChangeAudioWhenConfirmRequiredException : Exception
{
    public CannotChangeAudioWhenConfirmRequiredException() : base("The current user is not a sharer and needs to confirm")
    {
    }
}