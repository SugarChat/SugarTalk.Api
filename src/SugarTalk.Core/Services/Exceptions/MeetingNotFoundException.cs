using System;

namespace SugarTalk.Core.Services.Exceptions
{
    public class MeetingNotFoundException : Exception
    {
        public MeetingNotFoundException() : base("Meeting not found")
        {
            
        }
    }
}