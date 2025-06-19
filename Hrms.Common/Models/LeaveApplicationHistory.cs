using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("LEAVE_APPLICATION_HISTORY")]
    public class LeaveApplicationHistory
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public EmpDetail Emp { get; set; }

        [Column("EMP_ID")]
        public int EmpId { get; set; }

        public Leave Leave { get; set; }

        [Column("HALF_LEAVE_TYPE")]
        public byte HLeaveType { get; set; }

        [Column("LEAVE_ID")]
        public short LeaveId { get; set; }

        [Column("START_DATE")]
        public DateOnly StartDate { get; set; }

        [Column("END_DATE")]
        public DateOnly EndDate { get; set; }

        [Column("TOTAL_DAYS")]
        public decimal? TotalDays { get; set; }

        [Column("CONTACT_NUMBER", TypeName ="varchar(255)")]
        public string ContactNumber { get; set; }

        [Column("ADDRESS", TypeName ="varchar(255)")]
        public string? Address { get; set; }

        [Column("REASON", TypeName ="varchar(255)")]
        public string? Reason { get; set; }

        public LeaveYear LeaveYear { get; set; }

        [Column("LEAVE_YEAR_ID")]
        public int LeaveYearId { get; set; }

        public Company? Company { get; set; }

        [Column("COMPANY_ID")]
        public int? CompanyId { get; set; }

        [Column("IS_CLOSED")]
        public bool IsClosed { get; set; } = false;

        [Column("STATUS", TypeName = "varchar(255)")]
        public string? Status { get; set; }

        public User? ApprovedByUser { get; set; }

        [Column("APPROVED_BY_USER_ID")]
        public int? ApprovedByUserId { get; set; }

        public User? DisapprovedByUser { get; set; }

        [Column("DISAPPROVED_BY_USER_ID")]
        public int? DisapprovedByUserId { get; set; }

        [Column("REMARKS", TypeName = "varchar(255)")]
        public string? Remarks { get; set; }

        [Column("CANCELLATION_REMARKS", TypeName = "varchar(255)")]
        public string? CancellationRemarks { get; set; }

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
