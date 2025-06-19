using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("SYNC_ATTENDANCE_LOGS")]
    public class SyncAttendanceLog
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("TYPE", TypeName ="varchar(10)")]
        public string Type { get; set; }

        [Column("STATUS", TypeName ="varchar(255)")]
        public string Status { get; set; }

        [Column("ERROR_MESSAGE")]
        public string? ErrorMessage { get; set; }

        [Column("ERROR_TRACE")]
        public string? ErrorTrace { get; set; }

        [Column("SYNCED_AT")]
        public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
    }
}
