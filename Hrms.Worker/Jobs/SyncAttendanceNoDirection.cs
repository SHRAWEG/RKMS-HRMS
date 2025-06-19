using Hrms.Common.Models;
using Microsoft.IdentityModel.Tokens;
using NPOI.OpenXmlFormats.Dml.Diagram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Worker.Jobs
{
    public class SyncAttendanceNoDirection : IJob
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public SyncAttendanceNoDirection(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if(!await _context.AttendanceLogNoDirections.AnyAsync())
            {
                return;
            }

            //var settings = await _context.Settings.FirstOrDefaultAsync();

            //if (settings is null)
            //{
            //    return;
            //}

            string syncDate = _configuration["SyncDate"];

            DateOnly date = DateOnly.FromDateTime(DateTime.Now).AddDays(-7);

            if (!syncDate.IsNullOrEmpty())
            {
                date = DateOnlyHelper.ParseDateOrNow(syncDate);
            }

            try
            {
                do
                {
                    var attendanceData = await _context.AttendanceLogNoDirections
                        .Where(x => !x.IsSuccess && x.Date == date)
                        .OrderBy(x => x.Date)
                        .OrderBy(x => x.Time)
                        .ToListAsync();

                    if (attendanceData.Count == 0)
                    {
                        date = date.AddDays(1);

                        continue;
                    }

                    List<Attendance> newAttendanceData = new();

                    foreach (var attendance in attendanceData)
                    {
                        Console.WriteLine(date);

                        var emp = await _context.EmpDeviceCodes
                            .Where(x => x.DeviceCode == attendance.DeviceCode && (true || x.DeviceId == attendance.DeviceId))
                            .FirstOrDefaultAsync();

                        if (emp == null)
                        {
                            attendance.Remarks = "Emp with the Device Code does not exists.";
                            attendance.UpdatedAt = DateTime.UtcNow;

                            continue;
                        }

                        WorkHour? workHour = null;

                        var roster = await _context.Rosters
                            .Where(x => x.Date == attendance.Date && x.EmpId == emp.EmpId)
                            .Include(x => x.WorkHour)
                            .FirstOrDefaultAsync();

                        workHour = roster?.WorkHour;

                        if (roster is null)
                        {
                            var defaultWorkHour = await _context.DefaultWorkHours
                                .Where(x => (x.EmpId == emp.EmpId || x.EmpId == null) && x.DayId == ((short)attendance.Date.DayOfWeek + 1))
                                .OrderBy(x => x.EmpId)
                                .Include(x => x.WorkHour)
                                .FirstOrDefaultAsync();

                            workHour = defaultWorkHour?.WorkHour;
                        }

                        if (workHour is null)
                        {
                            attendance.IsSuccess = false;
                            attendance.Remarks = "There is no default shift nor the empoloyee is assigned any.";
                            attendance.UpdatedAt = DateTime.UtcNow;

                            continue;
                        }

                        if (workHour.IsFlexible && workHour.IsNightShift)
                        {
                            if (string.IsNullOrEmpty(workHour.NightStartTime) || string.IsNullOrEmpty(workHour.NightEndTime))
                            {
                                attendance.IsSuccess = false;
                                attendance.Remarks = "Start Time or End Time for night shift is empty.";
                                attendance.UpdatedAt = DateTime.UtcNow;

                                continue;
                            }

                            TimeOnly nightStartTime = TimeOnly.ParseExact(workHour.NightStartTime ?? "00:00:00", "HH:mm:ss");
                            TimeOnly nightEndtime = TimeOnly.ParseExact(workHour.NightEndTime ?? "00:00:00", "HH:mm:ss");

                            if (attendance.Time > nightStartTime || attendance.Time < nightEndtime)
                            {
                                if (attendance.Time > nightStartTime)
                                {
                                    var existingNightAttendance = await _context.Attendances
                                        .Where(x => x.EmpId == emp.EmpId
                                                && x.TransactionDate == attendance.Date)
                                        .Include(x => x.WorkHour)
                                        .FirstOrDefaultAsync();

                                    if (existingNightAttendance is not null)
                                    {
                                        TimeOnly nightAttendanceInTime = TimeOnly.ParseExact(existingNightAttendance.InTime ?? "23:59:00", "HH:mm:ss");
                                        TimeOnly nightAttendanceOutTime = TimeOnly.ParseExact(existingNightAttendance.OutTime ?? "00:00:00", "HH:mm:ss");

                                        if (existingNightAttendance.WorkHour.IsFlexible && existingNightAttendance.WorkHour.IsNightShift)
                                        {
                                            if (attendance.Time < nightAttendanceInTime)
                                            {
                                                existingNightAttendance.InTime = attendance.Time.ToString("HH:mm:ss");
                                                existingNightAttendance.TransactionDate = attendance.Date;
                                                existingNightAttendance.InMode = "fingerprint";

                                                attendance.IsSuccess = true;
                                                attendance.Remarks = "Successfully synced.";
                                                attendance.UpdatedAt = DateTime.UtcNow;

                                                continue;
                                            }
                                            else if (existingNightAttendance.TransactionDateOut == null ||
                                                     (existingNightAttendance.TransactionDateOut == existingNightAttendance.TransactionDate && attendance.Time > nightAttendanceOutTime))
                                            {
                                                existingNightAttendance.OutTime = attendance.Time.ToString("HH:mm:ss");
                                                existingNightAttendance.TransactionDateOut = attendance.Date;
                                                existingNightAttendance.OutMode = "fingerprint";

                                                attendance.IsSuccess = true;
                                                attendance.Remarks = "Successfully synced.";
                                                attendance.UpdatedAt = DateTime.UtcNow;

                                                continue;
                                            }
                                            else
                                            {
                                                attendance.IsSuccess = true;
                                                attendance.Remarks = "Successfully synced.";
                                                attendance.UpdatedAt = DateTime.UtcNow;

                                                continue;
                                            }
                                        }
                                    }

                                    var syncedNightEmp = newAttendanceData.Where(x => x.EmpId == emp.EmpId && x.TransactionDate == attendance.Date).FirstOrDefault();

                                    if (syncedNightEmp is not null)
                                    {
                                        TimeOnly nightAttendanceInTime = TimeOnly.ParseExact(syncedNightEmp.InTime ?? "23:59:00", "HH:mm:ss");
                                        TimeOnly nightAttendanceOutTime = TimeOnly.ParseExact(syncedNightEmp.OutTime ?? "00:00:00", "HH:mm:ss");

                                        if (attendance.Time < nightAttendanceInTime)
                                        {
                                            syncedNightEmp.InTime = attendance.Time.ToString("HH:mm:ss");
                                            syncedNightEmp.TransactionDate = attendance.Date;
                                            syncedNightEmp.InMode = "fingerprint";

                                            attendance.IsSuccess = true;
                                            attendance.Remarks = "Successfully synced.";
                                            attendance.UpdatedAt = DateTime.UtcNow;

                                            continue;
                                        }
                                        else if (syncedNightEmp.TransactionDateOut == null ||
                                                 (syncedNightEmp.TransactionDateOut == syncedNightEmp.TransactionDate && attendance.Time > nightAttendanceOutTime))
                                        {
                                            syncedNightEmp.OutTime = attendance.Time.ToString("HH:mm:ss");
                                            syncedNightEmp.TransactionDateOut = attendance.Date;
                                            syncedNightEmp.OutMode = "fingerprint";

                                            attendance.IsSuccess = true;
                                            attendance.Remarks = "Successfully synced.";
                                            attendance.UpdatedAt = DateTime.UtcNow;

                                            continue;
                                        }
                                        else
                                        {
                                            attendance.IsSuccess = true;
                                            attendance.Remarks = "Successfully synced.";
                                            attendance.UpdatedAt = DateTime.UtcNow;

                                            continue;
                                        }
                                    }

                                    newAttendanceData.Add(new Attendance
                                    {
                                        WorkHourId = workHour.Id,
                                        EmpId = emp.EmpId,
                                        TransactionDate = attendance.Date,
                                        InTime = attendance.Time.ToString("HH:mm:ss"),

                                        //Defaults
                                        InMode = "fingerprint",
                                        AttendanceStatus = 0,
                                        FlagIn = false,
                                        FlagOut = false,
                                        AttendanceType = 0,
                                        CheckInMode = 'D',
                                        AttendanceId = 0,
                                        SignOutTimeStamp = 0,
                                        SignInTimeStamp = 0,
                                    });

                                    attendance.IsSuccess = true;
                                    attendance.Remarks = "Successfully synced.";
                                    attendance.UpdatedAt = DateTime.UtcNow;

                                    continue;
                                }

                                if (attendance.Time < nightEndtime)
                                {
                                    var existingNightAttendance = await _context.Attendances
                                        .Where(x => x.EmpId == emp.EmpId
                                                && x.TransactionDate == attendance.Date.AddDays(-1))
                                        .Include(x => x.WorkHour)
                                        .FirstOrDefaultAsync();

                                    if (existingNightAttendance is not null)
                                    {
                                        TimeOnly nightAttendanceInTime = TimeOnly.ParseExact(existingNightAttendance.InTime ?? "23:59:00", "HH:mm:ss");
                                        TimeOnly nightAttendanceOutTime = TimeOnly.ParseExact(existingNightAttendance.OutTime ?? "00:00:00", "HH:mm:ss");

                                        if (existingNightAttendance.WorkHour.IsFlexible && existingNightAttendance.WorkHour.IsNightShift)
                                        {
                                            if (existingNightAttendance.TransactionDateOut is null || existingNightAttendance.TransactionDateOut != attendance.Date)
                                            {
                                                existingNightAttendance.OutTime = attendance.Time.ToString("HH:mm:ss");
                                                existingNightAttendance.TransactionDateOut = attendance.Date;
                                                existingNightAttendance.OutMode = "fingerprint";

                                                attendance.IsSuccess = true;
                                                attendance.Remarks = "Successfully synced.";
                                                attendance.UpdatedAt = DateTime.UtcNow;

                                                continue;
                                            }

                                            if (attendance.Time > nightAttendanceOutTime)
                                            {
                                                existingNightAttendance.OutTime = attendance.Time.ToString("HH:mm:ss");
                                                existingNightAttendance.TransactionDateOut = attendance.Date;
                                                existingNightAttendance.OutMode = "fingerprint";

                                                attendance.IsSuccess = true;
                                                attendance.Remarks = "Successfully synced.";
                                                attendance.UpdatedAt = DateTime.UtcNow;

                                                continue;
                                            }
                                            else
                                            {
                                                attendance.IsSuccess = true;
                                                attendance.Remarks = "Successfully synced.";
                                                attendance.UpdatedAt = DateTime.UtcNow;

                                                continue;
                                            }
                                        }
                                    }

                                    var syncedNightEmp = newAttendanceData.Where(x => x.EmpId == emp.EmpId && x.TransactionDate == attendance.Date).FirstOrDefault();

                                    if (syncedNightEmp is not null)
                                    {
                                        TimeOnly nightAttendanceInTime = TimeOnly.ParseExact(syncedNightEmp.InTime ?? "23:59:00", "HH:mm:ss");
                                        TimeOnly nightAttendanceOutTime = TimeOnly.ParseExact(syncedNightEmp.OutTime ?? "00:00:00", "HH:mm:ss");

                                        if (syncedNightEmp.TransactionDateOut is null || syncedNightEmp.TransactionDateOut != attendance.Date)
                                        {
                                            syncedNightEmp.OutTime = attendance.Time.ToString("HH:mm:ss");
                                            syncedNightEmp.TransactionDateOut = attendance.Date;
                                            syncedNightEmp.OutMode = "fingerprint";

                                            attendance.IsSuccess = true;
                                            attendance.Remarks = "Successfully synced.";
                                            attendance.UpdatedAt = DateTime.UtcNow;

                                            continue;
                                        }

                                        if (attendance.Time > nightAttendanceOutTime)
                                        {
                                            syncedNightEmp.OutTime = attendance.Time.ToString("HH:mm:ss");
                                            syncedNightEmp.TransactionDateOut = attendance.Date;
                                            syncedNightEmp.OutMode = "fingerprint";

                                            attendance.IsSuccess = true;
                                            attendance.Remarks = "Successfully synced.";
                                            attendance.UpdatedAt = DateTime.UtcNow;

                                            continue;
                                        }
                                        else
                                        {
                                            attendance.IsSuccess = true;
                                            attendance.Remarks = "Successfully synced.";
                                            attendance.UpdatedAt = DateTime.UtcNow;

                                            continue;
                                        }
                                    }

                                    newAttendanceData.Add(new Attendance
                                    {
                                        WorkHourId = workHour.Id,
                                        EmpId = emp.EmpId,
                                        TransactionDate = attendance.Date.AddDays(-1),
                                        TransactionDateOut = attendance.Date,
                                        OutTime = attendance.Time.ToString("HH:mm:ss"),

                                        //Defaults
                                        OutMode = "fingerprint",
                                        AttendanceStatus = 0,
                                        FlagIn = false,
                                        FlagOut = false,
                                        AttendanceType = 0,
                                        CheckInMode = 'D',
                                        AttendanceId = 0,
                                        SignOutTimeStamp = 0,
                                        SignInTimeStamp = 0,
                                    });

                                    attendance.IsSuccess = true;
                                    attendance.Remarks = "Successfully synced.";
                                    attendance.UpdatedAt = DateTime.UtcNow;

                                    continue;
                                }
                            }
                        }

                        var existingAttendance = await _context.Attendances
                            .Where(x => x.EmpId == emp.EmpId
                                    && (x.TransactionDate == attendance.Date))
                            .Include(x => x.WorkHour)
                            .FirstOrDefaultAsync();

                        if (existingAttendance != null)
                        {
                            TimeOnly attendanceTimeIn = TimeOnly.ParseExact(existingAttendance.InTime ?? "23:59:00", "HH:mm:ss");
                            TimeOnly attendanceTimeOut = TimeOnly.ParseExact(existingAttendance.OutTime ?? "00:00:00", "HH:mm:ss");

                            if (attendance.Time < attendanceTimeIn)
                            {
                                if (existingAttendance.OutTime is null)
                                {
                                    existingAttendance.OutTime = existingAttendance.InTime;
                                    existingAttendance.TransactionDateOut = existingAttendance.TransactionDate;
                                    existingAttendance.OutMode = "fingerprint";
                                }

                                existingAttendance.InTime = attendance.Time.ToString("HH:mm:ss");
                                existingAttendance.TransactionDate = attendance.Date;
                                existingAttendance.InMode = "fingerprint";

                                attendance.IsSuccess = true;
                                attendance.Remarks = "Successfully synced.";
                                attendance.UpdatedAt = DateTime.UtcNow;

                                continue;
                            }
                            else if (attendance.Time > attendanceTimeOut)
                            {
                                existingAttendance.OutTime = attendance.Time.ToString("HH:mm:ss");
                                existingAttendance.TransactionDateOut = attendance.Date;
                                existingAttendance.OutMode = "fingerprint";

                                attendance.IsSuccess = true;
                                attendance.Remarks = "Successfully synced.";
                                attendance.UpdatedAt = DateTime.UtcNow;

                                continue;
                            }
                            else
                            {
                                attendance.IsSuccess = true;
                                attendance.Remarks = "Successfully synced.";
                                attendance.UpdatedAt = DateTime.UtcNow;

                                continue;
                            }
                        }

                        var syncedEmp = newAttendanceData.Where(x => x.EmpId == emp.EmpId && x.TransactionDate == attendance.Date).FirstOrDefault();

                        if (syncedEmp != null)
                        {
                            TimeOnly attendanceTimeIn = TimeOnly.ParseExact(syncedEmp.InTime ?? "23:59:00", "HH:mm:ss");
                            TimeOnly attendanceTimeOut = TimeOnly.ParseExact(syncedEmp.OutTime ?? "00:00:00", "HH:mm:ss");


                            if (attendance.Time < attendanceTimeIn)
                            {
                                if (syncedEmp.OutTime is null)
                                {
                                    syncedEmp.OutTime = syncedEmp.InTime;
                                    syncedEmp.TransactionDateOut = syncedEmp.TransactionDate;
                                    syncedEmp.OutMode = "fingerprint";
                                }

                                syncedEmp.InTime = attendance.Time.ToString("HH:mm:ss");
                                syncedEmp.TransactionDate = attendance.Date;
                                syncedEmp.InMode = "fingerprint";

                                attendance.IsSuccess = true;
                                attendance.Remarks = "Successfully synced.";
                                attendance.UpdatedAt = DateTime.UtcNow;

                                continue;
                            }
                            else if (attendance.Time > attendanceTimeOut)
                            {
                                syncedEmp.OutTime = attendance.Time.ToString("HH:mm:ss");
                                syncedEmp.TransactionDateOut = attendance.Date;
                                syncedEmp.OutMode = "fingerprint";

                                attendance.IsSuccess = true;
                                attendance.Remarks = "Successfully synced.";
                                attendance.UpdatedAt = DateTime.UtcNow;

                                continue;
                            }
                            else
                            {
                                attendance.IsSuccess = true;
                                attendance.Remarks = "Successfully synced.";
                                attendance.UpdatedAt = DateTime.UtcNow;

                                continue;
                            }
                        }
                        else
                        {
                            newAttendanceData.Add(new Attendance
                            {
                                WorkHourId = workHour.Id,
                                EmpId = emp.EmpId,
                                TransactionDate = attendance.Date,
                                InTime = attendance.Time.ToString("HH:mm:ss"),

                                //Defaults
                                InMode = "fingerprint",
                                AttendanceStatus = 0,
                                FlagIn = false,
                                FlagOut = false,
                                AttendanceType = 0,
                                CheckInMode = 'D',
                                AttendanceId = 0,
                                SignOutTimeStamp = 0,
                                SignInTimeStamp = 0,
                            });

                            attendance.IsSuccess = true;
                            attendance.Remarks = "Successfully synced.";
                            attendance.UpdatedAt = DateTime.UtcNow;
                        }
                    }

                    _context.AddRange(newAttendanceData);
                    await _context.SaveChangesAsync();

                    date = date.AddDays(1);

                } while (date <= DateOnly.FromDateTime(DateTime.UtcNow));

                _context.SyncAttendanceLogs.Add(new SyncAttendanceLog
                {
                    Type = "sync",
                    Status = "success",
                    SyncedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _context.SyncAttendanceLogs.Add(new SyncAttendanceLog
                {
                    Type = "sync",
                    Status = "fail",
                    ErrorTrace = ex.StackTrace,
                    ErrorMessage = ex.Message,
                    SyncedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            
            return;
        }
    }
}
