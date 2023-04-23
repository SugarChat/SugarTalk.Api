using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR.Message.Contract.Command;
using Smarties.Messages.Enums.Foundation;

namespace SugarTalk.Core.Domain.Foundation
{
    [Table("rm_unit")]
    public class RmUnit : IEntity<Guid>
    {
        [Column("id", TypeName = "char(36)")]
        public Guid Id { get; set; }
        
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }
        
        [Column("type_id")]
        public UnitType? TypeId { get; set; }
        
        [Column("parent_id", TypeName = "char(36)")]
        public Guid? ParentId { get; set; }
        
        [Column("leader_id", TypeName = "char(36)")]
        public Guid? LeaderId { get; set; }
        
        [Column("leader_country_code")]
        public CountryCode? LeaderCountryCode { get; set; }
        
        [Column("location_code")]
        [StringLength(16000)]
        public string LocationCode { get; set; }
        
        [Column("description")]
        [StringLength(200)]
        public string Description { get; set; }
        
        [Column("country_code")]
        public CountryCode? CountryCode { get; set; }
        
        [Column("is_active")]
        public bool? IsActive { get; set; }
    }
}
