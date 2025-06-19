using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("REGULARISATION_TYPE")]
    public class RegularisationType
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME", TypeName ="varchar(255)")]
        public string Name { get; set; }

        [Column("DISPLAY_NAME", TypeName ="varchar(255)")]
        public string DisplayName { get; set; }

        [Column("CREATED_AT")]  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
