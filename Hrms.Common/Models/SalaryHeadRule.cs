using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("SALARY_HEAD_RULES")]
    public class SalaryHeadRule
    {
        [Key]
        [Column("SHR_ID")]
        public int ShrId { get; set; }

        public SalaryHead SalaryHead { get; set; }

        [ForeignKey("SalaryHead")]
        [Column("SH_ID")]
        public int ShId { get; set; }

        public State? State { get; set; }

        [Column("STATE_ID")]
        public int? StateId { get; set; }

        [Column("MAX_AMOUNT", TypeName = "numeric(18,2)")]
        public decimal? MaxAmount { get; set; }

        [Column("APPLICABLE_BASIS", TypeName = "varchar(255)")] //Daily, Monthly or Annualy
        public string ApplicableBasis { get; set; }

        [Column("APPLICABLE_REFERENCE")]
        public string ApplicableReference { get; set; }

        [Column("APPLICABLE_MIN_AMOUNT", TypeName = "numeric(18,2)")]
        public decimal? ApplicableMinAmount { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
