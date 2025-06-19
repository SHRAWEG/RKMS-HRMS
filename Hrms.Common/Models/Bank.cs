using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("tblBank")]
    public class Bank
    {
        [Key]
        [Column("BankId")]
        public int Id { get; set; }

        [Column("BankName", TypeName ="varchar(200)")]
        public string Name { get; set; }

        [Column("BankAddress", TypeName ="varchar(200)")]
        public string? Address { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
