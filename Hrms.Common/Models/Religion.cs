using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("RELIGION")]
    public class Religion
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME", TypeName ="varchar(255)")]
        public string Name { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
