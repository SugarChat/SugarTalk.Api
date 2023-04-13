using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Domain.Account
{
    [Table("user_auth_token")]
    public class UserAuthToken : IEntity
    {
        [Key]
        [Column("id", TypeName = "char(36)")]
        public Guid Id { get; set; }
        
        [Column("access_token"), StringLength(128)]
        public string AccessToken { get; set; }
        
        [Column("expired_at")] 
        public DateTimeOffset ExpiredAt { get; set; }
        
        [Column("third_party_from")] 
        public ThirdPartyFrom ThirdPartyFrom { get; set; }
        
        [Column("payload"), StringLength(128)]
        public string Payload { get; set; }
    }
}