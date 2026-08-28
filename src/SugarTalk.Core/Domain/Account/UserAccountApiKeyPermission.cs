using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Account;

[Table("user_account_api_key_permission")]
public class UserAccountApiKeyPermission : IEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("user_account_api_key_id")]
    public int UserAccountApiKeyId { get; set; }

    [Column("permission_name")]
    [StringLength(255)]
    public string PermissionName { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; } = DateTime.Now;
}
