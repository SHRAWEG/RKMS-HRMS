using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("ATTENDANCE_LOG_NO_DIRECTION")]
    public class AttendanceLogNoDirection
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Column("DEVICE_CODE", TypeName ="varchar(20)")]
        public string DeviceCode { get; set; }

        [Column("DATE")]
        public DateOnly Date { get; set; }

        [Column("TIME")]
        public TimeOnly Time { get; set; }

        [Column("IS_SUCCESS")]
        public bool IsSuccess { get; set; } = false;

        [Column("REMARKS", TypeName ="varchar(255)")]
        public string? Remarks { get; set; }

        public DeviceSetting? Device { get; set; }

        [Column("DEVICE_ID")]
        public int? DeviceId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
