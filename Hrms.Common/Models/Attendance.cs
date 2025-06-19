using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("ATTENDANCE")]
    public class Attendance
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public WorkHour? WorkHour { get; set; }

        [Column("WORKHOUR_ID")]
        public short? WorkHourId { get; set; }

        [ForeignKey("EmpId")]
        public EmpDetail EmpDetail { get; set; }

        [Column("Emp_Id")]
        public int EmpId { get; set; }

        [Column("TDATE")]
        public DateOnly TransactionDate { get; set; }

        [Column("TDATE_OUT")]
        public DateOnly? TransactionDateOut { get; set; }

        [Column("INTIME", TypeName ="varchar(15)")]
        public string? InTime { get; set; }

        [Column("INMODE", TypeName ="varchar(20)")]
        public string? InMode { get; set; }

        [Column("INREMARKS", TypeName ="varchar(255)")]
        public string? InRemarks { get; set; }

        [Column("OUTTIME", TypeName = "varchar(15)")]
        public string? OutTime { get; set; }

        [Column("OUTMODE", TypeName = "varchar(20)")]
        public string? OutMode { get; set; }

        [Column("OUTREMARKS", TypeName = "varchar(255)")]
        public string? OutRemarks { get; set; }

        [Column("LUNCHIN", TypeName ="varchar(15)")]
        public string? LunchIn { get; set; }

        [Column("LUNCHINMODE", TypeName ="varchar(20)")]
        public string? LunchInMode { get; set; }

        [Column("LUNCHINREMARKS", TypeName ="varchar(50)")]
        public string? LunchInRemarks { get; set; }

        [Column("LUNCHOUT", TypeName = "varchar(15)")]
        public string? LunchOut { get; set; }

        [Column("LUNCHOUTMODE", TypeName = "varchar(20)")]
        public string? LunchOutMode { get; set; }

        [Column("LUNCHOUTREMARKS", TypeName = "varchar(50)")]
        public string? LunchOutRemarks { get; set; }

        [Column("TIFFININ", TypeName = "varchar(15)")]
        public string? TiffinIn { get; set; }

        [Column("TIFFININMODE", TypeName = "varchar(20)")]
        public string? TiffinInMode { get; set; }

        [Column("TIFFININREMARKS", TypeName = "varchar(50)")]
        public string? TiffinInRemarks { get; set; }

        [Column("TIFFINOUT", TypeName = "varchar(15)")]
        public string? TiffinOut { get; set; }

        [Column("TIFFINOUTMODE", TypeName = "varchar(20)")]
        public string? TiffinOutMode { get; set; }

        [Column("TIFFINOUTREMARKS", TypeName = "varchar(50)")]
        public string? TiffinOutRemarks { get; set; }

        [Column("ATT_STATUS")]
        public byte AttendanceStatus { get; set; }

        [Column("OUT_VNO", TypeName ="varchar(20)")]
        public string? OutVNO { get; set; }

        [Column("IS_HALTED")]
        public byte? IsHalted { get; set; }

        [Column("FLGIN")]
        public bool FlagIn { get; set; } = false;

        [Column("FLGOUT")]
        public bool FlagOut { get; set; } = false;

        [Column("DT_LOUT")]
        public DateOnly? LunchOutDate { get; set; }

        [Column("DT_LIN")]
        public DateOnly? LunchInDate { get; set; }

        [Column("DT_TOUT")]
        public DateOnly? TiffinOutdate { get; set; }

        [Column("DT_TIN")]
        public DateOnly? TiffinIndate { get; set; }

        [Column("ATT_TYPE")]
        public byte AttendanceType { get; set; }

        [Column("CHECKIN_MODE", TypeName ="char(1)")]
        public char CheckInMode { get; set; }

        [Column("ATTID", TypeName ="numeric(18,0)")]
        public decimal AttendanceId { get; set; }

        [Column("SIN_TIMEVAL")]
        public long SignInTimeStamp { get; set; }

        [Column("SOUT_TIMEVAL")]
        public long SignOutTimeStamp { get; set; }

        public Regularisation? Regularisation { get; set; }

        [Column("REGULARISATION_ID")]
        public int? RegularisationId { get; set; }
    }
}
