using System;

namespace SugarTalk.Core.Services.Exceptions;

public class MeetingCreatedException : Exception
{
    public MeetingCreatedException() : base("Meeting cannot be created")
    {
    }
}
