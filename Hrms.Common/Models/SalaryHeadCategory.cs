using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("SALARY_HEAD_CATEGORY")]
    public class SalaryHeadCategory
    {
        [Key]
        [Column("SHC_ID")]
        public int ShcId { get; set; }

        [Column("NAME", TypeName = "varchar(250)")]
        public string Name { get; set; }

        [Column("CATEGORY", TypeName = "varchar(250)")]
        public string Category { get; set; }

        [Column("SHC_TYPE", TypeName = "varchar(250)")]
        public string Shc_Type { get; set; }

        [Column("S_NO")]
        public int SNO { get; set; }

        [Column("SHOW_CATEGORY")]
        public bool ShowCategory { get; set; } = false;

        [Column("FLG_USE")]
        public bool FlgUse { get; set; } = false;

        [Column("FLG_ASSIGN")]
        public bool FlgAssign { get; set; } = false;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigational Properties
        public List<SalaryHead> SalaryHeads { get; set; }
    }
}
