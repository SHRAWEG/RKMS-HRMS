using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("SALARY_ANNEXURE_HEADS")]
    public class SalaryAnnexureHead
    {
        [Key]
        [Column("SALARY_ANNEXURE_HEAD_ID")]
        public int SalaryAnnexureHeadId { get; set; }

        public SalaryAnnexure SalaryAnnexure { get; set; }

        [ForeignKey(nameof(SalaryAnnexure))]
        [Column("ANX_ID")]
        public int AnxId { get; set; }

        //[Column("SALARY_HEAD_NAME", TypeName = "varchar(250)")]
        //public string ShName { get; set; }

        public SalaryHead SalaryHead { get; set; }
        [ForeignKey(nameof(SalaryHead))]
        [Column("SH_ID")]
        public int ShId { get; set; }

        [Column("SH_CALC_DATATYPE")]
        public string ShDataType { get; set; }

        [Column("ANNUAL_PERCENT", TypeName = "numeric(18,2)")]
        public decimal? AnnualPercent { get; set; }

        [Column("HAS_OFFICE_CONTRIBUTION")]
        public bool? HasOfficeContribution { get; set; } = false;

        [Column("OFFICE_CONTRIBUTION", TypeName = "numeric(18, 2)")]
        public decimal? OfficeContribution { get; set; }

        [Column("CONTRIBUTION_TYPE")]
        public int? ContributionType { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigaitional Property

        public ICollection<SalaryAnnexureHeadDetail> SalaryAnnexureHeadDetails;
    }
}
