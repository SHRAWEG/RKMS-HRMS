using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EDUCATION_LEVEL")]
    public class EducationLevel
    {
        [Key]
        [Column("ID")]
        public short Id { get; set; }

        [Column("LEVELNAME", TypeName ="varchar(100)")]
        public string? Name { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
