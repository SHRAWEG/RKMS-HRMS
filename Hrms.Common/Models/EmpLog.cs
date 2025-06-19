using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_LOG")]
    public class EmpLog
    {
        [Key]
        [Column("PID")]
        public int Id { get; set; }

        [Column("EMP_ID")]
        public int? EmployeeId { get; set; }

        [Column("TDATE")]
        public DateTime? TransactionDate { get; set; } = DateTime.UtcNow;
    }
}
