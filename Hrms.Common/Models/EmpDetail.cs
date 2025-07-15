using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_DETAIL")]
    public class EmpDetail
    {
        [Key]
        [Column("EMP_ID")]
        public int Id { get; set; }

        [Column("EMP_TITLE", TypeName ="varchar(10)")]
        public string? Title { get; set; }

        [Column("EMP_FIRSTNAME", TypeName ="varchar(30)")]
        public string? FirstName { get; set; }

        [Column("EMP_MIDDLENAME", TypeName = "varchar(30)")]
        public string? MiddleName { get; set; }

        [Column("EMP_LASTNAME", TypeName = "varchar(30)")]
        public string? LastName { get; set; }
            
        [Column("EMP_TADD", TypeName = "varchar(255)")]
        public string? TemporaryAddress { get; set; }

        [Column("EMP_TSTREET", TypeName = "varchar(50)")]
        public string? TemporaryStreet { get; set; }

        [Column("EMP_TDISTRICT", TypeName = "varchar(255)")]
        public string? TemporaryDistrict { get; set; }

        [Column("EMP_TZONE", TypeName = "varchar(15)")]
        public string? TemporaryZone { get; set; }

        [Column("EMP_TCOUNTRY", TypeName = "varchar(15)")]
        public string? TemporaryCountry { get; set; }

        [Column("EMP_PADD", TypeName = "varchar(255)")]
        public string? PermanentAddress { get; set; }

        [Column("EMP_PADD2", TypeName = "varchar(255)")]
        public string? PermanentAddress2 { get; set; }

        [Column("EMP_PCITY", TypeName = "varchar(255)")]
        public string? PermanentCity { get; set; }

        [Column("EMP_PPINCODE", TypeName ="varchar(255)")]
        public string? PermanentPincode { get; set; }

        [Column("EMP_PSTATE", TypeName ="varchar(255)")]
        public string? PermanentState { get; set; }

        [Column("EMP_PDISTRICT", TypeName = "varchar(255)")]
        public string? PermanentDistrict { get; set; }

        [Column("EMP_PSTREET", TypeName = "varchar(50)")]
        public string? PermanentStreet { get; set; }

        [Column("EMP_PZONE", TypeName = "varchar(15)")]
        public string? PermanentZone { get; set; }

        [Column("EMP_PCOUNTRY", TypeName = "varchar(15)")]
        public string? PermanentCountry { get; set; }

        [Column("EMP_CADD", TypeName = "varchar(255)")]
        public string? CorrespondanceAddress { get; set; }

        [Column("EMP_CADD2", TypeName = "varchar(255)")]
        public string? CorrespondanceAddress2 { get; set; }

        [Column("EMP_CCITY", TypeName = "varchar(255)")]
        public string? CorrespondanceCity { get; set; }

        [Column("EMP_CPINCODE", TypeName = "varchar(255)")]
        public string? CorrespondancePincode { get; set; }

        [Column("EMP_CSTATE", TypeName = "varchar(255)")]
        public string? CorrespondanceState { get; set; }

        [Column("EMP_CDISTRICT", TypeName = "varchar(20)")]
        public string? CorrespondanceDistrict { get; set; }

        [Column("EMP_NATIONALITY", TypeName = "varchar(255)")]
        public string? Nationality { get; set; }

        [Column("EMP_PASSPORT_NUMBER", TypeName ="varchar(255)")]
        public string? PassportNumber { get; set; }

        [Column("EMP_ADHAR_NUMBER", TypeName = "varchar(255)")]
        public string? AadharNumber { get; set; }

        [Column("EMP_CITIZENSHIPNO", TypeName = "varchar(50)")]
        public string? CitizenshipNumber { get; set; }

        [Column("EMP_PHONE", TypeName = "varchar(14)")]
        public string? ContactNumber { get; set; }

        [Column("EMP_DOB")]
        public DateOnly? DateOfBirth { get; set; }

        public Country? BirthCountry { get; set; }

        [Column("EMP_BIRTH_COUNTRY_ID")]
        public int? BirthCountryId { get; set; }

        public Religion? Religion { get; set; }

        [Column("EMP_RELIGION_ID")]
        public int? ReligionId { get; set; }

        public State? BirthState { get; set; }

        [Column("EMP_BIRTH_STATE_ID")]
        public int? BirthStateId { get; set; }

        [Column("EMP_BIRTH_PLACE", TypeName ="varchar(255)")]
        public string? BirthPlace { get; set; }

        [Column("EMP_EDUCATION", TypeName = "varchar(100)")]
        public string? Education { get; set; }

        [Column("PHOTO")]
        public string? Photo { get; set; }

        [Column("EMP_PASSWORD", TypeName ="varchar(20)")]
        public string? Password { get; set; }

        [Column("EMP_SIGNINMODE", TypeName ="varchar(10)")]
        public string? SignInMode { get; set; }

        [Column("EMP_JOINDATE")]
        public DateOnly? JoinDate { get; set; }

        [Column("EMP_MARRIAGE_DATE")]
        public DateOnly? MarriageDate { get; set; }

        [Column("EMP_RELEVING_DATE")]
        public DateOnly? RelevingDate { get; set; }

        [Column("UpTo_Date")]
        public DateOnly? UpToDate { get; set; }

        [Column("EMP_CARDID", TypeName ="varchar(20)")]
        public string? CardId { get; set; }

        [Column("EMP_MOBILE", TypeName ="varchar(15)")]
        public string? Mobile { get; set; }

        [Column("EMP_GENDER", TypeName ="char(1)")]
        public char? Gender { get; set; }

        [Column("EMP_MARITALSTATUS", TypeName ="char(1)")]
        public char? MaritalStatus { get; set; }

        [Column("ATTBWISE")]
        public byte? ATTBWISE { get; set; }

        [Column("EMP_APPDATE")]
        public DateOnly? AppointedDate { get; set; }

        [Column("EMP_PANNO", TypeName ="varchar(20)")]
        public string? PanNumber { get; set; }

        [Column("EMP_APPOINTED")]
        public byte Appointed { get; set; }

        [Column("EMP_EMAIL", TypeName ="varchar(100)")]
        public string? Email { get; set; }

        [Column("EMP_BLOOD_GROUP", TypeName ="varchar(250)")]
        public string BloodGroup { get; set; }

        [Column("EMP_CONTACT_PERSON", TypeName ="varchar(250)")]
        public string EmergencyContactPerson { get; set; }

        [Column("EMP_FATHER_NAME", TypeName ="varchar(250)")]
        public string FatherName { get; set; }

        [Column("EMP_MOTHER_NAME", TypeName = "varchar(250)")]
        public string MotherName { get; set; }

        [Column("EMP_GRAND_FATHER_NAME", TypeName = "varchar(250)")]
        public string GrandFatherName { get; set; }

        [Column("EMP_DRVLISCENCENO", TypeName ="varchar(250)")]
        public string DrivingLicenseNumber { get; set; }

        [Column("EMP_CONTACTNO", TypeName ="varchar(250)")]
        public string EmergencyContactNumber { get; set; }

        public EmpDocument? PassportDocument { get; set; }

        [Column("PASSPORT_DOC_ID")]
        public int? PassportDocumentId { get; set; }

        public EmpDocument? AadharDocument { get; set; }

        [Column("AADHAR_DOC_ID")]
        public int? AadharDocumentId { get; set; }

        public EmpDocument? PanDocument { get; set; }

        [Column("PAN_DOC_ID")]
        public int? PanDocumentId { get; set; }

        public EmpDocument? DrivingLicenseDocument { get; set; }

        [Column("DRIVING_LICENSE_DOC_ID")]
        public int? DrivingLicenseDocumentId { get; set; }

        [Column("T_Date")]
        public DateTime? TransactionDate { get; set; } = DateTime.UtcNow;

        [Column("TUser", TypeName ="varchar(250)")]
        public string TransactionUser { get; set; }

        [Column("EMP_BDay_CelebDt")]
        public DateOnly? BirthdayCelebrationDate { get; set; }

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("Profile_DOC_ID")]
        public int? ProfileDocumentId { get; set; }

        public EmpDocument? ProfileDocument { get; set; }

    }
}
