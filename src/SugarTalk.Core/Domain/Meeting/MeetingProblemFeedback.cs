using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_problem_feedback")]
public class MeetingProblemFeedback : IEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("creator"), StringLength(128)]
    public string Creator { get; set; }

    [Column("category")]
    public MeetingCategoryType Category { get; set; }

    [Column("description"), StringLength(2048)]
    public string Description { get; set; }

    [Column("created_by")] 
    public int CreateBy { get; set; }
    
    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }

    [Column("is_new")]
    public bool IsNew { get; set; }
}