using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("LEAVE_YEAR_COMPANIES")]
    public class LeaveYearCompany
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public Company? Company { get; set; }

        [Column("COMPANY_ID")]
        public int? CompanyId { get; set; }

        public LeaveYear LeaveYear { get; set; }

        [Column("LEAVE_YEAR_ID")]
        public int LeaveYearId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
