using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_LEAVE")]
    public class EmpLeave
    {
        [Column("ID")]
        public int Id { get; set; }

        [Column("PID")]
        public int PId { get; set; }

        public Leave Leave { get; set; }

        [Column("LEAVEID")]
        public short LeaveId { get; set; }

        [Column("DAYS")]
        public short Days { get; set; }

        [Column("LEAVEBY", TypeName ="varchar(12)")]
        public string? LeaveBy { get; set; }

        [Column("MAXDAYS")]
        public short? MaxDays { get; set; }

        [Column("PAID", TypeName ="varchar(7)")]
        public string? Paid { get; set; }

        [Column("UPTOMONTH")]
        public short? UptoMonth { get; set; }

        [Column("UPTOYEAR")]
        public short? UptoYear { get; set; }

        [Column("BYDATE", TypeName ="varchar(10)")]
        public string? ByDate { get; set; }

        [Column("TRNUSER", TypeName ="varchar(20)")]
        public string? TransactionUser { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
