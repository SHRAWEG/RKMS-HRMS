using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("LOANAPPLICATION")]
    public class LoanApplication
    {
        [Key]
        [Column("LOAN_ID")]
        public int LoanId { get; set; }

        [Column("EMPLOYEE_ID")]
        public int? EmployeeId { get; set; }
        public EmpDetail? Employee { get; set; }        
        
        [Column("LOAN_TYPE", TypeName = "varchar(50)")]
        public string? LoanType { get; set; }

        [Column("LOAN_AMOUNT")]
        public decimal? LoanAmount { get; set; }

        [Column("REPAYMENT_PERIOD")]
        public decimal? RepaymentPeriod { get; set; }

        [Column("INTEREST_RATE")]
        public decimal? InterestRate { get; set; }

      
        [Column("UPDATED_AT")]
        public DateTime? UpdatedAt { get; set; }

        [Column("CREATED_AT")]
        public DateTime? CreatedAt { get; set; }
    }
}
