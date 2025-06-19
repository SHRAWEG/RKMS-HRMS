using Hrms.Common.Models;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.Design;
using System.Globalization;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class LeaveManagementsController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public LeaveManagementsController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: LeaveManagements
        [HttpGet("History")]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string fromDate, string toDate)
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

            var query = _context.LeaveApplicationHistories
                .Include(x => x.Leave)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ApprovedByUser.Emp)
                .Include(x => x.DisapprovedByUser)
                .Include(x => x.DisapprovedByUser.Emp)
                .Where(x => x.EmpId == user.EmpId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);
                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

                query = query.Where(b => b.StartDate >= FromDate && b.EndDate <= ToDate);
            }

            Expression<Func<LeaveApplicationHistory, object>> field = sortColumn switch
            {
                "CreatedAt" => x => x.CreatedAt,
                "LeaveId" => x => x.LeaveId,
                "StartDate" => x => x.StartDate,
                "EndDate" => x => x.EndDate,
                "Status" => x => x.Status,
                _ => x => x.Id
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

            var data = await PagedList<LeaveApplicationHistory>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    EmpId = x.EmpId,
                    LeaveId = x.LeaveId,
                    LeaveName = x.Leave.Name,
                    RequestedDate = x.CreatedAt,
                    ProcessedDate = x.UpdatedAt,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByUserCode = x.ApprovedByUser?.UserName,
                    ApprovedByUserName = Helper.FullName(x.ApprovedByUser?.Emp?.FirstName, x.ApprovedByUser?.Emp?.MiddleName, x.ApprovedByUser?.Emp?.LastName),
                    DisapprovedByUserId = x.DisapprovedByUserId,
                    DisapprovedByUserCode = x.DisapprovedByUser?.UserName,
                    DisapprovedByUserName = Helper.FullName(x.DisapprovedByUser?.Emp?.FirstName, x.DisapprovedByUser?.Emp?.MiddleName, x.DisapprovedByUser?.Emp?.LastName),
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        [HttpGet("History/Subordinate")]
        public async Task<IActionResult> GetSubordinateHistory(int page, int limit, string sortColumn, string sortDirection, 
            int? empId, string empName, string empCode, string fromDate, string toDate)
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

            var query = _context.LeaveApplicationHistories
                .Include(x => x.Leave)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ApprovedByUser.Emp)
                .Include(x => x.DisapprovedByUser)
                .Include(x => x.DisapprovedByUser.Emp)
                .Where(x => subordinateIds.Contains(x.EmpId))
                .AsQueryable();

            if (empId is not null)
            {
                query = query.Where(x => x.EmpId == empId);
            }

            if (!string.IsNullOrEmpty(empCode))
            {
                query = query.Where(x => x.Emp.CardId.ToLower().Contains(empCode.ToLower()));
            }

            if (!string.IsNullOrEmpty(empName))
            {
                query = query.Where(x => string.Concat(x.Emp.FirstName, x.Emp.MiddleName, x.Emp.LastName).ToLower().Contains(empName.Replace(" ", string.Empty).ToLower()));
            }

            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);
                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

                query = query.Where(b => b.StartDate >= FromDate && b.EndDate <= ToDate);
            }

            Expression<Func<LeaveApplicationHistory, object>> field = sortColumn switch
            {
                "CreatedAt" => x => x.CreatedAt,
                "LeaveId" => x => x.LeaveId,
                "StartDate" => x => x.StartDate,
                "EndDate" => x => x.EndDate,
                "Status" => x => x.Status,
                _ => x => x.Id
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

            var data = await PagedList<LeaveApplicationHistory>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    EmpId = x.EmpId,
                    EmpCode = x.Emp.CardId,
                    EmpName = Helper.FullName(x.Emp.FirstName, x.Emp.MiddleName, x.Emp.LastName),
                    LeaveId = x.LeaveId,
                    LeaveName = x.Leave.Name,
                    RequestedDate = x.CreatedAt,
                    ProcessedDate = x.UpdatedAt,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByUserCode = x.ApprovedByUser?.UserName,
                    ApprovedByUserName = Helper.FullName(x.ApprovedByUser?.Emp?.FirstName, x.ApprovedByUser?.Emp?.MiddleName, x.ApprovedByUser?.Emp?.LastName),
                    DisapprovedByUserId = x.DisapprovedByUserId,
                    DisapprovedByUserCode = x.DisapprovedByUser?.UserName,
                    DisapprovedByUserName = Helper.FullName(x.DisapprovedByUser?.Emp?.FirstName, x.DisapprovedByUser?.Emp?.MiddleName, x.DisapprovedByUser?.Emp?.LastName),
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        [HttpGet("Request")]
        public async Task<IActionResult> Requests()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

            var data = await _context.LeaveApplicationHistories
                .Where(x => x.EmpId == user.EmpId && x.Status == "pending")
                .Include(x => x.Leave)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = data.Count,
                Data = data.Select(x => new
                {
                    x.Id,
                    x.EmpId,
                    x.LeaveId,
                    LeaveName = x.Leave.Name,
                    x.CreatedAt,
                    x.UpdatedAt,
                    x.StartDate,
                    x.EndDate,
                })
            });
        }

        [HttpGet("Request/Subordinate")]
        public async Task<IActionResult> SubordinateRequests()
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

            var data = await _context.LeaveApplicationHistories
                .Where(x => subordinateIds.Contains(x.EmpId) && x.Status == "pending")
                .Include(x => x.Leave)
                .Include(x => x.Emp)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = data.Count,
                Data = data.Select(x => new
                {
                    x.Id,
                    x.EmpId,
                    EmpCode = x.Emp.CardId,
                    EmpName = FullName(x.Emp.FirstName, x.Emp.MiddleName, x.Emp.LastName),
                    x.LeaveId,
                    LeaveName = x.Leave.Name,
                    x.CreatedAt,
                    x.UpdatedAt,
                    x.StartDate,
                    x.EndDate,
                })
            });
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var leaveApplicationHistory = await _context.LeaveApplicationHistories
                .Include(x => x.Emp)
                .Include(x => x.Leave)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ApprovedByUser.Emp)
                .Include(x => x.DisapprovedByUser)
                .Include(x => x.DisapprovedByUser.Emp)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (leaveApplicationHistory is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            var user = await _userManager.GetUserAsync(User);

            var emp = await _context.EmpLogs.Where(x => leaveApplicationHistory.EmpId == x.EmployeeId)
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

            if (user.EmpId != leaveApplicationHistory.EmpId && user.EmpId != emp.HodEmpId && user.EmpId != emp.RmEmpId)
            {
                return Forbid();
            }

            return Ok(new
            {
                Id = leaveApplicationHistory.Id,
                Name = Helper.FullName(leaveApplicationHistory.Emp.FirstName, leaveApplicationHistory.Emp.MiddleName, leaveApplicationHistory.Emp.LastName),
                EmpCode = leaveApplicationHistory.Emp.CardId,
                DepartmentName = emp.DepartmentName,
                DesignationName = emp.DesignationName,
                LocationName = emp.LocationName,
                DateOfJoining = leaveApplicationHistory.Emp.JoinDate,
                LeaveName = leaveApplicationHistory.Leave.Name,
                LeaveAbbreviation = leaveApplicationHistory.Leave.Abbreviation,
                LeaveType = leaveApplicationHistory.HLeaveType == 0 ? "Full Day" : "Half Day",
                Status = leaveApplicationHistory.Status,
                RequestedDate = leaveApplicationHistory.CreatedAt,
                FromDate = leaveApplicationHistory.StartDate,
                ToDate = leaveApplicationHistory.EndDate,
                ContactNumber = leaveApplicationHistory.ContactNumber,
                Reason = leaveApplicationHistory.Reason,
                Address = leaveApplicationHistory.Address,
                ApprovedByUserId = leaveApplicationHistory.ApprovedByUserId,
                ApprovedByUserCode = leaveApplicationHistory.ApprovedByUser?.UserName,
                ApprovedByUserName = Helper.FullName(leaveApplicationHistory.ApprovedByUser?.Emp?.FirstName, leaveApplicationHistory.ApprovedByUser?.Emp?.MiddleName, leaveApplicationHistory.ApprovedByUser?.Emp?.LastName),
                DisapprovedByUserId = leaveApplicationHistory.DisapprovedByUserId,
                DisapprovedByUserCode = leaveApplicationHistory.DisapprovedByUser?.UserName,
                DisapprovedByUserName = Helper.FullName(leaveApplicationHistory.DisapprovedByUser?.Emp?.FirstName, leaveApplicationHistory.DisapprovedByUser?.Emp?.MiddleName, leaveApplicationHistory.DisapprovedByUser?.Emp?.LastName),
                CancellationRemarks = leaveApplicationHistory.CancellationRemarks,
                ProcessedDate = leaveApplicationHistory.UpdatedAt,
            });
        }

        // Get Leaves assigned to the employee : id = PId
        [HttpGet("EmpLeave")]
        public async Task<IActionResult> GetEmpLeaves()
        {
            var user = await _userManager.GetUserAsync(User);

            var empLog = await _context.EmpLogs.Where(x => x.EmployeeId == user.EmpId).FirstOrDefaultAsync();

            var data = await _context.EmpLeaves.Where(x => x.PId == empLog.Id)
                            .Include(x => x.Leave)
                            .ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.LeaveId,
                    x.Leave.Name,
                    x.Leave.IsHalfLeave
                })
            });
        }

        // Get Leave Summary for filling Leave Application
        [HttpGet("Summary")]
        public async Task<IActionResult> GetLeaveSummary()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

            int? companyId = await _context.EmpLogs.Where(x => x.EmployeeId == user.EmpId)
                .Join(_context.EmpTransactions.AsQueryable(),
                    el => el.Id,
                    et => et.Id,
                    (el, et) => et.CompanyId)
                .FirstOrDefaultAsync();

            if (companyId is null)
            {
                return ErrorHelper.ErrorResult("Id", "You are not assigned to any company.");
            }

            var leaveYear = await _context.LeaveYearCompanies
                .Where(x => x.CompanyId == companyId || x.CompanyId == null)
                .OrderByDescending(x => x.CompanyId)
                .FirstOrDefaultAsync();

            if (leaveYear is null)
            {
                return ErrorHelper.ErrorResult("Id", "No leave year defined.");
            }

            var leaves = await (from leave in _context.Leaves
                                join leaveLedger in _context.LeaveLedgers.Where(x => x.EmpId == user.EmpId && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == companyId && !x.IsClosed)
                                .GroupBy(x => x.LeaveId)
                                .Select(x => new
                                {
                                    LeaveId = x.Key,
                                    TotalGiven = x.Sum(x => x.Given),
                                    TotalTaken = x.Sum(x => x.Taken),
                                }) on leave.Id equals leaveLedger.LeaveId into leaveLedgers
                                from leaveLedger in leaveLedgers.DefaultIfEmpty()
                                select new
                                {
                                    LeaveId = leave.Id,
                                    LeaveName = leave.Name,
                                    IsPaidLeave = leave.IsPaidLeave,
                                    Credited = leaveLedger != null ? leaveLedger.TotalGiven ?? 0 : 0,
                                    Availed = leaveLedger != null ? leaveLedger.TotalTaken ?? 0 : 0,
                                    Balance = leaveLedger != null ? (leaveLedger.TotalGiven - leaveLedger.TotalTaken) ?? 0 : 0,
                                })
                       .ToListAsync();

            var pendingLeaves = await _context.LeaveApplicationHistories
                .Where(x => x.EmpId == user.EmpId && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == companyId && !x.IsClosed && x.Status == "pending")
                .GroupBy(x => x.LeaveId)
                .Select(x => new
                {
                    LeaveId = x.Key,
                    TotalPending = x.Sum(y => y.TotalDays)
                })
                .ToListAsync();

            List<LeaveSummary> data = new();

            foreach (var leave in leaves)
            {
                var totalPending = pendingLeaves.Where(x => x.LeaveId == leave.LeaveId).Select(x => x.TotalPending).SingleOrDefault();

                data.Add(new LeaveSummary
                {
                    LeaveId = leave.LeaveId,
                    LeaveName = leave.LeaveName,
                    Credited = leave.Credited,
                    Availed = leave.Availed,
                    Pending = totalPending ?? 0,
                    Balance = leave.IsPaidLeave == 1 ? leave.Balance - (totalPending ?? 0) : 0
                });
            }

            return Ok(new
            {
                Data = data
            });
        }

        //Leave Application
        [HttpPost("LeaveApplication")]
        public async Task<IActionResult> LeaveApplication(LeaveApplicationModel input)
        {
            DateOnly startDate = DateOnlyHelper.ParseDateOrNow(input.StartDate);
            DateOnly endDate = DateOnlyHelper.ParseDateOrNow(input.EndDate);

            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly firstDay = new(currentDate.Year, currentDate.Month, 1);

            if (startDate < firstDay)
            {
                return ErrorHelper.ErrorResult("StartDate", "Cannot apply leave for previous months.");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user.EmpId is null)
            {
                return Unauthorized();
            }

            var emp = await _context.EmpLogs.Where(x => x.EmployeeId == user.EmpId)
                                       .Join(_context.EmpTransactions, el => el.Id, et => et.Id,
                                       (el, et) => new
                                       {
                                           et.StatusId,
                                           et.CompanyId
                                       })
                                       .FirstOrDefaultAsync();

            if (emp is null)
            {
                return ErrorHelper.ErrorResult("Id", "You are not registered. Please contact administrator.");
            }

            if (emp?.StatusId != 1)
            {
                return ErrorHelper.ErrorResult("Id", "Employee is not active.");
            }

            if (emp.CompanyId is null)
            {
                return ErrorHelper.ErrorResult("Id", "You are not assigned to any company.");
            }

            var leaveYear = await _context.LeaveYearCompanies
                .Where(x => x.CompanyId == emp.CompanyId || x.CompanyId == null)
                .Include(x => x.LeaveYear)
                .OrderByDescending(x => x.CompanyId)
                .FirstOrDefaultAsync();

            if (leaveYear is null)
            {
                return ErrorHelper.ErrorResult("Id", "No leave year defined.");
            }

            if (startDate > leaveYear.LeaveYear.EndDate || endDate > leaveYear.LeaveYear.EndDate || startDate < leaveYear.LeaveYear.StartDate || endDate < leaveYear.LeaveYear.StartDate)
            {
                return ErrorHelper.ErrorResult("StartDate", "Date should be between selected leave year.");
            }

            var leaveLedgers = await _context.LeaveLedgers
                .Where(x => x.EmpId == user.EmpId && x.LeaveId == input.LeaveId && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == emp.CompanyId && !x.IsClosed)
                .ToListAsync();

            var pendingLeaves = await _context.LeaveApplicationHistories
                .Where(x => x.EmpId == user.EmpId && x.LeaveId == input.LeaveId 
                    && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == emp.CompanyId && !x.IsClosed
                    && x.Status == "pending")
                .ToListAsync();

            if (await _context.LeaveLedgers
                .AnyAsync(x => x.EmpId == user.EmpId 
                    && x.Taken > 0 
                    && x.Leave_Date >= startDate && x.Leave_Date <= endDate))
            {
                return ErrorHelper.ErrorResult("StartDate", "Employee has already taken leave somewhere in the given date range.");
            };

            if (await _context.LeaveApplicationHistories.AnyAsync(x => x.EmpId == user.EmpId 
                    && (x.Status == "pending" || x.Status == "approved")
                    && x.StartDate <= endDate && x.EndDate >= startDate))
            {
                return ErrorHelper.ErrorResult("StartDate", "Employee has already pending or approved leave somewhere in the given date range.");
            };

            //var calendarId = await _context.EmpCalendars.Where(x => x.EmpId == user.EmpId).Select(x => x.CalendarId).FirstOrDefaultAsync();

            //if (await _context.HolidayCalendars
            //    .Include(x => x.Holiday)
            //    .Where(x => x.Holiday.Date >= startDate && x.Holiday.Date <= endDate && x.CalendarId == calendarId)
            //    .AnyAsync())
            //{
            //    return ErrorHelper.ErrorResult("StartDate", "Employee has holiday somewhere in the given date range.");
            //}

            //List<DateOnly> weekends = await Helper.GetWeekends(startDate, endDate, user.EmpId, _context);

            //if (weekends.Any(x => x >= startDate && x <= endDate))
            //{
            //    return ErrorHelper.ErrorResult("StartDate", "Employee has weekend somewhere in the given date range.");
            //}

            decimal? balance = leaveLedgers.Sum(x => x.Given) - leaveLedgers.Sum(x => x.Taken) - pendingLeaves.Sum(x => x.TotalDays);
            decimal? totalDays = (decimal) ((endDate.DayNumber - startDate.DayNumber) + 1) / (input.HLeaveType == 0 || input.HLeaveType == null ? 1 : 2);

            var leave = await _context.Leaves.FirstOrDefaultAsync(x => x.Id == input.LeaveId);

            if (!leave.IsHalfLeave && input.HLeaveType == 1)
            {
                return ErrorHelper.ErrorResult("HLeaveType", "This leave cannot be taken for half day.");
            }

            if (leave.IsPaidLeave != 0 && balance < totalDays)
            {
                return ErrorHelper.ErrorResult("StartDate", "Not enough balance on selected leave.");
            };

            //short? month = leaveLedgers.Where(x => x.IsRegular == 1).Select(x => x.GivenMonth).SingleOrDefault();

            //if(startDate.Month < month)
            //{
            //    return ErrorHelper.ErrorResult("StartDate", "No balance on selected month");
            //};

            LeaveApplicationHistory history = new LeaveApplicationHistory
            {
                EmpId = user.EmpId ?? 0,
                LeaveId = (short)input.LeaveId,
                StartDate = startDate,
                EndDate = endDate,
                Address = input.Address,
                ContactNumber = input.ContactNumber,
                Reason = input.Reason,
                Status = "pending",
                LeaveYearId = leaveYear.LeaveYearId,
                CompanyId = emp.CompanyId,
                HLeaveType = (byte)(input.HLeaveType ?? 0),
                TotalDays = totalDays
            };

            _context.Add(history);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Approve/{id}")]
        public async Task<IActionResult> ApproveLeave(int id)
        {
            var leaveHistory = await _context.LeaveApplicationHistories.Where(x => x.Id == id && x.Status == "pending").SingleOrDefaultAsync();

            if (leaveHistory == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            var emp = await _context.EmpLogs.Where(x => x.EmployeeId == leaveHistory.EmpId)
                                       .Join(_context.EmpTransactions, el => el.Id, et => et.Id,
                                       (el, et) => new
                                       {
                                           et.StatusId,
                                           et.CompanyId
                                       })
                                       .FirstOrDefaultAsync();

            if (emp is null)
            {
                return ErrorHelper.ErrorResult("Id", "You are not registered. Please contact administrator.");
            }

            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly firstDay = new(currentDate.Year, currentDate.Month, 1);

            if (leaveHistory.StartDate < firstDay)
            {
                return ErrorHelper.ErrorResult("Id", "Cannot approve leave for previous months.");
            }

            if (emp?.StatusId != 1)
            {
                return ErrorHelper.ErrorResult("Id", "Employee is not active.");
            }

            if (emp.CompanyId is null)
            {
                return ErrorHelper.ErrorResult("Id", "You are not assigned to any company.");
            }

            var leaveYear = await _context.LeaveYearCompanies
                .Where(x => x.CompanyId == emp.CompanyId || x.CompanyId == null)
                .Include(x => x.LeaveYear)
                .OrderByDescending(x => x.CompanyId)
                .FirstOrDefaultAsync();

            if (leaveYear is null)
            {
                return ErrorHelper.ErrorResult("Id", "No leave year defined.");
            }

            var calendarId = await _context.EmpCalendars.Where(x => x.EmpId == leaveHistory.EmpId).Select(x => x.CalendarId).FirstOrDefaultAsync();

            if (await _context.HolidayCalendars
            .Include(x => x.Holiday)
            .Where(x => x.Holiday.Date >= leaveHistory.StartDate && x.Holiday.Date <= leaveHistory.EndDate && x.CalendarId == calendarId)
                .AnyAsync())
            {
                leaveHistory.DisapprovedByUserId = 1;
                leaveHistory.CancellationRemarks = "Weekend";
                leaveHistory.Status = "cancelled";

                await _context.SaveChangesAsync();

                return ErrorHelper.ErrorResult("Id", "Employee has holiday somewhere in the given date range. Leave is disapproved.");
            }

            List<DateOnly> weekends = await Helper.GetWeekends(leaveHistory.StartDate, leaveHistory.EndDate, leaveHistory.EmpId, _context);

            if (weekends.Any(x => x >= leaveHistory.StartDate && x <= leaveHistory.EndDate))
            {
                leaveHistory.DisapprovedByUserId = 1;
                leaveHistory.CancellationRemarks = "Holiday";
                leaveHistory.Status = "cancelled";

                await _context.SaveChangesAsync();

                return ErrorHelper.ErrorResult("Id", "Employee has weekend somewhere in the given date range. Leave is disapproved");
            }

            List<LeaveLedger> data = new();

            DateOnly startDate = leaveHistory.StartDate;

            do
            {
                data.Add(new LeaveLedger
                {
                    EmpId = leaveHistory.EmpId,
                    LeaveId = leaveHistory.LeaveId,
                    Leave_Date = startDate,
                    Remarks = leaveHistory.Reason,
                    Address = leaveHistory.Address,
                    ContactNumber = leaveHistory.ContactNumber,
                    ApprovedById = User.GetUserId(),
                    TransactionUser = User.GetUsername(),
                    LeaveYearId = leaveYear.LeaveYearId,
                    CompanyId = emp.CompanyId,
                    HLeaveType = leaveHistory.HLeaveType,

                    //Default
                    Taken = (leaveHistory.HLeaveType == (byte)0 ? (decimal)1 : (decimal)0.5),
                    IsRegular = 0,
                    Adjusted = 0,
                    NoHrs = 0,
                });

                startDate = startDate.AddDays(1);

            } while (startDate <= leaveHistory.EndDate);

            leaveHistory.Status = "approved";
            leaveHistory.ApprovedByUserId = User.GetUserId();
            leaveHistory.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.AddRange(data);
                await _context.SaveChangesAsync();
            } catch (Exception ex)
            {
                leaveHistory.Status = "pending";
                leaveHistory.ApprovedByUserId = null;
                leaveHistory.UpdatedAt = DateTime.UtcNow;

                _context.RemoveRange(data);

                await _context.SaveChangesAsync();

                return ErrorHelper.ErrorResult("Id", ex.Message);
            }

            return Ok();
        }

        [HttpPost("Disapprove/{id}")]
        public async Task<IActionResult> DisapproveLeave(int id)
        {
            var leaveHistory = await _context.LeaveApplicationHistories.Where(x => x.Id == id && x.Status == "pending").SingleOrDefaultAsync();

            if (leaveHistory == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly firstDay = new(currentDate.Year, currentDate.Month, 1);

            if (leaveHistory.StartDate < firstDay)
            {
                return ErrorHelper.ErrorResult("Id", "Cannot disapprove leave for previous months.");
            }


            var leaveLedgerTemps = await _context.LeaveLedgersTemp
                .Where(x => x.EmpId == leaveHistory.EmpId && x.LeaveId == leaveHistory.LeaveId && x.Leave_Date >= leaveHistory.StartDate && x.Leave_Date <= leaveHistory.EndDate)
                .ToListAsync();

            leaveHistory.Status = "disapproved";
            leaveHistory.DisapprovedByUserId = User.GetUserId();
            leaveHistory.UpdatedAt = DateTime.UtcNow;

            _context.RemoveRange(leaveLedgerTemps);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // Put: LeaveApplicationHistories/Cancel/{id}
        [HttpPost("Cancel/{id}")]
        public async Task<IActionResult> Cancel(int id, CancelInputModel input)
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

            var data = await _context.LeaveApplicationHistories.Where(x => x.Id == id && x.Status == "pending").SingleOrDefaultAsync();

            if (data.EmpId != user.EmpId)
            {
                return Unauthorized("Invalid User");
            }

            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly firstDay = new(currentDate.Year, currentDate.Month, 1);

            if (data.StartDate < firstDay)
            {
                return ErrorHelper.ErrorResult("Id", "Cannot cancel leave for previous months.");
            }

            var leaveLedgerTemps = await _context.LeaveLedgersTemp
                .Where(x => x.EmpId == data.EmpId && x.LeaveId == data.LeaveId && x.Leave_Date >= data.StartDate && x.Leave_Date <= data.EndDate)
                .ToListAsync();

            data.CancellationRemarks = input.CancellationRemarks;
            data.Status = "cancelled";
            data.UpdatedAt = DateTime.UtcNow;

            _context.RemoveRange(leaveLedgerTemps);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private static string FullName(string firstName, string middleName, string lastName)
        {
            return (firstName + " " + middleName + middleName ?? " " + lastName);
        }

        public class LeaveSummary
        {
            public short? LeaveId { get; set; }
            public string? LeaveName { get; set; }
            public decimal? Credited { get; set; }
            public decimal? Availed { get; set; }
            public decimal? Pending { get; set; }
            public decimal? Balance { get; set; }

        }

        public class LeaveApplicationModel
        {
            public int LeaveId { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string Reason { get; set; }
            public string Address { get; set; }
            public string ContactNumber { get; set; }
            public int? HLeaveType { get; set; }
        }

        public class CancelInputModel
        {
            public string CancellationRemarks { get; set; }
        }

        public class LeaveApplicationModelValidator : AbstractValidator<LeaveApplicationModel>
        {
            private readonly DataContext _context;

            public LeaveApplicationModelValidator(DataContext context)
            {
                _context = context;

                Transform(x => x.LeaveId, v => (short)v)
                    .NotEmpty()
                    .IdMustExist(_context.Leaves.AsQueryable());

                RuleFor(x => x.Address)
                    .NotEmpty();

                RuleFor(x => x.ContactNumber)
                    .NotEmpty()
                    .MustBeDigits(10);

                RuleFor(x => x.HLeaveType)
                    .GreaterThan(-1)
                    .LessThan(3)
                    .Unless(x => x.HLeaveType is null);

                RuleFor(x => x.StartDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.EndDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.StartDate, "Start Date");
            }
        }

        public class CancelInputModelValidator : AbstractValidator<CancelInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public CancelInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();
            }

            protected override bool PreValidate(ValidationContext<CancelInputModel> context, ValidationResult result)
            {
                if (_context.LeaveApplicationHistories.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
