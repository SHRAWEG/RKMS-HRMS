using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("LEAVE")]
    public class Leave
    {
        [Key]
        [Column("LEAVE_ID")]
        public short Id { get; set; }

        [Column("LEAVE_NAME", TypeName ="varchar(50)")]
        public string? Name { get; set; }

        [Column("LEAVE_DAYS")]
        public int? Days { get; set; }

        [Column("LEAVE_TYPE")]
        public byte? Type { get; set; }

        [Column("LEAVE_MAX")]
        public short? LeaveMax { get; set; }

        [Column("ISPAIDLEAVE")]
        public byte IsPaidLeave { get; set; }

        [Column("LEAVE_PAY_QTY", TypeName ="numeric(18,2)")]
        public decimal? PayQuantity { get; set; }

        [Column("Abbr", TypeName ="varchar(10)")]
        public string Abbreviation { get; set; }

        [Column("UseLimit")]
        public byte UseLimit { get; set; }

        [Column("LEAVEEARN")]
        public byte LeaveEarnType { get; set; }

        [Column("LEAVEEARN_DAYS")]
        public double? LeaveEarnDays { get; set; }

        [Column("LEAVEEARN_QUANTITY")]
        public decimal? LeaveEarnQuantity { get; set; }

        [Column("IS_HALF_LEAVE")] 
        public bool IsHalfLeave { get; set; } = true;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("EL_BALANCE")]
       public decimal? EL_Balance { get; set; }      
    }
}
