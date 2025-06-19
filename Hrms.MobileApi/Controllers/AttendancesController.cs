//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore.Diagnostics;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using System;

//namespace Hrms.MobileApi.Controllers
//{
//    [Route("[controller]")]
//    [ApiController]
//    [Authorize(Roles = "employee")]
//    public class AttendancesController : Controller
//    {
//        private readonly DataContext _context;
//        private readonly UserManager<User> _userManager;

//        public AttendancesController(DataContext context, UserManager<User> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        //// GET: GetAttendance
//        //[HttpGet]
//        //public async Task<IActionResult> Get(int? year, int? month)
//        //{
//        //    DateOnly date = DateOnly.FromDateTime(DateTime.Today);

//        //    var firstDayOfMonth = new DateOnly(year ?? date.Year, month ?? date.Month, 1);
//        //    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

//        //    var user = await _userManager.GetUserAsync(User);

//        //    var branchId = await _context.EmpLogs
//        //        .Where(x => x.EmployeeId == user.EmpId)
//        //        .Join(_context.EmpTransactions,
//        //            el => el.Id,
//        //            et => et.Id,
//        //            (el, et) => et.BranchId
//        //        ).FirstOrDefaultAsync();

//        //    var attendances = await _context.Attendances
//        //        .Include(x => x.RegularisationType)
//        //        .Where(x => x.EmpId == user.EmpId &&
//        //                    (x.TransactionDate >= firstDayOfMonth || x.TransactionDateOut >= firstDayOfMonth) &&
//        //                    (x.TransactionDate <= lastDayOfMonth || x.TransactionDateOut <= lastDayOfMonth))
//        //        .Select(x => new
//        //        {
//        //            x.Id,
//        //            x.TransactionDate,
//        //            x.TransactionDateOut,
//        //            x.EmpId,
//        //            x.InTime,
//        //            x.OutTime,
//        //            x.RegularisationTypeId,
//        //            RegularisationTypeName = x.RegularisationType != null ? x.RegularisationType.Name : "",
//        //        })
//        //        .ToListAsync();

//        //    var weekendDetail = await _context.WeekendDetails
//        //        .Where(x => x.EmpId == user.EmpId || x.BranchId == branchId || x.EmpId == null && x.ValidFrom <= firstDayOfMonth)
//        //        .OrderBy(x => x.EmpId)
//        //        .OrderBy(x => x.BranchId)
//        //        .OrderByDescending(x => x.ValidFrom)
//        //        .FirstOrDefaultAsync();

//        //    var holidays = await _context.Holidays
//        //        .Where(x => x.Date >= firstDayOfMonth && x.Date <= lastDayOfMonth)
//        //        .ToListAsync();

//        //    var rosters = await _context.Rosters
//        //        .Where(x => x.Date >= firstDayOfMonth && x.Date <= lastDayOfMonth && x.EmpId == user.EmpId)
//        //        .Include(x => x.WorkHour)
//        //        .Select(x => new
//        //        {
//        //            x.Id,
//        //            x.Date,
//        //            x.WorkHour.StartTime,
//        //            x.WorkHour.EndTime,
//        //            x.WorkHour.LateInGraceTime
//        //        })
//        //        .ToListAsync();

//        //    var defaultWorkHours = await _context.DefaultWorkHours
//        //        .Where(x => x.EmpId == user.EmpId || x.EmpId == null)
//        //        .OrderByDescending(x => x.EmpId)
//        //        .Include(x => x.WorkHour)
//        //        .Select(x => new
//        //        {
//        //            x.Id,
//        //            x.EmpId,
//        //            x.DayId,
//        //            x.WorkHour.StartTime,
//        //            x.WorkHour.EndTime,
//        //            x.WorkHour.LateInGraceTime,
//        //        })
//        //        .ToListAsync();

//        //    var leaves = await _context.LeaveLedgers
//        //        .Where(x => x.Leave_Date >= firstDayOfMonth && x.Leave_Date <= lastDayOfMonth && x.EmpId == user.EmpId)
//        //        .Include(x => x.Leave)
//        //        .Select(x => new
//        //        {
//        //            x.Leave_Date,
//        //            LeaveName = x.Leave.Name,
//        //        })
//        //        .ToListAsync();

