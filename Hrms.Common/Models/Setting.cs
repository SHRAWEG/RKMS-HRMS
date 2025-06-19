using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("SETTING")]
    public class Setting
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("GRANT_LEAVE_TYPE", TypeName ="varchar(255)")]
        public string GrantLeaveType { get; set; }

        public LeaveYear LeaveYear { get; set; }

        [Column("LEAVE_YEAR_ID")]
        public int LeaveYearId { get; set; }

        [Column("ATTENDANCE_IN_BS")]
        public bool AttendanceReportInBs { get; set; }

        [Column("DAILY_ATTENDANCE")]
        public bool DailyAttendance { get; set; }

        [Column("DAILY_ATTENDNACE_IN_BS")]
        public bool DailyAttendanceInBs { get; set; }

        [Column("UNIQUE_DEVICE_CODE")]
        public bool UniqueDeviceCode { get; set; } = false;
    }
}
