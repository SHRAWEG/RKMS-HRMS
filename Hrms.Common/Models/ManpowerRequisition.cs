using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    [Table("MANPOWER_REQUISITIONS")]
    public class ManpowerRequisition
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("JOB_TITLE", TypeName ="varchar(255)")]
        public string JobTitle { get; set; }

        [Column("STARTING_DATE")]
        public DateOnly? StartingDate { get; set; }

        public Department? Department { get; set; }

        [Column("DEPARTMENT_ID")]
        public int? DepartmentId { get; set; }

        public Designation? Designation { get; set; }

        [Column("DESIGNATION_ID")]
        public short? DesignationId { get; set; }

        public Branch? Branch { get; set; }

        [Column("BRANCH_ID")]
        public short? BranchId { get; set; }

        public EmpDetail? ReportingToEmp { get; set; }

        [Column("REPORTING_TO_EMP_ID")]
        public int? ReportingToEmpId { get; set; }

        [Column("QUANTITY")]
        public int Quantity { get; set; }

        [Column("IS_GENDER_SPECIFIC")]
        public bool IsGenderSpecific { get; set; } = false;

        [Column("GENDER", TypeName ="varchar(10)")]
        public string? Gender { get; set; }

        [Column("EMPLOYMENT_NATURE", TypeName ="varchar(255)")]
        public string EmploymentNature { get; set; }

        public EmpDetail? ReplacedEmp { get; set; }

        [Column("REPLACED_EMP_ID")]
        public int? ReplacedEmpId { get; set; }

        public EmploymentType? EmploymentType { get; set; }

        [Column("EMPLOYMENT_TYPE_ID")]
        public int? EmploymentTypeId { get; set; }

        [Column("QUALIFICATIONS", TypeName ="varchar(255)")]
        public string? Qualifications { get; set; }

        [Column("EXPERIENCE", TypeName ="varchar(255)")]
        public string? Experience { get; set; }

        [Column("KEY_COMPETENCIES", TypeName ="varchar(255)")]
        public string? KeyCompetencies { get; set; }

        [Column("JOB_DESCRIPTION")]
        public string? JobDescription { get; set; }

        [Column("SALARY_RANGE_FROM")]
        public double? SalaryRangeFrom { get; set; }

        [Column("SALARY_RANGE_TO")]
        public double? SalaryRangeTo { get; set; }

        [Column("STATUS")]
        public string Status { get; set; }

        public EmpDetail? RequestedByEmp { get; set; }

        [Column("REQUESTED_BY_Emp_ID")]
        public int? RequestedByEmpId { get; set; }

        public User? ProcessedByUser { get; set; }

        [Column("PROCESSED_BY_USER_ID")]
        public int? ProcessedByUserId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
