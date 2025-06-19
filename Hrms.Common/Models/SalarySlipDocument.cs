using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("SALARY_SLIP_DOCUMENTS")]
    public class SalarySlipDocument
    {
        [Key]
        [Column("SALARY_SLIP_DOCUMENT_ID")]
        public int Id { get; set; }

        [Column("FILENAME", TypeName ="varchar(255)")]
        public string Filename { get; set; }

        public User User { get; set; }

        [Column("USER_ID")]
        public int UserId { get; set; }

        [Column("FILE_PATH", TypeName ="varchar(255)")]
        public string FilePath { get; set; }

        public EmpDetail Emp { get; set; }

        [Column("EMP_ID")]
        public int EmpId { get; set; }

        [Column("YEAR")]
        public int Year { get; set; }

        [Column("MONTH")]
        public int Month { get; set; }

        [Column("UPLOADED_AT")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
