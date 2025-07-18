using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_ADDITIONAL_DOCUMENTS")]
    public class EmpAdditionalDocument
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public EmpDetail? Emp { get; set; }

        [Column("EMP_ID")]
        public int EmpId { get; set; }

        [Column("DOCUMENT_TYPE_ID")]
        public int DocumentTypeId { get; set; }

        [Column("DOCUMENT_NUMBER", TypeName = "varchar(255)")]
        public string? DocumentNumber { get; set; }

        [ForeignKey("DocumentTypeId")]
        public DocumentType? DocumentType { get; set; }
        public EmpDocument? Document { get; set; }

        [Column("DOCUMENT_ID")]
        public int? DocumentId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
