using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("GRADE_TYPE")]
    public class GradeType
    {
        [Key]
        [Column("GType", TypeName = "varchar(50)")]
        public string GType { get; set; }
    }
}
