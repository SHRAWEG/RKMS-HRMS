using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public class LoanDisbursement
    {
        public int Id { get; set; }
        public int? LoanApplicationLoanId { get; set; }
        public LoanApplication LoanApplication{ get; set; }
        public string? LoanApplicatonStatus { get; set; }
        public decimal? LoanDisbursementAmount { get; set; }
        public DateTime? DisbursementDate { get; set; } 
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }    
    }
}
