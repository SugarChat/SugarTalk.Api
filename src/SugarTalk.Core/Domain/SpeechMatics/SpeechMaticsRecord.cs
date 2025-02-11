using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.SpeechMatics;

namespace SugarTalk.Core.Domain.SpeechMatics;

[Table("speech_matics_record")]
public class SpeechMaticsRecord : IEntity, IHasCreatedFields
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("status")]
    public SpeechMaticsStatus Status { get; set; }

    [Column("meeting_number")]
    public string MeetingNumber { get; set; }

    [Column("meeting_record_id")]
    public Guid MeetingRecordId { get; set; }

    [Column("transcription_job_id")]
    public string TranscriptionJobId { get; set; }
    
    [Column("created_date")]
    public int CreatedBy { get; set; }
    
    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
}