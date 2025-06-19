using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class DashboardsController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public DashboardsController(DataContext context, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: EmpDetail
        [HttpGet("EmpDetail")]
        public async Task<IActionResult> GetDetails()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

            var data = await (from detail in _context.EmpDetails
                              where detail.Id == user.EmpId
                              join log in _context.EmpLogs on detail.Id equals log.EmployeeId into logs
                              from log in logs.DefaultIfEmpty()
                              join tran in _context.EmpTransactions on log.Id equals tran.Id into trans
                              from tran in trans.DefaultIfEmpty()
                              select new
                              {
                                  EmpCode = detail.CardId,
                                  Name = FullName(detail.FirstName, detail.MiddleName, detail.LastName),
                                  detail.Email,
                                  detail.ContactNumber,
                                  detail.JoinDate,
                                  detail.BloodGroup,
                                  detail.DateOfBirth,
                                  detail.MarriageDate,
                                  BranchName = tran != null ? (tran.Branch != null ? tran.Branch.Name : "") : "",
                                  DepartmentName = tran != null ? (tran.Department != null ? tran.Department.Name : "") : "",
                                  DesignationName = tran != null ? (tran.Designation != null ? tran.Designation.Name : "") : "",
                                  GradeName = tran != null ? (tran.Grade != null ? tran.Grade.Name : "") : "",
                                  RmEmpName = tran.RmEmp != null ? FullName(tran.RmEmp.FirstName, tran.RmEmp.MiddleName, tran.RmEmp.LastName) : "",
                                  HodEmpName = tran.HodEmp != null ? FullName(tran.HodEmp.FirstName, tran.HodEmp.MiddleName, tran.HodEmp.LastName) : "",
                                  
                              }
            ).SingleOrDefaultAsync();

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                ContactDetails = new
                {
                    data.EmpCode,
                    data.Email,
                    data.ContactNumber
                },
                EmpDetail = new
                {
                    data.Name,
                    data.DesignationName,
                    data.DepartmentName,
                    data.BranchName,
                    data.JoinDate,
                    data.BloodGroup,
                    data.GradeName,
                    data.HodEmpName,
                    data.RmEmpName,
                    data.DateOfBirth,
                    data.MarriageDate
                    
                }
            });
        }

        // GET ALL Birthday EMP
        [HttpGet("Birthday")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.EmpDetails
                        .Where(x => x.DateOfBirth != null ? 
                                    x.DateOfBirth.Value.Month == DateTime.UtcNow.Month && x.DateOfBirth.Value.Day == DateTime.UtcNow.Day 
                                    : false)
                        .Select(x => new
                        {
                            x.Id,
                            Name = FullName(x.FirstName, x.MiddleName, x.LastName),
                            x.DateOfBirth
                        })
                        .ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        [HttpGet("Request/Subordinate/Count")]
        public async Task<IActionResult> GetRequestCount()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

            var subordinateIds = await _context.EmpLogs
                .Join(_context.EmpTransactions.Where(x => x.RmEmpId == user.EmpId || x.HodEmpId == user.EmpId),
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        PId = el.Id,
                        EmpId = el.EmployeeId,
                    }
                )
                .Select(x => x.EmpId)
                .ToListAsync();

            var leaveCount = await _context.LeaveApplicationHistories.Where(x => subordinateIds.Contains(x.EmpId) && x.Status == "pending").CountAsync();
            var regularisationCount = await _context.Regularisations.Where(x => subordinateIds.Contains(x.EmpId) && x.Status == "pending").CountAsync();

            List<CountDto> data = new List<CountDto>
            {
                new CountDto
                {
                    Name = "Leave",
                    Count = leaveCount
                },
                new CountDto
                {
                    Name = "Regualaristaion",
                    Count = regularisationCount
                }
            };
            
            return Ok(new
            {
                Data = data
            });
        }

        public class CountDto
        {
            public string Name { get; set; }
            public int Count { get; set; }
        }

        private static string FullName(string firstName, string middleName, string lastName)
        {
            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
        }
    }
}
