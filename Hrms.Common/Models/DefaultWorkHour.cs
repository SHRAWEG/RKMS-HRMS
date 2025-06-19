using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("DEFAULT_WORKHOUR")]
    public class DefaultWorkHour
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public EmpDetail? Emp { get; set; }

        [Column("Emp_Id")]
        public int? EmpId { get; set; }

        [Column("DAY_ID")]
        public short DayId { get; set; }

        public WorkHour WorkHour { get; set; }

        [Column("WorkHour_Id")]
        public short WorkHourId { get; set; }

        [Column("CREATED_AT")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
