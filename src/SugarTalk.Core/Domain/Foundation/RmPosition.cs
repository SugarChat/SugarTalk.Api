using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR.Message.Contract.Command;

namespace SugarTalk.Core.Domain.Foundation
{
    [Table("rm_position")]
    public class RmPosition : IEntity<Guid>
    {
        [Column("id", TypeName = "char(36)")]
        public Guid Id { get; set; }
        
        [Column("unit_id", TypeName = "char(36)")]
        public Guid? UnitId { get; set; }
        
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }
        
        [Column("description")]
        [StringLength(150)]
        public string Description { get; set; }
        
        [Column("country_code")]
        public CountryCode? CountryCode { get; set; }
        
        [Column("is_active")]
        public bool? IsActive { get; set; }
    }
}
