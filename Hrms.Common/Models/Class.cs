using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("CLASS")]
    public class Class
    {
        [Key]
        [Column("CLASS_ID")]
        public int Id { get; set; }

        [Column("CLASS_NAME", TypeName ="varchar(100)")]
        public string Name { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
