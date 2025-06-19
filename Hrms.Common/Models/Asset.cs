using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("ASSET")]
    public class Asset
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public EmpDetail Emp { get; set; }

        [Column("EMP_Id")]
        public int EmpId { get; set; }

        public AssetType AssetType { get; set; }

        [Column("ASSET_TYPE_ID")]
        public int AssetTypeId { get; set; }

        [Column("GIVEN_DATE")]
        public DateOnly GivenDate { get; set; }

        [Column("RETURN_DATE")]
        public DateOnly? ReturnDate { get; set; }

        [Column("ASSET_DETAILS", TypeName ="varchar(255)")]
        public string? AssetDetails { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
