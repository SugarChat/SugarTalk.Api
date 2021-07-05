using System;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Messages.Dtos
{
    public class MeetingDto
    {
        public Guid Id { get; set; }
        
        public MeetingType MeetingType { get; set; }
    }
}