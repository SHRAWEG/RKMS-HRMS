using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("PAY_BILLS")]
    public class PayBill
    {
        [Key]
        [Column("PAY_BILL_ID")]
        public int PayBillId { get; set; }

        public bool IsRun { get; set; }

        public User? RunByUser { get; set; }

        [Column("RUN_BY_USER_ID")]
        public int? RunByUserId { get; set; }

        public EmpDetail Emp { get; set; }

        [Column("EMP_ID")]
        public int EmpId { get; set; }

        [Column("FROM_DATE")]
        public DateOnly FromDate { get; set; }

        [Column("TO_DATE")]
        public DateOnly? ToDate { get; set; }

        public User CreatedByUser { get; set; }

        [Column("CREATED_BY_USER_ID")]
        public int CreatedByUserId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigaitional Property

        public ICollection<PayBillSalaryHead> PayBillSalaryHeads;
    }
}
