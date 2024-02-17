using System;

namespace SugarTalk.Core.Services.Exceptions;

public class MeetingRecordNotOpenException:Exception
{
    public MeetingRecordNotOpenException():base("The meeting record not opened")
    {
        
    }
}