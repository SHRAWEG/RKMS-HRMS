using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_TRAN")]
    public class EmpTransaction
    {
        [Key]
        [Column("PID")]
        public int Id { get; set; }

        [Column("EMP_TRANMODE", TypeName ="varchar(20)")]
        public string TransactionMode { get; set; }

        public EmpDetail? Employee { get; set; }

        [Column("EMP_ID")]
        public int? EmployeeId { get; set; }

        public Department? Department { get; set; }

        [Column("DEPT_ID")]
        public int? DepartmentId { get; set; }

        public Designation? Designation { get; set; }

        [Column("DEG_ID")]
        public short? DesignationId { get; set; }

        public Branch? Branch { get; set; }

        [Column("BRANCH_ID")]
        public short? BranchId { get; set; }

        public Grade? Grade { get; set; }

        [Column("GRADE")]
        public int? GradeId { get; set; }

        public Mode? Mode { get; set; }

        [Column("MODE_ID")]
        public short? ModeId { get; set; }
    
        public Status? Status { get; set; }

        [Column("STATUS_ID")]
        public short? StatusId { get; set; }

        [Column("BSALARY", TypeName ="numeric(12,2)")]
        public decimal? BasicSalary { get; set; }

        [Column("BTAX")]
        public float? BasicTax { get; set; }

        [Column("JDATE")]
        public DateOnly? LastTransactionDate { get; set; }

        [Column("JBSDATE", TypeName ="varchar(12)")]
        public string? LastTransactionDateBS { get; set; }

        [Column("BYDATE", TypeName ="varchar(5)")]
        public string? DateType { get; set; }

        [Column("NEGATIVELEAVE")]
        public bool NegativeLeave { get; set; }

        [Column("GENERATEDBY", TypeName ="varchar(50)")]
        public string? GeneratedBy { get; set; }

        [Column("REMARKS", TypeName ="varchar(100)")]
        public string? Remarks { get; set; }

        [Column("EMP_CATEGORY")]
        public byte? EmployeeCategory { get; set; }

        [Column("EMP_ATTGROUP")]
        public byte? EmployeeAttGroup { get; set; }

        [Column("TDS", TypeName ="numeric(10,2)")]
        public decimal? TDS { get; set; }

        [Column("EXT", TypeName ="varchar(10)")]
        public string? ExtensionNumber { get; set; }

        [Column("CLASS")]
        public int? Class { get; set; }

        [Column("AccountNo", TypeName ="varchar(20)")]
        public string? AccountNumber { get; set; }

        [Column("TDSTYPE")]
        public byte TDSType { get; set; }

        [Column("EMP_CHECKIN_MODE")]
        public byte CheckInMode { get; set; }

        [Column("DEF_CALC_DUTYHOUR", TypeName ="numeric(18,2)")]
        public decimal DefCalcDutyHour { get; set; }

        [Column("TUser", TypeName ="varchar(100)")]
        public string TransactionUser { get; set; }

        [Column("GRADEAMNT", TypeName ="numeric(18,2)")]
        public decimal GradeAmount { get; set; }

        [Column("GDEAMNT", TypeName ="numeric(18,2)")]
        public decimal GradeAmountDaily { get; set; }

        [Column("DailyWage")]
        public byte DailyWage { get; set; }

        public Bank? SalaryBank { get; set; }

        [Column("SAL_BANKID")]
        public int? SalaryBankId { get; set; }

        public Bank? PFBank { get; set; }

        [Column("PF_BANKID")]
        public int? PFBankId { get; set; }

        [Column("PF_ACCOUNTNO", TypeName ="varchar(50)")]
        public string? PfAccountNumber { get; set; }

        [Column("T_Date")]
        public DateTime? TransactionDate { get; set; } = DateTime.UtcNow;

        [Column("PAYROLLMODE", TypeName ="varchar(50)")]
        public string PayRollMode { get; set; }

        [Column("CalcLate")]
        public byte? CalculateLate { get; set; }

        [Column("Emp_Terminate")]
        public byte Terminate { get; set; }

        [Column("Emp_Terminate_Month")]
        public int TerminateMonth { get; set; }

        [Column("Emp_Terminate_Year")]
        public int TerminateYear { get; set; }

        // New Fields
        [Column("OFFICIAL_CONTACT_NUMBER", TypeName ="varchar(15)")]
        public string? OfficialContactNumber { get; set; }

        [Column("OFFICIAL_EMAIL", TypeName ="varchar(100)")]
        public string? OfficialEmail { get; set; }

        public EmpDetail RmEmp { get; set; }

        [Column("RM_EMP_ID")]
        public int? RmEmpId { get; set; }

        public EmpDetail HodEmp { get; set; }

        [Column("HOD_EMP_ID")]
        public int? HodEmpId{ get; set; }

        public Company? Company { get; set; }

        [Column("COMPANY_ID")]
        public int? CompanyId { get; set; }

        public BusinessUnit? BusinessUnit { get; set; }

        [Column("BUSINESS_UNIT_ID")]
        public int? BusinessUnitId { get; set; }

        public Plant? Plant { get; set; }
        
        [Column("PLANT_ID")]
        public int? PlantId { get; set; }
    
        public Region? Region { get; set; }

        [Column("REGION_ID")]
        public int? RegionId { get; set; }

        public Division? Division { get; set; }

        [Column("DIVISION_ID")]
        public int? DivisionId { get; set; }

        public Department? SubDepartment { get; set; }

        [Column("SUB_DEPARTMENT_ID")]
        public int? SubDepartmentId { get; set; }

        [Column("PERSONAL_AREA", TypeName ="varchar(255)")]
        public string? PersonalArea { get; set; }

        [Column("SUB_AREA", TypeName ="varchar(255)")]
        public string? SubArea { get; set; }

        public CostCenter? CostCenter { get; set; }

        [Column("COST_CENTER_ID")]
        public int? CostCenterId { get; set; }

        [Column("POSITION_CODE", TypeName ="varchar(255)")]
        public string? PositionCode { get; set; }

        [Column("EMP_SUB_TYPE", TypeName ="varchar(255)")]
        public string? SubType { get; set; }

        [Column("PERSON_TYPE", TypeName ="varchar(255)")]
        public string? PersonType { get; set; }

        [Column("UNIFORM_STATUS")]
        public bool? UniformStatus { get; set; } = false;

        public UniformType? UniformType { get; set; }

        [Column("UNIFORM_TYPE_ID")]
        public int? UniformTypeId { get; set; }

        [Column("EXTRA_UNIFORM")]
        public bool? ExtraUniform { get; set; } = false;

        [Column("ESI_NUMBER", TypeName ="varchar(255)")]
        public string? EsiNumber { get; set; }

        [Column("UAN_NUMBER", TypeName ="varchar(255)")]
        public string? UanNumber { get; set; }

        [Column("PF_APPLICABLE")]
        public bool? PfApplicable { get; set; } = false;

        [Column("IS_CEILING")]
        public bool? IsCeiling { get; set; } = false;

        [Column("EPS_APPLICABLE")]
        public bool? EpsApplicable { get; set; } = false;

        [Column("IFSC_CODE", TypeName ="varchar(255)")]
        public string? IfscCode { get; set; }

        [Column("VPF_APPLICABLE")]
        public bool? VpfApplicable { get; set; } = false;

        [Column("VPF_AMOUNT", TypeName = "numeric(18,2)")]
        public decimal? VpfAmount { get; set; }    
    }
}
