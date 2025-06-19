using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public  class LoanRepayment
    {
        public int Id { get; set; }
        public int? LoanApplicationLoanId { get; set; }        
        public LoanApplication LoanApplication { get; set; }           
        public decimal? PaymentAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? CreatedAt { get; set; }        
        public DateTime? UpdatedAt { get; set; }        
    }
}
