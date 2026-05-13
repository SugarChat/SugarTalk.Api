using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.OME;

[Table("ome_user_account")]
public class OMEUserAccount : IEntity
{
    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Column("user_name"), StringLength(255)]
    public string UserName { get; set; }

    [Column("nick_name"), StringLength(255)]
    public string NickName { get; set; }

    [Column("created_way"), StringLength(255)]
    public string CreatedWay { get; set; }

    [Column("expire_time")]
    public int ExpireTime { get; set; }

    [Column("aud")]
    public string Aud { get; set; }

    [Column("created_time")]
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.Now;
}