using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Account;

[Table("user_account_api_key")]
public class UserAccountApiKey : IEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("user_account_id")]
    public int UserAccountId { get; set; }
    
    [Column("api_key")]
    [StringLength(128)]
    public string ApiKey { get; set; }
    
    [Column("description")]
    [StringLength(256)]
    public string Description { get; set; }
}