using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("SALARY_ANNEXURE_HEAD_DETAILS")]
    public class SalaryAnnexureHeadDetail
    {
        [Key]
        [Column("ANX_DETAIL_ID")]
        public int AnxDetailId { get; set; }

        public SalaryAnnexureHead SalaryAnnexureHead { get; set; }

        [Column("SALARY_ANNEXURE_HEAD_ID")]
        public int SalaryAnnexureHeadId { get; set; }

        public SalaryAnnexureHead? ReferenceSalaryAnnexureHead { get; set; }

        [Column("REFERENCE_SALARY_ANNEXURE_HEAD_ID")]
        public int? ReferenceSalaryAnnexureHeadId { get; set; }

        [Column("IS_PERCENTAGE_OF_MONTHLY_SALARY")]
        public bool IsPercentageOfMonthlySalary { get; set; } = false;

        [Column("AMOUNT", TypeName ="numeric(18,2)")]
        public decimal? Amount { get; set; }

        [Column("PERCENT", TypeName ="numeric(18,2)")]
        public decimal? Percent { get; set; }

        [Column("PER_UNIT_RATE", TypeName = "numeric(18,2)")]
        public decimal? PerUnitRate { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
