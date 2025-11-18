using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Account;

[Table("user_account_profile")]
public class UserAccountProfile : IEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("user_account_id")]
    public int UserAccountId { get; set; }

    [Column("url")]
    public string Url { get; set; }

    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
}