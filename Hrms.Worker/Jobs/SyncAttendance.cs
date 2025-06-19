using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Worker.Jobs
{
    public class SyncAttendance : IJob
    {
        private readonly DataContext _context;

        public SyncAttendance(DataContext context)
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {

            do
            {
                var attendanceData = await _context.AttendanceLogs
                    .Where(x => !x.IsSuccess && x.Date > DateOnly.FromDateTime(DateTime.Today).AddDays(-6))
                    .OrderByDescending(x => x.Date)
                    .Take(1000)
                    .ToListAsync();

                List<Attendance> newAttendanceData = new();

                foreach (var attendance in attendanceData)
                {
                    var emp = await _context.EmpDeviceCodes.Where(x => x.DeviceCode == attendance.DeviceCode).FirstOrDefaultAsync();

                    if (emp == null)
                    {
                        continue;
                    }

                    if (attendance.Direction == "in" && newAttendanceData.Any(x => x.EmpId == emp.EmpId && x.TransactionDate == attendance.Date))
                    {
                        attendance.IsSuccess = true;

                        continue;
                    }

                    if (attendance.Direction == "out" && newAttendanceData.Any(x => x.EmpId == emp.EmpId && x.TransactionDateOut == attendance.Date))
                    {
                        attendance.IsSuccess = true;

                        continue;
                    }

                    var existingAttendance = await _context.Attendances.Where(x => x.EmpId == emp.EmpId && (x.TransactionDate == attendance.Date || x.TransactionDateOut == attendance.Date)).FirstOrDefaultAsync();

                    if (attendance.Direction == "in")
                    {
                        if (existingAttendance == null)
                        {
                            newAttendanceData.Add(new Attendance
                            {
                                EmpId = emp.EmpId,
                                TransactionDate = attendance.Date,
                                InTime = attendance.Time,

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

                            attendance.IsSuccess = true;
                        }
                        else if (existingAttendance != null && existingAttendance.InTime != null)
                        {
                            existingAttendance.InTime = attendance.Time;
                            existingAttendance.TransactionDate = attendance.Date;
                            existingAttendance.InMode = "fingerprint";

                            attendance.IsSuccess = true;
                        }
                    }
                    else
                    {
                        if (existingAttendance == null)
                        {
                            newAttendanceData.Add(new Attendance
                            {
                                EmpId = emp.EmpId,
                                TransactionDateOut = attendance.Date,
                                OutTime = attendance.Time,

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

                            attendance.IsSuccess = true;
                        }
                        else if (existingAttendance != null && existingAttendance.OutTime != null)
                        {
                            existingAttendance.OutTime = attendance.Time;
                            existingAttendance.TransactionDateOut = attendance.Date;
                            existingAttendance.OutMode = "fingerprint";

                            attendance.IsSuccess = true;
                        }
                    }
                }

                _context.AddRange(newAttendanceData);
                await _context.SaveChangesAsync();
                 
            } while (await _context.AttendanceLogs.Where(x => !x.IsSuccess && x.Date > DateOnly.FromDateTime(DateTime.Today).AddDays(-6)).AnyAsync());
        }
    }
}
