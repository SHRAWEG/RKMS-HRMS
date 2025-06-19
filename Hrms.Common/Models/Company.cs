using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("COMPANY")]
    public class Company
    {
        [Key]
        [Column("Company_Id")]
        public int Id { get; set; }

        [Column("Company_Name", TypeName ="varchar(250)")]
        public string Name { get; set; }

        [Column("CODE", TypeName ="varchar(255)")]
        public string? Code { get; set; }

        [Column("Company_Add1", TypeName ="varchar(50)")]
        public string? Address1 { get; set; }

        [Column("Company_Add2", TypeName ="varchar(50)")]
        public string? Address2 { get; set; }

        [Column("Company_Tel", TypeName ="varchar(50)")]
        public string? Telephone { get; set; }

        [Column("StartDate")]
        public DateOnly? StartDate { get; set; }

        [Column("EndDate")]
        public DateOnly? EndDate { get; set; }

        [Column("IsDefault")]
        public bool? IsDefault { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigational Property
        public ICollection<LeaveYearCompany> LeaveYearCompanies { get; set; }
    }
}
