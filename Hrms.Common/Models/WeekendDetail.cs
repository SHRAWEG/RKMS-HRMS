using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("WEEKEND_DETAIL")]
    public class WeekendDetail
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Emp_ID")]
        public int? EmpId { get; set; }

        public Branch? Branch { get; set; }

        [Column("Branch_Id")]
        public short? BranchId { get; set; }

        [Column("TDate")]
        public DateOnly? ValidFrom { get; set; }

        [Column("Sun")]
        public bool Sunday { get; set; }

        [Column("Mon")]
        public bool Monday { get; set; }

        [Column("Tue")]
        public bool Tuesday { get; set; }

        [Column("Wed")]
        public bool Wednesday { get; set; }

        [Column("Thu")]
        public bool Thursday { get; set; }

        [Column("Fri")]
        public bool Friday { get; set; }

        [Column("Sat")]
        public bool Saturday { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
