using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    [Table("CANDIDATE_ACTIVITY_LOG")]
    public class CandidateActivityLog
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public Candidate Candidate { get; set; }

        [Column("CANDIDATE_ID")]
        public int CandidateId { get; set; }

        [Column("Activity", TypeName ="varchar(255)")]
        public string Activity { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
