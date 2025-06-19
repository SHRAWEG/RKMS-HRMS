using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_EMPLOYMENTHISTORY")]
    public class EmploymentHistory
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public EmpDetail Emp { get; set; }

        [Column("Emp_Id")]
        public int EmpId { get; set; }

        [Column("EH_ORG", TypeName ="varchar(100)")]
        public string? Organization { get; set; }

        [Column("FROM_DATE")]
        public DateOnly? FromDate { get; set; }

        [Column("TO_DATE")]
        public DateOnly? ToDate { get; set; }

        [Column("DESIGNATION", TypeName ="varchar(255)")]
        public string? Designation { get; set; }

        [Column("LOCATION", TypeName = "varchar(255)")]
        public string? Location { get; set; }

        [Column("CITY", TypeName = "varchar(255)")]
        public string? City { get; set; }

        [Column("EH_ADD", TypeName = "varchar(100)")]
        public string? Address { get; set; }

        [Column("EH_JOBDESC", TypeName = "varchar(100)")]
        public string? JobDescription { get; set; }

        [Column("EH_DURATION", TypeName = "varchar(100)")]
        public string? Duration { get; set; }

        public EmpDocument? Document { get; set; }

        [Column("DOCUMENT_ID")]
        public int? DocumentId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
