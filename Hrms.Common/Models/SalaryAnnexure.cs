using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("SALARY_ANNEXURE")]
    public class SalaryAnnexure
    {
        [Key]
        [Column("ANX_ID")]
        public int AnxId { get; set; }

        [Column("NAME", TypeName = "varchar(250)")]
        public string Name { get; set; }

        [Column("ANNUAL_SALARY_ESTIMATE")]
        public int AnnualSalaryEstimate { get; set; }

        public User User { get; set; }
        [ForeignKey(nameof(User))]
        [Column("CREATED_BY_USER_ID")]
        public int CreatedByUserId { get; set; }

        [Column("IsDraft")]
        public bool IsDraft { get; set; } = true;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigational Properties
        public ICollection<SalaryAnnexureHead> SalaryAnnexureHeads { get; set; }

    }
}