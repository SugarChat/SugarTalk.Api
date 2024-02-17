using System;

namespace SugarTalk.Core.Services.Exceptions;

public class MeetingRecordUrlNotFoundException:Exception
{
    public MeetingRecordUrlNotFoundException():base("The Url returned by liveKitClient could not be obtained")
    {
        
    }
}