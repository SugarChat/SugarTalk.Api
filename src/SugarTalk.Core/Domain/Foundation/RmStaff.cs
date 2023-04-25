using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR.Message.Contract.Command;
using Smarties.Messages.Enums.Foundation;

namespace SugarTalk.Core.Domain.Foundation
{
    [Table("rm_staff")]
    public class RmStaff : IEntity<Guid>
    {
        [Column("id", TypeName = "char(36)")]
        public Guid Id { get; set; }
        
        [Column("userid", TypeName = "char(36)")]
        public Guid? UserId { get; set; }
        
        [Column("user_name")]
        [StringLength(50)]
        public string UserName { get; set; }
        
        [Column("name_cn_long")]
        [StringLength(150)]
        public string NameCNLong { get; set; }
        
        [Column("name_en_long")]
        [StringLength(150)]
        public string NameENLong { get; set; }
        
        [Column("company_id", TypeName = "char(36)")]
        public Guid? CompanyId { get; set; }
        
        [Column("company_name")]
        [StringLength(50)]
        public string CompanyName { get; set; }
        
        [Column("department_id", TypeName = "char(36)")]
        public Guid? DepartmentId { get; set; }
        
        [Column("department_name")]
        [StringLength(50)]
        public string DepartmentName { get; set; }
        
        [Column("group_id", TypeName = "char(36)")]
        public Guid? GroupID { get; set; }
        
        [Column("group_name")]
        [StringLength(50)]
        public string GroupName { get; set; }
        
        [Column("position_id", TypeName = "char(36)")]
        public Guid? PositionId { get; set; }
        
        [Column("position_name")]
        [StringLength(50)]
        public string PositionName { get; set; }
        
        [Column("position_cn_status")]
        public PositionCnStatus? PositionCNStatus { get; set; }
        
        [Column("position_us_status")]
        public PositionUsStatus? PositionUSStatus { get; set; }
        
        [Column("location_id", TypeName = "char(36)")]
        public Guid? LocationId { get; set; }
        
        [Column("location_name")]
        [StringLength(50)]
        public string LocationName { get; set; }
        
        [Column("superior_id", TypeName = "char(36)")]
        public Guid? SuperiorId { get; set; }
        
        [Column("phone_number")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        [Column("email")]
        [StringLength(50)]
        public string Email { get; set; }
        
        [Column("work_place")]
        [StringLength(50)]
        public string WorkPlace { get; set; }
        
        [Column("country_code")]
        public CountryCode? CountryCode { get; set; }
    }
}
