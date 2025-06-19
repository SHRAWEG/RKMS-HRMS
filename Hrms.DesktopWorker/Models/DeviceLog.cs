using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.DesktopWorker.Models
{
    [Table("DEVICE_LOG")]
    public class DeviceLog
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        public DeviceSetting Device { get; set; }

        [Column("DEVICE_ID")]
        public int? DeviceId { get; set; }

        [Column("IS_SUCCESS")]
        public bool IsSuccess { get; set; } = false;

        [Column("REMARKS", TypeName ="varchar(255)")]
        public string Remarks { get; set; }

        [Column("ERROR_MESSAGE")]
        public string? ErrorMessage { get; set; }

        [Column("ERROR_TRACE")]
        public string? ErrorTrace { get; set; }

        [Column("DATE")]
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
