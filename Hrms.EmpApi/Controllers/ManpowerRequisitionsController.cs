using Hrms.Common.Models;
using ICSharpCode.SharpZipLib.Zip.Compression;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NPOI.OpenXmlFormats.Dml.Diagram;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class ManpowerRequisitionsController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public ManpowerRequisitionsController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ManpowerRequisitions
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, 
            string status, string jobTitle, string startingDate, int? departmentId, int? designationId, int? branchId, int? reportingToEmpId)
        {
            var user = await _userManager.GetUserAsync(User);

            var query = _context.ManpowerRequisitions
                .Include(x => x.Department)
                .Include(x => x.Designation)
                .Include(x => x.Branch)
                .Include(x => x.EmploymentType)
                .Include(x => x.ReplacedEmp)
                .Include(x => x.ReportingToEmp)
                .Include(x => x.RequestedByEmp)
                .Include(x => x.ProcessedByUser)
                .Include(x => x.ProcessedByUser.Emp)
                .Where(x => x.RequestedByEmpId == user.EmpId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrEmpty(jobTitle))
            {
                query = query.Where(x => x.JobTitle!.ToLower().Contains(jobTitle.ToLower()));
            }

            if (!string.IsNullOrEmpty(startingDate))
            {
                DateOnly StartingDate = DateOnlyHelper.ParseDateOrNow(startingDate);

                query = query.Where(x => x.StartingDate == StartingDate);
            }

            if (departmentId != null)
            {
                query = query.Where(x => x.DepartmentId == departmentId);
            }

            if (designationId != null)
            {
                query = query.Where(x => x.DesignationId == designationId);
            }

            if (branchId != null)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (reportingToEmpId != null)
            {
                query = query.Where(x => x.ReportingToEmpId == reportingToEmpId);
            }

            Expression<Func<ManpowerRequisition, object>> field = sortColumn switch
            {
                "JobTitle" => x => x.JobTitle,
                "Startingdate" => x => x.StartingDate!,
                "Status" => x => x.Status,
                _ => x => x.Id,
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Id);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<ManpowerRequisition>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    JobTitle = x.JobTitle,
                    StartingDate = x.StartingDate,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department?.Name,
                    DesignationId = x.DesignationId,
                    DesignationName = x.Designation?.Name,
                    BrnachId = x.BranchId,
                    BranchName = x.Branch?.Name,
                    EmploymentTypeId = x.EmploymentTypeId,
                    EmploymentTypeName = x.EmploymentType?.Name,
                    ReplacedEmpId = x.ReplacedEmpId,
                    ReplacedEmpCode = x.ReplacedEmp?.CardId,
                    ReplacedEmpName = Helper.FullName(x.ReplacedEmp?.FirstName,x.ReplacedEmp?.MiddleName, x.ReplacedEmp?.LastName),
                    ReportingToEmpId = x.ReportingToEmpId,
                    ReportingToEmpName = Helper.FullName(x.ReportingToEmp?.FirstName, x.ReportingToEmp?.MiddleName, x.ReportingToEmp?.LastName),
                    ReportingToEmpCode = x.ReportingToEmp?.CardId,
                    Status = x.Status,
                    RequestedByEmpId = x.RequestedByEmpId,
                    RequestedByEmpName = Helper.FullName(x.RequestedByEmp?.FirstName, x.RequestedByEmp?.MiddleName, x.RequestedByEmp?.LastName),
                    RequestedByEmpCode = x.RequestedByEmp?.CardId,
                    ProcessedByEmpId = x.ProcessedByUser?.EmpId,
                    ProcessedByEmpName = Helper.FullName(x.ProcessedByUser?.Emp?.FirstName, x.ProcessedByUser?.Emp?.MiddleName, x.ProcessedByUser?.Emp?.LastName),
                    ProcessedByEmpCode = x.ProcessedByUser?.Emp?.CardId,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: ManpowerRequisitions
        [HttpGet("Request")]
        public async Task<IActionResult> GetRequest(int page, int limit, string sortColumn, string sortDirection, 
            string jobTitle, string startingDate, int? departmentId, int? designationId, int? branchId, int? reportingToEmpId)
        {
            var user = await _userManager.GetUserAsync(User);

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

            var query = _context.ManpowerRequisitions
                .Include(x => x.Department)
                .Include(x => x.Designation)
                .Include(x => x.Branch)
                .Include(x => x.EmploymentType)
                .Include(x => x.ReplacedEmp)
                .Include(x => x.ReportingToEmp)
                .Include(x => x.RequestedByEmp)
                .Include(x => x.ProcessedByUser)
                .Include(x => x.ProcessedByUser.Emp)
                .Where(x => x.Status == "pending" && subordinateIds.Contains(x.RequestedByEmpId))
                .AsQueryable();

            if (!string.IsNullOrEmpty(jobTitle))
            {
                query = query.Where(x => x.JobTitle!.ToLower().Contains(jobTitle.ToLower()));
            }

            if (!string.IsNullOrEmpty(startingDate))
            {
                DateOnly StartingDate = DateOnlyHelper.ParseDateOrNow(startingDate);

                query = query.Where(x => x.StartingDate == StartingDate);
            }

            if (departmentId != null)
            {
                query = query.Where(x => x.DepartmentId == departmentId);
            }

            if (designationId != null)
            {
                query = query.Where(x => x.DesignationId == designationId);
            }

            if (branchId != null)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (reportingToEmpId != null)
            {
                query = query.Where(x => x.ReportingToEmpId == reportingToEmpId);
            }

            Expression<Func<ManpowerRequisition, object>> field = sortColumn switch
            {
                "JobTitle" => x => x.JobTitle,
                "Startingdate" => x => x.StartingDate!,
                "Status" => x => x.Status,
                _ => x => x.Id,
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Id);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<ManpowerRequisition>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    JobTitle = x.JobTitle,
                    StartingDate = x.StartingDate,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department?.Name,
                    DesignationId = x.DesignationId,
                    DesignationName = x.Designation?.Name,
                    BrnachId = x.BranchId,
                    BranchName = x.Branch?.Name,
                    EmploymentTypeId = x.EmploymentTypeId,
                    EmploymentTypeName = x.EmploymentType?.Name,
                    ReplacedEmpId = x.ReplacedEmpId,
                    ReplacedEmpCode = x.ReplacedEmp?.CardId,
                    ReplacedEmpName = Helper.FullName(x.ReplacedEmp?.FirstName, x.ReplacedEmp?.MiddleName, x.ReplacedEmp?.LastName),
                    ReportingToEmpId = x.ReportingToEmpId,
                    ReportingToEmpName = Helper.FullName(x.ReportingToEmp?.FirstName, x.ReportingToEmp?.MiddleName, x.ReportingToEmp?.LastName),
                    ReportingToEmpCode = x.ReportingToEmp?.CardId,
                    Status = x.Status,
                    RequestedByEmpId = x.RequestedByEmpId,
                    RequestedByEmpName = Helper.FullName(x.RequestedByEmp?.FirstName, x.RequestedByEmp?.MiddleName, x.RequestedByEmp?.LastName),
                    RequestedByEmpCode = x.RequestedByEmp?.CardId,
                    ProcessedByEmpId = x.ProcessedByUser?.EmpId,
                    ProcessedByEmpName = Helper.FullName(x.ProcessedByUser?.Emp?.FirstName, x.ProcessedByUser?.Emp?.MiddleName, x.ProcessedByUser?.Emp?.LastName),
                    ProcessedByEmpCode = x.ProcessedByUser?.Emp?.CardId,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: ManpowerRequisitions
        [HttpGet("Subordinate/History")]
        public async Task<IActionResult> GetSubordinateHistory(int page, int limit, string sortColumn, string sortDirection,
            string status, int? requestedByEmpId, string jobTitle, string startingDate, int? departmentId, int? designationId, int? branchId, int? reportingToEmpId)
        {
            var user = await _userManager.GetUserAsync(User);

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

            var query = _context.ManpowerRequisitions
                .Include(x => x.Department)
                .Include(x => x.Designation)
                .Include(x => x.Branch)
                .Include(x => x.EmploymentType)
                .Include(x => x.ReplacedEmp)
                .Include(x => x.ReportingToEmp)
                .Include(x => x.RequestedByEmp)
                .Include(x => x.ProcessedByUser)
                .Include(x => x.ProcessedByUser.Emp)
                .Where(x => subordinateIds.Contains(x.RequestedByEmpId))
                .AsQueryable();

            if (requestedByEmpId is not null)
            {
                if (!subordinateIds.Contains(requestedByEmpId))
                {
                    return ErrorHelper.ErrorResult("Id", "Employee is not a subordinate.");
                }

                query = query.Where(x => x.RequestedByEmpId == requestedByEmpId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrEmpty(jobTitle))
            {
                query = query.Where(x => x.JobTitle!.ToLower().Contains(jobTitle.ToLower()));
            }

            if (!string.IsNullOrEmpty(startingDate))
            {
                DateOnly StartingDate = DateOnlyHelper.ParseDateOrNow(startingDate);

                query = query.Where(x => x.StartingDate == StartingDate);
            }

            if (departmentId != null)
            {
                query = query.Where(x => x.DepartmentId == departmentId);
            }

            if (designationId != null)
            {
                query = query.Where(x => x.DesignationId == designationId);
            }

            if (branchId != null)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (reportingToEmpId != null)
            {
                query = query.Where(x => x.ReportingToEmpId == reportingToEmpId);
            }

            Expression<Func<ManpowerRequisition, object>> field = sortColumn switch
            {
                "JobTitle" => x => x.JobTitle,
                "Startingdate" => x => x.StartingDate!,
                "Status" => x => x.Status,
                _ => x => x.Id,
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Id);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<ManpowerRequisition>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    JobTitle = x.JobTitle,
                    StartingDate = x.StartingDate,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department?.Name,
                    DesignationId = x.DesignationId,
                    DesignationName = x.Designation?.Name,
                    BrnachId = x.BranchId,
                    BranchName = x.Branch?.Name,
                    EmploymentTypeId = x.EmploymentTypeId,
                    EmploymentTypeName = x.EmploymentType?.Name,
                    ReplacedEmpId = x.ReplacedEmpId,
                    ReplacedEmpCode = x.ReplacedEmp?.CardId,
                    ReplacedEmpName = Helper.FullName(x.ReplacedEmp?.FirstName, x.ReplacedEmp?.MiddleName, x.ReplacedEmp?.LastName),
                    ReportingToEmpId = x.ReportingToEmpId,
                    ReportingToEmpName = Helper.FullName(x.ReportingToEmp?.FirstName, x.ReportingToEmp?.MiddleName, x.ReportingToEmp?.LastName),
                    ReportingToEmpCode = x.ReportingToEmp?.CardId,
                    Status = x.Status,
                    RequestedByEmpId = x.RequestedByEmpId,
                    RequestedByEmpName = Helper.FullName(x.RequestedByEmp?.FirstName, x.RequestedByEmp?.MiddleName, x.RequestedByEmp?.LastName),
                    RequestedByEmpCode = x.RequestedByEmp?.CardId,
                    ProcessedByEmpId = x.ProcessedByUser?.EmpId,
                    ProcessedByEmpName = Helper.FullName(x.ProcessedByUser?.Emp?.FirstName, x.ProcessedByUser?.Emp?.MiddleName, x.ProcessedByUser?.Emp?.LastName),
                    ProcessedByEmpCode = x.ProcessedByUser?.Emp?.CardId,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        //GET : ManpowerRequisitions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var data = await _context.ManpowerRequisitions
                .Include(x => x.Department)
                .Include(x => x.Designation)
                .Include(x => x.Branch)
                .Include(x => x.EmploymentType)
                .Include(x => x.ReplacedEmp)
                .Include(x => x.ReportingToEmp)
                .Include(x => x.RequestedByEmp)
                .Include(x => x.ProcessedByUser)
                .Include(x => x.ProcessedByUser.Emp)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                City = new
                {
                    Id = data.Id,
                    JobTitle = data.JobTitle,
                    StartingDate = data.StartingDate,
                    DepartmentId = data.DepartmentId,
                    DepartmentName = data.Department?.Name,
                    DesignationId = data.DesignationId,
                    DesignationName = data.Designation?.Name,
                    BrnachId = data.BranchId,
                    BranchName = data.Branch?.Name,
                    ReportingToEmpId = data.ReportingToEmpId,
                    ReportingToEmpName = Helper.FullName(data.ReportingToEmp?.FirstName, data.ReportingToEmp?.MiddleName, data.ReportingToEmp?.LastName),
                    Quantity = data.Quantity,
                    IsGenderSpecific = data.IsGenderSpecific,
                    Gender = data.Gender,
                    EmploymentNatureId = data.EmploymentNature,
                    EmploymentNatureName = Enumeration.GetAll<EmploymentNature>().Where(x => x.Id == data.EmploymentNature).FirstOrDefault().Name,
                    ReplacedEmpId = data.ReplacedEmpId,
                    ReplacedEmpName = Helper.FullName(data.ReplacedEmp?.FirstName, data.ReplacedEmp?.MiddleName, data.ReplacedEmp?.LastName),
                    ReplacedEmpCode = data.ReplacedEmp?.CardId,
                    EmploymentTypeId = data.EmploymentTypeId,
                    EmploymentTypeName = data.EmploymentType?.Name,
                    Qualifications = data.Qualifications,
                    Experience = data.Experience,
                    KeyCompetencies = data.KeyCompetencies,
                    JobDescription = data.JobDescription,
                    SalaryRangeFrom = data.SalaryRangeFrom,
                    SalaryRangeTo = data.SalaryRangeTo,
                    Status = data.Status,
                    RequestedByEmpId = data.RequestedByEmpId,
                    RequestedByEmpName = Helper.FullName(data.RequestedByEmp?.FirstName, data.RequestedByEmp?.MiddleName, data.RequestedByEmp?.LastName),
                    RequestedByEmpCode = data.RequestedByEmp?.CardId,
                    ProcessedByEmpId = data.ProcessedByUser?.EmpId,
                    ProcessedByEmpName = Helper.FullName(data.ProcessedByUser?.Emp?.FirstName, data.ProcessedByUser?.Emp?.MiddleName, data.ProcessedByUser?.Emp?.LastName),
                    ProcessedByEmpCode = data.ProcessedByUser?.Emp?.CardId,
                    CreatedAt = data.CreatedAt,
                    UpdatedAt = data.UpdatedAt
                }
            });
        }

        // Post: ManpowerRequisition/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input) 
        {
            var user = await _userManager.GetUserAsync(User);

            DateOnly? startingDate = null;

            if (!string.IsNullOrEmpty(input.StartingDate))
            {
                startingDate = DateOnlyHelper.ParseDateOrNow(input.StartingDate);
            }

            ManpowerRequisition data = new()
            {
                JobTitle = input.JobTitle,
                StartingDate = startingDate,
                DepartmentId = input.DepartmentId,
                DesignationId = input.DesignationId,
                BranchId = input.BranchId,
                ReportingToEmpId = input.ReportingToEmpId,
                Quantity = input.Quantity ?? 0,
                IsGenderSpecific = input.IsGenderSpecific ?? false,
                Gender = input.IsGenderSpecific == true ? input.Gender : null,
                EmploymentNature = input.EmploymentNature,
                ReplacedEmpId = input.EmploymentNature == "replacement" ? input.ReplacedEmpId : null,
                EmploymentTypeId = input.EmploymentTypeId,
                Qualifications = input.Qualifications,
                Experience = input.Experience,
                KeyCompetencies = input.KeyCompetencies,
                JobDescription = input.JobDescription,
                SalaryRangeFrom = input.SalaryRangeFrom,
                SalaryRangeTo = input.SalaryRangeTo,
                Status = "pending",
                RequestedByEmpId = user.EmpId ?? 0
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: WeekendDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.ManpowerRequisitions.FirstOrDefaultAsync(c => c.Id == id);

            if (data.Status != "pending")
            {
                return ErrorHelper.ErrorResult("Id", "Request is already processed.");
            }

            DateOnly? startingDate = null;

            if (!string.IsNullOrEmpty(input.StartingDate))
            {
                startingDate = DateOnlyHelper.ParseDateOrNow(input.StartingDate);
            }

            data.JobTitle = input.JobTitle;
            data.StartingDate = startingDate;
            data.DepartmentId = input.DepartmentId;
            data.DesignationId = input.DesignationId;
            data.BranchId = input.BranchId;
            data.ReportingToEmpId = input.ReportingToEmpId;
            data.Quantity = input.Quantity ?? 0;
            data.IsGenderSpecific = input.IsGenderSpecific ?? false;
            data.Gender = input.IsGenderSpecific == true ? input.Gender : null;
            data.EmploymentNature = input.EmploymentNature;
            data.ReplacedEmpId = input.EmploymentNature == "replacement" ? input.ReplacedEmpId : null;
            data.EmploymentTypeId = input.EmploymentTypeId;
            data.Qualifications = input.Qualifications;
            data.Experience = input.Experience;
            data.KeyCompetencies = input.KeyCompetencies;
            data.JobDescription = input.JobDescription;
            data.SalaryRangeFrom = input.SalaryRangeFrom;
            data.SalaryRangeTo = input.SalaryRangeTo;
            data.Status = "pending";

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: WeekendDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.ManpowerRequisitions.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (data.Status != "pending")
            {
                return ErrorHelper.ErrorResult("Id", "Request is already processed.");
            }
            
            _context.ManpowerRequisitions.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var user = await _userManager.GetUserAsync(User);

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

            var manpowerRequisition = await _context.ManpowerRequisitions.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (manpowerRequisition == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (!subordinateIds.Contains(manpowerRequisition.RequestedByEmpId))
            {
                return Forbid();
            }

            manpowerRequisition.Status = "approved";
            manpowerRequisition.ProcessedByUserId = User.GetUserId();
            manpowerRequisition.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Disapprove/{id}")]
        public async Task<IActionResult> Disapprove(int id)
        {
            var user = await _userManager.GetUserAsync(User);

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

            var manpowerRequisition = await _context.ManpowerRequisitions.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (manpowerRequisition == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (!subordinateIds.Contains(manpowerRequisition.RequestedByEmpId))
            {
                return Forbid();
            }

            manpowerRequisition.Status = "disapproved";
            manpowerRequisition.ProcessedByUserId = User.GetUserId();
            manpowerRequisition.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string JobTitle { get; set; }
            public string StartingDate { get; set; }
            public int? DepartmentId { get; set; }
            public short? DesignationId { get; set; }
            public short? BranchId { get; set; }
            public int? ReportingToEmpId { get; set; }
            public int? Quantity { get; set; }
            public bool? IsGenderSpecific { get; set; }
            public string Gender { get; set; }
            public string EmploymentNature { get; set; }
            public int? ReplacedEmpId { get; set; }
            public int? EmploymentTypeId { get; set; }
            public string Qualifications { get; set; }
            public string Experience { get; set; }
            public string KeyCompetencies { get; set; }
            public string JobDescription { get; set; }
            public double? SalaryRangeFrom { get; set; }
            public double? SalaryRangeTo { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.JobTitle)
                    .NotEmpty();

                RuleFor(x => x.StartingDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterNow();

                RuleFor(x => x.DepartmentId)
                    .NotEmpty()
                    .IdMustExist(_context.Departments.AsQueryable());

                RuleFor(x => x.DesignationId)
                    .NotEmpty()
                    .IdMustExist(_context.Designations.AsQueryable());

                RuleFor(x => x.BranchId)
                    .NotEmpty()
                    .IdMustExist(_context.Branches.AsQueryable());

                RuleFor(x => x.ReportingToEmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.Quantity)
                    .NotEmpty()
                    .GreaterThan(0);

                RuleFor(x => x.IsGenderSpecific)
                    .NotEmpty();

                RuleFor(x => x.Gender)
                    .NotEmpty()
                    .Unless(x => x.IsGenderSpecific == false);

                RuleFor(x => x.EmploymentNature)
                    .NotEmpty()
                    .MustBeIn(Enumeration.GetAll<EmploymentNature>().Select(x => x.Id).ToList());

                RuleFor(x => x.ReplacedEmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable())
                    .Unless(x => x.EmploymentNature != "replacement");

                RuleFor(x => x.EmploymentTypeId)
                    .NotEmpty()
                    .IdMustExist(_context.EmploymentTypes.AsQueryable());

                RuleFor(x => x.Qualifications)
                    .NotEmpty();

                RuleFor(x => x.Experience)
                    .NotEmpty();

                RuleFor(x => x.KeyCompetencies)
                    .NotEmpty();

                RuleFor(x => x.JobDescription)
                    .NotEmpty();

                RuleFor(x => x.SalaryRangeFrom)
                    .NotEmpty()
                    .GreaterThan(0);

                RuleFor(x => x.SalaryRangeTo)
                    .NotEmpty()
                    .GreaterThan(x => x.SalaryRangeFrom);
            }
        }

        public class UpdateInputModelValidator : AbstractValidator<UpdateInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public UpdateInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

                RuleFor(x => x.JobTitle)
                    .NotEmpty();

                RuleFor(x => x.StartingDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterNow();

                RuleFor(x => x.DepartmentId)
                    .NotEmpty()
                    .IdMustExist(_context.Departments.AsQueryable());

                RuleFor(x => x.DesignationId)
                    .NotEmpty()
                    .IdMustExist(_context.Designations.AsQueryable());

                RuleFor(x => x.BranchId)
                    .NotEmpty()
                    .IdMustExist(_context.Branches.AsQueryable());

                RuleFor(x => x.ReportingToEmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.Quantity)
                    .NotEmpty()
                    .GreaterThan(0);

                RuleFor(x => x.IsGenderSpecific)
                    .NotEmpty();

                RuleFor(x => x.Gender)
                    .NotEmpty()
                    .Unless(x => x.IsGenderSpecific == false);

                RuleFor(x => x.EmploymentNature)
                    .NotEmpty()
                    .MustBeIn(Enumeration.GetAll<EmploymentNature>().Select(x => x.Id).ToList());

                RuleFor(x => x.ReplacedEmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable())
                    .Unless(x => x.EmploymentNature != "replacement");

                RuleFor(x => x.EmploymentTypeId)
                    .NotEmpty()
                    .IdMustExist(_context.EmploymentTypes.AsQueryable());

                RuleFor(x => x.Qualifications)
                    .NotEmpty();

                RuleFor(x => x.Experience)
                    .NotEmpty();

                RuleFor(x => x.KeyCompetencies)
                    .NotEmpty();

                RuleFor(x => x.JobDescription)
                    .NotEmpty();

                RuleFor(x => x.SalaryRangeFrom)
                    .NotEmpty()
                    .GreaterThan(0);

                RuleFor(x => x.SalaryRangeTo)
                    .NotEmpty()
                    .GreaterThan(x => x.SalaryRangeFrom);
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.ManpowerRequisitions.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
