using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Domain.Meeting
{
    [Table("meeting")]
    public class Meeting: IEntity
    {
        public Meeting()
        {
            CreatedDate = DateTimeOffset.Now;
        }
        
        [Key]
        [Column("id", TypeName = "char(36)")]
        public Guid Id { get; set; }
        
        [Column("created_date")]
        public DateTimeOffset CreatedDate { get; set; }
        
        [Column("meeting_number"), StringLength(256)]
        public string MeetingNumber { get; set; }
            
        [Column("origin_address")]
        public string OriginAddress { get; set; }
        
        [Column("meeting_stream_mode")]
        public MeetingStreamMode MeetingStreamMode { get; set; }
    }
}