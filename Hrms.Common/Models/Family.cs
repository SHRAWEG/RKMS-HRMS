using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_FAMILY")]
    public class Family
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public EmpDetail? Emp { get; set; }

        [Column("EMP_ID")]
        public int? EmpId { get; set; }

        [Column("RELATIONSHIP_TYPE", TypeName ="varchar(255)")]
        public string RelationshipType { get; set; }

        [Column("NAME", TypeName ="varchar(255)")]
        public string Name { get; set; }

        [Column("GENDER", TypeName ="varchar(255)")]
        public string Gender { get; set; }

        [Column("DATE_OF_BIRTH")]
        public DateOnly DateOfBirth { get; set; }

        [Column("IS_WORKING")]
        public bool IsWorking { get; set; } = true;

        [Column("PLACE_OF_BIRTH", TypeName ="varchar(255)")]
        public string? PlaceOfBirth { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [Column("IS_NOMINEE")]
        public bool IsNominee { get; set; }

        [Column("CONTACT_NUMBER", TypeName = "varchar(20)")]
        public string? ContactNumber { get; set; }

        [Column("PERCENTAGE_OF_SHARE", TypeName = "decimal(5,2)")]
        public decimal? PercentageOfShare { get; set; }

        public EmpDocument? Document { get; set; }
        [Column("DOCUMENT_ID")]
        public int? DocumentId { get; set; }


    }
}
