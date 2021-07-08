using System;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Messages.Dtos.Meetings
{
    public class MeetingDto
    {
        public Guid Id { get; set; }
        
        public DateTimeOffset CreatedDate { get; set; }
        
        public string MeetingNumber { get; set; }
        
        public MeetingType MeetingType { get; set; }
    }
}