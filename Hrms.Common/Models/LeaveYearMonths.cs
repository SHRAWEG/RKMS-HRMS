using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("LEAVE_YEAR_MONTHS")]
    public class LeaveYearMonths
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public LeaveYear LeaveYear { get; set; }

        [Column("LEAVE_YEAR_ID")]
        public int LeaveYearId { get; set; }

        [Column("MONTH_SEQUENCE")]
        public int MonthSequence { get; set; }

        [Column("MONTH")]
        public int Month { get; set; }

        [Column("YEAR")]
        public int Year { get; set; }
    }
}
