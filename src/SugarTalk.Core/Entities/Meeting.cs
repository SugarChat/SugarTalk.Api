using System;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Entities
{
    public class Meeting: IEntity
    {
        public Meeting()
        {
            CreatedDate = DateTimeOffset.Now;
        }
        
        public Guid Id { get; set; }
        
        public DateTimeOffset CreatedDate { get; set; }
        
        public MeetingType MeetingType { get; set; }
    }
}