using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_SALARY_HEADS")]
    public class EmpSalaryHead
    {
        [Key]
        [Column("EMP_SH_ID")]
        public int EmpShId { get; set; }

        public EmpSalaryRecord EmpSalaryRecord { get; set; }

        [Column("EMP_SALARY_RECORD_ID")]
        public int EmpSalaryRecordId { get; set; }

        public SalaryHead SalaryHead { get; set; }

        [ForeignKey(nameof(SalaryHead))]
        [Column("SH_ID")]
        public int ShId { get; set; }

        [Column("SH_CALC_DATATYPE")]
        public string ShDataType { get; set; }

        [Column("HAS_OFFICE_CONTRIBUTION")]
        public bool? HasOfficeContribution { get; set; } = false;

        [Column("OFFICE_CONTRIBUTION", TypeName = "numeric(18, 2)")]
        public decimal? OfficeContribution { get; set; }

        [Column("CONTRIBUTION_TYPE")]
        public int? ContributionType { get; set; }

        [Column("PER_UNIT_RATE", TypeName = "numeric(18,2)")]
        public decimal? PerUnitRate { get; set; }

        [Column("AMOUNT", TypeName = "numeric(18,2)")]
        public decimal? Amount { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigaitional Property

        public ICollection<EmpSalaryHeadDetail> EmpSalaryHeadDetails;
    }
}
