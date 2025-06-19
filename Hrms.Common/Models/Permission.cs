using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("PERMISSIONS")]
    public class Permission
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("CATEGORY", TypeName ="varchar(255)")]
        public string Category { get; set; }

        [Column("SUB_CATEGORY", TypeName ="varchar(255)")]
        public string SubCategory { get; set; }

        [Column("NAME", TypeName ="varchar(255)")]
        public string Name { get; set; }

        [Column("DISPLAY_NAME", TypeName ="varchar(255)")]
        public string DisplayName { get; set; }
    }
}
