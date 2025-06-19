using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("GRADE")]
    public class Grade
    {
        [Key]
        [Column("GRADE_ID")]
        public int Id { get; set; }

        [Column("GRADE_NAME", TypeName ="varchar(30)")]
        public string? Name { get; set; }

        [Column("CODE", TypeName ="varchar(255)")]
        public string? Code { get; set; }

        [ForeignKey("GType")]
        public virtual GradeType GradeType { get; set; }

        [Column("GRADE_TYPE", TypeName ="varchar(50)")]
        public string? GType { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
