using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    [Table("CANDIDATE_STAGES")]
    public class CandidateStage
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public Candidate Candidate { get; set; }

        [Column("CANDIDATE_ID")]
        public int CandidateId { get; set; }

        public HiringStage Stage { get; set; }

        [Column("STAGE_ID")]
        public int StageId { get; set; }

        public EmpDetail? ConcernedEmp { get; set; }

        [Column("CONCERNED_EMP_ID")]
        public int? ConcernedEmpId { get; set; }

        [Column("SCHEDULED_DATE")]
        public DateOnly? ScheduledDate { get; set; }

        [Column("OVERALL_RATING")]
        public int? OverallRating { get; set; }

        [Column("REMARKS", TypeName ="varchar(255)")]
        public string? Remarks { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
