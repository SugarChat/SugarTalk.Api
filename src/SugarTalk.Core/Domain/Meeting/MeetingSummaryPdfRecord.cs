using System;
using SugarTalk.Messages.Dto.Translation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_summary_pdf_record")]
public class MeetingSummaryPdfRecord : IEntity, IHasCreatedFields
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("summary_id")] 
    public int SummaryId { get; set; }

    [Column("target_language")]
    public TranslationLanguage TargetLanguage { get; set; }
    
    [Column("pdf_url")]
    public string PdfUrl { get; set; }

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
}