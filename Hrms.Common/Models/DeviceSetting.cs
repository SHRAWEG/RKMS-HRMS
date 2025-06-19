using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("DEVICE_SETTINGS")]
    public class DeviceSetting
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("DEVICE_MODEL", TypeName = "varchar(255)")]
        public string DeviceModel { get; set; }

        [Column("DEVICE_IP", TypeName = "varchar(255)")]
        public string DeviceIp { get; set; }

        [Column("PORT_NUMBER")]
        public int PortNumber { get; set; }

        [Column("CLEAR_DEVICE_LOG")]
        public bool ClearDeviceLog { get; set; } = false;

        [Column("AttendanceMode", TypeName = "varchar(255)")]
        public string AttendanceMode { get; set; }

        [Column("LAST_FETCHED_DATE")]
        public DateOnly? LastFetchedDate { get; set; }

        [Column("LAST_FETCHED_TIME")]
        public TimeOnly? LastFetchedTime { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
