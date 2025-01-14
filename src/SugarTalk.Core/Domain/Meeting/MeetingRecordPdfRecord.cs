using System;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Dto.Translation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_record_pdf_record")]
public class MeetingRecordPdfRecord : IEntity, IHasCreatedFields
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("record_id")] 
    public Guid RecordId { get; set; }
    
    [Column("pdf_export_type")]
    public PdfExportType PdfExportType { get; set; }

    [Column("target_language")]
    public TranslationLanguage TargetLanguage { get; set; }
    
    [Column("pdf_url")]
    public string PdfUrl { get; set; }

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
}