using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("DESIGNATION")]
    public class Designation
    {
        [Key]
        [Column("DEG_ID")]
        public short Id { get; set; }

        [Column("DEG_PARENT", TypeName ="varchar(100)")]
        public string? Parent { get; set; }

        [Column("DEG_NAME", TypeName ="varchar(100)")]
        public string? Name { get; set; }

        [Column("CODE", TypeName ="varchar(255)")]
        public string? Code { get; set; }

        [Column("[Level]")]
        public short? Level { get; set; }

        [Column("Deg_Rank")]
        public int Rank { get; set; }

        [Column("NOSTAFF")]
        public int TotalStaffs { get; set; }

        [Column("JOBDESC")]
        public string JobDescription { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
