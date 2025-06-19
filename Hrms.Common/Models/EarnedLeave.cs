using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EARNED_LEAVE")]
    public class EarnedLeave
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public EmpDetail Emp { get; set; }

        [Column("EMP_ID")]
        public int EmpId { get; set; }  

        public Leave Leave { get ; set; }

        [Column("LEAVE_ID")]
        public short LeaveId { get; set; }

        [Column("FROM_DATE")]
        public DateOnly FromDate { get; set; }

        [Column("TO_DATE")]
        public DateOnly ToDate { get; set; }

        [Column("IS_HALF_DAY")]
        public bool IsHalfDay { get; set; } = false;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