//        //    var pendingLeaves = await _context.LeaveApplicationHistories
//        //        .Where(x => x.StartDate <= lastDayOfMonth && x.EndDate >= firstDayOfMonth && x.EmpId == user.EmpId && x.Status == "pending")
//        //        .Include(x => x.Leave)
//        //        .Select(x => new
//        //        {
//        //            x.StartDate,
//        //            x.EndDate,
//        //            LeaveName = x.Leave.Name,
//        //        })
//        //        .ToListAsync();

//        //    var pendingOfficeOuts = await _context.OfficeOuts
//        //        .Where(x => x.OutDate <= lastDayOfMonth && x.InDate >= firstDayOfMonth && x.EmpId == user.EmpId && x.Status == "pending")
//        //        .Include(x => x.RegularisationType)
//        //        .Select(x => new
//        //        {
//        //            RegularisationTypeName = x.RegularisationType.Name,
//        //            x.InTime,
//        //            x.OutTime,
//        //            x.InDate,
//        //            x.OutDate
//        //        })
//        //        .ToListAsync();

//        //    var pendingMisPunches = await _context.ForcedAttendances
//        //        .Where(x => x.Date <= lastDayOfMonth && x.Date >= firstDayOfMonth && x.EmpId == user.EmpId && x.Status == "pending")
//        //        .Include(x => x.RegularisationType)
//        //        .Select(x => new
//        //        {
//        //            RegularisationTypeName = x.RegularisationType.Name,
//        //            x.Time,
//        //            x.Date,
//        //        })
//        //        .ToListAsync();

//        //    List<int> weekends = new();

//        //    if (weekendDetail?.Sunday == true)
//        //    {
//        //        weekends.Add(1);
//        //    }

//        //    if (weekendDetail?.Monday == true)
//        //    {
//        //        weekends.Add(2);
//        //    }

//        //    if (weekendDetail?.Tuesday == true)
//        //    {
//        //        weekends.Add(3);
//        //    }

//        //    if (weekendDetail?.Wednesday == true)
//        //    {
//        //        weekends.Add(4);
//        //    }

//        //    if (weekendDetail?.Thursday == true)
//        //    {
//        //        weekends.Add(5);
//        //    }

//        //    if (weekendDetail?.Friday == true)
//        //    {
//        //        weekends.Add(6);
//        //    }

//        //    if (weekendDetail?.Saturday == true)
//        //    {
//        //        weekends.Add(7);
//        //    }

//        //    List<AttendanceData> data = new();

//        //    DateOnly currentDate = firstDayOfMonth;

//        //    int lateInCount = 1;

//        //    do
//        //    {
//        //        if(currentDate > date)
//        //        {
//        //            data.Add(new AttendanceData
//        //            {
//        //                Date = currentDate,
//        //            });

//        //            currentDate = currentDate.AddDays(1);
//        //            continue;
//        //        }

//        //        var leave = leaves.Where(x => x.Leave_Date == currentDate).FirstOrDefault();

//        //        if (leave != null)
//        //        {
//        //            data.Add(new AttendanceData
//        //            {
//        //                Date = currentDate,
//        //                DayStatus = GetAbbreviation(leave.LeaveName) + "-A",
//        //                DayStatusDescription = leave.LeaveName + " - Approved",
//        //                Color = "#A020F0"
//        //            });

//        //            currentDate = currentDate.AddDays(1);
//        //            continue;
//        //        }

//        //        var pendingLeave = pendingLeaves.Where(x => x.StartDate <= currentDate && x.EndDate >= currentDate).FirstOrDefault();

//        //        if (pendingLeave != null)
//        //        {
//        //            data.Add(new AttendanceData
//        //            {
//        //                Date = currentDate,
//        //                DayStatus = GetAbbreviation(pendingLeave.LeaveName) + "-P",
//        //                DayStatusDescription = pendingLeave.LeaveName + " - Pending",
//        //                Color = "#A020F0"
//        //            });

//        //            currentDate = currentDate.AddDays(1);
//        //            continue;
//        //        }

//        //        var pendingOfficeOut = pendingOfficeOuts.Where(x => x.OutDate <= currentDate && x.InDate >= currentDate).FirstOrDefault();

//        //        TimeOnly? inTime = new();
//        //        TimeOnly? outTime = new();
//        //        TimeSpan? dailyHour = new();

