using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_invitation_record")]
public class MeetingInvitationRecord : IEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("meeting_id")]
    public Guid MeetingId { get; set; }

    [Column("meeting_sub_id")] 
    public Guid? MeetingSubId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("user_name")]
    public string UserName { get; set; }

    [Column("inviter_id")]
    public int BeInviterUserId { get; set; }

    [Column("invitation_status")]
    public MeetingInvitationStatus? InvitationStatus { get; set; }

    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
}