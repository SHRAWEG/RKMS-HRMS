using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("SALARY_HEADS")]
    public class SalaryHead
    {
        [Key]
        [Column("SH_ID")]
        public int ShId { get; set; }

        [Column("SH_NAME", TypeName = "varchar(250)")]
        public string Name { get; set; }

        public SalaryHeadCategory ShCategory { get; set; }

        [ForeignKey("ShCategory")]
        [Column("SHC_ID")]
        public int ShcId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User? CreatedByUser { get; set; }

        [Column("CREATED_BY_USER_ID")]
        public int? CreatedByUserId { get; set; }
    }
}