//        //        if (pendingOfficeOut != null)
//        //        {
//        //            inTime = pendingOfficeOut.OutTime != null ? TimeOnly.ParseExact(pendingOfficeOut.OutTime, "HH:mm:ss") : null;
//        //            outTime = pendingOfficeOut.InTime != null ? TimeOnly.ParseExact(pendingOfficeOut.InTime, "HH:mm:ss") : null;

//        //            dailyHour = outTime - inTime;

//        //            data.Add(new AttendanceData
//        //            {
//        //                Date = currentDate,
//        //                DayStatus = GetAbbreviation(pendingOfficeOut.RegularisationTypeName) + "-P",
//        //                DayStatusDescription = pendingOfficeOut.RegularisationTypeName + " - Pending",
//        //                DailyHour = dailyHour,
//        //                Color = "#ECFFDC"
//        //            });

//        //            currentDate = currentDate.AddDays(1);
//        //            continue;
//        //        }

//        //        var attendance = attendances.Where(x => x.TransactionDate == currentDate).FirstOrDefault();

//        //        if (attendance != null)
//        //        {
//        //            inTime = attendance.InTime != null ? TimeOnly.ParseExact(attendance.InTime, "HH:mm:ss") : null;
//        //            outTime = attendance.OutTime != null ? TimeOnly.ParseExact(attendance.OutTime, "HH:mm:ss") : null;

//        //            dailyHour = outTime - inTime;
//        //        }

//        //        if (weekends.Contains((int)currentDate.DayOfWeek + 1))
//        //        {
//        //            if (attendance != null)
//        //            {
//        //                data.Add(new AttendanceData
//        //                {
//        //                    Date = currentDate,
//        //                    DayStatus = "POW",
//        //                    DayStatusDescription = "Present On Weekly-Off",
//        //                    DailyHour = dailyHour,
//        //                    Color = "#ADD8E6"
//        //                });
//        //            }
//        //            else
//        //            {
//        //                data.Add(new AttendanceData
//        //                {
//        //                    Date = currentDate,
//        //                    DayStatus = "Weekly-Off",
//        //                    DayStatusDescription = "Weekly-Off",
//        //                    Color = "#ADD8E6"
//        //                });
//        //            }

//        //            currentDate = currentDate.AddDays(1);
//        //            continue;
//        //        }

//        //        var holiday = holidays.Where(x => x.Date == currentDate).FirstOrDefault();

//        //        if (holiday != null)
//        //        {
//        //            if (attendance != null)
//        //            {
//        //                data.Add(new AttendanceData
//        //                {
//        //                    Date = currentDate,
//        //                    DayStatus = "POH",
//        //                    DayStatusDescription = "Present On Holiday",
//        //                    DailyHour = dailyHour,
//        //                    Color = "#87CEEB"
//        //                });
//        //            }
//        //            else
//        //            {
//        //                data.Add(new AttendanceData
//        //                {
//        //                    Date = currentDate,
//        //                    DayStatus = "Holiday",
//        //                    DayStatusDescription = "Holiday",
//        //                    Color = "#87CEEB"
//        //                });
//        //            }

//        //            currentDate = currentDate.AddDays(1);
//        //            continue;
//        //        }

//        //        if (attendance is null)
//        //        {
//        //            data.Add(new AttendanceData
//        //            {
//        //                Date = currentDate,
//        //                DayStatus = "Absent",
//        //                DayStatusDescription = "Absent",
//        //                Color = "#FF0000"
//        //            });

//        //            currentDate = currentDate.AddDays(1);
//        //            continue;
//        //        }

//        //        var roster = rosters.Where(x => x.Date == currentDate).FirstOrDefault();
//        //        var defaultWorkHour = defaultWorkHours.Where(x => x.DayId == (short)currentDate.DayOfWeek + 1).OrderBy(x => x.EmpId).FirstOrDefault();

//        //        TimeOnly startTime = new();
//        //        TimeOnly lateInGrace = new();

//        //        if (roster != null)
//        //        {
//        //            startTime = TimeOnly.ParseExact(roster.StartTime, "HH:mm:ss");
//        //            lateInGrace = startTime.AddMinutes((double)(roster.LateInGraceTime ?? 0));
//        //        }
//        //        else
//        //        {
//        //            startTime = TimeOnly.ParseExact(defaultWorkHour.StartTime, "HH:mm:ss");
//        //            lateInGrace = startTime.AddMinutes((double)(defaultWorkHour.LateInGraceTime ?? 0));
//        //        }

