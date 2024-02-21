using System;

namespace SugarTalk.Core.Services.Meetings.Exceptions;

public class FailedMeetingSummaryException : Exception
{
    public FailedMeetingSummaryException() : base("Failed summary meeting")
    {
    }
}