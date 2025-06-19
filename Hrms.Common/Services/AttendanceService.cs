using Hrms.Common.Interfaces;
using Hrms.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Hrms.Common.Services
{
    public class AttendanceService : IAttendnaceService
    {
        private readonly DataContext _context;

        public AttendanceService(DataContext context)
        {
            _context = context;
        }

        public async Task<double> CalculateAttendance(int empId, DateOnly fromDate, DateOnly toDate)
        {
            int days = 0;

            var emp = await _context.EmpLogs.Where(x => x.EmployeeId == empId)
                .Join(_context.EmpTransactions,
                el => el.Id,
                et => et.Id,
                (el, et) => new
                {
                    EmployeeId = el.EmployeeId,
                    CompanyId = et.CompanyId
                })
                .FirstOrDefaultAsync();

            if (emp is null)
            {
                return 0;
            }

            DateOnly currentDate = fromDate;
            double totalWorkingDays = 0;
            double totalPresentDays = 0;
            int lateInCount = 1;
            double totalOnDuty = 0;

            var branchId = await _context.EmpLogs
                .Where(x => x.EmployeeId == emp.EmployeeId)
                .Join(_context.EmpTransactions,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => et.BranchId
                ).FirstOrDefaultAsync();

            var attendances = await _context.Attendances
                .Include(x => x.Regularisation)
                .Where(x => x.EmpId == emp.EmployeeId &&
                            (x.TransactionDate >= fromDate || x.TransactionDateOut >= fromDate) &&
                            (x.TransactionDate <= toDate || x.TransactionDateOut <= toDate))
                .Select(x => new
                {
                    Id = x.Id,
                    WorkHourId = x.WorkHourId,
                    WorkHourName = x.WorkHour.Name,
                    x.WorkHour.StartTime,
                    x.WorkHour.EndTime,
                    x.WorkHour.LateInGraceTime,
                    x.WorkHour.HalfDayStartTime,
                    x.WorkHour.HalfDayEndTime,
                    x.WorkHour.MinDutyTime,
                    x.WorkHour.MinHalfDayTime,
                    x.WorkHour.IsFlexible,
                    x.TransactionDate,
                    x.TransactionDateOut,
                    x.EmpId,
                    x.InTime,
                    x.OutTime
                })
                .ToListAsync();

            var calendarId = await _context.EmpCalendars.Where(x => x.EmpId == empId).Select(x => x.CalendarId).FirstOrDefaultAsync();

            var holidays = await _context.HolidayCalendars
                .Include(x => x.Holiday)
                .Where(x => x.Holiday.Date >= fromDate && x.Holiday.Date <= toDate && x.CalendarId == calendarId)
                .Select(x => new
                {
                    Name = x.Holiday.Name,
                    Date = x.Holiday.Date,
                })
                .ToListAsync();

            var leaves = await _context.LeaveLedgers
                .Where(x => x.Leave_Date >= fromDate && x.Leave_Date <= toDate && x.EmpId == emp.EmployeeId)
                .Include(x => x.Leave)
                .Select(x => new
                {
                    x.Leave_Date,
                    LeaveName = x.Leave.Name,
                    HLeaveType = x.HLeaveType,
                    IsPaidLeave = x.Leave.IsPaidLeave
                })
                .ToListAsync();

            var pendingLeaves = await _context.LeaveApplicationHistories
                .Where(x => x.StartDate <= toDate && x.EndDate >= fromDate && x.EmpId == emp.EmployeeId && x.Status == "pending")
                .Include(x => x.Leave)
                .Select(x => new
                {
                    x.StartDate,
                    x.EndDate,
                    LeaveName = x.Leave.Name,
                    HLeaveType = x.HLeaveType
                })
                .ToListAsync();

            var pendingRegularisations = await _context.Regularisations
                .Where(x => x.FromDate <= toDate && x.ToDate >= fromDate && x.EmpId == emp.EmployeeId && x.Status == "pending")
                .Select(x => new
                {
                    RegularisationTypeId = x.RegularisationType,
                    x.FromTime,
                    x.ToTime,
                    x.FromDate,
                    x.ToDate
                })
                .ToListAsync();

            List<DateOnly> weekends = await Helper.GetWeekends(fromDate, toDate, empId, _context);

            double payingDays = 0;

            do
            {
                TimeOnly? inTime = null;
                TimeOnly? odInTime = null;
                TimeOnly? gatePassInTime = null;
                TimeOnly? outTime = null;
                TimeOnly? odOutTime = null;
                TimeOnly? gatePassOutTime = null;
                TimeSpan? requiredHours = new();
                TimeSpan? requiredHalfHours = new();
                TimeSpan? dailyHour = new();
                TimeSpan? odHour = null;
                TimeSpan? gatePassHour = null;
                TimeSpan? totalHour = new();
                TimeSpan? overTime = new();
                TimeOnly startTime = new();
                TimeOnly endTime = new();
                TimeOnly lateInGrace = new();
                TimeOnly? halfDayEndTime = null;
                TimeOnly? halfDayStartTime = null;
                bool isFlexible = false;
                string attendanceType = "";
                bool middleGatePass = false;

                var attendance = attendances.Where(x => x.TransactionDate == currentDate).FirstOrDefault();

                var gatePasses = await _context.Regularisations

                    .Where(x => x.Status == "approved" && x.RegularisationType.Name == "gate-pass" && x.EmpId == emp.EmployeeId && x.FromDate == currentDate)
                    .ToListAsync();

                var onDuties = await _context.Regularisations
                    .Where(x => x.Status == "approved" && (x.RegularisationType.Name == "on-duty" || x.RegularisationType.Name == "work-from-home") && x.EmpId == emp.EmployeeId && x.FromDate <= currentDate && x.ToDate >= currentDate)
                    .ToListAsync();

                if (gatePasses.Count > 0)
                {
                    gatePassHour = new();

                    foreach (var gatePass in gatePasses)
                    {
                        gatePassHour += gatePass.ToTime - gatePass.FromTime;
                    }
                }

                totalWorkingDays++;

                var leave = leaves.FirstOrDefault(x => x.Leave_Date == currentDate);
                var holiday = holidays.Where(x => x.Date == currentDate).FirstOrDefault();

                if (weekends.Contains(currentDate))
                {
                    totalWorkingDays--;
                }

                if (holiday != null) 
                {
                    totalWorkingDays--;
                }

                if (attendance is not null)
                {
                    startTime = TimeOnly.ParseExact(attendance.StartTime, "HH:mm:ss");
                    endTime = TimeOnly.ParseExact(attendance.EndTime, "HH:mm:ss");
                    halfDayEndTime = !string.IsNullOrEmpty(attendance.HalfDayEndTime) ? TimeOnly.ParseExact(attendance.HalfDayEndTime, "HH:mm:ss") : null;
                    halfDayStartTime = !string.IsNullOrEmpty(attendance.HalfDayStartTime) ? TimeOnly.ParseExact(attendance.HalfDayStartTime, "HH:mm:ss") : null;
                    lateInGrace = startTime.AddMinutes((double)(attendance.LateInGraceTime ?? 0));
                    TimeSpan? calculatedGatePassHour = gatePassHour;
                    TimeSpan? calculatedOdHour = new();

                    bool middleOd = false;

                    inTime = attendance.InTime != null ? TimeOnly.ParseExact(attendance.InTime, "HH:mm:ss") : null;
                    outTime = attendance.OutTime != null ? TimeOnly.ParseExact(attendance.OutTime, "HH:mm:ss") : null;

                    inTime = inTime is not null ? new TimeOnly(inTime?.Hour ?? 0, inTime?.Minute ?? 0) : null;
                    outTime = outTime is not null ? new TimeOnly(outTime?.Hour ?? 0, outTime?.Minute ?? 0) : null;

                    if (attendance.IsFlexible)
                    {
                        isFlexible = true;
                        requiredHours = TimeSpan.FromMinutes((double)attendance.MinDutyTime);
                        requiredHalfHours = TimeSpan.FromMinutes((double)attendance.MinHalfDayTime);
                    }
                    else
                    {
                        requiredHours = endTime - startTime;
                    }

                    if (gatePasses.Count > 0 && inTime != null && outTime != null)
                    {
                        foreach (var gatePass in gatePasses)
                        {
                            if (gatePass.FromTime >= inTime && gatePass.ToTime <= outTime)
                            {
                                calculatedGatePassHour -= gatePass.ToTime - gatePass.FromTime;
                            }

                            if (gatePass.FromTime >= inTime && gatePass.FromTime < outTime && gatePass.ToTime > outTime)
                            {
                                calculatedGatePassHour -= outTime - gatePass.FromTime;
                            }

                            if (gatePass.FromTime < inTime && gatePass.ToTime > inTime && gatePass.ToTime <= outTime)
                            {
                                calculatedGatePassHour -= gatePass.ToTime - inTime;
                            }
                        }
                    }

                    dailyHour = outTime - inTime;
                    dailyHour = dailyHour ?? new();


                    if (onDuties.Count > 0)
                    {
                        odHour = new();

                        totalOnDuty++;

                        foreach (var onDuty in onDuties)
                        {
                            odHour += onDuty.ToTime - onDuty.FromTime;
                        }

                        calculatedOdHour = odHour;

                        if (inTime != null && outTime != null)
                        {
                            foreach (var onDuty in onDuties)
                            {
                                if (onDuty.FromTime <= inTime && onDuty.ToTime >= outTime)
                                {
                                    calculatedOdHour -= dailyHour;
                                    middleOd = true;
                                }

                                if (onDuty.FromTime > inTime && onDuty.ToTime < outTime)
                                {
                                    calculatedOdHour -= dailyHour - (onDuty.ToTime - onDuty.FromTime);
                                    middleOd = true;
                                }

                                if (onDuty.FromTime >= inTime && onDuty.FromTime < outTime && onDuty.ToTime > outTime)
                                {
                                    calculatedOdHour -= outTime - onDuty.FromTime;
                                    middleOd = true;
                                }

                                if (onDuty.FromTime < inTime && onDuty.ToTime > inTime && onDuty.ToTime <= outTime)
                                {
                                    calculatedOdHour -= onDuty.ToTime - inTime;
                                    middleOd = true;
                                }
                            }
                        }

                        odInTime = onDuties.Min(x => x.FromTime);
                        odOutTime = onDuties.Max(x => x.ToTime);
                    }

                    totalHour = dailyHour;

                    if (calculatedOdHour is not null)
                    {
                        totalHour += calculatedOdHour;
                    }

                    if (calculatedGatePassHour is not null)
                    {
                        totalHour += calculatedGatePassHour;
                    }

                    double currentPresentDay = 0;

                    if (isFlexible)
                    {
                        if (totalHour >= requiredHours)
                        {
                            totalPresentDays++;
                            payingDays++;
                            currentPresentDay = 1;

                            attendanceType = "full-day";
                        }
                        else if (totalHour >= requiredHalfHours)
                        {
                            totalPresentDays += 0.5;
                            payingDays += 0.5;
                            currentPresentDay = 0.5;

                            attendanceType = "half-day";
                        }
                    }
                    else
                    {
                        if (gatePasses.Count > 0)
                        {
                            gatePassInTime = gatePasses.Select(x => x.FromTime).Min();
                            gatePassOutTime = gatePasses.Select(x => x.ToTime).Max();
                        }

                        TimeOnly? earliestTime = Helper.GetEarliestTime(inTime, odInTime, gatePassInTime);
                        TimeOnly? latestTime = Helper.GetLatestTime(outTime, odOutTime, gatePassOutTime);

                        TimeSpan? firstHalfHours = halfDayEndTime - startTime;
                        TimeSpan? secondHalfHours = endTime - halfDayStartTime;

                        if (earliestTime is not null && latestTime is not null)
                        {
                            if (latestTime < endTime)
                            {
                                if (latestTime >= halfDayEndTime && totalHour >= firstHalfHours)
                                {
                                    if (earliestTime <= startTime)
                                    {
                                        totalPresentDays += 0.5;
                                        payingDays += 0.5;
                                        currentPresentDay = 0.5;

                                        attendanceType = "half-day";
                                    }
                                    else if (earliestTime <= lateInGrace && lateInCount < 4)
                                    {
                                        totalPresentDays += 0.5;
                                        payingDays += 0.5;
                                        lateInCount++;
                                        currentPresentDay = 0.5;

                                        attendanceType = "half-day";
                                    }
                                }
                            }
                            else
                            {
                                if (earliestTime <= startTime && totalHour >= requiredHours)
                                {
                                    totalPresentDays++;
                                    payingDays++;
                                    currentPresentDay = 1;

                                    attendanceType = "full-day";
                                }
                                else if (earliestTime <= lateInGrace && lateInCount < 4 && totalHour >= requiredHours)
                                {
                                    totalPresentDays++;
                                    payingDays++;
                                    lateInCount++;
                                    currentPresentDay = 1;

                                    attendanceType = "full-day";
                                }
                                else if (earliestTime <= halfDayStartTime && totalHour >= secondHalfHours)
                                {
                                    totalPresentDays += 0.5;
                                    payingDays += 0.5;
                                    currentPresentDay = 0.5;

                                    attendanceType = "half-day";
                                }
                            }
                        }
                    }

                    if (totalHour is not null && requiredHours is not null && totalHour > requiredHours)
                    {
                        overTime = totalHour - requiredHours;
                    }

                    if (leave != null)
                    {
                        if (leave.IsPaidLeave == 1)
                        {
                            if (leave.HLeaveType == 1 || leave.HLeaveType == 2)
                            {
                                if (currentPresentDay != 1)
                                {
                                    payingDays += 0.5;
                                }
                            }
                            else
                            {
                                if (currentPresentDay == 0)
                                {
                                    payingDays++;
                                }

                                if (currentPresentDay == 0.5)
                                {
                                    payingDays += 0.5;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (leave is not null)
                    {
                        if (leave.IsPaidLeave == 1)
                        {
                            if (leave.HLeaveType == 1 || leave.HLeaveType == 2)
                            {
                                payingDays += 0.5;
                            }
                            else
                            {
                                payingDays++;
                            }
                        }
                    }
                }
                days++;
                currentDate = currentDate.AddDays(1);
            } while (currentDate <= toDate);

            double offDays = days - totalWorkingDays;

            double payableDays = 0;

            if (offDays == 0)
            {
                payableDays = payingDays;
            }
            else
            {
                double average = totalWorkingDays / offDays;
                double additionalDays = Math.Round(payingDays / average);
                payableDays = payingDays + additionalDays;
            };

            return (payableDays);
        }
    }
}
