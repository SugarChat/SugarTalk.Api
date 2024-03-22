using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_speech_setting")]
public class MeetingSpeechSetting : IEntity
{
    public MeetingSpeechSetting ()
    {
        CreatedDate = DateTimeOffset.Now;
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("voice_name")]
    public string VoiceName { get; set; }

    [Column("Created_date")]
    public DateTimeOffset CreatedDate { get; set; }
}