using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotChangeAudioWhenConfirmRequiredException : Exception
{
    public CannotChangeAudioWhenConfirmRequiredException() : base("The current user cannot change audio and needs to confirm it")
    {
    }
}