//        //        if (attendance.RegularisationTypeId != null)
//        //        {
//        //            data.Add(new AttendanceData
//        //            {
//        //                Date = currentDate,
//        //                DayStatus = GetAbbreviation(attendance.RegularisationTypeName) + "-A",
//        //                DayStatusDescription = attendance.RegularisationTypeName + " - Approved",
//        //                InTime = attendance.InTime,
//        //                OutTime = attendance.OutTime,
//        //                DailyHour = dailyHour,
//        //                Color = "#ECFFDC"
//        //            });

//        //            currentDate = currentDate.AddDays(1);
//        //            continue;
//        //        }

//        //        if (inTime is null)
//        //        {
//        //            var pendingMisPunch = pendingMisPunches.Where(x => x.Date == currentDate).FirstOrDefault();

//        //            if (pendingMisPunch != null)
//        //            {
//        //                data.Add(new AttendanceData
//        //                {
//        //                    Date = currentDate,
//        //                    DayStatus = GetAbbreviation(pendingMisPunch.RegularisationTypeName) + "-P",
//        //                    DayStatusDescription = pendingMisPunch.RegularisationTypeName + " - Pending",
//        //                    OutTime = attendance.OutTime,
//        //                    Color = "#ECFFDC"
//        //                });

//        //                currentDate = currentDate.AddDays(1);
//        //                continue;
//        //            }
//        //        }

//        //        if (outTime is null)
//        //        {
//        //            var pendingMisPunch = pendingMisPunches.Where(x => x.Date == currentDate).FirstOrDefault();

//        //            if (pendingMisPunch != null)
//        //            {
//        //                data.Add(new AttendanceData
//        //                {
//        //                    Date = currentDate,
//        //                    DayStatus = GetAbbreviation(pendingMisPunch.RegularisationTypeName) + "-P",
//        //                    DayStatusDescription = pendingMisPunch.RegularisationTypeName + " - Pending",
//        //                    InTime = attendance.InTime,
//        //                    Color = "#ECFFDC"
//        //                });

//        //                currentDate = currentDate.AddDays(1);
//        //                continue;
//        //            }
//        //        }

//        //        if (inTime > lateInGrace)
//        //        {
//        //            data.Add(new AttendanceData
//        //            {
//        //                Date = currentDate,
//        //                DayStatus = "Half-Day",
//        //                DayStatusDescription = "Half-Day",
//        //                InTime = attendance.InTime,
//        //                OutTime = attendance.OutTime,
//        //                DailyHour = dailyHour,
//        //                Color = "#4F7942"
//        //            });
//        //        }
//        //        else if (inTime > startTime)
//        //        {
//        //            if (lateInCount > 2)
//        //            {
//        //                data.Add(new AttendanceData
//        //                {
//        //                    Date = currentDate,
//        //                    DayStatus = "Half-Day",
//        //                    DayStatusDescription = "Half-Day",
//        //                    InTime = attendance.InTime,
//        //                    OutTime = attendance.OutTime,
//        //                    DailyHour = dailyHour,
//        //                    Color = "#4F7942"
//        //                });
//        //            }
//        //            else
//        //            {
//        //                data.Add(new AttendanceData
//        //                {
//        //                    Date = currentDate,
//        //                    DayStatus = "Late-In",
//        //                    DayStatusDescription = "Late-In",
//        //                    InTime = attendance.InTime,
//        //                    OutTime = attendance.OutTime,
//        //                    DailyHour = dailyHour,
//        //                    Color = "#FFC0CB"
//        //                });

//        //                lateInCount++;
//        //            }
//        //        }
//        //        else
//        //        {
//        //            data.Add(new AttendanceData
//        //            {
//        //                Date = currentDate,
//        //                DayStatus = "Present",
//        //                DayStatusDescription = "Present",
//        //                InTime = attendance.InTime,
//        //                OutTime = attendance.OutTime,
//        //                DailyHour = dailyHour,
//        //                Color = "#7CFC00"
//        //            });
//        //        }

//        //        currentDate = currentDate.AddDays(1);
//        //    } while (currentDate <= lastDayOfMonth);

//        //    var summary = data
//        //        .GroupBy(x => new { x.DayStatus, x.DayStatusDescription, x.Color })
//        //        .Select(x => new
//        //        {
//        //            x.Key.DayStatus,
//        //            x.Key.DayStatusDescription,
//        //            x.Key.Color,
//        //            Total = x.Count()
//        //        })
//        //        .ToList();

