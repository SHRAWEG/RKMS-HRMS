
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql.Replication.PgOutput.Messages;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class ProfilesController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public ProfilesController(DataContext context, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        // Get: Emp Details
        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);

            var data = await (from detail in _context.EmpDetails where detail.Id == user.EmpId
                         join log in _context.EmpLogs on detail.Id equals log.EmployeeId into logs
                         from log in logs.DefaultIfEmpty()
                         join tran in _context.EmpTransactions on log.Id equals tran.Id into trans
                         from tran in trans.DefaultIfEmpty()
                         select new
                         {
                             PId = log != null ? log.Id : 0,
                             EmpId = detail.Id,
                             EmpCode = detail.CardId,
                             detail.Title,
                             detail.FirstName,
                             detail.MiddleName,
                             detail.LastName,
                             Name = FullName(detail.FirstName, detail.MiddleName, detail.LastName),
                             detail.Email,
                             detail.ContactNumber,
                             BusinessUnitName = tran != null ? (tran.BusinessUnit != null ? tran.BusinessUnit.Name : "") : "",
                             CityName = tran != null ? (tran.Branch != null ? tran.Branch.City.Name : "") : "",
                             CompanyName = tran != null ? (tran.Company != null ? tran.Company.Name : "") : "",
                             PlantName = tran != null ? (tran.Plant != null ? tran.Plant.Name : "") : "",
                             DepartmentName = tran != null ? (tran.Department != null ? tran.Department.Name : "") : "",
                             SubDepartment = tran != null ? (tran.SubDepartment != null ? tran.SubDepartment.Name : "") : "",
                             BranchName = tran != null ? (tran.Branch != null ? tran.Branch.Name : "") : "",
                             DivisionName = tran != null ? (tran.Division != null ? tran.Division.Name : "") : "",
                             DesignationName = tran != null ? (tran.Designation != null ? tran.Designation.Name : "") : "",
                             GradeName = tran != null ? (tran.Grade != null ? tran.Grade.Name : "") : "",
                             detail.Gender,
                             PersonType = tran != null ? tran.PersonType : "",
                             CostCenterName = tran != null ? (tran.CostCenter != null ? tran.CostCenter.Name : "") : "",
                             PositionCode = tran != null ? tran.PositionCode : "",
                             detail.DateOfBirth,
                             detail.JoinDate,
                             detail.Nationality,
                             ReligionName = detail.Religion != null ? detail.Religion.Name : "",
                             detail.BirthPlace,
                             detail.BloodGroup,
                             detail.BirthStateId,
                             BirthStateName = detail.BirthState != null ? detail.BirthState.Name : "",
                             detail.BirthCountryId,
                             BirthCountryName = detail.BirthCountry != null ? detail.BirthCountry.Name : "",
                             detail.MaritalStatus,
                             detail.AppointedDate,
                             RmEmpName = tran != null ? (tran.RmEmp != null ? FullName(tran.RmEmp.FirstName, tran.RmEmp.MiddleName, tran.RmEmp.LastName) : "") : "",
                             RmEmpCode = tran != null ? (tran.RmEmp != null ? tran.RmEmp.CardId : "") : "",
                             HodEmpName = tran != null ? (tran.HodEmp != null ? FullName(tran.HodEmp.FirstName, tran.HodEmp.MiddleName, tran.HodEmp.LastName) : "") : "",
                             HodEmpCode = tran != null ? (tran.HodEmp != null ? tran.HodEmp.CardId : "") : "",
                         }
                        ).SingleOrDefaultAsync();


            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                EmpDetail = data
            });
        }

        [HttpPut()]
        public async Task<IActionResult> Update(GeneralDetailModel input)
        {
            var user = await _userManager.GetUserAsync(User);

            var data = await _context.EmpDetails.FindAsync(user.EmpId);

            data.BirthPlace = input.BirthPlace;
            data.BirthStateId = input.BirthStateId;
            data.MaritalStatus = input.MaritalStatus;
            data.BloodGroup = input.BloodGroup;
            data.BirthCountryId = input.BirthCountryId;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("ContactDetail")]
        public async Task<IActionResult> GetContactDetails()
        {
            var user = await _userManager.GetUserAsync(User);

            var data = await _context.EmpDetails
                            .Where(x => x.Id == user.EmpId)
                            .Select(x => new
                            {
                                EmpId = x.Id,
                                x.Email,
                                x.ContactNumber,
                                x.EmergencyContactNumber,
                                x.EmergencyContactPerson,
                                x.PermanentAddress,
                                x.PermanentAddress2,
                                x.PermanentCity,
                                x.PermanentPincode,
                                x.PermanentState,
                                x.PermanentDistrict,
                                x.CorrespondanceAddress,
                                x.CorrespondanceAddress2,
                                x.CorrespondanceCity,
                                x.CorrespondancePincode,
                                x.CorrespondanceState,
                                x.CorrespondanceDistrict,
                            }).SingleOrDefaultAsync();

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                ContactDetail = data
            });
        }

        [HttpPut("ContactDetail")]
        public async Task<IActionResult> UpdateContactDetail(ContactDetailModel input)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!string.IsNullOrEmpty(input.Email))
            {
                if (await _context.EmpDetails.AnyAsync(x => x.Id != user.EmpId && input.Email == x.Email))
                {
                    return ErrorHelper.ErrorResult("Email", "Email is already taken.");
                }
            }
            var data = await _context.EmpDetails.FindAsync(user.EmpId);

            data.Email = input.Email.Trim().ToLower();
            data.ContactNumber = input.ContactNumber;
            data.EmergencyContactPerson = input.EmergencyContactPerson;
            data.EmergencyContactNumber = input.EmergencyContactNumber;
            data.PermanentAddress = input.PermanentAddress;
            data.PermanentAddress2 = input.PermanentAddress2;
            data.PermanentCity = input.PermanentCity;
            data.PermanentPincode = input.PermanentPincode;
            data.PermanentState = input.PermanentState;
            data.PermanentDistrict = input.PermanentDistrict;
            data.CorrespondanceAddress = input.CorrespondanceAddress;
            data.CorrespondanceAddress2 = input.CorrespondanceAddress2;
            data.CorrespondanceCity = input.CorrespondanceCity;
            data.CorrespondancePincode = input.CorrespondancePincode;
            data.CorrespondanceState = input.CorrespondanceState;
            data.CorrespondanceDistrict = input.CorrespondanceDistrict;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("OtherDetail")]
        public async Task<IActionResult> GetOtherDetail()
        {
            var user = await _userManager.GetUserAsync(User);

            var data = await (from detail in _context.EmpDetails
                              where detail.Id == user.EmpId
                              join log in _context.EmpLogs on detail.Id equals log.EmployeeId into logs
                              from log in logs.DefaultIfEmpty()
                              join tran in _context.EmpTransactions on log.Id equals tran.Id into trans
                              from tran in trans.DefaultIfEmpty()
                              select new
                              {
                                  PId = log != null ? log.Id : 0,
                                  EmpId = detail.Id,
                                  detail.PassportNumber,
                                  detail.AadharNumber,
                                  UanNumber = tran != null ? tran.UanNumber : "",
                                  detail.PanNumber,
                                  detail.DrivingLicenseNumber,
                                  UniformStatus = tran != null ? tran.UniformStatus : null,
                                  UniformTypeId = tran != null ? tran.UniformTypeId : 0,
                                  UniformTypeName = tran != null ? (tran.UniformType != null ? tran.UniformType.Name : "") : "",
                                  ExtraUniform = tran != null ? tran.ExtraUniform : null,
                                  EsiNumber = tran != null ? tran.EsiNumber : "",
                                  PfAccountNumber = tran != null ? tran.PfAccountNumber : "",
                                  ModeId = tran != null ? tran.ModeId : 0,
                                  ModeName = tran != null ? (tran.Mode != null ? tran.Mode.Name : "") : "",
                                  CheckInMode = tran != null ? tran.CheckInMode : 0,
                              }
                            ).SingleOrDefaultAsync();


            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                OtherDetail = data
            });
        }

        [HttpPut("OtherDetail")]
        public async Task<IActionResult> UpdateOtherDetails(OtherDetailModel input)
        {
            var user = await _userManager.GetUserAsync(User);

            var detail = await _context.EmpDetails.FindAsync(user.EmpId);

            var log = await _context.EmpLogs.Where(x => x.EmployeeId == user.EmpId).SingleOrDefaultAsync();

            var tran = await _context.EmpTransactions.FindAsync(log.Id);

            detail.PassportNumber = input.PassportNumber;
            detail.AadharNumber = input.AadharNumber;
            tran.UanNumber = input.UanNumber;
            detail.PanNumber = input.PanNumber;
            detail.DrivingLicenseNumber = input.DrivingLicenseNumber ?? "";
            tran.UniformStatus = input.UniformStatus;
            tran.UniformTypeId = input.UniformTypeId;
            tran.ExtraUniform = input.ExtraUniform;
            tran.EsiNumber = input.EsiNumber;
            tran.PfAccountNumber = input.PfAccountNumber;
            tran.ModeId = input.ModeId;
            tran.CheckInMode = input.CheckInMode ?? 0;

            await _context.SaveChangesAsync();

            return Ok();
        }

        private static string FullName(string firstName, string middleName, string lastName)
        {
            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
        }

        public class GeneralDetailModel
        {
            public string BirthPlace { get; set; }
            public int? BirthStateId { get; set; }
            public char MaritalStatus { get; set; }
            public string BloodGroup { get; set; }
            public int? BirthCountryId { get; set; }
        }

        public class ContactDetailModel
        {
            public string Email { get; set; }
            public string ContactNumber { get; set; }
            public string EmergencyContactPerson { get; set; }
            public string EmergencyContactNumber { get; set; }
            public string PermanentAddress { get; set; }
            public string PermanentAddress2 { get; set; }
            public string PermanentCity { get; set; }
            public string PermanentPincode { get; set; }
            public string PermanentState { get; set; }
            public string PermanentDistrict { get; set; }
            public string CorrespondanceAddress { get; set; }
            public string CorrespondanceAddress2 { get; set; }
            public string CorrespondanceCity { get; set; }
            public string CorrespondancePincode { get; set; }
            public string CorrespondanceState { get; set; }
            public string CorrespondanceDistrict { get; set; }
        }

        public class OtherDetailModel
        {
            public string PassportNumber { get; set; }
            public string AadharNumber { get; set; }
            public string UanNumber { get; set; }
            public string PanNumber { get; set; }
            public string DrivingLicenseNumber { get; set; }
            public bool? UniformStatus { get; set; }
            public int? UniformTypeId { get; set; }
            public bool? ExtraUniform { get; set; }
            public string EsiNumber { get; set; }
            public string PfAccountNumber { get; set; }
            public short? ModeId { get; set; }
            public byte? CheckInMode { get; set; }
        }



        public class GeneralDetailModelValidator : AbstractValidator<GeneralDetailModel>
        {
            private readonly DataContext _context;

            public GeneralDetailModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.MaritalStatus)
                    .MustBeValues(new List<char> { 'M', 'S' });

                RuleFor(x => x.BirthCountryId)
                    .IdMustExist(_context.Countries.AsQueryable())
                    .Unless(x => x.BirthCountryId is null);

                RuleFor(x => x.BirthStateId)
                    .IdMustExist(_context.States.AsQueryable())
                    .Unless(x => x.BirthStateId is null);
            }
        }

        public class ContactDetailModelValidator : AbstractValidator<ContactDetailModel>
        {
            private readonly DataContext _context;

            public ContactDetailModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.Email)
                    .EmailAddress();

                RuleFor(x => x.ContactNumber)
                    .NotEmpty()
                    .MustBeDigits(10)
                    .Unless(x => string.IsNullOrEmpty(x.ContactNumber));

                RuleFor(x => x.EmergencyContactNumber)
                    .NotEmpty()
                    .MustBeDigits(10)
                    .Unless(x => string.IsNullOrEmpty(x.EmergencyContactNumber));
            }
        }

        public class OtherDetailModelValidator : AbstractValidator<OtherDetailModel>
        {
            private readonly DataContext _context;

            public OtherDetailModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.UniformTypeId)
                    .IdMustExist(_context.UniformTypes.AsQueryable())
                    .Unless(x => x.UniformTypeId is null);
            }
        }
    }
}
