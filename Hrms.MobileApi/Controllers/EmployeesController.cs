using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.MobileApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class EmployeesController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public EmployeesController(DataContext context, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("DepartmentChart")]
        public async Task<IActionResult> GetDepartmentChart()
        {
            var user = await _userManager.GetUserAsync(User);

            var data = await _context.EmpLogs
            .Join(_context.EmpDetails,
                el => el.EmployeeId,
                ed => ed.Id,
                (el, ed) => new
                {
                    EmpId = ed.Id,
                    PId = el.Id,
                    Name = FullName(ed.FirstName, ed.MiddleName, ed.LastName),
                    EmpCode = ed.CardId,
                    ed.JoinDate
                }
            )
            .Join(_context.EmpTransactions,
                e => e.PId,
                et => et.Id,
                (e, et) => new
                {
                    e.EmpId,
                    e.PId,
                    e.Name,
                    e.EmpCode,
                    e.JoinDate,
                    RmEmpId = et != null ? et.RmEmpId : 0,
                    BranchName = et != null ? (et.Branch != null ? et.Branch.Name : "") : "",
                    StateName = et != null ? (et.Branch != null ? et.Branch.State.Name : "") : "",
                    DepartmentName = et != null ? (et.Department != null ? et.Department.Name : "") : "",
                    DesignationName = et != null ? (et.Designation != null ? et.Designation.Name : "") : ""
                }
            )
            .Where(x => x.RmEmpId == user.EmpId || x.EmpId == user.EmpId)
            .ToListAsync();

            return Ok(new
            { 
                User = data.Where(x => x.EmpId == user.EmpId).SingleOrDefault(),
                SubOrdinates = data.Where(x => x.EmpId != user.EmpId).ToList()
            });
        }

        [HttpGet("Directory")]
        public async Task<IActionResult> GetDirectories(int page, int limit, string sortColumn, string sortDirection,
            string empCode, string name, int? branchId, int? departmentId, short? designationId, int? gradeId)
        {
            var query = _context.EmpLogs
            .Join(_context.EmpDetails,
                el => el.EmployeeId,
                ed => ed.Id,
                (el, ed) => new 
                {
                    EmpId = ed.Id,
                    PId = el.Id,
                    Name = FullName(ed.FirstName, ed.MiddleName, ed.LastName),
                    EmpCode = ed.CardId,
                    ed.Email,
                    ed.BloodGroup,
                    ed.ContactNumber,
                    ed.EmergencyContactNumber,
                    ed.EmergencyContactPerson,
                }
            )
            .Join(_context.EmpTransactions,
                e => e.PId,
                et => et.Id,
                (e, et) => new
                {
                    e.EmpId,
                    e.PId,
                    e.Name,
                    e.EmpCode,
                    e.Email,
                    e.BloodGroup,
                    e.ContactNumber,
                    e.EmergencyContactPerson,
                    e.EmergencyContactNumber,
                    et.RmEmpId,
                    RmEmpName = et.RmEmp != null ? FullName(et.RmEmp.FirstName, et.RmEmp.MiddleName, et.RmEmp.LastName) : "",
                    et.HodEmpId,
                    HodEmpName = et.HodEmp != null ? FullName(et.HodEmp.FirstName, et.HodEmp.MiddleName, et.HodEmp.LastName) : "",
                    BranchId = et != null ? et.BranchId : 0,
                    BranchName = et != null ? (et.Branch != null ? et.Branch.Name : "") : "",
                    DepartmentId = et != null ? et.DepartmentId : 0,
                    DepartmentName = et != null ? (et.Department != null ? et.Department.Name : "") : "",
                    DesignationId = et != null ? et.DesignationId : 0,
                    DesignationName = et != null ? (et.Designation != null ? et.Designation.Name : "") : "",
                    GradeId = et != null ? et.GradeId : 0,
                    GradeName = et != null ? (et.Grade != null ? et.Grade.Name : "") : "",
                }
            )
            .AsQueryable();

            if (!string.IsNullOrEmpty(empCode))
            {
                query = query.Where(x => x.EmpCode!.ToLower().Contains(empCode.ToLower()));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name!.ToLower().Contains(name.ToLower()));
            }

            if (branchId != null)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (departmentId != null)
            {
                query = query.Where(x => x.DepartmentId == departmentId);
            }

            if (designationId != null)
            {
                query = query.Where(x => x.DesignationId == designationId);
            }

            if (gradeId != null)
            {
                query = query.Where(x => x.GradeId == gradeId);
            }

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.EmpId);
            }
            else if (sortDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderBy(x => x.EmpCode);
                        break;

                    case "Name":
                        query.OrderBy(x => x.Name);
                        break;

                    case "BranchName":
                        query.OrderBy(x => x.BranchId);
                        break;

                    case "DepartmentName":
                        query.OrderBy(x => x.DepartmentId);
                        break;

                    case "DesignationName":
                        query.OrderBy(x => x.DesignationId);
                        break;

                    case "GradeName":
                        query.OrderBy(x => x.GradeName);
                        break;

                    case "Email":
                        query.OrderBy(x => x.Email);
                        break;

                    default:
                        query.OrderBy(x => x.EmpId);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderByDescending(x => x.EmpCode);
                        break;

                    case "Name":
                        query.OrderByDescending(x => x.Name);
                        break;

                    case "BranchName":
                        query.OrderByDescending(x => x.BranchId);
                        break;

                    case "DepartmentName":
                        query.OrderByDescending(x => x.DepartmentId);
                        break;

                    case "DesignationName":
                        query.OrderByDescending(x => x.DesignationId);
                        break;

                    case "GradeName":
                        query.OrderByDescending(x => x.GradeName);
                        break;

                    case "Email":
                        query.OrderBy(x => x.Email);
                        break;

                    default:
                        query.OrderByDescending(x => x.EmpId);
                        break;
                }
            }

            var TotalCount = await query.CountAsync();
            var TotalPages = (int)Math.Ceiling(TotalCount / (double)page);
            var data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return Ok(new
            {
                Data = data,
                TotalPages,
                TotalCount
            });
        }
        
        //[HttpGet("Directory/{id}")]
        //public async Task<IActionResult> GetDirectory(int id)
        //{
        //    var data = await _context.EmpLogs
        //    .Where(x => x.EmployeeId == id)
        //    .Join(_context.EmpDetails,
        //        el => el.EmployeeId,
        //        ed => ed.Id,
        //        (el, ed) => new
        //        {
        //            EmpId = ed.Id,
        //            PId = el.Id,
        //            Name = FullName(ed.FirstName, ed.MiddleName, ed.LastName),
        //            EmpCode = ed.CardId,
        //            ed.Email,
        //            ed.BloodGroup,
        //            ed.ContactNumber,
        //            ed.EmergencyContactNumber,
        //            ed.EmergencyContactPerson,
        //        }
        //    )
        //    .Join(_context.EmpTransactions,
        //        e => e.PId,
        //        et => et.Id,
        //        (e, et) => new
        //        {
        //            e.EmpId,
        //            e.PId,
        //            e.Name,
        //            e.EmpCode,
        //            e.Email,
        //            e.BloodGroup,
        //            e.ContactNumber,
        //            e.EmergencyContactPerson,
        //            e.EmergencyContactNumber,
        //            et.RmEmpId,
        //            RmEmpName = et.RmEmp != null ? FullName(et.RmEmp.FirstName, et.RmEmp.MiddleName, et.RmEmp.LastName) : "",
        //            et.HodEmpId,
        //            HodEmpName = et.HodEmp != null ? FullName(et.HodEmp.FirstName, et.HodEmp.MiddleName, et.HodEmp.LastName) : "",
        //            BranchId = et != null ? et.BranchId : 0,
        //            BranchName = et != null ? (et.Branch != null ? et.Branch.Name : "") : "",
        //            DepartmentId = et != null ? et.DepartmentId : 0,
        //            DepartmentName = et != null ? (et.Department != null ? et.Department.Name : "") : "",
        //            DesignationId = et != null ? et.DesignationId : 0,
        //            DesignationName = et != null ? (et.Designation != null ? et.Designation.Name : "") : "",
        //            GradeId = et != null ? et.GradeId : 0,
        //            GradeName = et != null ? (et.Grade != null ? et.Grade.Name : "") : "",
                    
        //        }
        //    )
        //    .FirstOrDefaultAsync();

        //    return Ok(new
        //    {
        //        Data = data
        //    });
        //}

        // GET ALL Birthday EMP
        [HttpGet("Birthdays")]
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

        private static string FullName(string firstName, string middleName, string lastName)
        {
            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
        }
    }
}