//        //    return Ok(new
//        //    {
//        //        Data = data,
//        //        Summary = summary
//        //    });
//        //}

//        // GET: GetAttendance
//        [HttpGet("{empId}")]
//        public async Task<IActionResult> GetTeamAttendance(int empId, int? year, int? month)
//        {
//            if(!await _context.EmpLogs.AnyAsync(x => x.EmployeeId == empId))
//            {
//                return ErrorHelper.ErrorResult("EmpId", "Employee Id is invalid");
//            }

//            DateOnly date = DateOnly.FromDateTime(DateTime.Today);

//            var firstDayOfMonth = new DateOnly(year ?? date.Year, month ?? date.Month, 1);
//            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

//            var user = await _userManager.GetUserAsync(User);
            
//            var emp = await _context.EmpLogs.Where(x => x.EmployeeId == empId)
//                .Join(_context.EmpTransactions,
//                        e => e.Id,
//                        et => et.Id,
//                        (e, et) => new
//                        {
//                            EmpId = e.EmployeeId,
//                            HodEmpId = et != null ? et.HodEmpId : null,
//                            RmEmpId = et != null ? et.RmEmpId : null,
//                            BranchId = et != null ? et.BranchId : null,
//                        }
//                    )
//                .SingleOrDefaultAsync();

//            if (!(user.EmpId == emp.HodEmpId || user.EmpId == emp.RmEmpId || user.EmpId == empId))
//            {
//                return ErrorHelper.ErrorResult("EmpId", "You are not the manager of the employee");
//            }

//            var attendances = await _context.Attendances
//                .Include(x => x.RegularisationType)
//                .Where(x => x.EmpId == emp.EmpId &&
//                            (x.TransactionDate >= firstDayOfMonth || x.TransactionDateOut >= firstDayOfMonth) &&
//                            (x.TransactionDate <= lastDayOfMonth || x.TransactionDateOut <= lastDayOfMonth))
//                .Select(x => new
//                {
//                    x.Id,
//                    x.TransactionDate,
//                    x.TransactionDateOut,
//                    x.EmpId,
//                    x.InTime,
//                    x.OutTime,
//                    x.RegularisationTypeId,
//                    RegularisationTypeName = x.RegularisationType != null ? x.RegularisationType.Name : "",
//                })
//                .ToListAsync();

//            var weekendDetail = await _context.WeekendDetails
//                .Where(x => x.EmpId == emp.EmpId || x.BranchId == emp.BranchId || x.EmpId == null && x.ValidFrom <= firstDayOfMonth)
//                .OrderBy(x => x.EmpId)
//                .OrderBy(x => x.BranchId)
//                .OrderByDescending(x => x.ValidFrom)
//                .FirstOrDefaultAsync();

//            var holidays = await _context.Holidays
//                .Where(x => x.Date >= firstDayOfMonth && x.Date <= lastDayOfMonth)
//                .ToListAsync();

//            var rosters = await _context.Rosters
//                .Where(x => x.Date >= firstDayOfMonth && x.Date <= lastDayOfMonth && x.EmpId == emp.EmpId)
//                .Include(x => x.WorkHour)
//                .Select(x => new
//                {
//                    x.Id,
//                    x.Date,
//                    x.WorkHour.StartTime,
//                    x.WorkHour.EndTime,
//                    x.WorkHour.LateInGraceTime
//                })
//                .ToListAsync();

//            var defaultWorkHours = await _context.DefaultWorkHours
//                .Where(x => x.EmpId == emp.EmpId || x.EmpId == null)
//                .OrderByDescending(x => x.EmpId)
//                .Include(x => x.WorkHour)
//                .Select(x => new
//                {
//                    x.Id,
//                    x.EmpId,
//                    x.DayId,
//                    x.WorkHour.StartTime,
//                    x.WorkHour.EndTime,
//                    x.WorkHour.LateInGraceTime,
//                })
//                .ToListAsync();

//            var leaves = await _context.LeaveLedgers
//                .Where(x => x.Leave_Date >= firstDayOfMonth && x.Leave_Date <= lastDayOfMonth && x.EmpId == emp.EmpId)
//                .Include(x => x.Leave)
//                .Select(x => new
//                {
//                    x.Leave_Date,
//                    LeaveName = x.Leave.Name,
//                })
//                .ToListAsync();

