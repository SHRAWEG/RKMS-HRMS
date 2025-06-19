using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_EDUTRN")]
    public class Education
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        public EmpDetail Emp { get; set; }

        [Column("Emp_Id")]
        public int EmpId { get; set; }

        public EducationLevel? EducationLevel { get; set; }

        [Column("ELevel")]
        public short? EducationLevelId { get; set; }

        [Column("TName", TypeName ="varchar(100)")]
        public string CertificateName { get; set; }

        [Column("TDate")]
        public DateOnly StartDate { get; set; }

        [Column("TDateEnd")]
        public DateOnly EndDate { get; set; }

        [Column("MSubject", TypeName ="varchar(255)")]
        public string? Subject { get; set; }

        [Column("Institute", TypeName ="varchar(250)")]
        public string? Institute { get; set; }

        [Column("EDivision", TypeName ="varchar(250)")]
        public string? FinalGrade { get; set; }

        [Column("University", TypeName ="varchar(255)")]
        public string? University { get; set; }

        public Country? Country { get; set; }

        [Column("CountryId")]
        public int? CountryId { get; set; }

        [Column("flg")]
        public byte Flag { get; set; }

        [Column("Duration", TypeName ="varchar(150)")]
        public string? Duration { get; set; }

        [Column("InstituteAdd", TypeName ="varchar(250)")]
        public string? InstituteAddress { get; set; }

        public EmpDocument? Document { get; set; }

        [Column("DocumentId")]
        public int? DocumentId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
