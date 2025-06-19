using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("HOLIDAY")]
    public class Holiday
    {
        [Key]
        [Column("HOLIDAY_ID")]
        public int Id { get; set; }

        public EmpDetail? Emp { get; set; }

        [Column("EMP_ID")]
        public int? EmpId { get; set; }

        [Column("HOLIDAY_NAME", TypeName ="varchar(50)")]
        public string? Name { get; set; }

        [Column("HOLIDAY_DATE")]
        public DateOnly? Date { get; set; }

        [Column("DAY", TypeName ="varchar(255)")]
        public string? Day { get; set; }

        [Column("DAY_TYPE", TypeName ="varchar(255)")]
        public string? DayType { get; set; }

        [Column("TYPE", TypeName ="varchar(255)")]
        public string? Type { get; set; }

        [Column("HOLIDAY_QTY", TypeName ="numeric(15,2)")]
        public decimal? Quantity { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
