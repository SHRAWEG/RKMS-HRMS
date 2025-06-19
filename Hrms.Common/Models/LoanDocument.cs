using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("LOAN_DOCUMENT")]
    public class LoanDocument
    {
        [Key]
        [Column("DOC_NO")]
        public int Id { get; set; }

        [Column("FILE_NAME", TypeName = "varchar(250)")]
        public string FileName { get; set; }

        [Column("FILE_EXT", TypeName = "varchar(50)")]
        public string FileExtension { get; set; }

        [Column("FILE_DESC", TypeName = "varchar(250)")]
        public string FileDescription { get; set; }

        [Column("REMARKS", TypeName = "varchar(250)")]
        public string? Remarks { get; set; }

        [Column("LOAN_ID")]
        public int? LoanLoanId { get; set; }    
        public LoanApplication? Loan { get; set; }       

        [Column("UPLOAD_DT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
