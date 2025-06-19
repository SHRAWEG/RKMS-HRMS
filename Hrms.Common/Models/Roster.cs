using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("ROSTER")]
    public class Roster
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public EmpDetail Emp { get; set; }

        [Column("EMP_ID")]
        public int EmpId { get; set; }

        [Column("DATE")]
        public DateOnly Date { get; set; }

        public WorkHour WorkHour { get; set; }

        [Column("WORK_HOUR_ID")]
        public short WorkHourId { get; set; }

        public User User { get; set; }

        [Column("USER_ID")]
        public int UserId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
