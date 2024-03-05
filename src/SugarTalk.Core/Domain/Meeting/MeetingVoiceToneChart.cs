using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_voice_tone_chart")]
public class MeetingVoiceToneChart : IEntity
{
    public MeetingVoiceToneChart()
    {
        CreatedDate = DateTimeOffset.Now;
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("setting_id")]
    public int SettingId { get; set; }

    [Column("voice_id")]
    public string VoiceId { get; set; }

    [Column("language_id")]
    public string LanguageId { get; set; }

    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
}