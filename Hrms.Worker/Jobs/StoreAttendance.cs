using Hrms.Common.Models;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Worker.Jobs
{
    public class StoreAttendance : IJob
    {
        private readonly DataContext _context;
        private MySqlConnection connection;
        private AttendanceDataContext _attendanceContext;

        public StoreAttendance(DataContext context, IConfiguration config)
        {
            _context = context;
            connection = new MySqlConnection(config.GetConnectionString("AttendanceDbString"));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await connection.OpenAsync();

            try
            {
                DbContextOptionsBuilder<AttendanceDataContext> optionsBuilder = new DbContextOptionsBuilder<AttendanceDataContext>();
                optionsBuilder.UseMySQL(connection);
                _attendanceContext = new AttendanceDataContext(optionsBuilder.Options);

                DateTime currentDate = new(2025, 01, 01, 0, 0, 0);

                if (await _context.AttendanceLogNoDirections.AnyAsync())
                {
                    DateOnly lastDate = await _context.AttendanceLogNoDirections.MaxAsync(x => x.Date);

                    currentDate = new DateTime(lastDate.Year, lastDate.Month, lastDate.Day);
                    currentDate = currentDate.AddDays(-6);
                }

                List<AttendanceLogNoDirection> attendanceLogData = new();
                List<AttendanceSyncStatus> newSyncStatusData = new();

                do
                {
                    int page = 0;

                    do
                    {
                        var attendances = await _attendanceContext.DeviceLogsInfos
                            .Where(x => x.LogDate >= currentDate && x.LogDate <= currentDate.AddHours(3))
                            .Select(x => new
                            {
                                x.DeviceLogId,
                                x.UserId,
                                x.LogDate,
                                x.Direction,
                            })
                            .OrderBy(x => x.LogDate)
                            .Skip(page * 500)
                            .Take(500)
                            .ToListAsync();

                        page++;

                        var deviceLogIds = attendances.Select(x => x.DeviceLogId).ToList();

                        var syncedDeviceLogIds = await _attendanceContext.AttendanceSyncStatuses
                            .Where(x => deviceLogIds.Contains(x.DeviceLogId ?? 0))
                            .Select(x => x.DeviceLogId)
                            .ToListAsync();

                        var attendanceData = attendances.Where(x => !syncedDeviceLogIds.Contains(x.DeviceLogId)).ToList();

                        if (attendanceData.Count == 0)
                        {
                            continue;
                        }

                        foreach (var attendance in attendanceData)
                        {
                            DateOnly date = DateOnlyHelper.ParseDateOrNow(attendance.LogDate.ToString("yyyy-MM-dd"));
                            string time = attendance.LogDate.ToString("HH:mm:ss");
                            TimeOnly parsedTime = TimeOnly.ParseExact(time, "HH:mm:ss");

                            attendanceLogData.Add(new AttendanceLogNoDirection
                            {
                                DeviceCode = attendance.UserId,
                                Date = date,
                                Time = parsedTime,
                                IsSuccess = false,
                                Remarks = "Fingerprint."
                            });

                            newSyncStatusData.Add(new AttendanceSyncStatus
                            {
                                DeviceLogId = attendance.DeviceLogId,
                            });
                        }
                    } while (await _attendanceContext.DeviceLogsInfos
                                .Where(x => x.LogDate >= currentDate && x.LogDate <= currentDate.AddHours(3))
                                .Select(x => new
                                {
                                    x.LogDate,
                                })
                                .OrderBy(x => x.LogDate)
                                .Skip(page * 500)
                                .Take(500)
                                .AnyAsync());

                    currentDate = currentDate.AddHours(3);
                } while (currentDate <= DateTime.Now.AddDays(1));

                _context.AddRange(attendanceLogData);
                _attendanceContext.AddRange(newSyncStatusData);
                await _context.SaveChangesAsync();
                await _attendanceContext.SaveChangesAsync();

                _context.SyncAttendanceLogs.Add(new SyncAttendanceLog
                {
                    Type = "store",
                    Status = "success",
                    SyncedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

            } catch (Exception ex)
            {
                _context.SyncAttendanceLogs.Add(new SyncAttendanceLog
                {
                    Type = "store",
                    Status = "fail",
                    ErrorTrace = ex.StackTrace,
                    ErrorMessage = ex.Message,
                    SyncedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }
    }
}