//            var pendingLeaves = await _context.LeaveApplicationHistories
//                .Where(x => x.StartDate <= lastDayOfMonth && x.EndDate >= firstDayOfMonth && x.EmpId == emp.EmpId && x.Status == "pending")
//                .Include(x => x.Leave)
//                .Select(x => new
//                {
//                    x.StartDate,
//                    x.EndDate,
//                    LeaveName = x.Leave.Name,
//                })
//                .ToListAsync();

//            var pendingOfficeOuts = await _context.OfficeOuts
//                .Where(x => x.OutDate <= lastDayOfMonth && x.InDate >= firstDayOfMonth && x.EmpId == emp.EmpId && x.Status == "pending")
//                .Include(x => x.RegularisationType)
//                .Select(x => new
//                {
//                    RegularisationTypeName = x.RegularisationType.Name,
//                    x.InTime,
//                    x.OutTime,
//                    x.InDate,
//                    x.OutDate
//                })
//                .ToListAsync();

//            var pendingMisPunches = await _context.ForcedAttendances
//                .Where(x => x.Date <= lastDayOfMonth && x.Date >= firstDayOfMonth && x.EmpId == emp.EmpId && x.Status == "pending")
//                .Include(x => x.RegularisationType)
//                .Select(x => new
//                {
//                    RegularisationTypeName = x.RegularisationType.Name,
//                    x.Time,
//                    x.Date,
//                })
//                .ToListAsync();

//            List<int> weekends = new();

//            if (weekendDetail?.Sunday == true)
//            {
//                weekends.Add(1);
//            }

//            if (weekendDetail?.Monday == true)
//            {
//                weekends.Add(2);
//            }

//            if (weekendDetail?.Tuesday == true)
//            {
//                weekends.Add(3);
//            }

//            if (weekendDetail?.Wednesday == true)
//            {
//                weekends.Add(4);
//            }

//            if (weekendDetail?.Thursday == true)
//            {
//                weekends.Add(5);
//            }

//            if (weekendDetail?.Friday == true)
//            {
//                weekends.Add(6);
//            }

//            if (weekendDetail?.Saturday == true)
//            {
//                weekends.Add(7);
//            }

//            List<AttendanceData> data = new();

//            DateOnly currentDate = firstDayOfMonth;

//            int lateInCount = 1;

//            do
//            {
//                if (currentDate > date)
//                {
//                    data.Add(new AttendanceData
//                    {
//                        Date = currentDate,
//                    });

//                    currentDate = currentDate.AddDays(1);
//                    continue;
//                }

//                var leave = leaves.Where(x => x.Leave_Date == currentDate).FirstOrDefault();

//                if (leave != null)
//                {
//                    data.Add(new AttendanceData
//                    {
//                        Date = currentDate,
//                        DayStatus = GetAbbreviation(leave.LeaveName) + "-A",
//                        DayStatusDescription = leave.LeaveName + " - Approved",
//                        Color = "#A020F0"
//                    });

//                    currentDate = currentDate.AddDays(1);
//                    continue;
//                }

//                var pendingLeave = pendingLeaves.Where(x => x.StartDate <= currentDate && x.EndDate >= currentDate).FirstOrDefault();

//                if (pendingLeave != null)
//                {
//                    data.Add(new AttendanceData
//                    {
//                        Date = currentDate,
//                        DayStatus = GetAbbreviation(pendingLeave.LeaveName) + "-P",
//                        DayStatusDescription = pendingLeave.LeaveName + " - Pending",
//                        Color = "#A020F0"
//                    });

//                    currentDate = currentDate.AddDays(1);
//                    continue;
//                }

//                var pendingOfficeOut = pendingOfficeOuts.Where(x => x.OutDate <= currentDate && x.InDate >= currentDate).FirstOrDefault();

//                TimeOnly? inTime = new();
//                TimeOnly? outTime = new();
//                TimeSpan? dailyHour = new();

//                if (pendingOfficeOut != null)
//                {
//                    inTime = pendingOfficeOut.OutTime != null ? TimeOnly.ParseExact(pendingOfficeOut.OutTime, "HH:mm:ss") : null;
//                    outTime = pendingOfficeOut.InTime != null ? TimeOnly.ParseExact(pendingOfficeOut.InTime, "HH:mm:ss") : null;

