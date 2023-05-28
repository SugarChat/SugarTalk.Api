using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.Foundation;

namespace SugarTalk.Core.Domain.Foundation
{
    [Table("rm_us_location")]
    public class RmUsLocation : IEntity<Guid>
    {
        [Column("id", TypeName = "char(36)")]
        public Guid Id { get; set; }
        
        [Column("location_code")]
        [StringLength(50)]
        public string LocationCode { get; set; }
        
        [Column("warehouse_code")]
        [StringLength(10)]
        public string WarehouseCode { get; set; }
        
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }
        
        [Column("type")]
        public UsLocationType? Type { get; set; }
        
        [Column("status")]
        public UsLocationStatus? Status { get; set; }
    }
}
