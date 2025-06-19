using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public class LeaveEncashmentRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int RequestedEL { get; set; } // Requested Earned Leave to encash
        public string? Status { get; set; } // e.g., "Pending", "Pending HR Approval", "Approved", "Rejected", "On Hold"
        public string? HOD_Approval { get; set; } // "Approved" or "Rejected"
        public string? HR_Approval { get; set; } // "Approved", "Rejected", or "Hold"
        public string? Remarks { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
    }

}
