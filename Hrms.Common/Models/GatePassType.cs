using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("GATE_PASS_TYPES")]
    public class GatePassType
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; } 

        [Column("NAME", TypeName = "varchar(255)")]
        public string Name { get; set; }
    }
}
