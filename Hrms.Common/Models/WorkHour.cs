using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("WORK_HOUR")]
    public class WorkHour
    {
        [Key]
        [Column("WORK_ID")]
        public short Id { get; set; }

        [Column("WORK_HOUR", TypeName ="varchar(6)")]
        public string TotalHour { get; set; }

        [Column("IN_START", TypeName ="varchar(15)")]
        public string? InStart { get; set; }

        [Column("OUT_START", TypeName ="varchar(15)")]
        public string? OutStart { get; set; }

        [Column("LUNCHTIME")]
        public short? LunchTime { get; set; }

        [Column("TIFFINTIME")]
        public short? TiffinTime { get; set; }

        [Column("IS_NIGHTSHIFT")]
        public bool IsNightShift { get; set; }

        [Column("WORK_TYPE")]
        public byte WorkType { get; set; }

        [Column("WORK_DAYCOUNT", TypeName ="numeric(18,2)")]
        public decimal WorkDayCount { get; set; }

        [Column("INSTARTGRACE")]
        public short InStartGrace { get; set; }

        [Column("INENDGRACE")]
        public short InEndGrace { get; set; }

        [Column("OUTSTARTGRACE")]
        public short OutStartGrace { get; set; }

        [Column("OUTENDGRACE")]
        public short OutEndGrace { get; set; }

        [Column("STMVAL")]
        public short STMVAL { get; set; }

        [Column("ETMVAL")]
        public short ETMVAL { get; set; }

        [Column("T_Date")]
        public DateTime? TransactionDate { get; set; } = DateTime.UtcNow;

        [Column("TUser", TypeName ="varchar(50)")]
        public string? TransactionUser { get; set; }

        [Column("LOCKIN")]
        public byte LockIn { get; set; }

        [Column("LOCKINTIME", TypeName ="varchar(15)")]
        public string? LockInTime { get; set; }

        [Column("LOCKOUT")]
        public byte LockOut { get; set; }

        [Column("LOCKOUTTIME", TypeName ="varchar(15)")]
        public string? LockOutTime { get; set; }

        [Column("LOCKLT")]
        public byte LockLunch { get; set; }

        [Column("LOCKLTTIME", TypeName ="varchar(15)")]
        public string? LockLunchTime { get; set; }

        [Column("IS_MIN_DURATION")]
        public bool IsFlexible { get; set; } = false;

        [Column("NAME", TypeName ="varchar(255)")]
        public string? Name { get; set; }

        [Column("START_TIME", TypeName = "varchar(255)")]
        public string? StartTime { get; set; }

        [Column("END_TIME", TypeName ="varchar(255)")]
        public string? EndTime { get; set; }

        [Column("NIGHT_START_TIME", TypeName ="varchar(255)")]
        public string? NightStartTime { get; set; }

        [Column("NIGHT_END_TIME", TypeName ="varchar(255)")]
        public string? NightEndTime { get; set; }

        [Column("HALF_DAY_START_TIME", TypeName ="varchar(255)")]
        public string? HalfDayStartTime { get; set; }

        [Column("HALF_DAY_END_TIME", TypeName ="varchar(255)")]
        public string? HalfDayEndTime { get; set; }

        [Column("FLEXI_DURATION")]
        public int? FlexiDuration { get; set; }

        [Column("LATE_IN_GRACE_TIME")]
        public int? LateInGraceTime { get; set; }

        [Column("MIN_HALF_DAY_TIME")]
        public int? MinHalfDayTime { get; set; }

        [Column("MIN_DUTY_TIME")]
        public int? MinDutyTime { get; set; }

        [Column("IS_EARLY_GOING_BUT_NO_OT")]
        public bool? IsEarlyGoingButNoOt { get; set; }

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
