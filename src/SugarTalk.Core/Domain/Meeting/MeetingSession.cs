using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Domain.Meeting
{
    [Table("meeting_session")]
    public class MeetingSession : IEntity
    {
        [Key]
        [Column("id", TypeName = "char(36)")]
        public Guid Id { get; set; }
        
        [Column("meeting_id", TypeName = "char(36)")]
        public Guid MeetingId { get; set; }
        
        [Column("meeting_number"), StringLength(128)]
        public string MeetingNumber { set; get; }
        
        [Column("meeting_type")]
        public MeetingType MeetingType { get; set; }
    }
}