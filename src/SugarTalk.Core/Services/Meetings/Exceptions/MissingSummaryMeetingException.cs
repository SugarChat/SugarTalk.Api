using System;

namespace SugarTalk.Core.Services.Meetings.Exceptions;

public class MissingSummaryMeetingException : Exception
{
    public MissingSummaryMeetingException() : base("Missing summary meeting")
    {
    }
}