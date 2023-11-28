using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_user_setting")]
public class MeetingUserSetting : IEntity
{
    public MeetingUserSetting()
    {
        LastModifiedDate = DateTimeOffset.Now;
    }

    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Column("user_id")] 
    public int UserId { get; set; }

    [Column("listened_language_type")] 
    public VoiceSamplesByLanguageType ListenedLanguageType { get; set; } 

    [Column("target_language_type")] 
    public SpeechTargetLanguageType TargetLanguageType { get; set; } 
    
    [Column("last_modified_date")]
    public DateTimeOffset LastModifiedDate { get; set; }
}