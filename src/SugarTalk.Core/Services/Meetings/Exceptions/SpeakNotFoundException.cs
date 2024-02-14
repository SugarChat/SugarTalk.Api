using System;

namespace SugarTalk.Core.Services.Meetings.Exceptions;

public class SpeakNotFoundException : Exception
{
    public SpeakNotFoundException() : base("Speak not found")
    {
    }
}