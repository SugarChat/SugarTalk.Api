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
        SpanishToneType = SpanishToneType.ElviraNeural;
        CantoneseToneType = CantoneseToneType.XiaoMinNeural;
        EnglishToneType = EnglishToneType.AmberNeural;
        MandarinToneType = MandarinToneType.XiaochenNeural;
        TargetLanguageType = SpeechTargetLanguageType.Cantonese;
    }

    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }
    
    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }

    [Column("user_id")] 
    public int UserId { get; set; }
    
    [Column("user_entity_id", TypeName = "char(36)")]
    public Guid UserEntityId { get; set; }

    [Column("spanish_tone_type")] 
    public SpanishToneType SpanishToneType { get; set; }
    
    [Column("mandarin_tone_type")] 
    public MandarinToneType MandarinToneType { get; set; }
    
    [Column("english_tone_type")] 
    public EnglishToneType EnglishToneType{ get; set; }
    
    [Column("cantonese_tone_type")] 
    public CantoneseToneType CantoneseToneType { get; set; }

    [Column("target_language_type")] 
    public SpeechTargetLanguageType TargetLanguageType { get; set; } 
    
    [Column("last_modified_date")]
    public DateTimeOffset LastModifiedDate { get; set; }
}