using Hrms.Common.Data;
using Hrms.Common.Helpers;
using Hrms.Common.Models;
using Hrms.DatabaseWorker.AttendanceData;
using Hrms.DatabaseWorker.AttendanceModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.DatabaseWorker.Jobs
{
    public class SyncAttendance : BackgroundService
    {
        private readonly NpgsqlConnection npgsqlConnection;
        private readonly MySqlConnection mysqlConnection;
        private readonly ILogger<Worker> _logger;


        public SyncAttendance(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            npgsqlConnection = new NpgsqlConnection(config.GetConnectionString("DevString"));
            mysqlConnection = new MySqlConnection(config.GetConnectionString("AttendanceDbString"));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await npgsqlConnection.OpenAsync();
                await mysqlConnection.OpenAsync();

                DbContextOptionsBuilder<DataContext> npgsqlOptionsBuilder = new DbContextOptionsBuilder<DataContext>();
                npgsqlOptionsBuilder.UseNpgsql(npgsqlConnection);
                DataContext _context = new DataContext(npgsqlOptionsBuilder.Options);

                DbContextOptionsBuilder<AttendanceDataContext> mysqlOptionsBuilder = new DbContextOptionsBuilder<AttendanceDataContext>();
                mysqlOptionsBuilder.UseMySQL(mysqlConnection);
                AttendanceDataContext _attendanceContext = new AttendanceDataContext(mysqlOptionsBuilder.Options);

                DateTime mayDate = new DateTime(2023, 06, 03, 0, 0, 0);

                do
                {
                    do
                    {
                        var attendanceData = await (from detail in _attendanceContext.DeviceLogsInfos
                                                    join syncStatus in _attendanceContext.AttendanceSyncStatuses on detail.DeviceLogId equals syncStatus.DeviceLogId into syncStatuses
                                                    from syncStatus in syncStatuses.DefaultIfEmpty()
                                                    where syncStatus.DeviceLogId == null && detail.LogDate >= mayDate
                                                    && detail.LogDate <= mayDate.AddDays(1)
                                                    select new
                                                    {
                                                        detail.DeviceLogId,
                                                        detail.UserId,
                                                        detail.LogDate,
                                                        detail.Direction,
                                                    })
                              .OrderBy(x => x.LogDate)
                              .Take(100)
                              .ToListAsync();


                        List<Attendance> newAttendanceData = new();
                        List<AttendanceLog> attendanceLogData = new();
                        List<AttendanceSyncStatus> newSyncStatusData = new();

                        foreach (var attendance in attendanceData)
                        {
                            DateOnly date = DateOnlyHelper.ParseDateOrNow(attendance.LogDate.ToString("yyyy-MM-dd"));
                            string time = attendance.LogDate.ToString("HH:mm:ss");
                            TimeOnly parsedTime = TimeOnly.ParseExact(time, "HH:mm:ss");

                            var emp = await _context.EmpDeviceCodes.Where(x => x.DeviceCode == attendance.UserId).FirstOrDefaultAsync(stoppingToken);

                            if (emp == null)
                            {
                                newSyncStatusData.Add(new AttendanceSyncStatus
                                {
                                    DeviceLogId = attendance.DeviceLogId,
                                });

                                if (attendanceLogData.Any(x => x.Direction == attendance.Direction && x.DeviceCode == attendance.UserId && x.Date == date))
                                {
                                    continue;
                                }

                                if (await _context.AttendanceLogs.AnyAsync(x => x.Direction == attendance.Direction && x.DeviceCode == attendance.UserId && x.Date == date))
                                {
                                    continue;
                                }

                                attendanceLogData.Add(new AttendanceLog
                                {
                                    DeviceLogId = attendance.DeviceLogId,
                                    DeviceCode = attendance.UserId,
                                    Direction = attendance.Direction,
                                    Date = date,
                                    Time = time,
                                    IsSuccess = false,
                                    Remarks = "Device Code does not exist."
                                });

                                continue;
                            }

                            if (attendance.Direction == "in" && newAttendanceData.Any(x => x.EmpId == emp.EmpId && x.TransactionDate == date))
                            {
                                newSyncStatusData.Add(new AttendanceSyncStatus
                                {
                                    DeviceLogId = attendance.DeviceLogId,
                                });

                                continue;
                            }

                            if (attendance.Direction == "out" && newAttendanceData.Any(x => x.EmpId == emp.EmpId && x.TransactionDateOut == date))
                            {
                                newSyncStatusData.Add(new AttendanceSyncStatus
                                {
                                    DeviceLogId = attendance.DeviceLogId,
                                });
                                continue;
                            }

                            var existingAttendance = await _context.Attendances.Where(x => x.EmpId == emp.EmpId && (x.TransactionDate == date || x.TransactionDateOut == date)).FirstOrDefaultAsync();

                            if (attendance.Direction == "in")
                            {
                                if (existingAttendance == null)
                                {
                                    newAttendanceData.Add(new Attendance
                                    {
                                        EmpId = emp.EmpId,
                                        TransactionDate = date,
                                        InTime = time,

                                        // Defaults
                                        InMode = "fingerprint",
                                        AttendanceStatus = 0,
                                        FlagIn = false,
                                        FlagOut = false,
                                        AttendanceType = 0,
                                        CheckInMode = 'D',
                                        AttendanceId = 0,
                                        SignOutTimeStamp = 0,
                                        SignInTimeStamp = 0
                                    });

                                    attendanceLogData.Add(new AttendanceLog
                                    {
                                        DeviceLogId = attendance.DeviceLogId,
                                        DeviceCode = attendance.UserId,
                                        Direction = attendance.Direction,
                                        Date = date,
                                        Time = time,
                                        IsSuccess = true,
                                        Remarks = "Synced Successfully"
                                    });
                                }
                                else if (existingAttendance != null && existingAttendance.InTime == null)
                                {
                                    existingAttendance.InTime = time;
                                    existingAttendance.TransactionDate = date;
                                    existingAttendance.InMode = "fingerprint";

                                    attendanceLogData.Add(new AttendanceLog
                                    {
                                        DeviceLogId = attendance.DeviceLogId,
                                        DeviceCode = attendance.UserId,
                                        Direction = attendance.Direction,
                                        Date = date,
                                        Time = time,
                                        IsSuccess = true,
                                        Remarks = "Synced Successfully"
                                    });
                                }
                            }
                            else
                            {
                                if (existingAttendance == null)
                                {
                                    newAttendanceData.Add(new Attendance
                                    {
                                        EmpId = emp.EmpId,
                                        TransactionDate = date,
                                        TransactionDateOut = date,
                                        OutTime = time,

                                        // Defaults
                                        OutMode = "fingerprint",
                                        AttendanceStatus = 0,
                                        FlagIn = false,
                                        FlagOut = false,
                                        AttendanceType = 0,
                                        CheckInMode = 'D',
                                        AttendanceId = 0,
                                        SignOutTimeStamp = 0,
                                        SignInTimeStamp = 0
                                    });

                                    attendanceLogData.Add(new AttendanceLog
                                    {
                                        DeviceLogId = attendance.DeviceLogId,
                                        DeviceCode = attendance.UserId,
                                        Direction = attendance.Direction,
                                        Date = date,
                                        Time = time,
                                        IsSuccess = true,
                                        Remarks = "Synced Successfully"
                                    });
                                }
                                else if (existingAttendance != null && existingAttendance.OutTime == null)
                                {
                                    existingAttendance.OutTime = time;
                                    existingAttendance.TransactionDateOut = date;
                                    existingAttendance.OutMode = "fingerprint";

                                    attendanceLogData.Add(new AttendanceLog
                                    {
                                        DeviceLogId = attendance.DeviceLogId,
                                        DeviceCode = attendance.UserId,
                                        Direction = attendance.Direction,
                                        Date = date,
                                        Time = time,
                                        IsSuccess = true,
                                        Remarks = "Synced Successfully"
                                    });
                                }
                            }

                            newSyncStatusData.Add(new AttendanceSyncStatus
                            {
                                DeviceLogId = attendance.DeviceLogId,
                            });
                        }

                        _context.AddRange(newAttendanceData);
                        _context.AddRange(attendanceLogData);
                        _attendanceContext.AddRange(newSyncStatusData);
                        await _context.SaveChangesAsync();
                        await _attendanceContext.SaveChangesAsync();

                    } while ((from detail in _attendanceContext.DeviceLogsInfos
                              join syncStatus in _attendanceContext.AttendanceSyncStatuses.DefaultIfEmpty() on detail.DeviceLogId equals syncStatus.DeviceLogId into syncStatuses
                              from syncStatus in syncStatuses.DefaultIfEmpty()
                              where syncStatus.DeviceLogId == null && detail.LogDate >= mayDate
                                 && detail.LogDate <= mayDate.AddDays(1)
                              select new { detail.DeviceLogId }).Any());

                    mayDate = mayDate.AddDays(1);

                } while (mayDate <= DateTime.Now);

                if (mayDate > DateTime.Now)
                {
                    Environment.Exit(1);
                }

            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
