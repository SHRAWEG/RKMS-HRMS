    using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Globalization;
using Hrms.Common.Models;
using Microsoft.EntityFrameworkCore.Internal;
using System.Diagnostics.CodeAnalysis;
using static Hrms.AdminApi.Controllers.EmpTransactionsController;
using static Hrms.AdminApi.Controllers.EmployeesController;
using NPOI.SS.Formula.Functions;
using static Mysqlx.Notice.Warning.Types;
using static Hrms.AdminApi.Controllers.AttendancesController;
using Hrms.Common.Data.Migrations;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class LeaveManagementsController : Controller
    {
        private readonly DataContext _context;

        public LeaveManagementsController(DataContext context)
        {
            _context = context;
        }

        // GET: LeaveManagements
        [CustomAuthorize("leave-history")]
        [HttpGet("History")]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, 
            int? empId, string fromDate, string toDate, int? companyId, int? departmentId, int? divisionId)
        {
            var query = _context.LeaveApplicationHistories
                .Include(x => x.Emp)
                .Include(x => x.Leave)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ApprovedByUser.Emp)
                .Include(x => x.DisapprovedByUser)
                .Include(x => x.DisapprovedByUser.Emp)
                .Join(_context.EmpLogs, 
                    lah => lah.EmpId, 
                    el => el.EmployeeId, 
                    (lah, el) => new
                {
                    LeaveApplicationHistory = lah,
                    PId = el.Id,
                })
                .Join(_context.EmpTransactions.Include(x => x.Company).Include(x => x.Department).Include(x => x.Division), 
                    x => x.PId, 
                    et => et.Id, 
                    (x, et) => new LeaveHistoryDTO
                {
                    LeaveApplicationHistory = x.LeaveApplicationHistory,
                    EmpTransaction = et
                })
                .AsQueryable();

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                var empIds = await _context.EmpLogs
                    .Join(_context.EmpTransactions,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        EmpId = el.EmployeeId,
                        CompanyId = et.CompanyId
                    })
                    .Where(x => companyIds.Contains(x.CompanyId ?? 0))
                    .Select(x => x.EmpId)
                    .ToListAsync();

                query = query.Where(x => empIds.Contains(x.LeaveApplicationHistory.EmpId));
            }

            if (empId != null)
            {
                query = query.Where(x => x.LeaveApplicationHistory.EmpId == empId);
            }

            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);
                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

                query = query.Where(b => b.LeaveApplicationHistory.StartDate >= FromDate && b.LeaveApplicationHistory.EndDate <= ToDate);
            }

            if (companyId != null)
            {
                query = query.Where(x => x.EmpTransaction.CompanyId == companyId); 
            }

            if (departmentId != null)
            {
                query = query.Where(x => x.EmpTransaction.DepartmentId == departmentId);
            }

            if (divisionId != null)
            {
                query = query.Where(x => x.EmpTransaction.DivisionId == divisionId);
            }

            Expression<Func<LeaveHistoryDTO, object>> field = sortColumn switch
            {
                "EmpCode" => x => x.LeaveApplicationHistory.Emp.CardId,
                "CreatedAt" => x => x.LeaveApplicationHistory.CreatedAt,
                "LeaveId" => x => x.LeaveApplicationHistory.LeaveId,
                "StartDate" => x => x.LeaveApplicationHistory.StartDate,
                "EndDate" => x => x.LeaveApplicationHistory.EndDate,
                "Status" => x => x.LeaveApplicationHistory.Status,
                "DepartmentName" => x => x.EmpTransaction.Department.Name,
                "CompanyName" => x => x.EmpTransaction.Company.Name,
                "DivisionName" => x => x.EmpTransaction.Division.Name,
                _ => x => x.LeaveApplicationHistory.Id
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.LeaveApplicationHistory.Id);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<LeaveHistoryDTO>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.LeaveApplicationHistory.Id,
                    EmpId = x.LeaveApplicationHistory.EmpId,
                    EmpCode = x.LeaveApplicationHistory.Emp.CardId,
                    EmpName = Helper.FullName(x.LeaveApplicationHistory.Emp.FirstName, x.LeaveApplicationHistory.Emp.MiddleName, x.LeaveApplicationHistory.Emp.LastName),
                    LeaveId = x.LeaveApplicationHistory.LeaveId,
                    LeaveName = x.LeaveApplicationHistory.Leave.Name,
                    CreatedAt = x.LeaveApplicationHistory.CreatedAt,
                    UpdatedAt = x.LeaveApplicationHistory.UpdatedAt,
                    StartDate = x.LeaveApplicationHistory.StartDate,
                    EndDate = x.LeaveApplicationHistory.EndDate,
                    Status = x.LeaveApplicationHistory.Status,
                    ApprovedByUserId = x.LeaveApplicationHistory.ApprovedByUserId,
                    ApprovedByUserCode = x.LeaveApplicationHistory.ApprovedByUser?.UserName,
                    ApprovedByUserName = Helper.FullName(x.LeaveApplicationHistory.ApprovedByUser?.Emp?.FirstName, x.LeaveApplicationHistory.ApprovedByUser?.Emp?.MiddleName, x.LeaveApplicationHistory.ApprovedByUser?.Emp?.LastName),
                    DisapprovedByUserId = x.LeaveApplicationHistory.DisapprovedByUserId,
                    DisapprovedByUserCode = x.LeaveApplicationHistory.DisapprovedByUser?.UserName,
                    DisapprovedByUserName = Helper.FullName(x.LeaveApplicationHistory.DisapprovedByUser?.Emp?.FirstName, x.LeaveApplicationHistory.DisapprovedByUser?.Emp?.MiddleName, x.LeaveApplicationHistory.DisapprovedByUser?.Emp?.LastName),
                    CompanyId = x.EmpTransaction.CompanyId,
                    CompanyName = x.EmpTransaction.Company?.Name,
                    DepartmentId = x.EmpTransaction.DepartmentId,
                    DepartmentName = x.EmpTransaction.Department?.Name,
                    DivisionId = x.EmpTransaction.DivisionId,
                    DivisionName = x.EmpTransaction.Division?.Name,
                    TotalDays = x.LeaveApplicationHistory.TotalDays,
                    IsHalfLeave = x.LeaveApplicationHistory.HLeaveType != 0
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // Get Leaves assigned to the employee : id = PId
        [CustomAuthorize("leave-application")]
        [HttpGet("EmpLeave/{id}")]
        public async Task<IActionResult> GetEmpLeaves(int id)
        {
            var data = await _context.EmpLeaves.Where(x => x.PId == id)
                            .Include(x => x.Leave)
                            .ToListAsync();

            var unpaidLeaves = await _context.Leaves.Where(x => x.IsPaidLeave == 0).Select(x => new LeavesDto
            {
                LeaveId = x.Id,
                Name = x.Name,
                IsHalfLeave = x.IsHalfLeave
            }).ToListAsync();

            List<LeavesDto> leaves = data.Select(x => new LeavesDto
            {
                LeaveId = x.LeaveId,
                Name = x.Leave.Name,
                IsHalfLeave = x.Leave.IsHalfLeave
            }).ToList();

            leaves.AddRange(unpaidLeaves);

            return Ok(new
            {
                Data = leaves
            });
        }

        // Get Leave Summary for filling Leave Application
        [CustomAuthorize("leave-application")]
        [HttpGet("LeaveSummary/{id}")]
        public async Task<IActionResult> GetLeaveSummary(int id)
        {
            var emp = await _context.EmpDetails
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    Name = FullName(x.FirstName, x.MiddleName, x.LastName),

                }).FirstOrDefaultAsync();

            if (emp is null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id");
            }

            var empLog = await _context.EmpLogs.Where(x => x.EmployeeId == id).FirstOrDefaultAsync();

            if (empLog is null)
            {
                return ErrorHelper.ErrorResult("Id", "Emp is not registered.");
            }

            var empTran = await _context.EmpTransactions.FirstOrDefaultAsync(x => x.Id == empLog.Id);

            if (empTran is null)
            {
                return ErrorHelper.ErrorResult("Id", "Transaction record does not exist.");
            }

            if (empTran.CompanyId == null)
            {
                return ErrorHelper.ErrorResult("Id", "No company assigned to the employee.");
            }

            var leaveYear = await _context.LeaveYearCompanies
                .Where(x => x.CompanyId == empTran.CompanyId || x.CompanyId == null)
                .OrderBy(x => x.CompanyId)
                .FirstOrDefaultAsync();

            if (leaveYear is null)
            {
                return ErrorHelper.ErrorResult("Id", "No leave year defined.");
            }

            var leaves = await (from leave in _context.Leaves
                                join leaveLedger in _context.LeaveLedgers.Where(x => x.EmpId == id && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && !x.IsClosed)
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
                .Where(x => x.EmpId == id && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && !x.IsClosed && x.Status == "pending")
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

        [CustomAuthorize("leave-application")]
        [HttpPost("LeaveApplication")]
        public async Task<IActionResult> LeaveApplication(LeaveApplicationModel input)
        {
            var emp = await _context.EmpLogs.Where(x => x.EmployeeId == input.EmpId)
                            .Join(_context.EmpTransactions, el => el.Id, et => et.Id,
                            (el, et) => new
                            {
                                et.StatusId,
                                et.CompanyId
                            })
                            .FirstOrDefaultAsync();

            if (emp is null)
            {
                return ErrorHelper.ErrorResult("Id", "Employee is not registered");
            }

            if (emp.StatusId != 1)
            {
                return ErrorHelper.ErrorResult("Id", "Employee is not active.");
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Contains(emp?.CompanyId ?? 0))
                {
                    return Forbid();
                }
            }

            if (emp?.CompanyId == null)
            {
                return ErrorHelper.ErrorResult("Id", "No company assigned to the employee.");
            }

            var leaveYear = await _context.LeaveYearCompanies
                .Where(x => x.CompanyId == emp.CompanyId || x.CompanyId == null)
                .Include(x => x.LeaveYear)
                .OrderBy(x => x.CompanyId)
                .FirstOrDefaultAsync();

            if (leaveYear is null)
            {
                return ErrorHelper.ErrorResult("Id", "No leave year defined.");
            }

            DateOnly startDate = DateOnlyHelper.ParseDateOrNow(input.StartDate);
            DateOnly endDate = DateOnlyHelper.ParseDateOrNow(input.EndDate);

            if (startDate > leaveYear.LeaveYear.EndDate || endDate > leaveYear.LeaveYear.EndDate || startDate < leaveYear.LeaveYear.StartDate || endDate < leaveYear.LeaveYear.StartDate)
            {
                return ErrorHelper.ErrorResult("StartDate", "Date should be between selected leave year.");
            }

            var leaveLedgers = await _context.LeaveLedgers
                .Where(x => x.EmpId == input.EmpId && x.LeaveId == input.LeaveId && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == emp.CompanyId && !x.IsClosed)
                .ToListAsync();

            var pendingLeaves = await _context.LeaveApplicationHistories
                .Where(x => x.EmpId == input.EmpId && x.LeaveId == input.LeaveId 
                    && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == emp.CompanyId && !x.IsClosed
                    && x.Status == "pending")
                .ToListAsync();

            if (await _context.LeaveLedgers
                .AnyAsync(x => x.EmpId == input.EmpId 
                    && x.Taken > 0
                    && x.Leave_Date >= startDate && x.Leave_Date <= endDate))
            {
                return ErrorHelper.ErrorResult("StartDate", "Employee has already taken leave somewhere in the given date range.");
            };

            if (await _context.LeaveApplicationHistories.AnyAsync(x => x.EmpId == input.EmpId 
                    && (x.Status == "pending" || x.Status == "approved") 
                    && x.StartDate <= endDate && x.EndDate >= startDate))
            {
                return ErrorHelper.ErrorResult("StartDate", "Employee has already pending or approved leave somewhere in the given date range.");
            };

            //var calendarId = await _context.EmpCalendars.Where(x => x.EmpId == input.EmpId).Select(x => x.CalendarId).FirstOrDefaultAsync();

            //if (await _context.HolidayCalendars
            //    .Include(x => x.Holiday)
            //    .Where(x => x.Holiday.Date >= startDate && x.Holiday.Date <= endDate && x.CalendarId == calendarId)
            //    .AnyAsync())
            //{
            //    return ErrorHelper.ErrorResult("StartDate", "Employee has holiday somewhere in the given date range.");
            //}

            //List<DateOnly> weekends = await Helper.GetWeekends(startDate, endDate, input.EmpId, _context);

            //if (weekends.Any(x => x >= startDate && x <= endDate))
            //{
            //    return ErrorHelper.ErrorResult("StartDate", "Employee has weekend somewhere in the given date range.");
            //}

            decimal? balance = leaveLedgers.Sum(x => x.Given) - leaveLedgers.Sum(x => x.Taken) - pendingLeaves.Sum(x => x.TotalDays);
            decimal? totalDays = (decimal)((endDate.DayNumber - startDate.DayNumber) + 1) / (input.HLeaveType == 0 || input.HLeaveType == null ? 1 : 2);

            var leave = await _context.Leaves.Where(x => x.Id == input.LeaveId).FirstOrDefaultAsync();

            if (leave.IsPaidLeave != 0 && balance < totalDays)
            {
                return ErrorHelper.ErrorResult("StartDate", "Not enough balance on selected leave.");
            };

            if (!leave.IsHalfLeave && input.HLeaveType == 1)
            {
                return ErrorHelper.ErrorResult("HLeaveType", "This leave cannot be taken for half day.");
            }

            //short? month = leaveLedgers.Where(x => x.IsRegular == 1).Select(x => x.GivenMonth).SingleOrDefault();

            //if(startDate.Month < month)
            //{
            //    return ErrorHelper.ErrorResult("StartDate", "No balance on selected month");
            //};

            List<LeaveLedger> newLeaves = new();

            DateOnly date = startDate;

            do
            {
                newLeaves.Add(new LeaveLedger
                {
                    EmpId = input.EmpId,
                    LeaveId = (short)input.LeaveId,
                    Leave_Date = date,
                    Remarks = input.Reason,
                    Address = input.Address,
                    ContactNumber = input.ContactNumber,
                    ApprovedById = User.GetUserId(),
                    TransactionUser = User.GetUsername(),
                    LeaveYearId = leaveYear.LeaveYearId,
                    CompanyId = emp.CompanyId,
                    HLeaveType = (byte)(input.HLeaveType ?? 0),

                    //Default
                    Taken = (input.HLeaveType == 0 || input.HLeaveType == null ? (decimal)1 : (decimal)0.5),
                    IsRegular = 0,
                    Adjusted = 0,
                    NoHrs = 0,
                });

                date = date.AddDays(1);

            } while (date <= endDate);

            LeaveApplicationHistory history = new LeaveApplicationHistory
            {
                EmpId = input.EmpId,
                LeaveId = (short)input.LeaveId,
                StartDate = startDate,
                EndDate = endDate,
                Address = input.Address,
                ContactNumber = input.ContactNumber,
                Reason = input.Reason,
                Status = "approved",
                ApprovedByUserId = User.GetUserId(),
                LeaveYearId = leaveYear.LeaveYearId,
                CompanyId = emp.CompanyId,
                HLeaveType = (byte)(input.HLeaveType ?? 0),
                TotalDays = totalDays
            };

            try
            {
                _context.Add(history);
                _context.AddRange(newLeaves);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _context.Remove(history);
                _context.RemoveRange(newLeaves);

                await _context.SaveChangesAsync();

                return ErrorHelper.ErrorResult("Id", ex.Message);
            }

            return Ok();
        }

        // DELETE: LeaveLedger/5
        [CustomAuthorize("leave-cancellation")]
        [HttpPost("Cancel/{id}")]
        public async Task<IActionResult> Delete(int id, CancelInputModel input)
        {
            var leaveApplicationHistory = await _context.LeaveApplicationHistories.FindAsync(id);

            if (leaveApplicationHistory == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly firstDay = new(currentDate.Year, currentDate.Month, 1);

            if (currentDate.Day > 10)
            {
                if (leaveApplicationHistory.StartDate < firstDay)
                {
                    return ErrorHelper.ErrorResult("Id", "Cannot cancel leave for applied previous months.");
                }
            } else
            {
                if (leaveApplicationHistory.StartDate < firstDay.AddMonths(-1))
                {
                    return ErrorHelper.ErrorResult("Id", "Cannot cancel leave applied for previous months.");
                }
            }

            var emp = await _context.EmpLogs.Where(x => x.EmployeeId == leaveApplicationHistory.EmpId)
                .Join(_context.EmpTransactions, el => el.Id, et => et.Id,
                (el, et) => new
                {
                    et.StatusId,
                    et.CompanyId
                })
                .FirstOrDefaultAsync();

            if (emp?.StatusId != 1)
            {
                return ErrorHelper.ErrorResult("Id", "Employee is not active.");
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Contains(emp?.CompanyId ?? 0))
                {
                    return Forbid();
                }
            }

            leaveApplicationHistory.Status = "cancelled";
            leaveApplicationHistory.CancellationRemarks = input.CancellationRemarks;
            leaveApplicationHistory.DisapprovedByUserId = User.GetUserId();
            leaveApplicationHistory.UpdatedAt = DateTime.UtcNow;

            var leaveLedgers = await _context.LeaveLedgers.Where(x => x.EmpId == leaveApplicationHistory.EmpId && x.LeaveId == leaveApplicationHistory.LeaveId && x.Leave_Date >= leaveApplicationHistory.StartDate && x.Leave_Date <= leaveApplicationHistory.EndDate).ToListAsync();

            _context.RemoveRange(leaveLedgers);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("DownloadFormat")]
        public IActionResult DownloadFormat()
        {
            Type table = typeof(EmpLeaveHeader);

            byte[] data;

            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader(table);

                    csv.NextRecord();
                }

                data = stream.ToArray();
            }

            return File(data, "text/csv", "EmpLeaves.csv");
        }

        // IMPORT: LeaveManagements/imports
        [CustomAuthorize("import-leave-balance")]
        [HttpPost("Import")]
        public async Task<IActionResult> Import([FromForm] ImportModel input)
        {
            if (Path.GetExtension(input.File.FileName).ToLower() != ".csv")
            {
                return BadRequest("Invalid file type.");
            }

            List<Error> errors = new();

            using (var reader = new StreamReader(input.File.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<EmpLeaveHeader>().ToList();

                if (!records.Any())
                {
                    return BadRequest("No records found.");
                }

                bool isError = false;

                try
                {
                    int i = 2;

                    foreach (var record in records)
                    {
                        isError = false;
                        List<string> errorsData = new();
                        EmpTransaction? empTran = new();
                        LeaveYearCompany? leaveYear = new();

                        var emp = await _context.EmpDetails.Where(x => x.CardId == record.EmpCode).FirstOrDefaultAsync();
                        var leave = await _context.Leaves.Where(x => x.Abbreviation == record.LeaveAbbreviation).FirstOrDefaultAsync();

                        if (leave is null)
                        {
                            errorsData.Add("Leave does not exist.");
                            isError = true;
                        }

                        if (emp is null)
                        {
                            errorsData.Add("Employee does not exist.");
                            isError = true;
                        }
                        else
                        {
                            var empLog = await _context.EmpLogs.Where(x => x.EmployeeId == emp.Id).FirstOrDefaultAsync();

                            if (empLog is null)
                            {
                                errorsData.Add("Employee is not registered.");
                                isError = true;
                            } else
                            {
                               empTran = await _context.EmpTransactions.Where(x => x.Id == empLog.Id).FirstOrDefaultAsync();

                                if (empTran is null)
                                {
                                    errorsData.Add("Transaction record does not exist.");
                                    isError = true;
                                }
                                else
                                {
                                    if (empTran.StatusId != 1)
                                    {
                                        errorsData.Add("Employee is not active.");
                                        isError = true;
                                    }

                                    if (empTran.CompanyId == null)
                                    {
                                        errorsData.Add("No company assigned to the employee.");
                                        isError = true;
                                    }
                                    else
                                    {
                                        leaveYear = await _context.LeaveYearCompanies
                                            .Where(x => x.CompanyId == empTran.CompanyId || x.CompanyId == null)
                                            .OrderByDescending(x => x.CompanyId)
                                            .FirstOrDefaultAsync();

                                        if (leaveYear is null)
                                        {
                                            errorsData.Add("No leave year defined.");
                                            isError = true;
                                        }
                                    }
                                }

                                if (User.GetUserRole() != "super-admin")
                                {
                                    var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                                    if (!companyIds.Contains(empTran?.CompanyId ?? 0))
                                    {
                                        errorsData.Add("Forbidden");
                                        isError = true;
                                    }
                                }
                            }

                            if (empLog is not null && leave is not null && !await _context.EmpLeaves.AnyAsync(x => x.PId == empLog.Id && x.LeaveId == leave.Id))
                            {
                                errorsData.Add("Employee is not entitled to this leave.");
                                isError = true;
                            }
                        }

                        if (record.Days is null or 0)
                        {
                            errorsData.Add("Please specify number of Days.");
                            isError = true;
                        }

                        if (isError)
                        {
                            errors.Add(new Error
                            {
                                Record = i,
                                Errors = errorsData
                            });

                            i++;

                            continue;
                        }

                        _context.Add(new LeaveLedger
                        {
                            EmpId = emp.Id,
                            LeaveId = leave.Id,
                            Given = record.Days,
                            GivenMonth = (short)DateTime.Now.Month,
                            GivenYear = (short)DateTime.Now.Year,
                            Remarks = record.Remarks,
                            ApprovedById = User.GetUserId(),
                            TransactionUser = User.GetUsername(),
                            LeaveYearId = leaveYear.LeaveYearId,
                            CompanyId = empTran.CompanyId,

                            //Default
                            IsRegular = 1,
                            Adjusted = 0,
                            NoHrs = 0,
                            HLeaveType = 0,
                        });

                        i++;
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Errors = errors
            });
        }

        [HttpGet("LeaveMerge")]
        public async Task<IActionResult> Get()
        {
            var leaveApplications = await _context.LeaveApplicationHistories
                .Where(x => x.Status == "approved")
                .GroupBy(x => new { x.EmpId, x.StartDate, x.LeaveId })
                .Select(x => new
                {
                    EmpId = x.Key.EmpId,
                    LeaveId = x.Key.LeaveId,
                    StartDate = x.Key.StartDate,
                    Count = x.Count()
                })
                .Where(x => x.Count > 1)
                .ToListAsync();

            foreach (var leaveApplication in leaveApplications)
            {
                var l = await _context.LeaveApplicationHistories.Where(x => x.EmpId == leaveApplication.EmpId && x.LeaveId == leaveApplication.LeaveId && x.StartDate == leaveApplication.StartDate && x.Status == "approved").ToListAsync();

                var id = l.Min(x => x.Id);

                _context.RemoveRange(l.Where(x => x.Id != id));
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Data = leaveApplications
            });
        }

        [HttpGet("LeaveLedger")]
        public async Task<IActionResult> GetLL()
        {
            var leaveApplications = await _context.LeaveApplicationHistories.Where(x => x.Status == "approved").ToListAsync();

            var i = 0;

            List<LeaveLedger> data = new();

            foreach(var leaveApplication in leaveApplications)
            {
                var leaveLedger = await _context.LeaveLedgers.Where(x => x.EmpId == leaveApplication.EmpId && x.LeaveId == leaveApplication.LeaveId && x.Taken > 0 && x.Leave_Date >= leaveApplication.StartDate && x.Leave_Date <= leaveApplication.EndDate).ToListAsync();

                DateOnly currentDate = leaveApplication.StartDate;

                do
                {
                    if (!leaveLedger.Any(x => x.Leave_Date == currentDate))
                    {
                        data.Add(new LeaveLedger
                        {
                            EmpId = leaveApplication.EmpId,
                            LeaveId = leaveApplication.LeaveId,
                            Leave_Date = currentDate,
                            Remarks = "Reapply",
                            Address = leaveApplication.Address,
                            ContactNumber = leaveApplication.ContactNumber,
                            ApprovedById = leaveApplication.ApprovedByUserId,
                            TransactionUser = "admin",
                            LeaveYearId = leaveApplication.LeaveYearId,
                            HLeaveType = leaveApplication.HLeaveType,

                            //Default
                            Taken = (leaveApplication.HLeaveType == 0 ? (decimal)1 : (decimal)0.5),
                            IsRegular = 0,
                            Adjusted = 0,
                            NoHrs = 0,
                        });

                        i++;
                    }

                    currentDate = currentDate.AddDays(1);
                } while (currentDate <= leaveApplication.EndDate);
            }

            _context.AddRange(data);
            await _context.SaveChangesAsync();

            return Ok(data.Count + "records added.");
        }

        [HttpGet("Export")]
        public async Task<IActionResult> ExportLeaves()
        {
            var settings = await _context.Settings.SingleOrDefaultAsync();

            if (settings is null)
            {
                return ErrorHelper.ErrorResult("Id", "Settings not set.");
            }

            var emps = await _context.EmpLogs
                .Join(_context.EmpDetails,
                    el => el.EmployeeId,
                    ed => ed.Id,
                    (el, ed) => new
                    {
                        EmpId = el.EmployeeId,
                        PId = el.Id,
                        EmpCode = ed.CardId,
                        EmpName = Helper.FullName(ed.FirstName, ed.MiddleName, ed.LastName),
                    })
                .Join(_context.EmpTransactions,
                    eld => eld.PId,
                    et => et.Id,
                    (eld, et) => new
                    {
                        EmpId = eld.EmpId,
                        PId = eld.PId,
                        EmpCode = eld.EmpCode,
                        EmpName = eld.EmpName,
                        DepartmentId = et.DepartmentId,
                        DepartmentName = et.Department != null ? et.Department.Name : null,
                        DesignationId = et.DesignationId,
                        DesignationName = et.Designation != null ? et.Designation.Name : null,
                    }
                ).ToListAsync();

            List<ExportLeaveBalance> data = new();

            foreach(var emp in emps)
            {
                var leaves = await (from leave in _context.Leaves
                                    join leaveLedger in _context.LeaveLedgers.Where(x => x.EmpId == emp.EmpId && x.LeaveYearId == settings.LeaveYearId)
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
                    .Where(x => x.EmpId == emp.EmpId && x.LeaveYearId == settings.LeaveYearId && x.Status == "pending")
                    .GroupBy(x => x.LeaveId)
                    .Select(x => new
                    {
                        LeaveId = x.Key,
                        TotalPending = x.Sum(y => y.TotalDays)
                    })
                    .ToListAsync();

                foreach (var leave in leaves)
                {
                    var totalPending = pendingLeaves.Where(x => x.LeaveId == leave.LeaveId).Select(x => x.TotalPending).SingleOrDefault();

                    data.Add(new ExportLeaveBalance
                    {
                        EmpCode = emp.EmpCode,
                        EmpName = emp.EmpName,
                        Department = emp.DepartmentName,
                        Designation = emp.DesignationName,
                        LeaveName = leave.LeaveName,
                        LeaveCarryForwarded = 0,
                        LeaveCredited = leave.Credited,
                        LeaveAvailed = leave.Availed,
                        LeavePending = totalPending ?? 0,
                        LeaveEncashed = 0,
                        LeaveBalance = leave.IsPaidLeave == 1 ? leave.Balance - (totalPending ?? 0) : 0
                    });
                }
            }

            Response.ContentType = "text/csv";
            Response.Headers.Add("Content-Disposition", $"attachment; filename=Employees");

            await using (var writer = new StreamWriter(Response.Body))
            await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var record in data)
                {
                    await csv.WriteRecordsAsync(new ExportLeaveBalance[] { record });
                    await csv.FlushAsync();
                }
            }

            await Response.CompleteAsync();

            return new EmptyResult();
        }

        private static string FullName(string firstName, string middleName, string lastName)
        {
            return (firstName + " " + middleName + middleName ?? " " + lastName);
        }

        public class LeavesDto
        {
            public short LeaveId { get; set; }
            public string Name { get; set; }
            public bool IsHalfLeave { get; set; }
        }

        public class LeaveHistoryDTO
        {
            public LeaveApplicationHistory LeaveApplicationHistory { get; set; }
            public EmpTransaction EmpTransaction { get; set; }
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

        public class GiveLeaveModel
        {
            public int EmpId { get; set; }
            public short LeaveId { get; set; }
            public decimal Given { get; set; }
            public string Remarks { get; set; }
            public short GivenMonth { get; set; }
            public short GivenYear { get; set; }
        }

        public class LeaveApplicationModel
        {
            public int EmpId { get; set; }
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

        public class EmpLeaveHeader
        {
            [Name("EMPCODE")]
            public string EmpCode { get; set; }

            [Name("LEAVEABBREVIATION")]
            public string LeaveAbbreviation { get; set; }

            [Name("DAYS")]
            public decimal? Days { get; set; }

            [Name("REMARKS")]
            public string Remarks { get; set; }
        }

        public class ImportModel
        {
            public IFormFile File { get; set; }
        }

        public class Error
        {
            public int Record { get; set; }
            public List<string> Errors { get; set; }
        }

        public class ExportLeaveBalance
        {
            [Name("Employee Code")]
            public string EmpCode { get; set; }

            [Name("Employee Name")]
            public string EmpName { get; set; }

            [Name("Department")]
            public string Department { get; set; }

            [Name("Designation")]
            public string Designation { get; set; }

            [Name("Leave Name")]
            public string LeaveName { get; set; }

            [Name("Leave Carry Forwarded")]
            public decimal LeaveCarryForwarded { get; set; }

            [Name("Leave Credited")]
            public decimal LeaveCredited { get; set; }

            [Name("Leave Availed")]
            public decimal LeaveAvailed { get; set; }

            [Name("Leave Pending")]
            public decimal LeavePending { get; set; }

            [Name("Leave Encashed")]
            public decimal LeaveEncashed { get; set; }

            [Name("Leave Balance")]
            public decimal LeaveBalance { get; set; }
        }

        public class GiveLeaveModelValidator : AbstractValidator<GiveLeaveModel>
        {
            private readonly DataContext _context;

            public GiveLeaveModelValidator(DataContext context, DbHelper dbHelper)
            {
                _context = context;

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable())
                    .MustBeRegistered(dbHelper);

                Transform(x => x.LeaveId, v => (short)v)
                    .NotEmpty()
                    .IdMustExist(_context.Leaves.AsQueryable());

                RuleFor(x => x.Given)
                    .NotEmpty();

                RuleFor(x => x.Remarks)
                    .NotEmpty();

                RuleFor(x => x.GivenMonth)
                    .NotEmpty()
                    .GreaterThan((short)0)
                    .LessThan((short)13);

                RuleFor(x => x.GivenYear)
                    .NotEmpty();
            }
        }

        public class LeaveApplicationModelValidator : AbstractValidator<LeaveApplicationModel>
        {
            private readonly DataContext _context;

            public LeaveApplicationModelValidator(DataContext context, DbHelper dbHelper)
            {
                _context = context;

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable())
                    .MustBeRegistered(dbHelper);

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
                    .MustBeDate();
            }
        }
    }
}
