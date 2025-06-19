using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public  class CandidateDocument
    {
        [Key]
        [Column("DOC_NO")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("FILE_NAME", TypeName = "varchar(250)")]
        public string FileName { get; set; }

        [Column("FILE_EXT", TypeName = "varchar(50)")]
        public string FileExtension { get; set; }

        [Column("FILE_DESC", TypeName = "varchar(250)")]
        public string FileDescription { get; set; }

        [Column("REMARKS", TypeName = "varchar(250)")]
        public string? Remarks { get; set; }


        public int? CandidateId { get; set; }
        public Candidate? Candidate { get; set; }       

        [Column("UPLOAD_DT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
