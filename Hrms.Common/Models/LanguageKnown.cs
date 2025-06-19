using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_LANGUAGE_KNOWN")]
    public class LanguageKnown
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public EmpDetail? Emp { get; set; }

        [Column("EMP_ID")]
        public int? EmpId { get; set; }

        [Column("LANGUAGE", TypeName ="varchar(255)")]
        public string Language { get; set; }

        [Column("CAN_READ")]
        public bool CanRead { get; set; } = false;

        [Column("CAN_WRITE")]
        public bool CanWrite { get; set; } = false;

        [Column("CAN_SPEAK")] 
        public bool CanSpeak { get; set; } = false;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
