using System;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Entities
{
    public class MeetingSession : IEntity
    {
        public Guid Id { get; set; }
        public Guid MeetingId { get; set; }
        public string MeetingNumber { set; get; }
        public MeetingType MeetingType { get; set; }
    }
}