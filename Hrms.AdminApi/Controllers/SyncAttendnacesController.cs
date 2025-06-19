using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using NPOI.SS.Formula.Functions;
using static Hrms.AdminApi.Controllers.AttendancesController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class SyncAttendancesController : Controller
    {
        private readonly DataContext _context;
        private MySqlConnection connection;
        private AttendanceDataContext _attendanceContext;

        public SyncAttendancesController(DataContext context, IOptions<DatabaseSettings> options)
        {
            _context = context;
            connection = new MySqlConnection(options.Value.AttendanceConnectionString);
        }

        // Sync non-synced attendance
        [HttpPost()]
        public async Task<IActionResult> SyncAttendance(SyncInputModel input)
        {
            DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);
            DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(input.ToDate);

            try
            {
                do
                {
                    var attendanceData = await _context.AttendanceLogNoDirections
                        .Where(x => !x.IsSuccess && x.Date == FromDate)
                        .OrderBy(x => x.Date)
                        .OrderBy(x => x.Time)
                        .ToListAsync();

                    if (attendanceData.Count == 0)
                    {
                        FromDate = FromDate.AddDays(1);

                        continue;
                    }

                    List<Attendance> newAttendanceData = new();

                    foreach (var attendance in attendanceData)
                    {
                        var emp = await _context.EmpDeviceCodes
                            .Where(x => x.DeviceCode == attendance.DeviceCode && (attendance.DeviceId != null ? x.DeviceId == attendance.DeviceId : true))
                            .FirstOrDefaultAsync();

                        if (emp == null)
                        {
                            attendance.Remarks = "Emp with the Device Code does not exists.";

                            continue;
                        }

                        WorkHour? workHour;

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

                            continue;
                        }

                        if (workHour.IsFlexible && workHour.IsNightShift)
                        {
                            if (string.IsNullOrEmpty(workHour.NightStartTime) || string.IsNullOrEmpty(workHour.NightEndTime))
                            {
                                attendance.IsSuccess = false;
                                attendance.Remarks = "Start Time or End Time for night shift is empty.";

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

                                                continue;
                                            }
                                            else
                                            {
                                                attendance.IsSuccess = true;
                                                attendance.Remarks = "Successfully synced.";

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

                                            continue;
                                        }
                                        else
                                        {
                                            attendance.IsSuccess = true;
                                            attendance.Remarks = "Successfully synced.";

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

                                                continue;
                                            }

                                            if (attendance.Time > nightAttendanceOutTime)
                                            {
                                                existingNightAttendance.OutTime = attendance.Time.ToString("HH:mm:ss");
                                                existingNightAttendance.TransactionDateOut = attendance.Date;
                                                existingNightAttendance.OutMode = "fingerprint";

                                                attendance.IsSuccess = true;
                                                attendance.Remarks = "Successfully synced.";

                                                continue;
                                            }
                                            else
                                            {
                                                attendance.IsSuccess = true;
                                                attendance.Remarks = "Successfully synced.";

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

                                            continue;
                                        }

                                        if (attendance.Time > nightAttendanceOutTime)
                                        {
                                            syncedNightEmp.OutTime = attendance.Time.ToString("HH:mm:ss");
                                            syncedNightEmp.TransactionDateOut = attendance.Date;
                                            syncedNightEmp.OutMode = "fingerprint";

                                            attendance.IsSuccess = true;
                                            attendance.Remarks = "Successfully synced.";

                                            continue;
                                        }
                                        else
                                        {
                                            attendance.IsSuccess = true;
                                            attendance.Remarks = "Successfully synced.";

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
                                existingAttendance.InTime = attendance.Time.ToString("HH:mm:ss");
                                existingAttendance.TransactionDate = attendance.Date;
                                existingAttendance.InMode = "fingerprint";

                                if (existingAttendance.OutTime is null)
                                {
                                    existingAttendance.OutTime = existingAttendance.InTime;
                                    existingAttendance.TransactionDateOut = existingAttendance.TransactionDate;
                                    existingAttendance.OutMode = "fingerprint";
                                }

                                attendance.IsSuccess = true;
                                attendance.Remarks = "Successfully synced.";

                                continue;
                            }
                            else if (attendance.Time > attendanceTimeOut)
                            {
                                existingAttendance.OutTime = attendance.Time.ToString("HH:mm:ss");
                                existingAttendance.TransactionDateOut = attendance.Date;
                                existingAttendance.OutMode = "fingerprint";

                                attendance.IsSuccess = true;
                                attendance.Remarks = "Successfully synced.";

                                continue;
                            }
                            else
                            {
                                attendance.IsSuccess = true;
                                attendance.Remarks = "Successfully synced.";

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
                                syncedEmp.InTime = attendance.Time.ToString("HH:mm:ss");
                                syncedEmp.TransactionDate = attendance.Date;
                                syncedEmp.InMode = "fingerprint";

                                if (syncedEmp.OutTime is null)
                                {
                                    syncedEmp.OutTime = syncedEmp.InTime;
                                    syncedEmp.TransactionDateOut = syncedEmp.TransactionDate;
                                    syncedEmp.OutMode = "fingerprint";
                                }

                                attendance.IsSuccess = true;
                                attendance.Remarks = "Successfully synced.";

                                continue;
                            }
                            else if (attendance.Time > attendanceTimeOut)
                            {
                                syncedEmp.OutTime = attendance.Time.ToString("HH:mm:ss");
                                syncedEmp.TransactionDateOut = attendance.Date;
                                syncedEmp.OutMode = "fingerprint";

                                attendance.IsSuccess = true;
                                attendance.Remarks = "Successfully synced.";

                                continue;
                            }
                            else
                            {
                                attendance.IsSuccess = true;
                                attendance.Remarks = "Successfully synced.";

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
                        }
                    }

                    _context.AddRange(newAttendanceData);
                    await _context.SaveChangesAsync();

                    FromDate = FromDate.AddDays(1);

                } while (FromDate <= ToDate);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Delete current attendance and resync
        [HttpPost("Resync")]
        public async Task<IActionResult> ReSyncAttendance(SyncInputModel input)
        {
            DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);
            DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(input.ToDate);

            var empDeviceCodes = await _context.EmpDeviceCodes
                .Where(x => input.EmpId.Equals(x.EmpId))
                .ToListAsync();

            try
            {
                var removeAttendancesQuery = _context.Attendances
                    .Where(x => x.TransactionDate >= FromDate && x.TransactionDate <= ToDate)
                    .AsQueryable();

                if (input.EmpId is not null)
                {
                    removeAttendancesQuery = removeAttendancesQuery.Where(x => x.EmpId == input.EmpId);
                }

                var removeAttendances = await removeAttendancesQuery.ToListAsync();

                _context.RemoveRange(removeAttendances);
                await _context.SaveChangesAsync();

                var attendanceDataQuery = _context.AttendanceLogNoDirections
                    .Where(x => x.Date >= FromDate && x.Date <= ToDate)
                    .AsQueryable();

                if (input.EmpId is not null)
                {
                    var empDeviceCode = await _context.EmpDeviceCodes.Where(x => x.EmpId == input.EmpId).FirstOrDefaultAsync();

                    if (empDeviceCode is null)
                    {
                        return ErrorHelper.ErrorResult("Id", "Device code not set for the employee.");
                    }

                    attendanceDataQuery = attendanceDataQuery.Where(x => x.DeviceCode == empDeviceCode.DeviceCode && (x.DeviceId == null || x.DeviceId == empDeviceCode.DeviceId));
                }

                var attendanceData = await attendanceDataQuery.ToListAsync();

                List<Attendance> newAttendanceData = new();

                foreach (var attendance in attendanceData)
                {
                    var emp = await _context.EmpDeviceCodes
                        .Where(x => x.DeviceCode == attendance.DeviceCode && (attendance.DeviceId != null ? x.DeviceId == attendance.DeviceId : true))
                        .FirstOrDefaultAsync();

                    if (emp == null)
                    {
                        attendance.Remarks = "Emp with the Device Code does not exists.";

                        continue;
                    }

                    WorkHour? workHour;

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

                        continue;
                    }

                    if (workHour.IsFlexible && workHour.IsNightShift)
                    {
                        if (string.IsNullOrEmpty(workHour.NightStartTime) || string.IsNullOrEmpty(workHour.NightEndTime))
                        {
                            attendance.IsSuccess = false;
                            attendance.Remarks = "Start Time or End Time for night shift is empty.";

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

                                            continue;
                                        }
                                        else
                                        {
                                            attendance.IsSuccess = true;
                                            attendance.Remarks = "Successfully synced.";

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

                                        continue;
                                    }
                                    else
                                    {
                                        attendance.IsSuccess = true;
                                        attendance.Remarks = "Successfully synced.";

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

                                            continue;
                                        }

                                        if (attendance.Time > nightAttendanceOutTime)
                                        {
                                            existingNightAttendance.OutTime = attendance.Time.ToString("HH:mm:ss");
                                            existingNightAttendance.TransactionDateOut = attendance.Date;
                                            existingNightAttendance.OutMode = "fingerprint";

                                            attendance.IsSuccess = true;
                                            attendance.Remarks = "Successfully synced.";

                                            continue;
                                        }
                                        else
                                        {
                                            attendance.IsSuccess = true;
                                            attendance.Remarks = "Successfully synced.";

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

                                        continue;
                                    }

                                    if (attendance.Time > nightAttendanceOutTime)
                                    {
                                        syncedNightEmp.OutTime = attendance.Time.ToString("HH:mm:ss");
                                        syncedNightEmp.TransactionDateOut = attendance.Date;
                                        syncedNightEmp.OutMode = "fingerprint";

                                        attendance.IsSuccess = true;
                                        attendance.Remarks = "Successfully synced.";

                                        continue;
                                    }
                                    else
                                    {
                                        attendance.IsSuccess = true;
                                        attendance.Remarks = "Successfully synced.";

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

                            continue;
                        }
                        else if (attendance.Time > attendanceTimeOut)
                        {
                            existingAttendance.OutTime = attendance.Time.ToString("HH:mm:ss");
                            existingAttendance.TransactionDateOut = attendance.Date;
                            existingAttendance.OutMode = "fingerprint";

                            attendance.IsSuccess = true;
                            attendance.Remarks = "Successfully synced.";

                            continue;
                        }
                        else
                        {
                            attendance.IsSuccess = true;
                            attendance.Remarks = "Successfully synced.";

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

                            continue;
                        }
                        else if (attendance.Time > attendanceTimeOut)
                        {
                            syncedEmp.OutTime = attendance.Time.ToString("HH:mm:ss");
                            syncedEmp.TransactionDateOut = attendance.Date;
                            syncedEmp.OutMode = "fingerprint";

                            attendance.IsSuccess = true;
                            attendance.Remarks = "Successfully synced.";

                            continue;
                        }
                        else
                        {
                            attendance.IsSuccess = true;
                            attendance.Remarks = "Successfully synced.";

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
                    }
                }

                _context.AddRange(newAttendanceData);
                await _context.SaveChangesAsync();

                var regularisations = await _context.Regularisations
                    .Include(x => x.RegularisationType)
                    .Where(x => x.Status == "approved" && (x.RegularisationType.Name == "on-duty" || x.RegularisationType.Name == "work-from-home") && x.FromDate <= ToDate && x.ToDate >= FromDate)
                    .ToListAsync();

                foreach (var regularisation in regularisations)
                {
                    DateOnly date = regularisation.FromDate ?? new();

                    do
                    {
                        if (date > ToDate)
                        {
                            date = date.AddDays(1);

                            continue;
                        }

                        var attendance = await _context.Attendances.Where(x => x.TransactionDate == date && x.EmpId == regularisation.EmpId).FirstOrDefaultAsync();

                        if (attendance is null)
                        {
                            WorkHour? workHour;

                            var roster = await _context.Rosters
                                .Where(x => x.Date == date && x.EmpId == regularisation.EmpId)
                                .Include(x => x.WorkHour)
                                .FirstOrDefaultAsync();

                            workHour = roster?.WorkHour;

                            if (roster is null)
                            {
                                var defaultWorkHour = await _context.DefaultWorkHours
                                    .Where(x => (x.EmpId == regularisation.EmpId || x.EmpId == null) && x.DayId == ((short)date.DayOfWeek + 1))
                                    .OrderBy(x => x.EmpId)
                                    .Include(x => x.WorkHour)
                                    .FirstOrDefaultAsync();

                                workHour = defaultWorkHour?.WorkHour;
                            }

                            if (workHour is null)
                            {
                                return ErrorHelper.ErrorResult("Id", "No shift assigned for employee.");
                            }

                            _context.Add(new Attendance
                            {
                                EmpId = regularisation.EmpId,
                                TransactionDate = date,
                                TransactionDateOut = date,
                                InTime = regularisation.FromTime?.ToString("HH:mm:ss"),
                                OutTime = regularisation.ToTime?.ToString("HH:mm:ss"),
                                InRemarks = "On Duty Regularised",
                                OutRemarks = "On Duty Regularised",
                                RegularisationId = regularisation.Id,
                                WorkHourId = workHour.Id,

                                //Defaults
                                AttendanceStatus = 0,
                                FlagIn = false,
                                FlagOut = false,
                                AttendanceType = 0,
                                CheckInMode = 'N',
                                AttendanceId = 0,
                                SignOutTimeStamp = 0,
                                SignInTimeStamp = 0,
                            });
                        }
                        else
                        {
                            attendance.RegularisationId = regularisation.Id;
                        }

                        await _context.SaveChangesAsync();

                        date = date.AddDays(1);
                    } while (date <= regularisation.ToDate);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: AssetTypes
        [HttpPost("Store")]
        public async Task<IActionResult> StoreAttendances(StoreInputModel input)
        {
            await connection.OpenAsync();

            DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);
            DateTime FromDateTime = new(FromDate.Year, FromDate.Month, FromDate.Day, 0, 0, 0);
            DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(input.ToDate);
            DateTime ToDateTime = new(ToDate.Year, ToDate.Month, ToDate.Day, 23, 59, 59);

            try
            {
                DbContextOptionsBuilder<AttendanceDataContext> optionsBuilder = new DbContextOptionsBuilder<AttendanceDataContext>();
                optionsBuilder.UseMySQL(connection);
                _attendanceContext = new AttendanceDataContext(optionsBuilder.Options);

                List<AttendanceLogNoDirection> attendanceLogData = new();
                List<AttendanceSyncStatus> newSyncStatusData = new();

                do
                {
                    int page = 0;

                    do
                    {
                        var attendances = await _attendanceContext.DeviceLogsInfos
                            .Where(x => x.LogDate >= FromDateTime && x.LogDate <= FromDateTime.AddHours(3))
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
                                .Where(x => x.LogDate >= FromDateTime && x.LogDate <= FromDateTime.AddHours(3))
                                .Select(x => new
                                {
                                    x.LogDate,
                                })
                                .OrderBy(x => x.LogDate)
                                .Skip(page * 500)
                                .Take(500)
                                .AnyAsync());

                    FromDateTime = FromDateTime.AddHours(3);
                } while (FromDateTime <= ToDateTime);

                _context.AddRange(attendanceLogData);
                _attendanceContext.AddRange(newSyncStatusData);
                await _context.SaveChangesAsync();
                await _attendanceContext.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Store/Log")]
        public async Task<IActionResult> StoreData(StoreDataInputModel input)
        {
            if (input.DeviceId is null)
            {
                _context.Add(new DeviceLog
                {
                    DeviceId = input.DeviceId,
                    Date = DateTime.UtcNow,
                    ErrorMessage = "Unable to connect device",
                    ErrorTrace = "",
                    IsSuccess = false
                });

                await _context.SaveChangesAsync();

                return BadRequest("Please specify device.");
            }

            List<AttendanceLogNoDirection> data = new();

            foreach (var i in input.Data)
            {
                if (string.IsNullOrEmpty(i.DeviceCode) || i.Date == null || i.Time == null)
                {
                    _context.Add(new DeviceLog
                    {
                        DeviceId = input.DeviceId,
                        Date = DateTime.UtcNow,
                        ErrorMessage = "Null data.",
                        Remarks = "Unsuccessful",
                        IsSuccess = false
                    });

                    await _context.SaveChangesAsync();

                    return BadRequest("Data cannot be null");
                }

                data.Add(new AttendanceLogNoDirection
                {
                    DeviceId = input.DeviceId,
                    DeviceCode = i.DeviceCode,
                    Date = DateOnly.Parse(i.Date),
                    Time = TimeOnly.Parse(i.Time),
                    Remarks = i.Remarks,
                    IsSuccess = false
                });
            }

            _context.AddRange(data);
            _context.Add(new DeviceLog
            {
                DeviceId = input.DeviceId,
                Date = DateTime.UtcNow,
                Remarks = "Successful",
                IsSuccess = true
            });

            await _context.SaveChangesAsync();

            if (await _context.AttendanceLogNoDirections.AnyAsync(x => x.DeviceId == input.DeviceId))
            {
                DateOnly lastDate = await _context.AttendanceLogNoDirections.Where(x => x.DeviceId == input.DeviceId).MaxAsync(x => x.Date);
                TimeOnly lastTime = await _context.AttendanceLogNoDirections.Where(x => x.DeviceId == input.DeviceId && x.Date == lastDate).MaxAsync(x => x.Time);

                var deviceSetting = await _context.DeviceSettings.Where(x => x.Id == input.DeviceId).FirstOrDefaultAsync();

                if (deviceSetting is not null)
                {
                    deviceSetting.LastFetchedDate = lastDate;
                    deviceSetting.LastFetchedTime = lastTime;
                };

                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost("DeviceLog")]
        public async Task<IActionResult> DeviceLog(int? deviceId, string remarks, string? errorMessage, string? errorTrace)
        {
            _context.Add(new DeviceLog
            {
                DeviceId = deviceId,
                Remarks = remarks,
                ErrorMessage = errorMessage,
                ErrorTrace = errorTrace,
                IsSuccess = false,
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string FromDate { get; set; }
            public string ToDate { get; set; }
        }

        public class StoreInputModel : BaseInputModel { }

        public class SyncInputModel : BaseInputModel
        {
            public int? EmpId { get; set; }
        }

        public class AttendanceData
        {
            public string DeviceCode { get; set; }
            public string Remarks { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
        }

        public class StoreDataInputModel
        {
            public int? DeviceId { get; set; }
            public List<AttendanceData> Data { get; set; }
        }

        public class AddInputModelValidator : AbstractValidator<StoreInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.FromDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateBeforeNow();

                RuleFor(x => x.ToDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.FromDate, "FromDate");
            }
        }

        public class UpdateInputModelValidator : AbstractValidator<SyncInputModel>
        {
            private readonly DataContext _context;

            public UpdateInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.FromDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateBeforeNow();

                RuleFor(x => x.ToDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.FromDate, "FromDate");

                RuleFor(x => x.EmpId)
                    .IdMustExist(_context.EmpDetails.AsQueryable())
                    .Unless(x => x.EmpId is not null);
            }
        }
    }
}
