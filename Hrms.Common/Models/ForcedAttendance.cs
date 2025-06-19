using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("tblFATTLOG")]
    public class ForcedAttendance
    {
        [Key]
        [Column("FAttId")]
        public int Id { get; set; }

        public Regularisation? Regularisation { get; set; }

        [Column("RegularisationId")]
        public int? RegularisationId { get; set; }

        [Column("Emp_Id")]
        public int EmpId { get; set; }

        [Column("TDate")]
        public DateOnly Date{ get; set; }

        [Column("TTime", TypeName ="varchar(15)")]
        public string Time { get; set; }

        [Column("ModeNM", TypeName ="varchar(50)")]
        public string ModeNM{ get; set; }

        [Column("Mode")]
        public byte Mode { get; set; }

        [Column("Flag")]
        public byte Flag { get; set; }

        [Column("TimeVal")]
        public int TimeVal { get; set; }

        [Column("TrnUser", TypeName ="varchar(50)")]
        public string TransactionUser { get; set; }

        [Column("TrnDate")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Column("Remarks", TypeName ="varchar(300)")]
        public string Remarks { get; set; }
    }
}
