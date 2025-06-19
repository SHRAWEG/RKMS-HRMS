using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("LEAVE_YEAR")]
    public class LeaveYear
    {
        [Key]
        [Column("LEAVE_YEAR_ID")]
        public int Id { get; set; }

        [Column("YEAR", TypeName ="varchar(255)")]
        public string Year { get; set; }

        [Column("BY_DATE", TypeName ="varchar(255)")]
        public string ByDate { get; set; }

        [Column("START_DATE")]
        public DateOnly StartDate { get; set; }

        [Column("END_DATE")]
        public DateOnly EndDate { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigational Property
        public ICollection<LeaveYearCompany> LeaveYearCompanies { get; set; }
    }
}
