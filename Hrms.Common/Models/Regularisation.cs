using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    [Table("REGULARISATION")]
    public class Regularisation
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public EmpDetail Emp { get; set; }

        [Column("EMP_ID")]
        public int EmpId { get; set; }

        public GatePassType? GatePassType { get; set; }

        [Column("GATE_PASS_TYPE_ID")]
        public int? GatePassTypeId { get; set; }

        public RegularisationType RegularisationType { get; set; }

        [Column("REGULARISATION_TYPE_ID")]
        public int RegularisationTypeId { get; set; }

        [Column("FROM_DATE")]
        public DateOnly? FromDate { get; set; }

        [Column("TO_DATE")]
        public DateOnly? ToDate { get; set; }

        [Column("FROM_TIME")]
        public TimeOnly? FromTime { get; set; }

        [Column("TO_TIME")]
        public TimeOnly? ToTime { get; set; }

        [Column("CONTACT_NUMBER", TypeName ="varchar(10)")]
        public string? ContactNumber { get; set; }

        [Column("PLACE", TypeName ="varchar(255)")]
        public string? Place { get; set; }

        [Column("REASON", TypeName ="varchar(255)")]
        public string? Reason { get; set; }

        [Column("STATUS", TypeName = "varchar(255)")]
        public string? Status { get; set; }

        public User? ApprovedByUser { get; set; }

        [Column("APPROVED_BY_USER_ID")]
        public int? ApprovedByUserId { get; set; }
        
        public User? DisapprovedByUser { get; set; }

        [Column("DISAPPROVED_BY_USER_ID")]
        public int? DisapprovedByUserId { get; set; }

        [Column("REMARKS", TypeName = "varchar(255)")]
        public string? Remarks { get; set; }

        [Column("CANCELLATION_REMARKS", TypeName ="varchar(255)")]
        public string? CancellationRemarks { get; set; }

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
