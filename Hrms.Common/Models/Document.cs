using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("DOCUMENTS")]
    public class Document
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("FILENAME", TypeName ="varchar(255)")]
        public string Filename { get; set; }

        public User User { get; set; }

        [Column("USER_ID")]
        public int UserId { get; set; }

        [Column("FILE_PATH", TypeName ="varchar(255)")]
        public string FilePath { get; set; }

        [Column("UPLOADED_AT")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
