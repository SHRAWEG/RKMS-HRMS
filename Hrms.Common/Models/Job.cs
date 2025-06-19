using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    [Table("JOBS")]
    public class Job
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("TITLE", TypeName ="varchar(255)")]
        public string Title { get; set; }

        public EmploymentType? EmploymentType { get; set; }

        [Column("EMPLOYMENT_TYPE_ID")]
        public int? EmploymentTypeId { get; set; }

        public Branch? Branch { get; set; }

        [Column("BRANCH_ID")]
        public short? BranchId { get; set; }

        public Department? Department { get; set; }

        [Column("DEPARTMENT_ID")]
        public int? DepartmentId { get; set; }

        [Column("QUANTITY")]
        public int Quantity { get; set; }

        [Column("DESCRIPTION")]
        public string Description { get; set; }

        [Column("STATUS")]
        public string Status { get; set; }

        [Column("ESTIMATED_DATE")]
        public DateOnly? EstimatedDate { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
