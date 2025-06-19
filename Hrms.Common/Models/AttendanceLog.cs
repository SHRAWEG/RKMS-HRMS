using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("ATTENDANCE_LOG")]
    public class AttendanceLog
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Column("DEVICE_LOG_ID")]
        public long DeviceLogId { get; set; }

        [Column("DEVICE_CODE", TypeName ="varchar(30)")]
        public string DeviceCode { get; set; }

        [Column("DIRECTION", TypeName ="varchar(10)")]
        public string Direction { get; set; }

        [Column("DATE")]
        public DateOnly Date { get; set; }

        [Column("TIME", TypeName ="varchar(10)")]
        public string Time { get; set; }

        [Column("IS_SUCCESS")]
        public bool IsSuccess { get; set; } = false;

        [Column("REMARKS", TypeName ="varchar(255)")]
        public string? Remarks { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
