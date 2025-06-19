using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_SALARY_HEAD_DETAIL")]
    public class EmpSalaryHeadDetail
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public EmpSalaryHead EmpSh { get; set; }

        [Column("EMP_SH_ID")]
        public int EmpShId { get; set; }

        public EmpSalaryHead? ReferenceEmpSh { get; set; }

        [Column("REFERENCE_SH_ID")]
        public int? ReferenceEmpShId { get; set; }

        [Column("IS_PERCENTAGE_OF_MONTHLY_SALARY")]
        public bool IsPercentageOfMonthlySalary { get; set; } = false;

        [Column("AMOUNT", TypeName ="numeric(18,2)")]
        public decimal? Amount { get; set; }

        [Column("PERCENT", TypeName ="numeric(18,2)")]
        public decimal? Percent { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
