using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("SALARY_HEADS_OLD")]
    public class SalaryHeadOld
    {
        [Key]
        [Column("SH_ID")]
        public int ShId { get; set; }

        [Column("TRN_CODE", TypeName = "varchar(200)")]
        public string TrnCode { get; set; } = "";

        [Column("SH_NAME", TypeName = "varchar(250)")]
        public string Name { get; set; }

        public SalaryHeadCategory ShCategory { get; set; }
        [ForeignKey("ShCategory")]
        [Column("SHC_ID")]
        public int ShcId { get; set; }

        [Column("REF_ID")]
        public int RefId { get; set; } = 0;

        [Column("DRCR", TypeName = "char(2)")]
        public char? DrCr { get; set; }

        //public SalaryHeadDataType ShDataType { get; set; }
        [Column("SH_CALC_DATATYPE")]
        public string ShDataType { get; set; }

        //public SalaryHeadCalcType ShCalcType { get; set; }
        [Column("SH_CALC_TYPE")]
        public string ShCalcType { get; set; }

        [Column("SH_CALC_MODE")]
        public string ShCalcMode { get; set; } = "FIXED";

        [Column("SH_CALC_CATEGORY")]
        public int ShCalcCategory { get; set; } = 0;

        [Column("IS_TAXABLE")]
        public bool IsTaxable { get; set; } = true;

        [Column("IS_ACTIVE")]
        public bool IsActive { get; set; } = true;

        [Column("MIN_HOURS", TypeName = "numeric(18, 2)")]
        public decimal MinHours { get; set; } = 0;

        [Column("MAX_NOS")]
        public int MaxNos { get; set; } = 0;

        [Column("PER_UNIT_RATE", TypeName = "numeric(18, 2)")]
        public decimal PerUnitRate { get; set; } = 0;

        [Column("DEF_VALUE", TypeName = "numeric(18, 2)")]
        public decimal DefValue { get; set; } = 0;

        [Column("UNIT_NAME", TypeName = "varchar(100)")]
        public string UnitName { get; set; } = "";

        [Column("HAS_OFFICE_CONTRIBUTION")]
        public bool HasOfficeContribution = false;

        [Column("OFFICE_CONTRIBUTION", TypeName = "numeric(18, 2)")]
        public decimal OfficeContribution { get; set; } = 0;

        [Column("CONTRIBUTION_TYPE")]
        public int ContributionType { get; set; } = 0;

        [Column("DED_TAX_FREE_LIMIT_CHECK")]
        public bool DedTaxFreeLimitCheck { get; set; } = false;

        [Column("DED_TAX_FREE_AMOUNT", TypeName = "numeric(18, 2)")]
        public decimal DedTaxFreeAmount { get; set; } = 0;

        [Column("ESTIMATE_POST_MONTHS")]
        public int EstimatePostMonths { get; set; } = 0;

        [Column("IS_LOCKED")]
        public bool IsLocked { get; set; } = false;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("CREATED_BY")]
        public int CreatedBy { get; set; }
    }
}
