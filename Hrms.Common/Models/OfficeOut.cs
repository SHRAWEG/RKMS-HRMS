using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_DAILYOUT")]
    public class OfficeOut
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Emp_Id")]
        public int? EmpId { get; set; }

        [Column("OutDate")]
        public DateOnly? OutDate { get; set; }

        [Column("OutTime", TypeName ="varchar(15)")]
        public string? OutTime { get; set; }

        [Column("OutRemarks", TypeName ="varchar(255)")]
        public string? OutRemarks { get; set; }

        [Column("InDate")]
        public DateOnly? InDate { get; set; }

        [Column("InTime", TypeName ="varchar(15)")]
        public string? InTime { get; set; }

        [Column("InRemarks", TypeName ="varchar(255)")]
        public string? InRemarks { get; set; }

        [Column("ATTID")]
        public long AttendanceId { get; set; }
    }
}
