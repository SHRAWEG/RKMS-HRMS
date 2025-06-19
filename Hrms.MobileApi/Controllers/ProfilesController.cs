 using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql.Replication.PgOutput.Messages;

namespace Hrms.MobileApi.Controllers
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

            var data = await _context.EmpLogs.Where(x => x.EmployeeId == user.EmpId)
                        .Join(_context.EmpDetails,
                            el => el.EmployeeId,
                            ed => ed.Id,
                            (el, ed) => new
                            {
                                PId = el.Id,
                                EmpId = ed.Id,
                                EmpCode = ed.CardId,
                                ed.Title,
                                ed.FirstName,
                                ed.MiddleName,
                                ed.LastName,
                                Name = FullName(ed.FirstName, ed.MiddleName, ed.LastName),
                                ed.Email,
                                ed.ContactNumber,
                                ed.DateOfBirth,
                                ed.JoinDate,
                                ed.Gender,
                                ed.Nationality,
                                ReligionName = ed.Religion != null ? ed.Religion.Name : "",
                                ed.BirthPlace,
                                ed.BloodGroup,
                                ed.BirthStateId,
                                BirthStateName = ed.BirthState != null ? ed.BirthState.Name : "",
                                ed.BirthCountryId,
                                BirthCountryName = ed.BirthCountry != null ? ed.BirthCountry.Name : "",
                                ed.MaritalStatus,
                                ed.AppointedDate,
                            })
                        .Join(_context.EmpTransactions,
                            e => e.PId,
                            et => et.Id,
                            (e, et) => new
                            {
                                e.PId,
                                e.EmpId,
                                e.EmpCode,
                                e.Title,
                                e.FirstName,
                                e.MiddleName,
                                e.LastName,
                                e.Name,
                                e.Email,
                                e.ContactNumber,
                                BusinessUnitName = et.BusinessUnit.Name,
                                CityName = et.Branch.City.Name,
                                CompanyName = et.Company.Name,
                                PlantName = et.Plant.Name,
                                DepartmentName = et.Department.Name,
                                SubDepartment = et.SubDepartment.Name,
                                BranchName = et.Branch.Name,
                                DivisionName = et.Division.Name,
                                DesignationName = et.Designation.Name,
                                GradeName = et.Grade.Name,
                                e.Gender,
                                et.PersonType,
                                CostCenterName = et != null ? (et.CostCenter != null ? et.CostCenter.Name : "") : "",
                                PositionCode = et != null ? et.PositionCode : "",
                                e.DateOfBirth,
                                e.JoinDate,
                                e.Nationality,
                                e.ReligionName,
                                e.BirthPlace,
                                e.BloodGroup,
                                e.BirthStateId,
                                e.BirthStateName,
                                e.BirthCountryId,
                                e.BirthCountryName,
                                e.MaritalStatus,
                                e.AppointedDate,
                                RmEmpName = et.RmEmp != null ? FullName(et.RmEmp.FirstName, et.RmEmp.MiddleName, et.RmEmp.LastName) : "",
                                RmEmpCode = et.RmEmp != null ? et.RmEmp.CardId : "",
                                HodEmpName = et.HodEmp != null ? FullName(et.HodEmp.FirstName, et.HodEmp.MiddleName, et.HodEmp.LastName) : "",
                                HodEmpCode = et.HodEmp != null ? et.HodEmp.CardId : "",
                            }
                        ).FirstOrDefaultAsync();

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                EmpDetail = data
            });
        }

        private static string FullName(string firstName, string middleName, string lastName)
        {
            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
        }
    }
}
