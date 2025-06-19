using Hrms.Common.Enumerations;
using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.SS.Formula.Functions;
using System.Drawing;
using System.Runtime.InteropServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]

    public class RegularisationsController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public RegularisationsController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Regularisations/history
        [HttpGet("History")]
        public async Task<IActionResult> GetHistory(int page, int limit, string sortColumn, string sortDirection, int? regularisationTypeId, string fromDate, string toDate)
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

            var query = _context.Regularisations
                .Where(x => x.EmpId == user.EmpId)
                .Include(x => x.RegularisationType)
                .Include(x => x.Emp)
                .Include(x => x.GatePassType)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ApprovedByUser.Emp)
                .Include(x => x.DisapprovedByUser)
                .Include(x => x.DisapprovedByUser.Emp)
                .AsQueryable();

            if (regularisationTypeId is not null)
            {
                query = query.Where(x => x.RegularisationTypeId == regularisationTypeId);
            }

            if (!string.IsNullOrEmpty(fromDate))
            {
                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);

                query = query.Where(b => b.FromDate >= FromDate);
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

                query = query.Where(x => x.ToDate <= ToDate);
            }

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Id);
            }
            else if (sortDirection == "asc")
            {
                switch (sortColumn) 
                {
                    case "EmpCode":
                        query.OrderBy(x => x.Emp.CardId);
                        break;

                    case "RegularisationTypeName":
                        query.OrderBy(x => x.RegularisationType.Name);
                        break;

                    case "FromDate":
                        query.OrderBy(x => x.FromDate);
                        break;

                    case "ToDate":
                        query.OrderBy(x => x.ToDate);
                        break;

                    case "FromTime":
                        query.OrderBy(x => x.FromTime);
                        break;

                    case "ToTime":
                        query.OrderBy(x => x.ToTime);
                        break;

                    case "ProcessedDate":
                        query.OrderBy(x => x.UpdatedAt);
                        break;

                    case "RequestedDate":
                        query.OrderBy(x => x.CreatedAt);
                        break;

                    default:
                        query.OrderBy(x => x.Id);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderByDescending(x => x.Emp.CardId);
                        break;

                    case "RegularisationTypeName":
                        query.OrderByDescending(x => x.RegularisationType.Name);
                        break;

                    case "FromDate":
                        query.OrderByDescending(x => x.FromDate);
                        break;

                    case "ToDate":
                        query.OrderByDescending(x => x.ToDate);
                        break;

                    case "FromTime":
                        query.OrderByDescending(x => x.FromTime);
                        break;

                    case "ToTime":
                        query.OrderByDescending(x => x.ToTime);
                        break;

                    case "ProcessedDate":
                        query.OrderByDescending(x => x.UpdatedAt);
                        break;

                    case "RequestedDate":
                        query.OrderByDescending(x => x.CreatedAt);
                        break;

                    default:
                        query.OrderByDescending(x => x.Id);
                        break;
                }
            }

            var TotalCount = await query.CountAsync();
            var TotalPages = (int)Math.Ceiling(TotalCount / (double)page);
            var data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    EmpId = x.EmpId,
                    EmpCode = x.Emp.CardId,
                    GatePassTypeId = x.GatePassTypeId,
                    GatePassTypeName = x.GatePassType?.Name,
                    RegularisationTypeId = x.RegularisationTypeId,
                    RegularistaionTypeName = x.RegularisationType.DisplayName,
                    FromDate = x.FromDate,
                    ToDate = x.ToDate,
                    FromTime = x.FromTime.ToString(),
                    ToTime = x.ToTime.ToString(),
                    ContactNumber = x.ContactNumber,
                    Place = x.Place,
                    Reason = x.Reason,
                    Status = x.Status,
                    CancellationRemarks = x.CancellationRemarks,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByUserCode = x.ApprovedByUser?.UserName,
                    ApprovedByUserName = Helper.FullName(x.ApprovedByUser?.Emp?.FirstName, x.ApprovedByUser?.Emp?.MiddleName, x.ApprovedByUser?.Emp?.LastName),
                    DisapprovedByUserId = x.DisapprovedByUserId,
                    DisapprovedByUserCode = x.DisapprovedByUser?.UserName,
                    DisapprovedByUserName = Helper.FullName(x.DisapprovedByUser?.Emp?.FirstName, x.DisapprovedByUser?.Emp?.MiddleName, x.DisapprovedByUser?.Emp?.LastName),
                    Remarks = x.Remarks,
                    RequestedDate = x.CreatedAt,
                    ProcessedDate = x.UpdatedAt,
                }),
                TotalCount,
                TotalPages
            });
        }

        // GET: Regularisations/history/subordinates
        [HttpGet("History/Subordinate")]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, 
            int? regularisationTypeId, int? empId, string empName, string empCode, string fromDate, string toDate)
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

            var query = _context.Regularisations
                .Where(x => subordinateIds.Contains(x.EmpId))
                .Include(x => x.RegularisationType)
                .Include(x => x.Emp)
                .Include(x => x.GatePassType)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ApprovedByUser.Emp)
                .Include(x => x.DisapprovedByUser)
                .Include(x => x.DisapprovedByUser.Emp)
                .AsQueryable();

            if (regularisationTypeId is not null)
            {
                query = query.Where(x => x.RegularisationTypeId == regularisationTypeId);
            }

            if (!string.IsNullOrEmpty(empName))
            {
                query = query.Where(x => string.Concat(x.Emp.FirstName, x.Emp.MiddleName, x.Emp.LastName).ToLower().Contains(empName.Replace(" ", string.Empty).ToLower()));
            }

            if (!string.IsNullOrEmpty(empCode))
            {
                query = query.Where(x => x.Emp.CardId.ToLower().Contains(empCode.ToLower()));
            }

            if (empId is not null)
            {
                query = query.Where(x => x.EmpId == empId);
            }

            if (!string.IsNullOrEmpty(fromDate))
            {
                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);

                query = query.Where(b => b.FromDate >= FromDate);
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

                query = query.Where(x => x.ToDate <= ToDate);
            }

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Id);
            }
            else if (sortDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderBy(x => x.Emp.CardId);
                        break;

                    case "RegularisationTypeName":
                        query.OrderBy(x => x.RegularisationType.Name);
                        break;

                    case "FromDate":
                        query.OrderBy(x => x.FromDate);
                        break;

                    case "ToDate":
                        query.OrderBy(x => x.ToDate);
                        break;

                    case "FromTime":
                        query.OrderBy(x => x.FromTime);
                        break;

                    case "ToTime":
                        query.OrderBy(x => x.ToTime);
                        break;

                    case "ProcessedDate":
                        query.OrderBy(x => x.UpdatedAt);
                        break;

                    case "RequestedDate":
                        query.OrderBy(x => x.CreatedAt);
                        break;

                    default:
                        query.OrderBy(x => x.Id);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderByDescending(x => x.Emp.CardId);
                        break;

                    case "RegularisationTypeName":
                        query.OrderByDescending(x => x.RegularisationType.Name);
                        break;

                    case "FromDate":
                        query.OrderByDescending(x => x.FromDate);
                        break;

                    case "ToDate":
                        query.OrderByDescending(x => x.ToDate);
                        break;

                    case "FromTime":
                        query.OrderByDescending(x => x.FromTime);
                        break;

                    case "ToTime":
                        query.OrderByDescending(x => x.ToTime);
                        break;

                    case "ProcessedDate":
                        query.OrderByDescending(x => x.UpdatedAt);
                        break;

                    case "RequestedDate":
                        query.OrderByDescending(x => x.CreatedAt);
                        break;

                    default:
                        query.OrderByDescending(x => x.Id);
                        break;
                }
            }

            var TotalCount = await query.CountAsync();
            var TotalPages = (int)Math.Ceiling(TotalCount / (double)page);
            var data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    EmpId = x.EmpId,
                    EmpCode = x.Emp.CardId,
                    EmpName = Helper.FullName(x.Emp.FirstName, x.Emp.MiddleName, x.Emp.LastName),
                    GatePassTypeId = x.GatePassTypeId,
                    GatePassTypeName = x.GatePassType?.Name,
                    RegularisationTypeId = x.RegularisationType,
                    RegularisationTypeName = x.RegularisationType.DisplayName,
                    FromDate = x.FromDate,
                    ToDate = x.ToDate,
                    FromTime = x.FromTime.ToString(),
                    ToTime = x.ToTime.ToString(),
                    ContactNumber = x.ContactNumber,
                    Place = x.Place,
                    Reason = x.Reason,
                    Status = x.Status,
                    CancellationRemarks = x.CancellationRemarks,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByUserCode = x.ApprovedByUser?.UserName,
                    ApprovedByUserName = Helper.FullName(x.ApprovedByUser?.Emp?.FirstName, x.ApprovedByUser?.Emp?.MiddleName, x.ApprovedByUser?.Emp?.LastName),
                    DisapprovedByUserId = x.DisapprovedByUserId,
                    DisapprovedByUserCode = x.DisapprovedByUser?.UserName,
                    DisapprovedByUserName = Helper.FullName(x.DisapprovedByUser?.Emp?.FirstName, x.DisapprovedByUser?.Emp?.MiddleName, x.DisapprovedByUser?.Emp?.LastName),
                    Remarks = x.Remarks,
                    RequestedDate = x.UpdatedAt,
                    ProcessedDate = x.CreatedAt,
                }),
                TotalCount,
                TotalPages
            });
        }

        // GET: Regularisations/requests
        [HttpGet("Request")]
        public async Task<IActionResult> GetRequests(int page, int limit, string sortColumn, string sortDirection, int? regularisationTypeId, string fromDate, string toDate)
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

            var query = _context.Regularisations
                .Where(x => x.EmpId == user.EmpId && x.Status == "pending")
                .Include(x => x.RegularisationType)
                .Include(x => x.Emp)
                .Include(x => x.GatePassType)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ApprovedByUser.Emp)
                .Include(x => x.DisapprovedByUser)
                .Include(x => x.DisapprovedByUser.Emp)
                .AsQueryable();

            if (regularisationTypeId is not null)
            {
                query = query.Where(x => x.RegularisationTypeId == regularisationTypeId);
            }

            if (!string.IsNullOrEmpty(fromDate))
            {
                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);

                query = query.Where(b => b.FromDate >= FromDate);
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

                query = query.Where(x => x.ToDate <= ToDate);
            }

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Id);
            }
            else if (sortDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderBy(x => x.Emp.CardId);
                        break;

                    case "RegularisationTypeName":
                        query.OrderBy(x => x.RegularisationType.Name);
                        break;

                    case "FromDate":
                        query.OrderBy(x => x.FromDate);
                        break;

                    case "ToDate":
                        query.OrderBy(x => x.ToDate);
                        break;

                    case "FromTime":
                        query.OrderBy(x => x.FromTime);
                        break;

                    case "ToTime":
                        query.OrderBy(x => x.ToTime);
                        break;

                    case "ProcessedDate":
                        query.OrderBy(x => x.UpdatedAt);
                        break;

                    case "RequestedDate":
                        query.OrderBy(x => x.CreatedAt);
                        break;

                    default:
                        query.OrderBy(x => x.Id);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderByDescending(x => x.Emp.CardId);
                        break;

                    case "RegularisationTypeName":
                        query.OrderByDescending(x => x.RegularisationType.Name);
                        break;

                    case "FromDate":
                        query.OrderByDescending(x => x.FromDate);
                        break;

                    case "ToDate":
                        query.OrderByDescending(x => x.ToDate);
                        break;

                    case "FromTime":
                        query.OrderByDescending(x => x.FromTime);
                        break;

                    case "ToTime":
                        query.OrderByDescending(x => x.ToTime);
                        break;

                    case "ProcessedDate":
                        query.OrderByDescending(x => x.UpdatedAt);
                        break;

                    case "RequestedDate":
                        query.OrderByDescending(x => x.CreatedAt);
                        break;

                    default:
                        query.OrderByDescending(x => x.Id);
                        break;
                }
            }

            var TotalCount = await query.CountAsync();
            var TotalPages = (int)Math.Ceiling(TotalCount / (double)page);
            var data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    EmpId = x.EmpId,
                    EmpCode = x.Emp.CardId,
                    GatePassTypeId = x.GatePassTypeId,
                    GatePassTypeName = x.GatePassType?.Name,
                    RegularisationTypeId = x.RegularisationType,
                    RegularistaionTypeName = x.RegularisationType.DisplayName,
                    FromDate = x.FromDate,
                    ToDate = x.ToDate,
                    FromTime = x.FromTime.ToString(),
                    ToTime = x.ToTime.ToString(),
                    ContactNumber = x.ContactNumber,
                    Place = x.Place,
                    Reason = x.Reason,
                    Status = x.Status,
                    CancellationRemarks = x.CancellationRemarks,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByUserCode = x.ApprovedByUser?.UserName,
                    ApprovedByUserName = Helper.FullName(x.ApprovedByUser?.Emp?.FirstName, x.ApprovedByUser?.Emp?.MiddleName, x.ApprovedByUser?.Emp?.LastName),
                    DisapprovedByUserId = x.DisapprovedByUserId,
                    DisapprovedByUserCode = x.DisapprovedByUser?.UserName,
                    DisapprovedByUserName = Helper.FullName(x.DisapprovedByUser?.Emp?.FirstName, x.DisapprovedByUser?.Emp?.MiddleName, x.DisapprovedByUser?.Emp?.LastName),
                    Remarks = x.Remarks,
                    RequestedDate = x.CreatedAt,
                    ProcessedDate = x.UpdatedAt,
                }),
                TotalCount,
                TotalPages
            });
        }

        // GET: Regularisations/requests/subordinates
        [HttpGet("Request/Subordinate")]
        public async Task<IActionResult> GetSubordinateRequests(int page, int limit, string sortColumn, string sortDirection, 
            int? empId, string empName, string empCode, int? regularisationTypeId, string fromDate, string toDate)
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

            var query = _context.Regularisations
                .Where(x => subordinateIds.Contains(x.EmpId) && x.Status == "pending")
                .Include(x => x.Emp)
                .Include(x => x.RegularisationType)
                .Include(x => x.GatePassType)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ApprovedByUser.Emp)
                .Include(x => x.DisapprovedByUser)
                .Include(x => x.DisapprovedByUser.Emp)
                .AsQueryable();

            if (regularisationTypeId is not null)
            {
                query = query.Where(x => x.RegularisationTypeId == regularisationTypeId);
            }

            if (!string.IsNullOrEmpty(fromDate))
            {
                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);

                query = query.Where(b => b.FromDate >= FromDate);
            }

            if (!string.IsNullOrEmpty(empName))
            {
                query = query.Where(x => string.Concat(x.Emp.FirstName, x.Emp.MiddleName, x.Emp.LastName).ToLower().Contains(empName.Replace(" ", string.Empty).ToLower()));
            }

            if (!string.IsNullOrEmpty(empCode))
            {
                query = query.Where(x => x.Emp.CardId.ToLower().Contains(empCode.ToLower()));
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

                query = query.Where(x => x.ToDate <= ToDate);
            }

            if (empId is not null)
            {
                query = query.Where(x => x.EmpId == empId);
            }

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Id);
            }
            else if (sortDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderBy(x => x.Emp.CardId);
                        break;

                    case "RegularisationTypeName":
                        query.OrderBy(x => x.RegularisationType.Name);
                        break;

                    case "FromDate":
                        query.OrderBy(x => x.FromDate);
                        break;

                    case "ToDate":
                        query.OrderBy(x => x.ToDate);
                        break;

                    case "FromTime":
                        query.OrderBy(x => x.FromTime);
                        break;

                    case "ToTime":
                        query.OrderBy(x => x.ToTime);
                        break;

                    case "ProcessedDate":
                        query.OrderBy(x => x.UpdatedAt);
                        break;

                    case "RequestedDate":
                        query.OrderBy(x => x.CreatedAt);
                        break;

                    default:
                        query.OrderBy(x => x.Id);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderByDescending(x => x.Emp.CardId);
                        break;

                    case "RegularisationTypeName":
                        query.OrderByDescending(x => x.RegularisationType.Name);
                        break;

                    case "FromDate":
                        query.OrderByDescending(x => x.FromDate);
                        break;

                    case "ToDate":
                        query.OrderByDescending(x => x.ToDate);
                        break;

                    case "FromTime":
                        query.OrderByDescending(x => x.FromTime);
                        break;

                    case "ToTime":
                        query.OrderByDescending(x => x.ToTime);
                        break;

                    case "ProcessedDate":
                        query.OrderByDescending(x => x.UpdatedAt);
                        break;

                    case "RequestedDate":
                        query.OrderByDescending(x => x.CreatedAt);
                        break;

                    default:
                        query.OrderByDescending(x => x.Id);
                        break;
                }
            }

            var TotalCount = await query.CountAsync();
            var TotalPages = (int)Math.Ceiling(TotalCount / (double)page);
            var data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    EmpId = x.EmpId,
                    EmpCode = x.Emp.CardId,
                    EmpName = Helper.FullName(x.Emp.FirstName, x.Emp.MiddleName, x.Emp.LastName),
                    GatePassTypeId = x.GatePassTypeId,
                    GatePassTypeName = x.GatePassType?.Name,
                    RegularisationTypeId = x.RegularisationType,
                    RegularisationTypeName = x.RegularisationType.DisplayName,
                    FromDate = x.FromDate,
                    ToDate = x.ToDate,
                    FromTime = x.FromTime.ToString(),
                    ToTime = x.ToTime.ToString(),
                    ContactNumber = x.ContactNumber,
                    Place = x.Place,
                    Reason = x.Reason,
                    Status = x.Status,
                    CancellationRemarks = x.CancellationRemarks,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByUserCode = x.ApprovedByUser?.UserName,
                    ApprovedByUserName = Helper.FullName(x.ApprovedByUser?.Emp?.FirstName, x.ApprovedByUser?.Emp?.MiddleName, x.ApprovedByUser?.Emp?.LastName),
                    DisapprovedByUserId = x.DisapprovedByUserId,
                    DisapprovedByUserCode = x.DisapprovedByUser?.UserName,
                    DisapprovedByUserName = Helper.FullName(x.DisapprovedByUser?.Emp?.FirstName, x.DisapprovedByUser?.Emp?.MiddleName, x.DisapprovedByUser?.Emp?.LastName),
                    Remarks = x.Remarks,
                    RequestedDate = x.CreatedAt,
                    ProcessedDate = x.UpdatedAt,
                }),
                TotalCount,
                TotalPages
            });
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var regularisation = await _context.Regularisations
                .Include(x => x.Emp)
                .Include(x => x.RegularisationType)
                .Include(x => x.GatePassType)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ApprovedByUser.Emp)
                .Include(x => x.DisapprovedByUser)
                .Include(x => x.DisapprovedByUser.Emp)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (regularisation is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            var user = await _userManager.GetUserAsync(User);

            var emp = await _context.EmpLogs.Where(x => regularisation.EmpId == x.EmployeeId)
                .Join(_context.EmpTransactions,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        HodEmpId = et.HodEmpId,
                        RmEmpId = et.RmEmpId,
                        DepartmentName = et.Department != null ? et.Department.Name : null,
                        DesignationName = et.Designation != null ? et.Designation.Name : null,
                        LocationName = et.Branch != null ? et.Branch.Name : null
                    })
                .FirstOrDefaultAsync();

            if (emp is null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Employee.");
            }

            if (user.EmpId != regularisation.EmpId && user.EmpId != emp.HodEmpId && user.EmpId != emp.RmEmpId)
            {
                return Forbid();
            }

            return Ok(new
            {
                Id = regularisation.Id,
                Name = Helper.FullName(regularisation.Emp.FirstName, regularisation.Emp.MiddleName, regularisation.Emp.LastName),
                EmpCode = regularisation.Emp.CardId,
                DepartmentName = emp.DepartmentName,
                DesignationName = emp.DesignationName,
                LocationName = emp.LocationName,
                DateOfJoining = regularisation.Emp.JoinDate,
                RegularisationType = regularisation.RegularisationType.DisplayName,
                GatePassTypeId = regularisation.GatePassTypeId,
                GatePassType = regularisation.GatePassType?.Name,
                Status = regularisation.Status,
                RequestDate = regularisation.CreatedAt,
                FromDate = regularisation.FromDate,
                ToDate = regularisation.ToDate,
                FromTime = regularisation.FromTime?.ToString("HH:mm:ss"),
                ToTime = regularisation.ToTime?.ToString("HH:mm:ss"),
                ContactNumber = regularisation.ContactNumber,
                Place = regularisation.Place,
                Reason = regularisation.Reason,
                Remarks = regularisation.Remarks,
                ApprovedByUserId = regularisation.ApprovedByUserId,
                ApprovedByUserCode = regularisation.ApprovedByUser?.UserName,
                ApprovedByUserName = Helper.FullName(regularisation.ApprovedByUser?.Emp?.FirstName, regularisation.ApprovedByUser?.Emp?.MiddleName, regularisation.ApprovedByUser?.Emp?.LastName),
                DisapprovedByUserId = regularisation.DisapprovedByUserId,
                DisapprovedByUserCode = regularisation.DisapprovedByUser?.UserName,
                DisapprovedByUserName = Helper.FullName(regularisation.DisapprovedByUser?.Emp?.FirstName, regularisation.DisapprovedByUser?.Emp?.MiddleName, regularisation.DisapprovedByUser?.Emp?.LastName),
                CancellationRemarks = regularisation.CancellationRemarks,
                ProcessedDate = regularisation.UpdatedAt
            });
        }

        // Post: Regularisations
        [HttpPost]
        public async Task<IActionResult> Create(RequestInputModel input)
        {
            DateOnly fromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);
            DateOnly toDate;

            TimeOnly fromTime = TimeOnly.Parse(input.FromTime);
            TimeOnly toTime;

            DateOnly todayDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly firstDay = new(todayDate.Year, todayDate.Month, 1);

            if (fromDate < firstDay)
            {
                return ErrorHelper.ErrorResult("FromDate", "Cannot apply regularisation for previous months.");
            }

            var user = await _userManager.GetUserAsync(User);

            var statusId = await _context.EmpLogs.Where(x => x.EmployeeId == user.EmpId)
                .Join(_context.EmpTransactions, el => el.Id, et => et.Id,
                (el, et) => et.StatusId)
                .FirstOrDefaultAsync();

            if (statusId != 1)
            {
                return ErrorHelper.ErrorResult("EmpId", "Employee is not active.");
            }

            if (!string.IsNullOrEmpty(input.ToDate))
            {
                toDate = DateOnlyHelper.ParseDateOrNow(input.ToDate);
            }

            if (!string.IsNullOrEmpty(input.ToTime))
            {
                toTime = TimeOnly.Parse(input.ToTime);
            }

            if (await _context.LeaveLedgers.AnyAsync(x => x.EmpId == user.EmpId && x.Taken > 0 && x.Leave_Date >= fromDate && x.Leave_Date <= toDate))
            {
                return ErrorHelper.ErrorResult("FromDate", "User has taken leave somewhere between the given dates.");
            }

            string? regularisationType = await _context.RegularisationTypes
                .Where(x => x.Id == input.RegularisationTypeId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            if (regularisationType == "in-punch-regularisation")
            {
                var attendance = await _context.Attendances.Where(x => x.TransactionDateOut == fromDate && x.EmpId == user.EmpId).FirstOrDefaultAsync();

                if (attendance is null)
                {
                    return ErrorHelper.ErrorResult("FromDate", "User has not Punched Out in the given date.");
                }

                if (attendance.InTime != null)
                {
                    return ErrorHelper.ErrorResult("FromDate", "User has already Punched In in the given date.");
                }

                Regularisation data = new()
                {
                    RegularisationTypeId = input.RegularisationTypeId,
                    EmpId = user.EmpId ?? 0,
                    FromDate = fromDate,
                    ToDate = fromDate,
                    FromTime = fromTime,
                    ToTime = fromTime,
                    ContactNumber = input.ContactNumber,
                    Status = "pending"
                };

                _context.Add(data);
                await _context.SaveChangesAsync();

                return Ok();
            };

            if (regularisationType == "out-punch-regularisation")
            {
                var attendance = await _context.Attendances.Where(x => x.TransactionDate == fromDate && x.EmpId == user.EmpId).FirstOrDefaultAsync();

                if (attendance is null)
                {
                    return ErrorHelper.ErrorResult("FromDate", "User has not Punched In in the given date.");
                }

                if (attendance.OutTime != null)
                {
                    return ErrorHelper.ErrorResult("FromDate", "User has already Punched Out in the given date.");
                }

                Regularisation data = new()
                {
                    RegularisationTypeId = input.RegularisationTypeId,
                    EmpId = user.EmpId ?? 0,
                    FromDate = fromDate,
                    ToDate = fromDate,
                    FromTime = fromTime,
                    ToTime = fromTime,
                    ContactNumber = input.ContactNumber,
                    Reason = input.Reason,
                    Status = "pending"
                };

                _context.Add(data);
                await _context.SaveChangesAsync();

                return Ok();
            };

            if (regularisationType == "on-duty")
            {
                if (await _context.Regularisations.AnyAsync(x => x.EmpId == user.EmpId &&
                    x.FromDate <= toDate && x.ToDate >= fromDate &&
                    x.FromTime < toTime && x.ToTime > fromTime &&
                    (x.Status == "approved" || x.Status == "pending")))
                {
                    return ErrorHelper.ErrorResult("FromDate", "Regularisation somewhere between the dates for the given time already exists.");
                }

                DateOnly currentDate = fromDate;

                do
                {
                    var existingAttendance = await _context.Attendances.FirstOrDefaultAsync(x => x.EmpId == user.EmpId && x.TransactionDate == currentDate);

                    if (existingAttendance is not null)
                    {
                        TimeOnly? inTime = string.IsNullOrEmpty(existingAttendance.InTime) ? null : TimeOnly.ParseExact(existingAttendance.InTime, "HH:mm:ss");
                        TimeOnly? outTime = string.IsNullOrEmpty(existingAttendance.OutTime) ? null : TimeOnly.ParseExact(existingAttendance.OutTime, "HH:mm:ss");

                        if (fromTime >= inTime && fromTime < outTime && toTime > outTime)
                        {
                            currentDate = currentDate.AddDays(1);

                            return ErrorHelper.ErrorResult("ToTime", "Invalid Time. Please check attendance in date: " + existingAttendance.TransactionDate.ToString());
                        }

                        if (fromTime < inTime && toTime > inTime && toTime <= outTime)
                        {
                            currentDate = currentDate.AddDays(1);

                            return ErrorHelper.ErrorResult("ToTime", "Invalid Time. Please check attendance in date: " + existingAttendance.TransactionDate.ToString());
                        }
                    }
                    currentDate = currentDate.AddDays(1);
                } while (currentDate <= toDate);

                Regularisation data = new()
                {
                    RegularisationTypeId = input.RegularisationTypeId,
                    EmpId = user.EmpId ?? 0,
                    FromDate = fromDate,
                    ToDate = toDate,
                    FromTime = fromTime,
                    ToTime = toTime,
                    ContactNumber = input.ContactNumber,
                    Reason = input.Reason,
                    Place = input.Place,
                    Status = "pending"
                };

                _context.Add(data);
                await _context.SaveChangesAsync();

                return Ok();
            };

            if (regularisationType == "work-from-home")
            {
                if (await _context.Attendances.AnyAsync(x =>
                                (x.TransactionDate >= fromDate) &&
                                (x.TransactionDate <= toDate) &&
                                x.EmpId == user.EmpId)
                    )
                {
                    return ErrorHelper.ErrorResult("FromDate", "User already has attendance somewhere between the given dates.");
                }

                Regularisation data = new()
                {
                    RegularisationTypeId = input.RegularisationTypeId,
                    EmpId = user.EmpId ?? 0,
                    FromDate = fromDate,
                    ToDate = toDate,
                    FromTime = fromTime,
                    ToTime = toTime,
                    ContactNumber = input.ContactNumber,
                    Reason = input.Reason,
                    Status = "pending"
                };

                _context.Add(data);
                await _context.SaveChangesAsync();

                return Ok();
            }

            if (regularisationType == "gate-pass")
            {
                //if (!await _context.Attendances.AnyAsync(x => x.EmpId == user.EmpId && x.TransactionDate == fromDate && !string.IsNullOrEmpty(x.InTime) && !string.IsNullOrEmpty(x.OutTime)))
                //{
                //    return ErrorHelper.ErrorResult("FromDate", "User must have punch in and punch out in the applied date.");
                //}

                if (await _context.Regularisations.AnyAsync(x => x.EmpId == user.EmpId
                    && x.FromDate == fromDate
                    && x.FromTime < fromTime.AddHours((double)input.GatePassHour) && x.ToTime > fromTime
                    && (x.Status == "approved" || x.Status == "pending")))
                {
                    return ErrorHelper.ErrorResult("FromDate", "Regularisation already exists in the given date and time.");
                }


                var firstDayOfMonth = new DateOnly(fromDate.Year, fromDate.Month , 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var regularisations = await _context.Regularisations
                    .Include(x => x.RegularisationType)
                    .Where(x => x.RegularisationType.Name == "gate-pass" && 
                            x.EmpId == user.EmpId && (x.Status == "approved" || x.Status == "pending") && 
                            x.FromDate >= firstDayOfMonth && 
                            x.ToDate <= lastDayOfMonth)
                    .ToListAsync();

                if (regularisations.Count == 2)
                {
                    return ErrorHelper.ErrorResult("FromDate", "Gate pass already applied for 2 hours in this month.");
                }

                if (regularisations.Count == 1)
                {
                    if (input.GatePassHour > 1)
                    {
                        return ErrorHelper.ErrorResult("GatePassHour", "Gate Pass already applied for 1 hour this month.");
                    }

                    var regularisation = regularisations.FirstOrDefault();

                    TimeSpan totalHours = (regularisation.ToTime ?? TimeOnly.MaxValue) - (regularisation.FromTime ?? TimeOnly.MinValue);

                    if (totalHours.Hours > 1)
                    {
                        return ErrorHelper.ErrorResult("FromDate", "Gate pass already applied for 2 hours in this month,");
                    }
                }

                var existingAttendance = await _context.Attendances
                    .Include(x => x.WorkHour)
                    .FirstOrDefaultAsync(x => x.EmpId == user.EmpId && x.TransactionDate == fromDate);

                TimeOnly? startTime = null;

                if (existingAttendance is not null)
                {
                    if (!string.IsNullOrEmpty(existingAttendance.OutTime))
                    {
                        TimeOnly outTime = TimeOnly.Parse(existingAttendance.OutTime);

                        if (fromTime < outTime && fromTime.AddHours(input.GatePassHour ?? 1) > outTime)
                        {
                            fromTime = outTime;

                            try
                            {
                                _context.Add(new Regularisation
                                {
                                    RegularisationTypeId = input.RegularisationTypeId,
                                    EmpId = user.EmpId ?? 0,
                                    GatePassTypeId = input.GatePassTypeId,
                                    FromDate = fromDate,
                                    ToDate = fromDate,
                                    FromTime = fromTime,
                                    ToTime = fromTime.AddHours(input.GatePassHour ?? 1),
                                    ContactNumber = input.ContactNumber,
                                    Reason = input.Reason,
                                    Status = "pending"
                                });

                                await _context.SaveChangesAsync();

                                return Ok();
                            }
                            catch (Exception ex)
                            {
                                return BadRequest(ex.StackTrace);
                            }
                        }
                    }

                    startTime = TimeOnly.Parse(existingAttendance.WorkHour.StartTime);
                }
                else
                {
                    var roster = await _context.Rosters
                        .Include(x => x.WorkHour)
                        .Where(x => x.Date == fromDate && x.EmpId == user.EmpId)
                        .FirstOrDefaultAsync();

                    if (roster is not null)
                    {
                        startTime = TimeOnly.Parse(roster.WorkHour.StartTime);
                    }
                    else
                    {
                        var defaultWorkHour = await _context.DefaultWorkHours
                            .Include(x => x.WorkHour)
                            .Where(x => x.EmpId == user.EmpId || x.EmpId == null && x.DayId == ((short)fromDate.DayOfWeek + 1))
                            .OrderBy(x => x.EmpId)
                            .FirstOrDefaultAsync();

                        if (defaultWorkHour is null)
                        {
                            return ErrorHelper.ErrorResult("FromDate", "You are not assigned with any shift.");
                        }

                        startTime = TimeOnly.Parse(defaultWorkHour.WorkHour.StartTime);
                    }
                }

                if (startTime > fromTime)
                {
                    return ErrorHelper.ErrorResult("FromTime", $"Time cannot be greater than the shift start time, {startTime}.");
                }

                Regularisation data = new()
                {
                    RegularisationTypeId = input.RegularisationTypeId,
                    EmpId = user.EmpId ?? 0,
                    GatePassTypeId = input.GatePassTypeId,
                    FromDate = fromDate,
                    ToDate = fromDate,
                    FromTime = fromTime,
                    ToTime = fromTime.AddHours(input.GatePassHour ?? 1),
                    ContactNumber = input.ContactNumber,
                    Reason = input.Reason,
                    Status = "pending"
                };

                _context.Add(data);
                await _context.SaveChangesAsync();

                return Ok();
            }

            return Ok();
        }

        // GET: Regularisations/approve/1
        [HttpPost("Approve/{id}")]
        public async Task<IActionResult> ApproveLeave(int id)
        {
            var regularisation = await _context.Regularisations
                .Include(x => x.RegularisationType)
                .Where(x => x.Id == id && x.Status == "pending").FirstOrDefaultAsync();

            if (regularisation == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            DateOnly todayDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly firstDay = new(todayDate.Year, todayDate.Month, 1);

            if (regularisation.FromDate < firstDay)
            {
                return ErrorHelper.ErrorResult("Id", "Cannot approve regularisation for previous months.");
            }

            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

            var emp = await _context.EmpLogs.Where(x => x.EmployeeId == regularisation.EmpId)
                .Join(_context.EmpTransactions,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        HodEmpId = et.HodEmpId,
                        RmEmpId = et.RmEmpId
                    })
                .FirstOrDefaultAsync();

            if (emp.HodEmpId != user.EmpId && emp.RmEmpId != user.EmpId)
            {
                return ErrorHelper.ErrorResult("Id", "Unauthorized");
            }

            if (await _context.LeaveLedgers.AnyAsync(x => x.EmpId == regularisation.EmpId && x.Taken > 0 && 
                                                          x.Leave_Date >= regularisation.FromDate && x.Leave_Date <= regularisation.ToDate))
            {
                return ErrorHelper.ErrorResult("FromDate", "User has taken leave somewhere between the given dates.");
            }

            if (regularisation.RegularisationType.Name == "in-punch-regularisation")
            {
                var attendance = await _context.Attendances.Where(x => x.TransactionDateOut == regularisation.FromDate && x.EmpId == regularisation.EmpId).FirstOrDefaultAsync();

                if (attendance is null)
                {
                    return ErrorHelper.ErrorResult("Date", "User has not Punched Out in the given date.");
                }

                if (attendance.InTime != null)
                {
                    return ErrorHelper.ErrorResult("Date", "User has already Punched In in the given date.");
                }

                try
                {
                    attendance.InTime = regularisation.FromTime?.ToString("HH:mm:ss");
                    attendance.InRemarks = "Regularised";
                    attendance.RegularisationId = regularisation.Id;

                    regularisation.ApprovedByUserId = user.Id;
                    regularisation.Status = "approved";
                    regularisation.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return Ok();
                }
                catch (Exception ex)
                {
                    attendance.InTime = null;
                    attendance.InRemarks = null;
                    attendance.RegularisationId = null;

                    await _context.SaveChangesAsync();

                    return BadRequest(ex.StackTrace);
                }
            };

            if (regularisation.RegularisationType.Name == "out-punch-regularisation")
            {
                var attendance = await _context.Attendances.Where(x => x.TransactionDate == regularisation.FromDate && x.EmpId == regularisation.EmpId).FirstOrDefaultAsync();

                if (attendance is null)
                {
                    return ErrorHelper.ErrorResult("Date", "User has not Punched In in the given date.");
                }

                if (attendance.OutTime != null)
                {
                    return ErrorHelper.ErrorResult("Date", "User has already Punched In in the given date.");
                }

                try
                {
                    attendance.OutTime = regularisation.FromTime?.ToString("HH:mm:ss");
                    attendance.TransactionDateOut = regularisation.FromDate;
                    attendance.OutRemarks = "Regularised";
                    attendance.RegularisationId = regularisation.Id;

                    regularisation.ApprovedByUserId = user.Id;
                    regularisation.Status = "approved";
                    regularisation.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return Ok();
                }
                catch (Exception ex)
                {
                    attendance.OutTime = null;
                    attendance.TransactionDateOut = null;
                    attendance.OutRemarks = null;
                    attendance.RegularisationId = null;

                    await _context.SaveChangesAsync();

                    return BadRequest(ex.StackTrace);
                }
            };

            if (regularisation.RegularisationType.Name == "on-duty")
            {
                List<Attendance> newAttendances = new();

                try
                {
                    DateOnly currentDate = regularisation.FromDate ?? new DateOnly();

                    do
                    {
                        var existingAttendance = await _context.Attendances.FirstOrDefaultAsync(x => x.EmpId == regularisation.EmpId && x.TransactionDate == currentDate);

                        if (existingAttendance is not null)
                        {
                            //TimeOnly inTime = TimeOnly.ParseExact(existingAttendance.InTime, "HH:mm:ss");
                            //TimeOnly outTime = TimeOnly.ParseExact(existingAttendance.OutTime, "HH:mm:ss");

                            //if (regularisation.FromTime >= inTime && regularisation.FromTime < outTime && regularisation.ToTime > outTime)
                            //{
                            //    return ErrorHelper.ErrorResult("ToTime", "Invalid Time. Please check attendance in date: " + existingAttendance.TransactionDate.ToString());
                            //}

                            //if (regularisation.FromTime < inTime && regularisation.ToTime > inTime && regularisation.ToTime <= outTime)
                            //{
                            //    return ErrorHelper.ErrorResult("ToTime", "Invalid Time. Please check attendance in date: " + existingAttendance.TransactionDate.ToString());
                            //}

                            existingAttendance.RegularisationId = regularisation.Id;
                        }
                        else
                        {
                            short? WorkHourId;

                            var roster = await _context.Rosters.Where(x => x.Date == currentDate && x.EmpId == regularisation.EmpId).FirstOrDefaultAsync();

                            WorkHourId = roster?.WorkHourId;

                            if (roster is null)
                            {
                                var defaultWorkHour = await _context.DefaultWorkHours
                                    .Where(x => x.EmpId == regularisation.EmpId || x.EmpId == null && x.DayId == ((short)currentDate.DayOfWeek + 1))
                                    .OrderBy(x => x.EmpId)
                                    .FirstOrDefaultAsync();

                                WorkHourId = defaultWorkHour?.WorkHourId;
                            }

                            if (WorkHourId is null)
                            {
                                return ErrorHelper.ErrorResult("FromDate", "There is no default shift nor the empoloyee is assigned any.");
                            }

                            newAttendances.Add(new Attendance
                            {
                                EmpId = regularisation.EmpId,
                                TransactionDate = currentDate,
                                TransactionDateOut = currentDate,
                                InTime = regularisation.FromTime?.ToString("HH:mm:ss"),
                                OutTime = regularisation.ToTime?.ToString("HH:mm:ss"),
                                InRemarks = "On Duty Regularised",
                                OutRemarks = "On Duty Regularised",
                                RegularisationId = regularisation.Id,
                                WorkHourId = WorkHourId,

                                //Defaults
                                AttendanceStatus = 0,
                                FlagIn = false,
                                FlagOut = false,
                                AttendanceType = 0,
                                CheckInMode = 'N',
                                AttendanceId = 0,
                                SignOutTimeStamp = 0,
                                SignInTimeStamp = 0,
                            });
                        }

                        currentDate = currentDate.AddDays(1);

                    } while (currentDate <= regularisation.ToDate);

                    _context.AddRange(newAttendances);

                    regularisation.ApprovedByUserId = user.Id;
                    regularisation.Status = "approved";
                    regularisation.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return Ok();
                }
                catch (Exception ex)
                {
                    _context.RemoveRange(newAttendances);

                    return BadRequest(ex.StackTrace);
                }
            };

            if (regularisation.RegularisationType.Name == "work-from-home")
            {
                if (await _context.Attendances.AnyAsync(x =>
                (x.TransactionDate >= regularisation.FromDate) &&
                                (x.TransactionDate <= regularisation.ToDate) &&
                                x.EmpId == regularisation.EmpId)
                    )
                {
                    return ErrorHelper.ErrorResult("FromDate", "User already has attendance somewhere between the given dates.");
                }

                List<Attendance> newAttendances = new();

                try
                {
                    DateOnly currentDate = regularisation.FromDate ?? new DateOnly();

                    do
                    {
                        short? WorkHourId;

                        var roster = await _context.Rosters.Where(x => x.Date == currentDate && x.EmpId == regularisation.EmpId).FirstOrDefaultAsync();

                        WorkHourId = roster?.WorkHourId;

                        if (roster is null)
                        {
                            var defaultWorkHour = await _context.DefaultWorkHours
                                .Where(x => x.EmpId == regularisation.EmpId || x.EmpId == null && x.DayId == ((short)currentDate.DayOfWeek + 1))
                                .OrderBy(x => x.EmpId)
                                .FirstOrDefaultAsync();

                            WorkHourId = defaultWorkHour?.WorkHourId;
                        }

                        if (WorkHourId is null)
                        {
                            return ErrorHelper.ErrorResult("FromDate", "There is no default shift nor the empoloyee is assigned any.");
                        }

                        newAttendances.Add(new Attendance
                        {
                            EmpId = regularisation.EmpId,
                            TransactionDate = currentDate,
                            TransactionDateOut = currentDate,
                            InTime = regularisation.FromTime?.ToString("HH:mm:ss"),
                            OutTime = regularisation.ToTime?.ToString("HH:mm:ss"),
                            InRemarks = "Work From Home Regularised",
                            OutRemarks = "Work From Home Regularised",
                            RegularisationId = regularisation.Id,
                            WorkHourId = WorkHourId,

                            //Defaults
                            AttendanceStatus = 0,
                            FlagIn = false,
                            FlagOut = false,
                            AttendanceType = 0,
                            CheckInMode = 'N',
                            AttendanceId = 0,
                            SignOutTimeStamp = 0,
                            SignInTimeStamp = 0,
                        });
                        currentDate = currentDate.AddDays(1);

                    } while (currentDate <= regularisation.ToDate);

                    _context.AddRange(newAttendances);

                    regularisation.ApprovedByUserId = user.Id;
                    regularisation.Status = "approved";
                    regularisation.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.StackTrace);
                }
            }

            if (regularisation.RegularisationType.Name == "gate-pass")
            {
                var firstDayOfMonth = new DateOnly(regularisation.FromDate?.Year ?? 0, regularisation.FromDate?.Month ?? 0, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                try
                {
                    var existingAttendance = await _context.Attendances
                        .Include(x => x.WorkHour)
                        .FirstOrDefaultAsync(x => x.EmpId == regularisation.EmpId && x.TransactionDate == regularisation.FromDate);

                    TimeOnly? startTime = null;

                    if (existingAttendance is not null)
                    {
                        startTime = TimeOnly.Parse(existingAttendance.WorkHour.StartTime);
                    }
                    else
                    {
                        var roster = await _context.Rosters
                            .Include(x => x.WorkHour)
                            .Where(x => x.Date == regularisation.FromDate && x.EmpId == regularisation.EmpId)
                            .FirstOrDefaultAsync();

                        if (roster is not null)
                        {
                            startTime = TimeOnly.Parse(roster.WorkHour.StartTime);
                        }
                        else
                        {
                            short? dayOfWeek = (short)(regularisation.FromDate?.DayOfWeek + 1);

                            var defaultWorkHour = await _context.DefaultWorkHours
                                .Include(x => x.WorkHour)
                                .Where(x => x.EmpId == regularisation.EmpId || x.EmpId == null && x.DayId == (dayOfWeek ?? 0))
                                .OrderBy(x => x.EmpId)
                                .FirstOrDefaultAsync();

                            if (defaultWorkHour is null)
                            {
                                return ErrorHelper.ErrorResult("Id", "Employee is not assigned with any shift.");
                            }

                            startTime = TimeOnly.Parse(defaultWorkHour.WorkHour.StartTime);
                        }
                    }

                    if (startTime > regularisation.FromTime)
                    {
                        return ErrorHelper.ErrorResult("Id", $"Time cannot be greater than the shift start time, {startTime}.");
                    }

                    regularisation.ApprovedByUserId = user.Id;
                    regularisation.Status = "approved";
                    regularisation.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.StackTrace);
                }
            }

            return Ok();
        }

        // GET: Regularisations/disapprove/1
        [HttpPost("Disapprove/{id}")]
        public async Task<IActionResult> DisapproveLeave(int id)
        {
            var regularisation = await _context.Regularisations.Where(x => x.Id == id && x.Status == "pending").FirstOrDefaultAsync();

            if (regularisation == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            DateOnly todayDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly firstDay = new(todayDate.Year, todayDate.Month, 1);

            if (regularisation.FromDate < firstDay)
            {
                return ErrorHelper.ErrorResult("Id", "Cannot disapprove regularisation for previous months.");
            }

            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

            var emp = await _context.EmpLogs.Where(x => x.EmployeeId == regularisation.EmpId)
                .Join(_context.EmpTransactions,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        HodEmpId = et.HodEmpId,
                        RmEmpId = et.RmEmpId
                    })
                .FirstOrDefaultAsync();

            if (emp.HodEmpId != user.EmpId && emp.RmEmpId != user.EmpId)
            {
                return ErrorHelper.ErrorResult("Id", "Unauthorized");
            }

            regularisation.Status = "disapproved";
            regularisation.DisapprovedByUserId = User.GetUserId();
            regularisation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: LeaveApplicationHistories/Cancel/{id}
        [HttpPost("Cancel/{id}")]
        public async Task<IActionResult> Cancel(int id, CancelInputModel input)
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

            var regularisation = await _context.Regularisations.Where(x => x.Id == id && x.Status == "pending").FirstOrDefaultAsync();

            if (regularisation is null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid id.");
            }

            if (regularisation.EmpId != user.EmpId)
            {
                return Unauthorized("Invalid User");
            }

            DateOnly todayDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly firstDay = new(todayDate.Year, todayDate.Month, 1);

            if (regularisation.FromDate < firstDay)
            {
                return ErrorHelper.ErrorResult("Id", "Cannot cancel regularisation for previous months.");
            }

            regularisation.CancellationRemarks = input.CancellationRemarks;
            regularisation.Status = "cancelled";
            regularisation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        public class RequestInputModel
        {
            public int RegularisationTypeId { get; set; }
            public int? GatePassTypeId { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string FromTime { get; set; }
            public string ToTime { get; set; }
            public string ContactNumber { get; set; }
            public string Place { get; set; }
            public string Reason { get; set; }
            public int? GatePassHour { get; set; }
        }

        public class CancelInputModel
        {
            public string CancellationRemarks { get; set; }
        }

        public class RequestInputModelValidator : AbstractValidator<RequestInputModel>
        {
            private readonly DataContext _context;

            public RequestInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.RegularisationTypeId)
                    .NotEmpty()
                    .IdMustExist(_context.RegularisationTypes.AsQueryable());

                RuleFor(x => x).Custom((request, context) =>
                {
                    var regularisationType = _context.RegularisationTypes
                        .FirstOrDefault(r => r.Id == request.RegularisationTypeId)?.Name ?? string.Empty;

                    if (regularisationType == "gate-pass")
                    {
                        if (request.GatePassTypeId == null)
                        {
                            context.AddFailure(nameof(request.GatePassTypeId), "GatePassTypeId is required for gate-pass.");
                        }
                        if (request.GatePassHour == null || (request.GatePassHour != 1 && request.GatePassHour != 2))
                        {
                            context.AddFailure(nameof(request.GatePassHour), "GatePassHour must be 1 or 2 for gate-pass.");
                        }
                    }

                    if (regularisationType == "on-duty" || regularisationType == "work-from-home")
                    {
                        if (string.IsNullOrEmpty(request.ToDate))
                        {
                            context.AddFailure(nameof(request.ToDate), "ToDate is required for on-duty or work-from-home.");
                        }
                        if (string.IsNullOrEmpty(request.ToTime))
                        {
                            context.AddFailure(nameof(request.ToTime), "ToTime is required for on-duty or work-from-home.");
                        }
                    }
                });

                RuleFor(x => x.FromDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.ToDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.FromDate, "From Date")
                    .Unless(x => string.IsNullOrEmpty(x.ToDate));

                RuleFor(x => x.FromTime)
                    .NotEmpty()
                    .MustBeTime();

                RuleFor(x => x.ToTime)
                    .NotEmpty()
                    .MustBeTime()
                    .MustBeTimeAfter(x => x.FromTime, "From Time")
                    .Unless(x => string.IsNullOrEmpty(x.ToTime));

                RuleFor(x => x.Reason)
                    .NotEmpty();

                RuleFor(x => x.ContactNumber)
                    .MustBeDigits(10)
                    .Unless(x => string.IsNullOrEmpty(x.ContactNumber));
            }
        }
    }
}

