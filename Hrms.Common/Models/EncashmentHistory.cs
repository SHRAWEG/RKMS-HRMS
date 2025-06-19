using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public class EncashmentHistory
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime EncashmentDate { get; set; } = DateTime.UtcNow;
        public int EncashmentCount { get; set; } // Number of leaves encashed
        public string Status { get; set; } // e.g., "Approved", "Rejected"
    }

}
