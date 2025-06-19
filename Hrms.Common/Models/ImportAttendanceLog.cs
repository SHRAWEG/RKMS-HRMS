using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("IMPORT_ATTENDANCE_LOGS")]
    public class ImportAttendanceLog
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("FILENAME", TypeName ="varchar(255)")]
        public string Filename { get; set; }

        [Column("ATTENDANCE_DATE")]
        public DateOnly AttendanceDate { get; set; }

        public User User { get; set; }

        [Column("USER_ID")]
        public int UserId { get; set; }

        [Column("STATUS", TypeName ="varchar(255)")]
        public string Status { get; set; }

        [Column("IS_UPlOADED")]
        public bool IsUploaded { get; set; }

        [Column("IS_SYNCED")]
        public bool IsSynced { get; set; } = false;

        [Column("ERROR_MESSAGE")]
        public string? ErrorMessage { get; set; }

        [Column("ERROR_TRACE")]
        public string? ErrorTrace { get; set; }

        [Column("FILE_PATH")]
        public string FilePath { get; set; }

        [Column("UPLOADED_AT")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Column("SYNCED_AT")]
        public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
    }
}
