using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("DEPARTMENT")]
    public class Department
    {
        [Key]
        [Column("DEPT_ID")]
        public int Id { get; set; }

        [Column("DEPT_PARENT", TypeName ="varchar(100)")]
        public string? Parent { get; set; }

        [Column("DEPT_NAME", TypeName ="varchar(100)")]
        public string? Name { get; set; }

        [Column("CODE", TypeName ="varchar(255)")]
        public string? Code { get; set; }

        [Column("[LEVEL]")]
        public int? Level { get; set; }

        [Column("FLDTYPE", TypeName ="varchar(1)")]
        public string? FLDType { get; set; }

        [Column("NOSTAFF")]
        public int TotalStaffs { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
