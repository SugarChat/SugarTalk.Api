using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_problem_feedback")]
public class MeetingProblemFeedback : IEntity, IHasCreatedFields, IHasModifiedFields
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("category")]
    public MeetingCategoryType Category { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("is_new")]
    public bool IsNew { get; set; }
    
    [Column("created_by")] 
    public int CreatedBy { get; set; }

    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }

    [Column("last_modified_by")] 
    public int LastModifiedBy { get; set; }
    
    [Column("last_modified_date")]
    public DateTimeOffset LastModifiedDate { get; set; }
}