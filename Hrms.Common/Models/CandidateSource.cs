using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    [Table("CANDIDATE_SOURCES")]
    public class CandidateSource
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public Candidate Candidate { get; set; }

        [Column("CANDIDATE_ID")]
        public int CandidateId { get; set; }

        public Source Source { get; set; }

        [Column("SOURCE_ID")]
        public int SourceId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
