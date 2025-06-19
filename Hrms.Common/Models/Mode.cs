using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("MODE")]
    public class Mode
    {
        [Key]
        [Column("MODE_ID")]
        public short Id { get; set; }

        [Column("MODE_NAME", TypeName ="varchar(30)")]
        public string Name { get; set; }

        [Column("MODE_ABB", TypeName ="varchar(2)")]
        public string Abbreviation { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
