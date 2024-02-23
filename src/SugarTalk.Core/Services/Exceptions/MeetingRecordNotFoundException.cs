using System;

namespace SugarTalk.Core.Services.Exceptions;

public class MeetingRecordNotFoundException:Exception
{
    public MeetingRecordNotFoundException() : base("Meeting Record not found")
    {
    }
}