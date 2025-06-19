using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql.Internal.TypeHandlers;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Ocsp;
using System.Collections.Generic;

namespace Hrms.EmpApi.Controllers
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

        [HttpGet()]
        public async Task<IActionResult> Get(int? departmentId, int? branchId)
        {
            int? empId = await _userManager.Users.Where(x => x.Id == User.GetUserId()).Select(x => x.EmpId).FirstOrDefaultAsync();

            var query = _context.EmpLogs
                .Join(_context.EmpTransactions.Where(x => x.RmEmpId == empId || x.HodEmpId == empId),
                el => el.Id,
                et => et.Id,
                (el, et) => new
                {
                    PId = el.Id,
                    EmpId = el.EmployeeId,
                    DepartmentId = et.DepartmentId,
                    BranchId = et.BranchId
                })
                .Join(_context.EmpDetails,
                elt => elt.EmpId,
                ed => ed.Id,
                (elt, ed) => new
                {
                    EmpId = elt.EmpId,
                    PId = elt.PId,
                    Name = Helper.FullName(ed.FirstName, ed.MiddleName, ed.LastName),
                    EmpCode = ed.CardId,
                    DepartmentId = elt.DepartmentId,
                    BranchId = elt.BranchId
                })
                .AsQueryable();

            if (departmentId is not null)
            {
                query = query.Where(x => x.DepartmentId == departmentId);
            }

            if (branchId is not null)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            var data = await query.ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAll(int? departmentId, int? branchId)
        {
            var query = _context.EmpLogs
                .Join(_context.EmpTransactions,
                el => el.Id,
                et => et.Id,
                (el, et) => new
                {
                    PId = el.Id,
                    EmpId = el.EmployeeId,
                    DepartmentId = et.DepartmentId,
                    DepartmentName = et.Department != null ? et.Department.Name : null,
                    BranchId = et.BranchId,
                    BranchName = et.Branch != null ? et.Branch.Name : null,
                    DesignationId = et.DesignationId,
                    DesignationName = et.Designation != null ? et.Designation.Name : null
                })
                .Join(_context.EmpDetails,
                elt => elt.EmpId,
                ed => ed.Id,
                (elt, ed) => new
                {
                    EmpId = elt.EmpId,
                    PId = elt.PId,
                    Name = Helper.FullName(ed.FirstName, ed.MiddleName, ed.LastName),
                    EmpCode = ed.CardId,
                    DepartmentId = elt.DepartmentId,
                    DepartmentName = elt.DepartmentName,
                    BranchId = elt.BranchId,
                    BranchName = elt.BranchName,
                    DesignationId = elt.DesignationId,
                    DesignationName = elt.DesignationName
                })
                .AsQueryable();

            if (departmentId is not null)
            {
                query = query.Where(x => x.DepartmentId == departmentId);
            }

            if (branchId is not null)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            var data = await query.ToListAsync();

            return Ok(new
            {
                Data = data
            });
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
                    ed.BloodGroup
                }
            )
            .Join(_context.EmpTransactions,
                e => e.PId,
                et => et.Id,
                (e, et) => new
                {
                    e.EmpId,
                    e.PId,
                    e.EmpCode,
                    e.Name,
                    e.Email,
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

        [HttpGet("Directory/{id}")]
        public async Task<IActionResult> GetDirectory(int id)
        {
            var data = await _context.EmpLogs
            .Where(x => x.EmployeeId == id)
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
                    ed.BloodGroup
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
            .FirstOrDefaultAsync();

            return Ok(new
            {
                Data = data
            });
        }


        [HttpGet("Wishtype")]
        public async Task<IActionResult> GetByType(string? typeofwish, DateTime? todaydate)
        {
            var todaydata = await _context.Wishlist
               .Where(x => x.Wish_Date != null ?
                                   x.Wish_Date.Value.Month == DateTime.UtcNow.Month && x.Wish_Date.Value.Day == DateTime.UtcNow.Day
                                   : false).ToListAsync();

            Wishlist data = null;
            if (string.Equals(typeofwish, "birthday", StringComparison.OrdinalIgnoreCase) || string.Equals(typeofwish, "Anniversaryday", StringComparison.OrdinalIgnoreCase))
            {
                data = await _context.Wishlist.FirstOrDefaultAsync(c => c.Wish_Type.ToLower() == typeofwish.ToLower());
            }
            else if (string.Equals(typeofwish, "Occasion", StringComparison.OrdinalIgnoreCase) && todaydate == DateTime.Now.Date)
            {
                return Ok(new
                {
                    Wishlist = todaydata
                });

            }

            if (data != null)
            {
                return Ok(new
                {
                    Wishlist = data
                });
            }

            else
            {
                return BadRequest();
            }
        }

        // GET ALL Birthday EMP
        [HttpGet("Birthdays")]
        public async Task<IActionResult> GetAllBirthdays()
        {

            var query = _context.EmpLogs
            .Join(_context.EmpDetails
            .Where(x => x.DateOfBirth != null ?
                                    x.DateOfBirth.Value.Month == DateTime.UtcNow.Month && x.DateOfBirth.Value.Day == DateTime.UtcNow.Day
                                    : false)
            ,
                el => el.EmployeeId,
                ed => ed.Id,
                (el, ed) => new
                {
                    EmpId = ed.Id,
                    PId = el.Id,
                    Name = FullName(ed.FirstName, ed.MiddleName, ed.LastName),
                }
            ).Join(_context.EmpTransactions, el => el.PId, et => et.Id, (el, et) => new
            {
                el.PId,
                el.EmpId,
                el.Name,
                et.DesignationId
            }).Join(_context.Designations, el => el.DesignationId, ed => ed.Id, (el, ed) => new
            {

                el.PId,
                el.EmpId,
                el.Name,
                el.DesignationId,
                DesignationName = ed.Name
            });


           

            return Ok(new
            {
                Data = query.ToList(),
            });
        }

        [HttpGet("MarriageAnniversary")]
        public async Task<IActionResult> GetAllAnniversary()
        {
            var query = _context.EmpLogs
           .Join(_context.EmpDetails
           .Where(x => x.MarriageDate != null ?
                                    x.MarriageDate.Value.Month == DateTime.UtcNow.Month && x.MarriageDate.Value.Day == DateTime.UtcNow.Day
                                    : false)
           ,
               el => el.EmployeeId,
               ed => ed.Id,
               (el, ed) => new
               {
                   EmpId = ed.Id,
                   PId = el.Id,
                   Name = FullName(ed.FirstName, ed.MiddleName, ed.LastName),
               }
           ).Join(_context.EmpTransactions, el => el.PId, et => et.Id, (el, et) => new
           {
               el.PId,
               el.EmpId,
               el.Name,
               et.DesignationId
           }).Join(_context.Designations, el => el.DesignationId, ed => ed.Id, (el, ed) => new
           {

               el.PId,
               el.EmpId,
               el.Name,
               el.DesignationId,
               DesignationName = ed.Name
           }).Distinct();


         
            return Ok(new
            {
                Data = query
            });
        }

        [HttpGet("HolidayCalendar")]
        public async Task<IActionResult> ViewEmpCalendarHoliday(int page, int limit, string sortColumn, string sortDirection, int? year)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!await _context.EmpDetails.AnyAsync(x => x.Id == user.EmpId))
            {
                return ErrorHelper.ErrorResult("Id", "Employee does not exist.");
            }

            var empCalendar = await _context.EmpCalendars
                .Include(x => x.Calendar)
                .Include(x => x.Emp)
                .FirstOrDefaultAsync(x => x.EmpId == user.EmpId);

            if (empCalendar is null)
            {
                return ErrorHelper.ErrorResult("Id", "No Calendar assigned.");
            }

            var query = await _context.HolidayCalendars
                    .Include(g => g.Holiday)
                    .Where(x => x.CalendarId == empCalendar.CalendarId).ToListAsync();

            if (year is null || year is 0)
            {
                year = DateTime.Now.Year;
            }

            query = query.Where(w => w.Holiday.Date.Value.Year == year).ToList();


            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.HolidayId).ToList();
            }
            else if (sortDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "Occasion":
                        query.OrderBy(x => x.Holiday.Name);
                        break;

                    case "Date":
                        query.OrderBy(x => x.Holiday.Date);
                        break;

                    case "Day":
                        query.OrderBy(x => x.Holiday.Day);
                        break;

                    case "DayType":
                        query.OrderBy(x => x.Holiday.DayType);
                        break;

                    case "HolidayType":
                        query.OrderBy(x => x.Holiday.Type);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "Occasion":
                        query.OrderByDescending(x => x.Holiday.Name);
                        break;

                    case "Date":
                        query.OrderByDescending(x => x.Holiday.Date);
                        break;

                    case "Day":
                        query.OrderByDescending(x => x.Holiday.Day);
                        break;

                    case "DayType":
                        query.OrderByDescending(x => x.Holiday.DayType);
                        break;

                    case "HolidayType":
                        query.OrderByDescending(x => x.Holiday.Type);
                        break;
                }
            }


            var TotalCount = query.Count();
            var TotalPages = (int)Math.Ceiling(TotalCount / (double)page);
            var data = query.Skip((page - 1) * limit).Take(limit).ToList();

            return Ok(new
            {
                Data = data.Select(h => new
                {
                    Occasion = h.Holiday.Name,
                    Date = h.Holiday.Date,
                    Day = h.Holiday.Day,
                    DayType = Enumeration.GetAll<HolidayDayType>().Where(x => x.Id == h.Holiday.DayType).FirstOrDefault().Name,
                    HolidayType = Enumeration.GetAll<HolidayType>().Where(x => x.Id == h.Holiday.Type).FirstOrDefault().Name,
                }).ToList(),
                TotalPages,
                TotalCount
            });

        }

        private static string FullName(string firstName, string middleName, string lastName)
        {
            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
        }
    }
}
