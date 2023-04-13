using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Domain.Account
{
    [Table("user")]
    public class User : IEntity
    {
        [Key]
        [Column("id", TypeName = "char(36)")]
        public Guid Id { get; set; }
        
        [Column("third_party_id"), StringLength(128)]
        public string ThirdPartyId { get; set; }
        
        [Column("third_party_from")]
        public ThirdPartyFrom ThirdPartyFrom { get; set; }
        
        [Column("email"), StringLength(512)]
        public string Email { get; set; }
        
        [Column("picture")]
        public string Picture { get; set; }
        
        [Column("display_name"), StringLength(128)]
        public string DisplayName { get; set; }
    }
}