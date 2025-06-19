using Hrms.DesktopWorker.Utilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Text.Json.Serialization;
using zkemkeeper;
using Hrms.DesktopWorker.Settings;
using System.Collections.Generic;
using Hrms.DesktopWorker.Data;
using Hrms.DesktopWorker.Models;
using Microsoft.EntityFrameworkCore.Storage;
using CsvHelper;
using System.Globalization;

namespace Hrms.DesktopWorker
{
    public class Worker : BackgroundService
    {
        public CZKEM objCZKEM = new CZKEM();
        IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<Worker> _logger;

        public Worker(IServiceScopeFactory serviceScopeFactory, ILogger<Worker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string errorLogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AttendanceErrorLog");
            Directory.CreateDirectory(errorLogFilePath);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogWarning("Service Started");

                    await using var scope = _serviceScopeFactory.CreateAsyncScope();
                    var _context = scope.ServiceProvider.GetRequiredService<DataContext>();

                    string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Attendance");
                    string deviceConfigPath = Path.Combine(appDataFolder, "devices.config.json");

                    if (!File.Exists(deviceConfigPath))
                    {
                        LogDeviceError(_context, null, "Device config file not found.");
                        await _context.SaveChangesAsync(stoppingToken);
                        continue;
                    }

                    List<Device> devices;

                    using (var sr = new StreamReader(deviceConfigPath))
                    {
                        string json = await sr.ReadToEndAsync();
                        devices = JsonConvert.DeserializeObject<List<Device>>(json);
                    }

                    if (devices is null || devices.Count == 0)
                    {
                        LogDeviceError(_context, null, "No Devices Set.");
                        await _context.SaveChangesAsync(stoppingToken);
                        continue;
                    }

                    foreach (var device in devices)
                    {
                        try
                        {
                            await ProcessDeviceAsync(device, _context, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            LogDeviceError(_context, null, $"Device processing failed: {ex.Message}", ex.StackTrace);
                            await _context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in worker service");

                    string filePath = Path.Combine(errorLogFilePath, "errorLog.csv");
                    using var writer = new StreamWriter(filePath, true);
                    using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
                    csvWriter.WriteRecord(new LogHeader
                    {
                        LogDate = DateTime.Now,
                        ErrorTrace = ex.StackTrace,
                        ErrorMessage = ex.Message
                    });
                    writer.WriteLine(); // Add newline after the CSV row
                }

                // Always delay 1 hour before the next execution
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task ProcessDeviceAsync(Device device, DataContext _context, CancellationToken stoppingToken)
        {
            int id = int.Parse(AesOperation.DecryptString(device.DeviceId));
            string ipAddress = AesOperation.DecryptString(device.IpAddress);
            int portNumber = device.PortNumber;

            if (string.IsNullOrWhiteSpace(ipAddress) || portNumber == 0)
            {
                LogDeviceError(_context, id, "Invalid IP or Port.");
                return;
            }

            var deviceSetting = await _context.DeviceSettings.FirstOrDefaultAsync(x => x.Id == id, stoppingToken);
            if (deviceSetting == null)
            {
                LogDeviceError(_context, id, "Device does not exist in the database.");
                return;
            }

            if (!UniversalStatic.ValidateIP(ipAddress))
            {
                LogDeviceError(_context, id, "Invalid IP Address.");
                return;
            }

            if (!UniversalStatic.PingTheDevice(ipAddress))
            {
                LogDeviceError(_context, id, "The Device did not respond.");
                return;
            }

            objCZKEM.Connect_Net(ipAddress, portNumber);
            _logger.LogWarning($"Device Connected at IP: {ipAddress}");

            List<AttendanceLogNoDirection> attendanceData = new();
            if (objCZKEM.ReadGeneralLogData(objCZKEM.MachineNumber))
            {
                DateOnly lastFetchedDate = deviceSetting.LastFetchedDate ?? new DateOnly(2025, 1, 1);
                TimeOnly lastFetchedTime = deviceSetting.LastFetchedTime ?? new TimeOnly(0, 0, 0);

                getAttendanceLogs(objCZKEM, attendanceData, id, lastFetchedDate, lastFetchedTime);

                if (attendanceData.Count > 0)
                {
                    _context.AddRange(attendanceData);

                    DateOnly maxDate = attendanceData.Max(x => x.Date);
                    TimeOnly maxTime = attendanceData.Where(x => x.Date == maxDate).Max(x => x.Time);

                    deviceSetting.LastFetchedDate = maxDate;
                    deviceSetting.LastFetchedTime = maxTime;

                    LogDeviceSuccess(_context, id, $"{attendanceData.Count} data fetched successfully.");
                }
                else
                {
                    LogDeviceError(_context, id, "No Data Found.");
                }
            }
            else
            {
                LogDeviceError(_context, id, "No Data Found.");
            }

            await _context.SaveChangesAsync(stoppingToken);
        }

        private void LogDeviceError(DataContext context, int? deviceId, string message, string stackTrace = null)
        {
            context.DeviceLogs.Add(new DeviceLog
            {
                DeviceId = deviceId,
                IsSuccess = false,
                Remarks = message,
                ErrorMessage = message,
                ErrorTrace = stackTrace
            });
        }

        private void LogDeviceSuccess(DataContext context, int deviceId, string message)
        {
            context.DeviceLogs.Add(new DeviceLog
            {
                DeviceId = deviceId,
                IsSuccess = true,
                Remarks = message
            });
        }

        static void getAttendanceLogs(CZKEM objCZKEM, List<AttendanceLogNoDirection> attendanceData, int id, DateOnly LastFetchedDate, TimeOnly LastFetchedTime)
        {
            string dwEnrollNumber;
            int dwVerifyMode;
            int dwInOutMode;
            int dwYear;
            int dwMonth;
            int dwDay;
            int dwHour;
            int dwMinute;
            int dwSecond;
            int dwWorkCode = 1;
            int AWorkCode;
            objCZKEM.GetWorkCode(dwWorkCode, out AWorkCode);

            while (true)
            {
                if (!objCZKEM.SSR_GetGeneralLogData(
                    objCZKEM.MachineNumber,
                    out dwEnrollNumber,
                    out dwVerifyMode,
                    out dwInOutMode,
                    out dwYear,
                    out dwMonth,
                    out dwDay,
                    out dwHour,
                    out dwMinute,
                    out dwSecond,
                    ref AWorkCode))
                {
                    break;
                }

                DateOnly date = DateOnly.Parse($"{dwYear}-{dwMonth:D2}-{dwDay:D2}");
                TimeOnly time = TimeOnly.Parse($"{dwHour:D2}:{dwMinute:D2}:{dwSecond:D2}");

                if (date < LastFetchedDate) continue;
                if (date == LastFetchedDate && time <= LastFetchedTime) continue;

                attendanceData.Add(new AttendanceLogNoDirection
                {
                    DeviceCode = dwEnrollNumber,
                    Remarks = verificationMode(dwVerifyMode),
                    Date = date,
                    Time = time,
                    IsSuccess = false,
                    DeviceId = id,
                });
            }
        }

        static string verificationMode(int verifyMode)
        {
            return verifyMode switch
            {
                0 => "Password",
                1 => "Fingerprint",
                2 => "Card",
                _ => "Unknown"
            };
        }
    }
}
