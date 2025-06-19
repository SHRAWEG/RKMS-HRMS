using Hrms.Common.Data.Migrations;
using Hrms.Common.Enumerations;
using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.Design;
using System.Drawing;
using System.Xml.Linq;
using static Hrms.AdminApi.Controllers.LeaveManagementsController;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]


    public class RegularisationsController : Controller
    {
        private readonly DataContext _context;

        public RegularisationsController(DataContext context)
        {
            _context = context;
        }

        // GET: Regularisations
        [CustomAuthorize("regularization-history")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, 
            int? regularisationTypeId, int? empId, string fromDate, string toDate, int? companyId, int? departmentId, int? divisionId)
        {
            var query = _context.Regularisations
                .Include(x => x.RegularisationType)
                .Include(x => x.Emp)
                .Include(x => x.GatePassType)
                .Include(x => x.ApprovedByUser)
                .ThenInclude(x => x.Emp)
                .Include(x => x.DisapprovedByUser)
                .ThenInclude(x => x.Emp)
                .Join(_context.EmpLogs,
                    r => r.EmpId,
                    el => el.EmployeeId,
                    (r, el) => new
                    {
                        Regularisation = r,
                        PId = el.Id,
                    })
                .Join(_context.EmpTransactions.Include(x => x.Company).Include(x => x.Department).Include(x => x.Division),
                    x => x.PId,
                    et => et.Id,
                    (x, et) => new RegularisationHistoryDTO
                    {
                        Regularisation = x.Regularisation,
                        EmpTransaction = et
                    })
                .AsQueryable();

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                var empIds = await _context.EmpLogs
                    .Join(_context.EmpTransactions,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        EmpId = el.EmployeeId,
                        CompanyId = et.CompanyId
                    })
                    .Where(x => companyIds.Contains(x.CompanyId ?? 0))
                    .Select(x => x.EmpId)
                    .ToListAsync();

                query = query.Where(x => empIds.Contains(x.Regularisation.EmpId));
            }

            if (regularisationTypeId != null)
            {
                query = query.Where(x => x.Regularisation.RegularisationTypeId == regularisationTypeId);
            }

            if (empId != null)
            {
                query = query.Where(x => x.Regularisation.EmpId == empId);
            }

            if (!string.IsNullOrEmpty(fromDate))
            {
                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);

                query = query.Where(b => b.Regularisation.FromDate >= FromDate);
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

                query = query.Where(x => x.Regularisation.ToDate <= ToDate);
            }

            if (companyId != null)
            {
                query = query.Where(x => x.EmpTransaction.CompanyId == companyId);
            }

            if (departmentId != null)
            {
                query = query.Where(x => x.EmpTransaction.DepartmentId == departmentId);
            }

            if (divisionId != null)
            {
                query = query.Where(x => x.EmpTransaction.DivisionId == divisionId);
            }

            Expression<Func<RegularisationHistoryDTO, object>> field = sortColumn switch
            {
                "EmpCode" => x => x.Regularisation.Emp.CardId,
                "ProcessedDate" => x => x.Regularisation.UpdatedAt,
                "RequestedDate" => x => x.Regularisation.CreatedAt,
                "RegularisationTypeName" => x => x.Regularisation.RegularisationType.Name,
                "FromDate" => x => x.Regularisation.FromDate,
                "ToDate" => x => x.Regularisation.ToDate,
                "FromTime" => x => x.Regularisation.FromTime,
                "ToTime" => x => x.Regularisation.ToTime,
                "Status" => x => x.Regularisation.Status,
                "DepartmentName" => x => x.EmpTransaction.Department.Name,
                "CompanyName" => x => x.EmpTransaction.Company.Name,
                "DivisionName" => x => x.EmpTransaction.Division.Name,
                _ => x => x.Regularisation.Id
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Regularisation.Id);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<RegularisationHistoryDTO>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Regularisation.Id,
                    EmpId = x.Regularisation.EmpId,
                    EmpCode = x.Regularisation.Emp.CardId,
                    GatePassTypeId = x.Regularisation.GatePassTypeId,
                    GatePassTypeName = x.Regularisation.GatePassType != null ? x.Regularisation.GatePassType.Name : "",
                    RegularisationTypeId = x.Regularisation.RegularisationTypeId,
                    RegularisationTypeName = x.Regularisation.RegularisationType.DisplayName,
                    FromDate = x.Regularisation.FromDate,
                    ToDate = x.Regularisation.ToDate,
                    FromTime = x.Regularisation.FromTime.ToString(),
                    ToTime = x.Regularisation.ToTime.ToString(),
                    ContactNumber = x.Regularisation.ContactNumber,
                    Place = x.Regularisation.Place,
                    Reason = x.Regularisation.Reason,
                    Status = x.Regularisation.Status,
                    CancellationRemarks = x.Regularisation.CancellationRemarks,
                    ApprovedByUserId = x.Regularisation.ApprovedByUserId,
                    ApprovedByUserCode = x.Regularisation.ApprovedByUser?.UserName,
                    ApprovedByUserName = Helper.FullName(x.Regularisation.ApprovedByUser?.Emp?.FirstName, x.Regularisation.ApprovedByUser?.Emp?.MiddleName, x.Regularisation.ApprovedByUser?.Emp?.LastName),
                    DisapprovedByUserId = x.Regularisation.DisapprovedByUserId,
                    DisapprovedByUserCode = x.Regularisation.DisapprovedByUser?.UserName,
                    DisapprovedByUserName = Helper.FullName(x.Regularisation.DisapprovedByUser?.Emp?.FirstName, x.Regularisation.DisapprovedByUser?.Emp?.MiddleName, x.Regularisation.DisapprovedByUser?.Emp?.LastName),
                    CompanyId = x.EmpTransaction.CompanyId,
                    CompanyName = x.EmpTransaction.Company?.Name,
                    DepartmentId = x.EmpTransaction.DepartmentId,
                    DepartmentName = x.EmpTransaction.Department?.Name,
                    DivisionId = x.EmpTransaction.DivisionId,
                    DivisionName = x.EmpTransaction.Division?.Name,
                    Remarks = x.Regularisation.Remarks,
                    RequestedDate = x.Regularisation.CreatedAt,
                    ProcessedDate = x.Regularisation.UpdatedAt,
                }),
                TotalCount = data.TotalCount,
                TotalPages = data.TotalPages
            });
        }

        // Post: Regularisations
        [CustomAuthorize("regularization-application")]
        [HttpPost]
        public async Task<IActionResult> Create(RequestInputModel input)
        {
            DateOnly fromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);
            DateOnly toDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);

            DateOnly todayDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly firstDay = new(todayDate.Year, todayDate.Month, 1);

            if (todayDate.Day > 10)
            {
                if (fromDate < firstDay)
                {
                    return ErrorHelper.ErrorResult("FromDate", "Cannot apply regularisation for previous months.");
                }
            }
            else
            {
                if (fromDate < firstDay.AddMonths(-1))
                {
                    return ErrorHelper.ErrorResult("FromDate", "Cannot apply regularisation for previous months.");
                }
            }

            var emp = await _context.EmpLogs.Where(x => x.EmployeeId == input.EmpId)
                            .Join(_context.EmpTransactions, el => el.Id, et => et.Id,
                            (el, et) => new
                            {
                                et.StatusId,
                                et.CompanyId
                            })
                            .FirstOrDefaultAsync();

            if (emp?.StatusId != 1)
            {
                return ErrorHelper.ErrorResult("Id", "Employee is not active.");
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Contains(emp?.CompanyId ?? 0))
                {
                    return Forbid();
                }
            }

            TimeOnly fromTime = TimeOnly.Parse(input.FromTime);
            TimeOnly toTime = TimeOnly.Parse(input.FromTime);

            if (!string.IsNullOrEmpty(input.ToDate))
            {
                toDate = DateOnlyHelper.ParseDateOrNow(input.ToDate);
            }

            if (!string.IsNullOrEmpty(input.ToTime))
            {
                toTime = TimeOnly.Parse(input.ToTime);
            }

            if (await _context.LeaveLedgers.AnyAsync(x => x.EmpId == input.EmpId && x.Taken > 0 && x.Leave_Date >= fromDate && x.Leave_Date <= toDate))
            {
                return ErrorHelper.ErrorResult("FromDate", "User has taken leave somewhere between the given dates.");
            }

            string? regularisationType = await _context.RegularisationTypes
                .Where(x => x.Id == input.RegularisationTypeId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            if (regularisationType == "in-punch-regularisation")
            {
                var attendance = await _context.Attendances.Where(x => x.TransactionDateOut == fromDate && x.EmpId == input.EmpId).FirstOrDefaultAsync();

                if (attendance is null)
                {
                    return ErrorHelper.ErrorResult("Date", "User has not Punched Out in the given date.");
                }

                if (attendance.InTime != null)
                {
                    return ErrorHelper.ErrorResult("Date", "User has already Punched In in the given date.");
                }

                Regularisation data = new()
                {
                    RegularisationTypeId = input.RegularisationTypeId,
                    EmpId = input.EmpId,
                    FromDate = fromDate,
                    ToDate = fromDate,
                    FromTime = fromTime,
                    ToTime = fromTime,
                    ContactNumber = input.ContactNumber,
                    ApprovedByUserId = User.GetUserId(),
                    Status = "approved"
                };

                try
                {
                    _context.Add(data);
                    await _context.SaveChangesAsync();

                    attendance.InTime = fromTime.ToString("HH:mm:ss");
                    attendance.InRemarks = "Regularised";
                    attendance.RegularisationId = data.Id;

                    await _context.SaveChangesAsync();

                    return Ok();
                }
                catch (Exception ex)
                {
                    _context.Remove(data);

                    attendance.InTime = null;
                    attendance.InRemarks = null;
                    attendance.RegularisationId = null;

                    await _context.SaveChangesAsync();

                    return BadRequest(ex.StackTrace);
                }
            };

            if (regularisationType == "out-punch-regularisation")
            {
                var attendance = await _context.Attendances.Where(x => x.TransactionDate == fromDate && x.EmpId == input.EmpId).FirstOrDefaultAsync();

                if (attendance is null)
                {
                    return ErrorHelper.ErrorResult("Date", "User has not Punched In in the given date.");
                }

                if (attendance.OutTime != null)
                {
                    return ErrorHelper.ErrorResult("Date", "User has already Punched In in the given date.");
                }

                Regularisation data = new()
                {
                    RegularisationTypeId = input.RegularisationTypeId,
                    EmpId = input.EmpId,
                    FromDate = fromDate,
                    ToDate = fromDate,
                    FromTime = fromTime,
                    ToTime = fromTime,
                    ContactNumber = input.ContactNumber,
                    Reason = input.Reason,
                    ApprovedByUserId = User.GetUserId(),
                    Status = "approved"
                };

                try
                {
                    _context.Add(data);
                    await _context.SaveChangesAsync();

                    attendance.OutTime = fromTime.ToString("HH:mm:ss");
                    attendance.TransactionDateOut = fromDate;
                    attendance.OutRemarks = "Regularised";
                    attendance.RegularisationId = data.Id;

                    await _context.SaveChangesAsync();

                    return Ok();
                }
                catch (Exception ex)
                {
                    _context.Remove(data);

                    attendance.OutTime = null;
                    attendance.TransactionDateOut = null;
                    attendance.OutRemarks = null;
                    attendance.RegularisationId = null;

                    await _context.SaveChangesAsync();

                    return BadRequest(ex.StackTrace);
                }
            };

            if (regularisationType == "on-duty")
            {
                if (await _context.Regularisations.AnyAsync(x => x.EmpId == input.EmpId &&
                    x.FromDate <= toDate && x.ToDate >= fromDate &&
                    x.FromTime < toTime && x.ToTime > fromTime &&
                    (x.Status == "approved" || x.Status == "pending")))
                {
                    return ErrorHelper.ErrorResult("FromDate", "Regularisation somewhere between the dates for the given time already exists.");
                }

                Regularisation data = new()
                {
                    RegularisationTypeId = input.RegularisationTypeId,
                    EmpId = input.EmpId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    FromTime = fromTime,
                    ToTime = toTime,
                    ContactNumber = input.ContactNumber,
                    Reason = input.Reason,
                    Place = input.Place,
                    ApprovedByUserId = User.GetUserId(),
                    Status = "approved"
                };

                List<Attendance> newAttendances = new();

                try
                {
                    _context.Add(data);

                    await _context.SaveChangesAsync();

                    DateOnly currentDate = fromDate;

                    do
                    {
                        var existingAttendance = await _context.Attendances.FirstOrDefaultAsync(x => x.EmpId == input.EmpId && x.TransactionDate == currentDate);

                        if (existingAttendance is not null)
                        {
                            //TimeOnly? inTime = string.IsNullOrEmpty(existingAttendance.InTime) ? null : TimeOnly.ParseExact(existingAttendance.InTime, "HH:mm:ss");
                            //TimeOnly? outTime = string.IsNullOrEmpty(existingAttendance.OutTime) ? null : TimeOnly.ParseExact(existingAttendance.OutTime, "HH:mm:ss");

                            ////if (fromTime >= inTime && fromTime < outTime && toTime > outTime)
                            ////{
                            ////    _context.Remove(data);
                            ////    await _context.SaveChangesAsync();

                            ////    return ErrorHelper.ErrorResult("ToTime", "Invalid Time. Please check attendance in date: " + existingAttendance.TransactionDate.ToString());
                            ////}

                            ////if (fromTime < inTime && toTime > inTime && toTime <= outTime) 
                            ////{
                            ////    _context.Remove(data);
                            ////    await _context.SaveChangesAsync();

                            ////    return ErrorHelper.ErrorResult("ToTime", "Invalid Time. Please check attendance in date: " + existingAttendance.TransactionDate.ToString());
                            ////}

                            existingAttendance.RegularisationId = data.Id;
                        } else
                        {
                            short? WorkHourId;

                            var roster = await _context.Rosters.Where(x => x.Date == currentDate && x.EmpId == input.EmpId).FirstOrDefaultAsync();

                            WorkHourId = roster?.WorkHourId;

                            if (roster is null)
                            {
                                var defaultWorkHour = await _context.DefaultWorkHours
                                    .Where(x => x.EmpId == input.EmpId || x.EmpId == null && x.DayId == ((short)currentDate.DayOfWeek + 1))
                                    .OrderBy(x => x.EmpId)
                                    .FirstOrDefaultAsync();

                                WorkHourId = defaultWorkHour?.WorkHourId;
                            }

                            if (WorkHourId is null)
                            {
                                _context.Remove(data);
                                await _context.SaveChangesAsync();

                                return ErrorHelper.ErrorResult("FromDate", "There is no default shift nor the empoloyee is assigned any.");
                            }

                            newAttendances.Add(new Attendance
                            {
                                EmpId = input.EmpId,
                                TransactionDate = currentDate,
                                TransactionDateOut = currentDate,
                                InTime = fromTime.ToString("HH:mm:ss"),
                                OutTime = toTime.ToString("HH:mm:ss"),
                                InRemarks = "On Duty Regularised",
                                OutRemarks = "On Duty Regularised",
                                RegularisationId = data.Id,
                                WorkHourId = WorkHourId,

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

                        currentDate = currentDate.AddDays(1);

                    } while (currentDate <= toDate);

                    _context.AddRange(newAttendances);
                    await _context.SaveChangesAsync();

                    return Ok();
                }
                catch (Exception ex)
                {
                    _context.Remove(data);
                    _context.RemoveRange(newAttendances);

                    return BadRequest(ex.StackTrace);
                }
            };

            if (regularisationType == "work-from-home")
            {
                if (await _context.Attendances.AnyAsync(x =>
                                (x.TransactionDate >= fromDate) &&
                                (x.TransactionDate <= toDate) &&
                                x.EmpId == input.EmpId)
                    )
                {
                    return ErrorHelper.ErrorResult("FromDate", "User already has attendance somewhere between the given dates.");
                }

                Regularisation data = new()
                {
                    RegularisationTypeId = input.RegularisationTypeId,
                    EmpId = input.EmpId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    FromTime = fromTime,
                    ToTime = toTime,
                    ContactNumber = input.ContactNumber,
                    Reason = input.Reason,
                    ApprovedByUserId = User.GetUserId(),
                    Status = "approved"
                };

                List<Attendance> newAttendances = new();

                try
                {
                    _context.Add(data);

                    await _context.SaveChangesAsync();

                    DateOnly currentDate = fromDate;

                    do
                    {
                        short? WorkHourId;

                        var roster = await _context.Rosters.Where(x => x.Date == currentDate && x.EmpId == input.EmpId).FirstOrDefaultAsync();

                        WorkHourId = roster?.WorkHourId;

                        if (roster is null)
                        {
                            var defaultWorkHour = await _context.DefaultWorkHours
                                .Where(x => x.EmpId == input.EmpId || x.EmpId == null && x.DayId == ((short)currentDate.DayOfWeek + 1))
                                .OrderBy(x => x.EmpId)
                                .FirstOrDefaultAsync();

                            WorkHourId = defaultWorkHour?.WorkHourId;
                        }

                        if (WorkHourId is null)
                        {
                            _context.Remove(data);
                            await _context.SaveChangesAsync();

                            return ErrorHelper.ErrorResult("FromDate", "There is no default shift nor the empoloyee is assigned any.");
                        } 

                        newAttendances.Add(new Attendance
                        {
                            EmpId = input.EmpId,
                            TransactionDate = currentDate,
                            TransactionDateOut = currentDate,
                            InTime = fromTime.ToString("HH:mm:ss"),
                            OutTime = toTime.ToString("HH:mm:ss"),
                            InRemarks = "Work From Home Regularised",
                            OutRemarks = "Work From Home Regularised",
                            RegularisationId = data.Id,
                            WorkHourId = WorkHourId,

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

                        currentDate = currentDate.AddDays(1);

                    } while (currentDate <= toDate);

                    _context.AddRange(newAttendances);
                    await _context.SaveChangesAsync();

                    return Ok();
                }
                catch (Exception ex)
                {
                    _context.Remove(data);
                    _context.RemoveRange(newAttendances);

                    return BadRequest(ex.StackTrace);
                }
            }

            if (regularisationType == "gate-pass")
            {
                //if (!await _context.Attendances.AnyAsync(x => x.EmpId == input.EmpId && x.TransactionDate == fromDate && !string.IsNullOrEmpty(x.InTime) && !string.IsNullOrEmpty(x.OutTime)))
                //{
                //    return ErrorHelper.ErrorResult("FromDate", "User must have punch in and punch out in the applied date.");
                //}

                if (await _context.Regularisations.AnyAsync(x => x.EmpId == input.EmpId 
                    && x.FromDate == fromDate 
                    && x.FromTime < fromTime.AddHours((double)input.GatePassHour) && x.ToTime > fromTime
                    && (x.Status == "approved" || x.Status == "pending"))) 
                {
                    return ErrorHelper.ErrorResult("FromDate", "Regularisation already exists in the given date and time.");
                }

                var firstDayOfMonth = new DateOnly(fromDate.Year, fromDate.Month , 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var regularisations = await _context.Regularisations
                    .Where(x => x.RegularisationTypeId == input.RegularisationTypeId && 
                            x.EmpId == input.EmpId && (x.Status == "approved" || x.Status == "pending") && 
                            x.FromDate >= firstDayOfMonth && 
                            x.ToDate <= lastDayOfMonth)
                    .ToListAsync();

                if (regularisations.Count > 0)
                {
                    var regularisation = regularisations.FirstOrDefault(x => x.FromDate == fromDate);

                    if (regularisation is not null && regularisation.FromTime == fromTime)
                    {
                        return ErrorHelper.ErrorResult("Id", "Gate Pass at this time has already been applied.");
                    }
                }

                if (regularisations.Count == 2)
                {
                    return ErrorHelper.ErrorResult("FromDate", "Gate pass already applied for 2 hours in this month.");
                }

                if (regularisations.Count == 1)
                {
                    if (input.GatePassHour > 1)
                    {
                        return ErrorHelper.ErrorResult("GatePassHour", "Gate Pass already applied for 1 hour this month.");
                    }

                    var regularisation = regularisations.FirstOrDefault();

                    TimeSpan totalHours = (regularisation.ToTime ?? TimeOnly.MaxValue) - (regularisation.FromTime ?? TimeOnly.MinValue);

                    if (totalHours.Hours > 1)
                    {
                        return ErrorHelper.ErrorResult("FromDate", "Gate pass already applied for 2 hours in this month,");
                    }
                }

                var existingAttendance = await _context.Attendances
                    .Include(x => x.WorkHour)
                    .FirstOrDefaultAsync(x => x.EmpId == input.EmpId && x.TransactionDate == fromDate);

                TimeOnly? startTime = null;

                if (existingAttendance is not null)
                {
                    if (!string.IsNullOrEmpty(existingAttendance.OutTime))
                    {
                         TimeOnly outTime = TimeOnly.Parse(existingAttendance.OutTime);

                        if (fromTime < outTime && fromTime.AddHours(input.GatePassHour ?? 1) > outTime)
                        {
                            fromTime = outTime;

                            try
                            {
                                _context.Add(new Regularisation
                                {
                                    RegularisationTypeId = input.RegularisationTypeId,
                                    EmpId = input.EmpId,
                                    GatePassTypeId = input.GatePassTypeId,
                                    FromDate = fromDate,
                                    ToDate = fromDate,
                                    FromTime = fromTime,
                                    ToTime = fromTime.AddHours(input.GatePassHour ?? 1),
                                    ContactNumber = input.ContactNumber,
                                    Reason = input.Reason,
                                    ApprovedByUserId = User.GetUserId(),
                                    Status = "approved"
                                });
                                await _context.SaveChangesAsync();

                                return Ok();
                            }
                            catch (Exception ex)
                            {
                                return BadRequest(ex.StackTrace);
                            }
                        }
                    }

                    startTime = TimeOnly.Parse(existingAttendance.WorkHour.StartTime);
                } else
                {
                    var roster = await _context.Rosters
                        .Include(x => x.WorkHour)
                        .Where(x => x.Date == fromDate && x.EmpId == input.EmpId)
                        .FirstOrDefaultAsync();

                    if (roster is not null)
                    {
                        startTime = TimeOnly.Parse(roster.WorkHour.StartTime);
                    }
                    else
                    {
                        var defaultWorkHour = await _context.DefaultWorkHours
                            .Include(x => x.WorkHour)
                            .Where(x => x.EmpId == input.EmpId || x.EmpId == null && x.DayId == ((short)fromDate.DayOfWeek + 1))
                            .OrderBy(x => x.EmpId)
                            .FirstOrDefaultAsync();

                        if (defaultWorkHour is null)
                        {
                            return ErrorHelper.ErrorResult("FromDate", "No shift assigned to the employee.");
                        }

                        startTime = TimeOnly.Parse(defaultWorkHour.WorkHour.StartTime);
                    }
                }

                if (startTime > fromTime)
                {
                    return ErrorHelper.ErrorResult("FromTime", $"Time cannot be less than the shift start time, {startTime}.");
                }

                Regularisation data = new()
                {
                    RegularisationTypeId = input.RegularisationTypeId,
                    EmpId = input.EmpId,
                    GatePassTypeId = input.GatePassTypeId,
                    FromDate = fromDate,
                    ToDate = fromDate,
                    FromTime = fromTime,
                    ToTime = fromTime.AddHours(input.GatePassHour ?? 1),
                    ContactNumber = input.ContactNumber,
                    Reason = input.Reason,
                    ApprovedByUserId = User.GetUserId(),
                    Status = "approved"
                };

                try
                {
                    _context.Add(data);
                    await _context.SaveChangesAsync();

                    return Ok();
                } catch (Exception ex)
                {
                    return BadRequest(ex.StackTrace);
                }
            }

            return Ok();
        }

        [CustomAuthorize("regularization-cancellation")]
        [HttpPost("Cancel/{id}")]
        public async Task<IActionResult> Cancel(int id, CancelInputModel input)
        {
            var regularisation = await _context.Regularisations
                .Include(x => x.RegularisationType)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (regularisation == null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id.");
            }

            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly firstDay = new(currentDate.Year, currentDate.Month, 1);

            if (currentDate.Day > 10)
            {
                if (regularisation.FromDate < firstDay)
                {
                    return ErrorHelper.ErrorResult("Id", "Cannot cancel regularisation applied for previous months.");
                }
            } else
            {
                if (regularisation.FromDate < firstDay.AddMonths(-1))
                {
                    return ErrorHelper.ErrorResult("Id", "Cannot cancel regularisation applied for previous months.");
                }
            }

            var emp = await _context.EmpLogs.Where(x => x.EmployeeId == regularisation.EmpId)
                .Join(_context.EmpTransactions, el => el.Id, et => et.Id,
                (el, et) => new
                {
                    et.StatusId,
                    et.CompanyId
                })
                .FirstOrDefaultAsync();

            if (emp?.StatusId != 1)
            {
                return ErrorHelper.ErrorResult("Id", "Employee is not active.");
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Contains(emp?.CompanyId ?? 0))
                {
                    return Forbid();
                }
            }

            if (regularisation.RegularisationType.Name == "in-punch-regularisation")
            {
                var attendance = await _context.Attendances.Where(x => x.RegularisationId == regularisation.Id).FirstOrDefaultAsync();

                attendance.InTime = null;
                attendance.InRemarks = null;
                attendance.RegularisationId = null;

                regularisation.Status = "cancelled";
                regularisation.DisapprovedByUserId = User.GetUserId();
                regularisation.CancellationRemarks = input.CancellationRemarks;
                regularisation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok();
            }

            if (regularisation.RegularisationType.Name == "out-punch-regularisation")
            {
                var attendance = await _context.Attendances.Where(x => x.RegularisationId == regularisation.Id).FirstOrDefaultAsync();

                attendance.TransactionDateOut = null;
                attendance.OutTime = null;
                attendance.OutRemarks = null;
                attendance.RegularisationId = null;

                regularisation.Status = "cancelled";
                regularisation.DisapprovedByUserId = User.GetUserId();
                regularisation.CancellationRemarks = input.CancellationRemarks;
                regularisation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok();
            }

            if (regularisation.RegularisationType.Name == "on-duty")
            {
                var attendances = await _context.Attendances.Where(x => x.RegularisationId == regularisation.Id).ToListAsync();

                foreach(var attendance in attendances)
                {
                    if (TimeOnly.Parse(attendance.InTime) == regularisation.FromTime && TimeOnly.Parse(attendance.OutTime) == regularisation.ToTime)
                    {
                        _context.Remove(attendance);

                        regularisation.Status = "cancelled";
                        regularisation.DisapprovedByUserId = User.GetUserId();
                        regularisation.CancellationRemarks = input.CancellationRemarks;
                        regularisation.UpdatedAt = DateTime.UtcNow;
                    } else
                    {
                        attendance.RegularisationId = null;

                        regularisation.Status = "cancelled";
                        regularisation.DisapprovedByUserId = User.GetUserId();
                        regularisation.CancellationRemarks = input.CancellationRemarks;
                        regularisation.UpdatedAt = DateTime.UtcNow;
                    }
                }

                regularisation.Status = "cancelled";
                regularisation.DisapprovedByUserId = User.GetUserId();
                regularisation.CancellationRemarks = input.CancellationRemarks;
                regularisation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok();
            }

            if (regularisation.RegularisationType.Name == "work-from-home")
            {
                var attendances = await _context.Attendances.Where(x => x.RegularisationId == regularisation.Id).ToListAsync();

                _context.RemoveRange(attendances);

                regularisation.Status = "cancelled";
                regularisation.DisapprovedByUserId = User.GetUserId();
                regularisation.CancellationRemarks = input.CancellationRemarks;
                regularisation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok();
            }

            if (regularisation.RegularisationType.Name == "gate-pass")
            {
                regularisation.Status = "cancelled";
                regularisation.DisapprovedByUserId = User.GetUserId();
                regularisation.CancellationRemarks = input.CancellationRemarks;
                regularisation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok();
            }

            return Ok();
        }

        public class RegularisationHistoryDTO
        {
            public Regularisation Regularisation { get; set; }
            public EmpTransaction EmpTransaction { get; set; }
        }

        public class RequestInputModel
        {
            public int RegularisationTypeId { get; set; }
            public int EmpId { get; set; }
            public int? GatePassTypeId { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string FromTime { get; set; }
            public string ToTime { get; set; }
            public string ContactNumber { get; set; }
            public string Place { get; set; }
            public string Reason { get; set; }
            public int? GatePassHour { get; set; }
        }

        public class CancelInputModel
        {
            public string CancellationRemarks { get; set; }
        }

        public class RequestInputModelValidator : AbstractValidator<RequestInputModel>
        {
            private readonly DataContext _context;

            public RequestInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.RegularisationTypeId)
                    .NotEmpty()
                    .IdMustExist(_context.RegularisationTypes.AsQueryable());

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x).Custom((request, context) =>
                {
                    var regularisationType = _context.RegularisationTypes
                        .FirstOrDefault(r => r.Id == request.RegularisationTypeId)?.Name ?? string.Empty;

                    if (regularisationType == "gate-pass")
                    {
                        if (request.GatePassTypeId == null)
                        {
                            context.AddFailure(nameof(request.GatePassTypeId), "GatePassTypeId is required for gate-pass.");
                        }
                        if (request.GatePassHour == null || (request.GatePassHour != 1 && request.GatePassHour != 2))
                        {
                            context.AddFailure(nameof(request.GatePassHour), "GatePassHour must be 1 or 2 for gate-pass.");
                        }
                    }

                    if (regularisationType == "on-duty" || regularisationType == "work-from-home")
                    {
                        if (string.IsNullOrEmpty(request.ToDate))
                        {
                            context.AddFailure(nameof(request.ToDate), "ToDate is required for on-duty or work-from-home.");
                        }
                        if (string.IsNullOrEmpty(request.ToTime))
                        {
                            context.AddFailure(nameof(request.ToTime), "ToTime is required for on-duty or work-from-home.");
                        }
                    }
                });

                RuleFor(x => x.FromDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.ToDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.FromDate, "From Date")
                    .Unless(x => string.IsNullOrEmpty(x.ToDate));

                RuleFor(x => x.FromTime)
                    .NotEmpty()
                    .MustBeTime();

                RuleFor(x => x.ToTime)
                    .NotEmpty()
                    .MustBeTime()
                    .MustBeTimeAfter(x => x.FromTime, "From Time")
                    .Unless(x => string.IsNullOrEmpty(x.ToTime));

                RuleFor(x => x.Reason)
                    .NotEmpty();

                RuleFor(x => x.ContactNumber)
                    .MustBeDigits(10)
                    .Unless(x => string.IsNullOrEmpty(x.ContactNumber));
            }
        }
    }
}