//                    dailyHour = outTime - inTime;

//                    data.Add(new AttendanceData
//                    {
//                        Date = currentDate,
//                        DayStatus = GetAbbreviation(pendingOfficeOut.RegularisationTypeName) + "-P",
//                        DayStatusDescription = pendingOfficeOut.RegularisationTypeName + " - Pending",
//                        DailyHour = dailyHour,
//                        Color = "#ECFFDC"
//                    });

//                    currentDate = currentDate.AddDays(1);
//                    continue;
//                }

//                var attendance = attendances.Where(x => x.TransactionDate == currentDate).FirstOrDefault();

//                if (attendance != null)
//                {
//                    inTime = attendance.InTime != null ? TimeOnly.ParseExact(attendance.InTime, "HH:mm:ss") : null;
//                    outTime = attendance.OutTime != null ? TimeOnly.ParseExact(attendance.OutTime, "HH:mm:ss") : null;

//                    dailyHour = outTime - inTime;
//                }

//                if (weekends.Contains((int)currentDate.DayOfWeek + 1))
//                {
//                    if (attendance != null)
//                    {
//                        data.Add(new AttendanceData
//                        {
//                            Date = currentDate,
//                            DayStatus = "POW",
//                            DayStatusDescription = "Present On Weekly-Off",
//                            DailyHour = dailyHour,
//                            Color = "#ADD8E6"
//                        });
//                    }
//                    else
//                    {
//                        data.Add(new AttendanceData
//                        {
//                            Date = currentDate,
//                            DayStatus = "Weekly-Off",
//                            DayStatusDescription = "Weekly-Off",
//                            Color = "#ADD8E6"
//                        });
//                    }

//                    currentDate = currentDate.AddDays(1);
//                    continue;
//                }

//                var holiday = holidays.Where(x => x.Date == currentDate).FirstOrDefault();

//                if (holiday != null)
//                {
//                    if (attendance != null)
//                    {
//                        data.Add(new AttendanceData
//                        {
//                            Date = currentDate,
//                            DayStatus = "POH",
//                            DayStatusDescription = "Present On Holiday",
//                            DailyHour = dailyHour,
//                            Color = "#87CEEB"
//                        });
//                    }
//                    else
//                    {
//                        data.Add(new AttendanceData
//                        {
//                            Date = currentDate,
//                            DayStatus = "Holiday",
//                            DayStatusDescription = "Holiday",
//                            Color = "#87CEEB"
//                        });
//                    }

//                    currentDate = currentDate.AddDays(1);
//                    continue;
//                }

//                if (attendance is null)
//                {
//                    data.Add(new AttendanceData
//                    {
//                        Date = currentDate,
//                        DayStatus = "Absent",
//                        DayStatusDescription = "Absent",
//                        Color = "#FF0000"
//                    });

//                    currentDate = currentDate.AddDays(1);
//                    continue;
//                }

//                var roster = rosters.Where(x => x.Date == currentDate).FirstOrDefault();
//                var defaultWorkHour = defaultWorkHours.Where(x => x.DayId == (short)currentDate.DayOfWeek + 1).OrderBy(x => x.EmpId).FirstOrDefault();

//                TimeOnly startTime = new();
//                TimeOnly lateInGrace = new();

//                if (roster != null)
//                {
//                    startTime = TimeOnly.ParseExact(roster.StartTime, "HH:mm:ss");
//                    lateInGrace = startTime.AddMinutes((double)(roster.LateInGraceTime ?? 0));
//                }
//                else
//                {
//                    startTime = TimeOnly.ParseExact(defaultWorkHour.StartTime, "HH:mm:ss");
//                    lateInGrace = startTime.AddMinutes((double)(defaultWorkHour.LateInGraceTime ?? 0));
//                }

//                if (attendance.RegularisationTypeId != null)
//                {
//                    data.Add(new AttendanceData
//                    {
//                        Date = currentDate,
//                        DayStatus = GetAbbreviation(attendance.RegularisationTypeName) + "-A",
//                        DayStatusDescription = attendance.RegularisationTypeName + " - Approved",
//                        InTime = attendance.InTime,
//                        OutTime = attendance.OutTime,
//                        DailyHour = dailyHour,
//                        Color = "#ECFFDC"
//                    });

//                    currentDate = currentDate.AddDays(1);
//                    continue;
//                }

