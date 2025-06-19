using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("PAY_BILL_SALARY_HEADS")]
    public class PayBillSalaryHead
    {
        [Key]
        [Column("PAY_BILL_SH_ID")]
        public int PayBillShId { get; set; }

        public PayBill PayBill { get; set; }

        [Column("PAY_BILL_ID")]
        public int PayBillId { get; set; }

        public EmpSalaryHead EmpSalaryHead { get; set; }

        [ForeignKey(nameof(EmpSalaryHead))]
        [Column("EMP_SH_ID")]
        public int EmpShId { get; set; }

        [Column("OFFICE_CONTRIBUTION_AMOUNT", TypeName = "numeric(18, 2)")]
        public decimal? OfficeContributionAmount { get; set; }

        [Column("TOTAL_UNIT", TypeName = "numeric(18,2)")]
        public decimal? TotalUnit { get; set; }

        [Column("AMOUNT", TypeName = "numeric(18,2)")]
        public decimal? TotalAmount { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
