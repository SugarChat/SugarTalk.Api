using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Account;

[Table("role_user")]
public class RoleUser : IEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("created_on")]
    public DateTime CreatedOn { get; set; }
    
    [Column("modified_on")]
    public DateTime ModifiedOn { get; set; }
    
    [Column("uuid", TypeName = "varchar(36)")]
    public Guid Uuid { get; set; }
    
    [Column("role_id")]
    public int RoleId { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
}