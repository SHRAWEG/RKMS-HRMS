using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public class Interview
    {
        public int Id { get; set; }

        public int? CandidateId { get; set; }
        public Candidate? Candidate { get; set; }

        public int? CreatedById { get; set; }
        public EmpDetail? CreatedBy { get; set; }

        public int? UpdatedById { get; set; }
        public EmpDetail? UpdatedBy { get; set; }

        public string? CandidateEmail { get; set; }

        public string? InterviewerEmail { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string? Subject { get; set; }

        public string? MeetingLink { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
