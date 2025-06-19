using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("LEAVE_LEDGER_TEMP")]
    public class LeaveLedgerTemp
    {
        [Column("ID")]
        public int Id { get; set; }

        [Column("Leave_Date")]
        public DateOnly? Leave_Date { get; set; }

        [Column("EMP_ID")]
        public int EmpId { get; set; }

        public Leave? Leave { get; set; }

        [Column("LEAVE_ID")]
        public short? LeaveId { get; set; }

        [Column("GIVEN", TypeName ="numeric(9,4)")]
        public decimal? Given { get; set; }

        [Column("TAKEN", TypeName ="numeric(9,4)")]
        public decimal? Taken { get; set; }

        [Column("REMARKS", TypeName ="varchar(50)")]
        public string? Remarks { get; set; }

        [Column("GIVENMONTH")]
        public short? GivenMonth { get; set; }

        [Column("GivenYear")]
        public int? GivenYear { get; set; }

        [Column("ApprovedBy")]
        public int? ApprovedByEmpId { get; set; }

        [Column("IsRegular")]
        public byte IsRegular { get; set; }

        [Column("ADJUSTED")]
        public byte Adjusted { get; set; }

        [Column("NOHRS", TypeName ="numeric(9,4)")]
        public decimal NoHrs { get; set; }

        [Column("TUSER", TypeName ="varchar(50)")]
        public string? TransactionUser { get; set; }

        [Column("TDATE")]
        public DateTime? TransactionDate { get; set; } = DateTime.UtcNow;

        [Column("REFATTDT")]
        public DateOnly? REFATTDT { get; set; }

        [Column("HLEAVETYPE")]
        public byte HLeaveType { get; set; }

        public LeaveYear LeaveYear { get; set; }

        [Column("LEAVE_YEAR_ID")]
        public int LeaveYearId { get; set; }
    }
}