//                if (inTime is null)
//                {
//                    var pendingMisPunch = pendingMisPunches.Where(x => x.Date == currentDate).FirstOrDefault();

//                    if (pendingMisPunch != null)
//                    {
//                        data.Add(new AttendanceData
//                        {
//                            Date = currentDate,
//                            DayStatus = GetAbbreviation(pendingMisPunch.RegularisationTypeName) + "-P",
//                            DayStatusDescription = pendingMisPunch.RegularisationTypeName + " - Pending",
//                            OutTime = attendance.OutTime,
//                            Color = "#ECFFDC"
//                        });

//                        currentDate = currentDate.AddDays(1);
//                        continue;
//                    }
//                }

//                if (outTime is null)
//                {
//                    var pendingMisPunch = pendingMisPunches.Where(x => x.Date == currentDate).FirstOrDefault();

//                    if (pendingMisPunch != null)
//                    {
//                        data.Add(new AttendanceData
//                        {
//                            Date = currentDate,
//                            DayStatus = GetAbbreviation(pendingMisPunch.RegularisationTypeName) + "-P",
//                            DayStatusDescription = pendingMisPunch.RegularisationTypeName + " - Pending",
//                            InTime = attendance.InTime,
//                            Color = "#ECFFDC"
//                        });

//                        currentDate = currentDate.AddDays(1);
//                        continue;
//                    }
//                }

//                if (inTime > lateInGrace)
//                {
//                    data.Add(new AttendanceData
//                    {
//                        Date = currentDate,
//                        DayStatus = "Half-Day",
//                        DayStatusDescription = "Half-Day",
//                        InTime = attendance.InTime,
//                        OutTime = attendance.OutTime,
//                        DailyHour = dailyHour,
//                        Color = "#4F7942"
//                    });
//                }
//                else if (inTime > startTime)
//                {
//                    if (lateInCount > 2)
//                    {
//                        data.Add(new AttendanceData
//                        {
//                            Date = currentDate,
//                            DayStatus = "Half-Day",
//                            DayStatusDescription = "Half-Day",
//                            InTime = attendance.InTime,
//                            OutTime = attendance.OutTime,
//                            DailyHour = dailyHour,
//                            Color = "#4F7942"
//                        });
//                    }
//                    else
//                    {
//                        data.Add(new AttendanceData
//                        {
//                            Date = currentDate,
//                            DayStatus = "Late-In",
//                            DayStatusDescription = "Late-In",
//                            InTime = attendance.InTime,
//                            OutTime = attendance.OutTime,
//                            DailyHour = dailyHour,
//                            Color = "#FFC0CB"
//                        });

//                        lateInCount++;
//                    }
//                }
//                else
//                {
//                    data.Add(new AttendanceData
//                    {
//                        Date = currentDate,
//                        DayStatus = "Present",
//                        DayStatusDescription = "Present",
//                        InTime = attendance.InTime,
//                        OutTime = attendance.OutTime,
//                        DailyHour = dailyHour,
//                        Color = "#7CFC00"
//                    });
//                }

//                currentDate = currentDate.AddDays(1);
//            } while (currentDate <= lastDayOfMonth);

//            var summary = data
//                .GroupBy(x => new { x.DayStatus, x.DayStatusDescription, x.Color })
//                .Select(x => new
//                {
//                    x.Key.DayStatus,
//                    x.Key.DayStatusDescription,
//                    x.Key.Color,
//                    Total = x.Count()
//                })
//                .ToList();

//            return Ok(new
//            {
//                Data = data,
//                Summary = summary
//            });
//        }

//        public static string GetAbbreviation(string str)
//        {
//            string firstLetters = "";

//            string[] words = str.Split(' ');

//            foreach (string word in words)
//            {
//                firstLetters += word[0];
//            }

//            return firstLetters;
//        }

//        public class AttendanceData
//        {
//            public DateOnly Date { get; set; }
//            public string DayStatus { get; set; }
//            public string DayStatusDescription { get; set; }
//            public string? InTime { get; set; }
//            public string? OutTime { get; set; }
//            public TimeSpan? DailyHour { get; set; }
//            public string Color { get; set; }
//        }

//        public class LeaveSummary
//        {
//            public string DayStatus { get; set; }
//            public string DayStatusDescription { get; set; }
//            public int Total { get; set; }
//            public string Color { get; set; }
//        }
//    }
//}

