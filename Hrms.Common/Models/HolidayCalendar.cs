using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("HOLIDAY_CALENDAR")]
    public class HolidayCalendar
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public Holiday Holiday { get; set; }

        [Column("HOLIDAY_ID")]
        public int HolidayId { get; set; }

        public Calendar Calendar { get; set; }

        [Column("CALENDAR_ID")]
        public int CalendarId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
