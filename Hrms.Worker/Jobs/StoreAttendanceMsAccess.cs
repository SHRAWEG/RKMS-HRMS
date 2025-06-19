using Hrms.Common.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using NPOI.HSSF.Model;
using NPOI.OpenXmlFormats.Vml;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Hrms.Worker.Jobs
{
    public class StoreAttendanceMsAccess : IJob
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public StoreAttendanceMsAccess(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public List<AttendanceLogNoDirection> FetchLogs()
        {
            var logs = new List<AttendanceLogNoDirection>();

            using (OdbcConnection connection = new OdbcConnection(_config.GetConnectionString("DSN")))
            {
                connection.Open();

                DateTime currentDate = DateTime.UtcNow;

                var tableName = $"DeviceLogs_{currentDate.Month}_{currentDate.Year}";

                var accessQuery = $@"
                        SELECT LogDate, UserId
                        FROM {tableName}
                        ORDER BY LogDate";

                OdbcCommand accessCommand = new OdbcCommand(accessQuery, connection);
                OdbcDataReader reader = accessCommand.ExecuteReader();
                    
                while (reader.Read())
                {
                    var logTime = reader.GetDateTime(0);
                    var date = DateOnly.FromDateTime(logTime);
                    var time = TimeOnly.FromDateTime(logTime);
                    var userId = reader.GetString(1);
                    logs.Add(new AttendanceLogNoDirection 
                    {
                        DeviceCode = userId,
                        Date = date,    
                        Time = time,
                        Remarks = "Fingerprint",
                        IsSuccess = false
                    });
                }

                reader.Close();
            }

            Console.WriteLine(logs.Count);

            return logs;
        }

        private async Task InsertBatchAsync(List<AttendanceLogNoDirection> batch)
        {
            // Fetch existing keys (Date + Time + EmpId) from the main table
            var existingKeys = _context.AttendanceLogNoDirections
                .Select(a => new { a.Date, a.Time, a.DeviceCode })
                .ToHashSet();

            // Filter the batch to include only new records
            var newRecords = batch.Where(b =>
                !existingKeys.Contains(new { b.Date, b.Time, b.DeviceCode })).ToList();

            // Insert only new records
            if (newRecords.Any())
            {
                await _context.AttendanceLogNoDirections.AddRangeAsync(newRecords);
                await _context.SaveChangesAsync();
            }
        }

        public void CreateIndex()
        {
            var createIndexQuery = @"
                    CREATE INDEX IF NOT EXISTS attendance_index
                    ON ""ATTENDANCE_LOG_NO_DIRECTION"" (""DATE"", ""TIME"", ""DEVICE_CODE"");
                ";

            _context.Database.ExecuteSqlRaw(createIndexQuery);
        }

        public async Task ProcessAndStoreLogsAsync()
        {
            int BatchSize = 1000;

            var records = FetchLogs();

            // Step 3c: Insert records into the temporary table in batches
            for (int i = 0; i < records.Count; i += BatchSize)
            {
                var batch = records.Skip(i).Take(BatchSize).ToList();
                await InsertBatchAsync(batch);
            }
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var logs = new List<(DateOnly Date, TimeOnly Time, string DeviceCode)>();

            //DateTime fromMonth = DateTime.UtcNow.AddMonths(-1);
            DateTime fromMonth = new DateTime(2025, 1, 1);
            DateTime currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            try
            {
                CreateIndex();

                await ProcessAndStoreLogsAsync();

                _context.SyncAttendanceLogs.Add(new SyncAttendanceLog
                {
                    Type = "store",
                    Status = "success",
                    SyncedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
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
