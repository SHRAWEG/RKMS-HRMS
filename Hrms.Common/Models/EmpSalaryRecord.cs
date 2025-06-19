using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_SALARY_RECORDS")]
    public class EmpSalaryRecord
    {
        [Key]
        [Column("EMP_SALARY_RECORD_ID")]
        public int EmpSalaryRecordId { get; set; }

        public EmpDetail Emp { get; set; }

        [Column("EMP_ID")]
        public int EmpId { get; set; }

        [Column("FROM_DATE")]
        public DateOnly FromDate { get; set; }

        [Column("TO_DATE")]
        public DateOnly? ToDate { get; set; }

        [Column("MONTHLY_SALARY")]
        public int MonthlySalary { get; set; }

        [Column("TOTAL_AMOUNT", TypeName = "numeric(18,2)")]
        public decimal? TotalAmount { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigaitional Property

        public ICollection<EmpSalaryHead> EmpSalaryHeads;
    }
}
