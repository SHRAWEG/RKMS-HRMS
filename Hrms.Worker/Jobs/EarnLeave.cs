using Hrms.Common.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Worker.Jobs
{
    public class EarnLeave : IJob
    {
        private readonly DataContext _context;

        public EarnLeave(DataContext context)
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var emps = await _context.EmpLogs
                .Join(_context.EmpTransactions,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        EmpId = el.EmployeeId,
                        PId = el.Id,
                        StatusName = et.Status != null ? et.Status.Name : null,
                        StatusId = et.StatusId,
                    })
                .ToListAsync();

            var settings = await _context.Settings.FirstOrDefaultAsync();

            foreach(var emp in emps)
            {
                if(emp.StatusName != "Active")
                {
                    continue;
                }

                var leaves = await _context.EmpLeaves
                    .Include(x => x.Leave)
                    .Where(x => x.PId == emp.PId && (x.Leave.LeaveEarnType == 1 || x.Leave.LeaveEarnType == 2))
                    .Select(x => new
                    {
                        LeaveId = x.LeaveId,
                        Days = x.Days,
                        LeaveEarnType = x.Leave.LeaveEarnType,
                        LeaveEarnDays = x.Leave.LeaveEarnDays,
                        LeaveEarnQuantity = x.Leave.LeaveEarnQuantity
                    })
                    .ToListAsync();

                if (leaves.Count < 1)
                {
                    continue;
                }

                List<EarnedLeave> newEarnedLeaves = new();
                List<LeaveLedger> leaveLedgers = new();

                foreach(var leave in leaves)
                {
                    decimal? totalLeaveEarned = await _context.LeaveLedgers
                        .Where(x => x.EmpId == emp.EmpId && x.LeaveYearId == settings.LeaveYearId && x.LeaveId == leave.LeaveId)
                        .SumAsync(x => x.Given);

                    if (totalLeaveEarned >= leave.LeaveEarnQuantity)
                    {
                        continue;
                    }

                    DateOnly? lastEarnedLeaveDate = await _context.EarnedLeaves.Where(x => x.EmpId == emp.EmpId && x.LeaveId == leave.LeaveId).MaxAsync(x => x.Date);

                    var query = _context.Attendances
                        .Where(x => x.EmpId == emp.EmpId)
                        .AsQueryable();

                    if (lastEarnedLeaveDate is not null)
                    {
                        query = query.Where(x => x.TransactionDate > lastEarnedLeaveDate);
                    }

                    var attendances = await query
                        .Include(x => x.Regularisation)
                        .Include(x => x.WorkHour)
                        .OrderBy(x => x.TransactionDate)
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
                            x.OutTime,
                            x.Regularisation
                        })
                        .ToListAsync();

                    if (leave.LeaveEarnType == 1)
                    {
                        continue;
                    } else
                    {
                        if (attendances.Count < leave.LeaveEarnDays)
                        {
                            continue;
                        }

                        TimeOnly? inTime = new();
                        TimeOnly? outTime = new();
                        TimeSpan? requiredHours = new();
                        TimeSpan? requiredHalfHours = new();
                        TimeSpan? dailyHour = new();
                        TimeSpan? overTime = new();
                        TimeOnly startTime = new();
                        TimeOnly endTime = new();
                        TimeOnly lateInGrace = new();
                        TimeOnly? halfDayEndTime = new();
                        TimeOnly? halfDayStartTime = new();
                        bool isFlexible = false;
                        double totalPresentDays = 0;
                        int lateInCount = 0;
                        decimal? newLeaveEarns = 0;

                        if (lastEarnedLeaveDate is not null)
                        {
                            DateOnly? firstDateOfMonth = DateOnly.FromDateTime(new DateTime(lastEarnedLeaveDate.Value.Year, lastEarnedLeaveDate.Value.Month, 1));

                            var oldAttendances = await _context.Attendances
                                .Include(x => x.Regularisation)
                                .Include(x => x.WorkHour)
                                .Where(x => x.EmpId == emp.EmpId && x.TransactionDate >= firstDateOfMonth && x.TransactionDate <= lastEarnedLeaveDate)
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
                                    x.OutTime,
                                    x.Regularisation
                                })
                                .ToListAsync();

                            foreach (var attendance in oldAttendances)
                            {
                                if (attendance is not null)
                                {
                                    startTime = TimeOnly.ParseExact(attendance.StartTime, "HH:mm:ss");
                                    endTime = TimeOnly.ParseExact(attendance.EndTime, "HH:mm:ss");
                                    halfDayEndTime = !string.IsNullOrEmpty(attendance.HalfDayEndTime) ? TimeOnly.ParseExact(attendance.HalfDayEndTime, "HH:mm:ss") : null;
                                    halfDayStartTime = !string.IsNullOrEmpty(attendance.HalfDayStartTime) ? TimeOnly.ParseExact(attendance.HalfDayStartTime, "HH:mm:ss") : null;
                                    lateInGrace = startTime.AddMinutes((double)(attendance.LateInGraceTime ?? 0));

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

                                    inTime = attendance.InTime != null ? TimeOnly.ParseExact(attendance.InTime, "HH:mm:ss") : null;
                                    outTime = attendance.OutTime != null ? TimeOnly.ParseExact(attendance.OutTime, "HH:mm:ss") : null;

                                    dailyHour = outTime - inTime;

                                    if (!isFlexible)
                                    {
                                        if (inTime is not null && outTime is not null)
                                        {
                                            if (outTime < endTime)
                                            {
                                                if (outTime >= halfDayEndTime)
                                                {
                                                    if (inTime > startTime && inTime <= lateInGrace && lateInCount < 3)
                                                    {
                                                        lateInCount++;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (inTime > startTime && inTime <= lateInGrace && lateInCount < 3)
                                                {
                                                    lateInCount++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        foreach (var attendance in attendances)
                        {
                            if (newLeaveEarns >= leave.Days)
                            {
                                break;
                            }

                            if (attendance.TransactionDate.Day == 1)
                            {
                                lateInCount = 0;
                            }

                            if (totalPresentDays >= leave.LeaveEarnDays)
                            {
                                newLeaveEarns += leave.LeaveEarnQuantity;

                                newEarnedLeaves.Add(new EarnedLeave
                                {
                                    EmpId = attendance.EmpId,
                                    Date = attendance.TransactionDate,
                                    LeaveId = leave.LeaveId,
                                });

                                leaveLedgers.Add(new LeaveLedger
                                {
                                    EmpId = emp.EmpId ?? 0,
                                    LeaveId = leave.LeaveId,
                                    Given = leave.LeaveEarnQuantity,
                                    GivenMonth = (short)DateTime.Now.Month,
                                    GivenYear = (short)DateTime.Now.Year,
                                    Remarks = "Leave Earned",
                                    ApprovedById = 1,
                                    TransactionUser = "admin",
                                    LeaveYearId = settings.LeaveYearId,

                                    //Default
                                    IsRegular = 1,
                                    Adjusted = 0,
                                    NoHrs = 0,
                                    HLeaveType = 0,
                                });

                                totalPresentDays = 0;
                            }

                            if (attendance is not null)
                            {
                                startTime = TimeOnly.ParseExact(attendance.StartTime, "HH:mm:ss");
                                endTime = TimeOnly.ParseExact(attendance.EndTime, "HH:mm:ss");
                                halfDayEndTime = !string.IsNullOrEmpty(attendance.HalfDayEndTime) ? TimeOnly.ParseExact(attendance.HalfDayEndTime, "HH:mm:ss") : null;
                                halfDayStartTime = !string.IsNullOrEmpty(attendance.HalfDayStartTime) ? TimeOnly.ParseExact(attendance.HalfDayStartTime, "HH:mm:ss") : null;
                                lateInGrace = startTime.AddMinutes((double)(attendance.LateInGraceTime ?? 0));

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

                                inTime = attendance.InTime != null ? TimeOnly.ParseExact(attendance.InTime, "HH:mm:ss") : null;
                                outTime = attendance.OutTime != null ? TimeOnly.ParseExact(attendance.OutTime, "HH:mm:ss") : null;

                                dailyHour = outTime - inTime;

                                if (isFlexible)
                                {
                                    if (dailyHour >= requiredHours)
                                    {
                                        totalPresentDays++;
                                    }
                                    else if (dailyHour >= requiredHalfHours)
                                    {
                                        totalPresentDays += 0.5;
                                    }
                                }
                                else
                                {
                                    if (inTime is not null && outTime is not null)
                                    {
                                        if (outTime < endTime)
                                        {
                                            if (outTime >= halfDayEndTime)
                                            {
                                                if (inTime <= startTime)
                                                {
                                                    totalPresentDays += 0.5;
                                                }
                                                else if (inTime <= lateInGrace && lateInCount < 3)
                                                {
                                                    totalPresentDays += 0.5;
                                                    lateInCount++;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (inTime <= startTime)
                                            {
                                                totalPresentDays++;
                                            }
                                            else if (inTime <= lateInGrace && lateInCount < 3)
                                            {
                                                totalPresentDays++;
                                                lateInCount++;
                                            }
                                            else if (inTime <= halfDayStartTime)
                                            {
                                                totalPresentDays += 0.5;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _context.AddRange(newEarnedLeaves);
                _context.AddRange(leaveLedgers);
                await _context.SaveChangesAsync();
            }
        }
    }
}
