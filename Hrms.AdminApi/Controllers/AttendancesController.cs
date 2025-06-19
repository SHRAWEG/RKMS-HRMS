using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Hrms.Common.Models;
using NepaliCalendarBS;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using Org.BouncyCastle.Asn1.Mozilla;
using System.Data;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class AttendancesController : Controller
    {
        private readonly DataContext _context;

        public AttendancesController(DataContext context)
        {
            _context = context;
        }

        [CustomAuthorize("attendance-report")]
        [HttpGet()]
        public async Task<IActionResult> Get(int? empId, int? companyId, int? departmentId, int? designationId, int? statusId, int? year, int? month)
        {
            DateOnly date = DateOnly.FromDateTime(DateTime.Today);

            var firstDayOfMonth = new DateOnly(year ?? date.Year, month ?? date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            int days = DateTime.DaysInMonth(firstDayOfMonth.Year, firstDayOfMonth.Month);

            if (firstDayOfMonth > date)
            {
                return Ok(new
                {
                    Data = Array.Empty<int>(),
                    Summary = Array.Empty<int>()
                });
            }

            var empTransactionQuery = _context.EmpTransactions.Include(x => x.Status).AsQueryable();
            var empDetailQuery = _context.EmpDetails.AsQueryable();

            var company = await _context.Companies.Where(x => x.Id == companyId).FirstOrDefaultAsync();
            var department = await _context.Departments.Where(x => x.Id == departmentId).FirstOrDefaultAsync();
            var designation = await _context.Designations.Where(x => x.Id == designationId).FirstOrDefaultAsync();
            var status = await _context.Statuses.Where(x => x.Id == statusId).FirstOrDefaultAsync();

            if (empId is not null)
            {
                empDetailQuery = empDetailQuery.Where(x => x.Id == empId);
            }

            if (companyId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.CompanyId == companyId);
            }

            if (departmentId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.DepartmentId == departmentId);
            }

            if (designationId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.DesignationId == designationId);
            }

            if (statusId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.StatusId == statusId);
            } else
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.StatusId == 1);
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                empTransactionQuery = empTransactionQuery.Where(x => companyIds.Contains(x.CompanyId ?? 0));
            }

            var empDetails = await _context.EmpLogs
                .Join(empTransactionQuery,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        el.EmployeeId,
                        et.CompanyId,
                        et.DepartmentId,
                        et.StatusId,
                        StatusName = et.Status != null ? et.Status.Name : null,
                    })
                .Join(empDetailQuery,
                    elt => elt.EmployeeId,
                    ed => ed.Id,
                    (elt, ed) => new
                    {
                        Id = ed.Id,
                        EmpCode = ed.CardId,
                        Name = string.Concat(ed.FirstName, " ", !string.IsNullOrEmpty(ed.MiddleName) ? ed.MiddleName + " " : "", ed.LastName),
                        CompanyId = elt.CompanyId,
                        DepartmentId = elt.DepartmentId,
                        StatusId = elt.StatusId,
                        StatusName = elt.StatusName
                    }
                ).ToListAsync();

            List<AttendanceData> data = new();

            HSSFWorkbook workbook = new HSSFWorkbook();

            HSSFFont headerFont = (HSSFFont)workbook.CreateFont();
            HSSFFont normalFont = (HSSFFont)workbook.CreateFont();

            headerFont.IsBold = true;
            headerFont.FontHeightInPoints = 8;
            headerFont.FontName = "Tahoma";

            normalFont.FontHeightInPoints = 8;
            normalFont.FontName = "Tahoma";

            HSSFCellStyle headerCellStyle = NpoiHelper.NormalCellStyle(workbook, headerFont);
            HSSFCellStyle borderedCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);

            HSSFCellStyle offCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //offCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;
            //offCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle presentCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //presentCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;
            //presentCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle offPresentCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //offPresentCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SeaGreen.Index;
            //offPresentCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle absentCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //absentCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Red.Index;
            //absentCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle halfDayCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //halfDayCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightOrange.Index;
            //halfDayCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle leaveCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //leaveCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Lavender.Index;
            //leaveCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle regularisationCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //regularisationCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightBlue.Index;
            //regularisationCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle greyCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //greyCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            //greyCellStyle.FillPattern = FillPattern.SolidForeground;

            ISheet Sheet = workbook.CreateSheet("Attendance Report");

            Sheet.CreateRow(0);
            Sheet.CreateRow(1);

            IRow headerRow = Sheet.CreateRow(2);

            Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(2, 2, 0, 5));
            NpoiHelper.CreateCell(headerRow, 0, $"Attendance of {firstDayOfMonth:MMMM}, {firstDayOfMonth.Year}", headerCellStyle);

            if (company is not null)
            {
                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(2, 2, 6, 12));

                NpoiHelper.CreateCell(headerRow, 6, $"Company: {company.Name}", headerCellStyle);
            }

            if (department is not null)
            {
                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(2, 2, 13, 19));
                NpoiHelper.CreateCell(headerRow, 13, $"Department: {department.Name}", headerCellStyle);
            }

            if (designation is not null)
            {
                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(2, 2, 20, 26));
                NpoiHelper.CreateCell(headerRow, 13, $"Department: {designation.Name}", headerCellStyle);
            }

            Sheet.CreateRow(3);

            int rowIndex = 4;

            if (empDetails.Count == 0)
            {
                return BadRequest("No employee for the given filter.");
            }

            int serialNumber = 1;

            foreach (var emp in empDetails)
            {
                DateOnly currentDate = firstDayOfMonth;
                double totalWorkingDays = 0;
                double totalPresentDays = 0;
                int lateInCount = 1;
                double totalOnDuty = 0;

                IRow serialNumberRow = Sheet.CreateRow(rowIndex);
                NpoiHelper.CreateCell(serialNumberRow, 0, serialNumber.ToString(), headerCellStyle);

                serialNumber++;

                rowIndex++;

                IRow nameRow = Sheet.CreateRow(rowIndex);
                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, 1, 6));
                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, 7, 10));
                NpoiHelper.CreateCell(nameRow, 0, emp.EmpCode, headerCellStyle);
                NpoiHelper.CreateCell(nameRow, 1, emp.Name, headerCellStyle);
                NpoiHelper.CreateCell(nameRow, 7, $"Status: {emp.StatusName}", headerCellStyle);

                rowIndex++;

                var branchId = await _context.EmpLogs
                    .Where(x => x.EmployeeId == emp.Id)
                    .Join(_context.EmpTransactions,
                        el => el.Id,
                        et => et.Id,
                        (el, et) => et.BranchId
                    ).FirstOrDefaultAsync();

                var attendances = await _context.Attendances
                    .Include(x => x.Regularisation)
                    .ThenInclude(x => x.RegularisationType)
                    .Where(x => x.EmpId == emp.Id &&
                                (x.TransactionDate >= firstDayOfMonth && x.TransactionDate <= lastDayOfMonth))
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

                var calendarId = await _context.EmpCalendars.Where(x => x.EmpId == emp.Id).Select(x => x.CalendarId).FirstOrDefaultAsync();

                var holidays = await _context.HolidayCalendars
                    .Include(x => x.Holiday)
                    .Where(x => x.Holiday.Date >= firstDayOfMonth && x.Holiday.Date <= lastDayOfMonth && x.CalendarId == calendarId)
                    .Select(x => new
                    {
                        Name = x.Holiday.Name,
                        Date = x.Holiday.Date,
                    })
                    .ToListAsync();

                var leaves = await _context.LeaveLedgers
                    .Where(x => x.Leave_Date >= firstDayOfMonth && x.Leave_Date <= lastDayOfMonth && x.EmpId == emp.Id)
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
                    .Where(x => x.StartDate <= lastDayOfMonth && x.EndDate >= firstDayOfMonth && x.EmpId == emp.Id && x.Status == "pending")
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
                    .Include(x => x.RegularisationType)
                    .Where(x => x.FromDate <= lastDayOfMonth && x.ToDate >= firstDayOfMonth && x.EmpId == emp.Id && x.Status == "pending")
                    .Select(x => new
                    {
                        x.RegularisationType,
                        x.FromTime,
                        x.ToTime,
                        x.FromDate,
                        x.ToDate
                    })
                    .ToListAsync();

                List<DateOnly> weekends = await Helper.GetWeekends(firstDayOfMonth, lastDayOfMonth, emp.Id, _context);

                int dateIndex = rowIndex;
                rowIndex++;
                int inTimeIndex = rowIndex;
                rowIndex++;
                int outTimeIndex = rowIndex;
                rowIndex++;
                int durationIndex = rowIndex;
                rowIndex++;
                int overTimeIndex = rowIndex;
                rowIndex++;
                int onDutyIndex = rowIndex;
                rowIndex++;
                int gatePassIndex = rowIndex;
                rowIndex++;
                int totalIndex = rowIndex;
                rowIndex++;

                IRow dateRow = Sheet.CreateRow(dateIndex);
                IRow inTimeRow = Sheet.CreateRow(inTimeIndex);
                IRow outTimeRow = Sheet.CreateRow(outTimeIndex);
                IRow durationRow = Sheet.CreateRow(durationIndex);
                IRow overTimeRow = Sheet.CreateRow(overTimeIndex);
                IRow onDutyRow = Sheet.CreateRow(onDutyIndex);
                IRow gatePassRow = Sheet.CreateRow(gatePassIndex);
                IRow totalDurationRow = Sheet.CreateRow(totalIndex);

                NpoiHelper.CreateCell(dateRow, 0, "Date", headerCellStyle);
                NpoiHelper.CreateCell(inTimeRow, 0, "In-Time", headerCellStyle);
                NpoiHelper.CreateCell(outTimeRow, 0, "Out-Time", headerCellStyle);
                NpoiHelper.CreateCell(durationRow, 0, "Duration", headerCellStyle);
                NpoiHelper.CreateCell(overTimeRow, 0, "OT-Hours", headerCellStyle);
                NpoiHelper.CreateCell(onDutyRow, 0, "OD-Hours", headerCellStyle);
                NpoiHelper.CreateCell(gatePassRow, 0, "GatePass", headerCellStyle);
                NpoiHelper.CreateCell(totalDurationRow, 0, "Total Duration", headerCellStyle);

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
                    TimeSpan? odHour = new();
                    TimeSpan? gatePassHour = new();
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
                        .Include(x => x.RegularisationType)
                        .Where(x => x.Status == "approved" && x.RegularisationType.Name == "gate-pass" && x.EmpId == emp.Id && x.FromDate == currentDate)
                        .ToListAsync();

                    var onDuties = await _context.Regularisations
                        .Include(x => x.RegularisationType) 
                        .Where(x => x.Status == "approved" && (x.RegularisationType.Name == "on-duty" || x.RegularisationType.Name == "work-from-home") && x.EmpId == emp.Id && x.FromDate <= currentDate && x.ToDate >= currentDate)
                        .ToListAsync();

                    if (gatePasses.Count > 0)
                    {
                        foreach(var gatePass in gatePasses)
                        {
                            gatePassHour += gatePass.ToTime - gatePass.FromTime;
                        }
                    }

                    totalWorkingDays++;

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
                        dailyHour ??= new();

                        if (onDuties.Count > 0)
                        {
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

                        totalHour = dailyHour + calculatedOdHour + calculatedGatePassHour;

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

                        var leave = leaves.Where(x => x.Leave_Date == currentDate).FirstOrDefault();

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

                            NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), leaveCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, currentDate.Day, ((leave.HLeaveType == 1 || leave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(leave.LeaveName) + "-A", leaveCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, currentDate.Day, ((leave.HLeaveType == 1 || leave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(leave.LeaveName) + "-A", leaveCellStyle);
                            NpoiHelper.CreateCell(durationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, currentDate.Day, overTime?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, currentDate.Day, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), leaveCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, totalHour?.ToString(@"hh\:mm"), leaveCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        var pendingLeave = pendingLeaves.Where(x => x.StartDate <= currentDate && x.EndDate >= currentDate).FirstOrDefault();

                        if (pendingLeave != null)
                        {
                            NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), leaveCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, currentDate.Day, ((pendingLeave.HLeaveType == 1 || pendingLeave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(pendingLeave.LeaveName) + "-P", leaveCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, currentDate.Day, ((pendingLeave.HLeaveType == 1 || pendingLeave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(pendingLeave.LeaveName) + "-P", leaveCellStyle);
                            NpoiHelper.CreateCell(durationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, currentDate.Day, overTime?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, currentDate.Day, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), leaveCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, totalHour?.ToString(@"hh\:mm"), leaveCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        if (weekends.Contains(currentDate))
                        {
                            totalWorkingDays--;

                            NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), offPresentCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, currentDate.Day, inTime?.ToString("HH:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, currentDate.Day, outTime?.ToString("HH:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(durationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, currentDate.Day, overTime?.ToString(@"hh\:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, currentDate.Day, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), offPresentCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, totalHour?.ToString(@"hh\:mm"), offPresentCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        var holiday = holidays.Where(x => x.Date == currentDate).FirstOrDefault();

                        if (holiday != null)
                        {
                            totalWorkingDays--;

                            NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), offPresentCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, currentDate.Day, inTime?.ToString("HH:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, currentDate.Day, outTime?.ToString("HH:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(durationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, currentDate.Day, overTime?.ToString(@"hh\:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, currentDate.Day, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), offPresentCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, totalHour?.ToString(@"hh\:mm"), offPresentCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        if (attendance.Regularisation is not null && attendance.Regularisation.RegularisationType.Name != "gate-pass")
                        {
                            if (attendance.Regularisation.RegularisationType.Name == "in-punch-regularistaion")
                            {
                                NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), regularisationCellStyle);
                                NpoiHelper.CreateCell(inTimeRow, currentDate.Day, inTime?.ToString("HH:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(outTimeRow, currentDate.Day, outTime?.ToString("HH:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(durationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(overTimeRow, currentDate.Day, overTime?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(onDutyRow, currentDate.Day, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), regularisationCellStyle);

                                currentDate = currentDate.AddDays(1);

                                continue;
                            }

                            if (attendance.Regularisation.RegularisationType.Name == "out-punch-regularistaion")
                            {
                                NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), regularisationCellStyle);
                                NpoiHelper.CreateCell(inTimeRow, currentDate.Day, inTime?.ToString("HH:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(outTimeRow, currentDate.Day, outTime?.ToString("HH:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(durationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(overTimeRow, currentDate.Day, overTime?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(onDutyRow, currentDate.Day, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), regularisationCellStyle);

                                currentDate = currentDate.AddDays(1);

                                continue;
                            }

                            if (attendance.Regularisation.RegularisationType.Name == "work-from-home")
                            {
                                NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), regularisationCellStyle);
                                NpoiHelper.CreateCell(inTimeRow, currentDate.Day, Helper.GetAbbreviation(attendance.Regularisation.RegularisationType.DisplayName) + "-A", regularisationCellStyle);
                                NpoiHelper.CreateCell(outTimeRow, currentDate.Day, Helper.GetAbbreviation(attendance.Regularisation.RegularisationType.DisplayName) + "-A", regularisationCellStyle);
                                NpoiHelper.CreateCell(durationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(overTimeRow, currentDate.Day, overTime?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(onDutyRow, currentDate.Day, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), regularisationCellStyle);
                                NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, totalHour?.ToString(@"hh\:mm"), regularisationCellStyle);

                                currentDate = currentDate.AddDays(1);

                                continue;
                            }

                            if (attendance.Regularisation.RegularisationType.Name == "on-duty")
                            {
                                NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), regularisationCellStyle);
                                NpoiHelper.CreateCell(inTimeRow, currentDate.Day, Helper.GetAbbreviation(attendance.Regularisation.RegularisationType.DisplayName) + "-A", regularisationCellStyle);
                                NpoiHelper.CreateCell(outTimeRow, currentDate.Day, Helper.GetAbbreviation(attendance.Regularisation.RegularisationType.DisplayName) + "-A", regularisationCellStyle);
                                NpoiHelper.CreateCell(durationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(overTimeRow, currentDate.Day, overTime?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(onDutyRow, currentDate.Day, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), regularisationCellStyle);
                                NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, totalHour?.ToString(@"hh\:mm"), regularisationCellStyle);

                                currentDate = currentDate.AddDays(1);

                                continue;
                            }
                        }

                        if (attendanceType == "full-day")
                        {
                            NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), presentCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, currentDate.Day, inTime?.ToString("HH:mm"), presentCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, currentDate.Day, outTime?.ToString("HH:mm"), presentCellStyle);
                            NpoiHelper.CreateCell(durationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), presentCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, currentDate.Day, overTime?.ToString(@"hh\:mm"), presentCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, currentDate.Day, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), presentCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), presentCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, totalHour?.ToString(@"hh\:mm"), presentCellStyle);

                        }
                        else if (attendanceType == "half-day")
                        {
                            NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), halfDayCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, currentDate.Day, inTime?.ToString("HH:mm"), halfDayCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, currentDate.Day, outTime?.ToString("HH:mm"), halfDayCellStyle);
                            NpoiHelper.CreateCell(durationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), halfDayCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, currentDate.Day, overTime?.ToString(@"hh\:mm"), halfDayCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, currentDate.Day, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), halfDayCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), halfDayCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, totalHour?.ToString(@"hh\:mm"), halfDayCellStyle);
                        }
                        else
                        {
                            NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), absentCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, currentDate.Day, inTime?.ToString("HH:mm"), absentCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, currentDate.Day, outTime?.ToString("HH:mm"), absentCellStyle);
                            NpoiHelper.CreateCell(durationRow, currentDate.Day, dailyHour?.ToString(@"hh\:mm"), absentCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, currentDate.Day, overTime?.ToString(@"hh\:mm"), absentCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, currentDate.Day, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), absentCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), absentCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, totalHour?.ToString(@"hh\:mm"), absentCellStyle);
                        }

                        currentDate = currentDate.AddDays(1);
                    } else
                    {
                        var leave = leaves.Where(x => x.Leave_Date == currentDate).FirstOrDefault();

                        if (leave != null)
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
                            
                            NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), leaveCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, currentDate.Day, ((leave.HLeaveType == 1 || leave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(leave.LeaveName) + "-A", leaveCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, currentDate.Day, ((leave.HLeaveType == 1 || leave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(leave.LeaveName) + "-A", leaveCellStyle);
                            NpoiHelper.CreateCell(durationRow, currentDate.Day, "", leaveCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, currentDate.Day, "", leaveCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, currentDate.Day, "", leaveCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, "", leaveCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        var pendingLeave = pendingLeaves.Where(x => x.StartDate <= currentDate && x.EndDate >= currentDate).FirstOrDefault();

                        if (pendingLeave != null)
                        {
                            NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), leaveCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, currentDate.Day, ((pendingLeave.HLeaveType == 1 || pendingLeave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(pendingLeave.LeaveName) + "-P", leaveCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, currentDate.Day, ((pendingLeave.HLeaveType == 1 || pendingLeave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(pendingLeave.LeaveName) + "-P", leaveCellStyle);
                            NpoiHelper.CreateCell(durationRow, currentDate.Day, "", leaveCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, currentDate.Day, "", leaveCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, currentDate.Day, "", leaveCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, "", leaveCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        var pendingRegularisation = pendingRegularisations.Where(x => x.FromDate <= currentDate && x.ToDate >= currentDate).FirstOrDefault();

                        if (pendingRegularisation != null)
                        {
                            if (attendance is null && (pendingRegularisation.RegularisationType.Name == "on-duty" || pendingRegularisation.RegularisationType.Name == "work-from-home"))
                            {
                                TimeSpan? totalHours = pendingRegularisation.FromTime - pendingRegularisation.ToTime;

                                NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), regularisationCellStyle);
                                NpoiHelper.CreateCell(inTimeRow, currentDate.Day, Helper.GetAbbreviation(pendingRegularisation.RegularisationType.DisplayName) + "-P", regularisationCellStyle);
                                NpoiHelper.CreateCell(outTimeRow, currentDate.Day, Helper.GetAbbreviation(pendingRegularisation.RegularisationType.DisplayName) + "-P", regularisationCellStyle);
                                NpoiHelper.CreateCell(durationRow, currentDate.Day, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(overTimeRow, currentDate.Day, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(onDutyRow, currentDate.Day, totalHours?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, totalHours?.ToString(@"hh\:mm"), regularisationCellStyle);

                                currentDate = currentDate.AddDays(1);

                                continue;
                            }

                            if (pendingRegularisation.RegularisationType.Name == "in-punch-regularisation" || pendingRegularisation.RegularisationType.Name == "out-punch-regularisation")
                            {
                                NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), regularisationCellStyle);
                                NpoiHelper.CreateCell(inTimeRow, currentDate.Day, Helper.GetAbbreviation(pendingRegularisation.RegularisationType.DisplayName) + "-P", regularisationCellStyle);
                                NpoiHelper.CreateCell(outTimeRow, currentDate.Day, Helper.GetAbbreviation(pendingRegularisation.RegularisationType.DisplayName) + "-P", regularisationCellStyle);
                                NpoiHelper.CreateCell(durationRow, currentDate.Day, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(overTimeRow, currentDate.Day, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(onDutyRow, currentDate.Day, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, "", regularisationCellStyle);

                                currentDate = currentDate.AddDays(1);

                                continue;
                            }
                        }

                        if (weekends.Contains(currentDate))
                        {
                            totalWorkingDays--;

                            NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), offCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, currentDate.Day, "W-OFF", offCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, currentDate.Day, "W-OFF", offCellStyle);
                            NpoiHelper.CreateCell(durationRow, currentDate.Day, "", offCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, currentDate.Day, "", offCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, currentDate.Day, "", offCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), offCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, "", offCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        var holiday = holidays.Where(x => x.Date == currentDate).FirstOrDefault();

                        if (holiday != null)
                        {
                            totalWorkingDays--;

                            NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), offCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, currentDate.Day, "H", offCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, currentDate.Day, "H", offCellStyle);
                            NpoiHelper.CreateCell(durationRow, currentDate.Day, "", offCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, currentDate.Day, "", offCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, currentDate.Day, "", offCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), offCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, "", offCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        if (currentDate > DateOnly.FromDateTime(DateTime.Now))
                        {
                            NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), borderedCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, currentDate.Day, "", borderedCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, currentDate.Day, "", borderedCellStyle);
                            NpoiHelper.CreateCell(durationRow, currentDate.Day, "", borderedCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, currentDate.Day, "", borderedCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, currentDate.Day, "", borderedCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), borderedCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, "", borderedCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        NpoiHelper.CreateCell(dateRow, currentDate.Day, currentDate.ToString("d (ddd)"), absentCellStyle);
                        NpoiHelper.CreateCell(inTimeRow, currentDate.Day, "", absentCellStyle);
                        NpoiHelper.CreateCell(outTimeRow, currentDate.Day, "", absentCellStyle);
                        NpoiHelper.CreateCell(durationRow, currentDate.Day, "", absentCellStyle);
                        NpoiHelper.CreateCell(overTimeRow, currentDate.Day, "", absentCellStyle);
                        NpoiHelper.CreateCell(onDutyRow, currentDate.Day, "", absentCellStyle);
                        NpoiHelper.CreateCell(gatePassRow, currentDate.Day, gatePassHour?.ToString(@"hh\:mm"), absentCellStyle);
                        NpoiHelper.CreateCell(totalDurationRow, currentDate.Day, "", absentCellStyle);

                        currentDate = currentDate.AddDays(1);
                    }         
                } while (currentDate <= lastDayOfMonth);

                // Total Payable Days
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

                    if (additionalDays > offDays)
                    {
                        additionalDays = offDays;
                    }

                    payableDays = payingDays + additionalDays;
                };

                // Total Working Days, Total Present Days and Total Payable Days
                NpoiHelper.CreateCell(nameRow, 12, "T.W.D", borderedCellStyle);
                NpoiHelper.CreateCell(nameRow, 13, totalWorkingDays.ToString(), borderedCellStyle);
                NpoiHelper.CreateCell(nameRow, 14, "P.D", borderedCellStyle);
                NpoiHelper.CreateCell(nameRow, 15, totalPresentDays.ToString(), borderedCellStyle);

                int leaveIndex = 16;

                var leaveSummary = await (from leave in _context.Leaves
                                          join leaveLedger in _context.LeaveLedgers.Where(x => x.EmpId == emp.Id && x.Leave_Date >= firstDayOfMonth && x.Leave_Date <= lastDayOfMonth)
                                          .GroupBy(x => x.LeaveId)
                                          .Select(x => new
                                          {
                                              LeaveId = x.Key,
                                              TotalTaken = x.Sum(x => x.Taken),
                                          }) on leave.Id equals leaveLedger.LeaveId into leaveLedgers
                                          from leaveLedger in leaveLedgers.DefaultIfEmpty()
                                          select new
                                          {
                                              LeaveId = leave.Id,
                                              Abbreviation = leave.Abbreviation,
                                              Total = leaveLedger != null ? leaveLedger.TotalTaken ?? 0 : 0,
                                          })
                       .ToListAsync();

                foreach (var leave in leaveSummary)
                {
                    NpoiHelper.CreateCell(nameRow, leaveIndex, leave.Abbreviation, borderedCellStyle);
                    leaveIndex++;
                    NpoiHelper.CreateCell(nameRow, leaveIndex, leave.Total.ToString(), borderedCellStyle);
                    leaveIndex++;
                }

                NpoiHelper.CreateCell(nameRow, leaveIndex, "IL", borderedCellStyle);
                leaveIndex++;
                NpoiHelper.CreateCell(nameRow, leaveIndex, offDays.ToString(), borderedCellStyle);
                leaveIndex++;
                NpoiHelper.CreateCell(nameRow, leaveIndex, "OD", borderedCellStyle);
                leaveIndex++;
                NpoiHelper.CreateCell(nameRow, leaveIndex, totalOnDuty.ToString(), borderedCellStyle);
                leaveIndex++;
                NpoiHelper.CreateCell(nameRow, leaveIndex, "T.P.D", borderedCellStyle);
                leaveIndex++;
                NpoiHelper.CreateCell(nameRow, leaveIndex, payableDays.ToString(), borderedCellStyle);

                rowIndex += 2;

                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, 0, days));

                Sheet.CreateRow(rowIndex);
                NpoiHelper.CreateCell(Sheet.CreateRow(rowIndex), 0, "", greyCellStyle);

                rowIndex += 2;
            }

            //Auto sized all the affected columns
            int lastColumNum = Sheet.GetRow(5).LastCellNum;
            for (int i = 0; i <= lastColumNum; i++)
            {
                Sheet.AutoSizeColumn(i);
                GC.Collect();
            }

            //Write the Workbook to a memory stream
            MemoryStream output = new MemoryStream();
            workbook.Write(output);

            //Return the result to the end user
            return File(output.ToArray(),   //The binary data of the XLS file
             "application/vnd.ms-excel", //MIME type of Excel files
             "Attendance.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        [CustomAuthorize("attendance-report")]
        [HttpGet("Bs")]
        public async Task<IActionResult> GetBs(int? empId, int? companyId, int? departmentId, int? designationId, int? statusId, int? year, int? month)
        {
            //if (!await _context.Settings.AnyAsync(x => x.AttendanceReportInBs))
            //{
            //    return Forbid("Feature not available.");
            //}

            var nepaliDate = NepaliCalendar.TodayBS();

            int days = NepaliCalendar.Get_TotalMonthDays(year ?? nepaliDate.Year, month ?? nepaliDate.Month);

            DateOnly firstDayOfMonth = DateOnly.FromDateTime(
                NepaliCalendar.Convert_BS2AD(string.Concat(year ?? nepaliDate.Year, "/", month?.ToString("00") ?? nepaliDate.Month.ToString("00"), "/01")));

            DateOnly lastDayOfMonth = DateOnly.FromDateTime(
                NepaliCalendar.Convert_BS2AD(string.Concat(year ?? nepaliDate.Year, "/", month?.ToString("00") ?? nepaliDate.Month.ToString("00"), "/", days.ToString("00"))));
            
            DateOnly date = DateOnly.FromDateTime(NepaliCalendar.Convert_BS2AD(string.Concat(nepaliDate.Year, "/", nepaliDate.Month.ToString("00"), "/", nepaliDate.Day.ToString("00"))));

            if (firstDayOfMonth > date)
            {
                return Ok(new
                {
                    Data = Array.Empty<int>(),
                    Summary = Array.Empty<int>()
                });
            }

            var empTransactionQuery = _context.EmpTransactions.AsQueryable();
            var empDetailQuery = _context.EmpDetails.AsQueryable();

            var company = await _context.Companies.Where(x => x.Id == companyId).FirstOrDefaultAsync();
            var department = await _context.Departments.Where(x => x.Id == departmentId).FirstOrDefaultAsync();
            var designation = await _context.Designations.Where(x => x.Id == designationId).FirstOrDefaultAsync();
            var status = await _context.Statuses.Where(x => x.Id == statusId).FirstOrDefaultAsync();

            if (empId is not null)
            {
                empDetailQuery = empDetailQuery.Where(x => x.Id == empId);
            }

            if (companyId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.CompanyId == companyId);
            }

            if (departmentId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.DepartmentId == departmentId);
            }

            if (designationId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.DesignationId == designationId);
            }

            if (statusId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.StatusId == statusId);
            } else
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.StatusId == 1);
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                empTransactionQuery = empTransactionQuery.Where(x => companyIds.Contains(x.CompanyId ?? 0));
            }

            var empDetails = await _context.EmpLogs
                .Join(empTransactionQuery,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        el.EmployeeId,
                        et.CompanyId,
                        et.DepartmentId
                    })
                .Join(empDetailQuery,
                    elt => elt.EmployeeId,
                    ed => ed.Id,
                    (elt, ed) => new
                    {
                        Id = ed.Id,
                        EmpCode = ed.CardId,
                        Name = string.Concat(ed.FirstName, " ", !string.IsNullOrEmpty(ed.MiddleName) ? ed.MiddleName + " " : "", ed.LastName),
                        CompanyId = elt.CompanyId,
                        DepartmentId = elt.DepartmentId,
                    }
                ).ToListAsync();

            List<AttendanceData> data = new();

            HSSFWorkbook workbook = new HSSFWorkbook();

            HSSFFont headerFont = (HSSFFont)workbook.CreateFont();
            HSSFFont normalFont = (HSSFFont)workbook.CreateFont();

            headerFont.IsBold = true;
            headerFont.FontHeightInPoints = 8;
            headerFont.FontName = "Tahoma";

            normalFont.FontHeightInPoints = 8;
            normalFont.FontName = "Tahoma";

            HSSFCellStyle headerCellStyle = NpoiHelper.NormalCellStyle(workbook, headerFont);
            HSSFCellStyle borderedCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);

            HSSFCellStyle offCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //offCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;
            //offCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle presentCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //presentCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;
            //presentCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle offPresentCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //offPresentCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SeaGreen.Index;
            //offPresentCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle absentCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //absentCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Red.Index;
            //absentCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle halfDayCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //halfDayCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightOrange.Index;
            //halfDayCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle leaveCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //leaveCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Lavender.Index;
            //leaveCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle regularisationCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //regularisationCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightBlue.Index;
            //regularisationCellStyle.FillPattern = FillPattern.SolidForeground;

            HSSFCellStyle greyCellStyle = NpoiHelper.BorderedCellStyle(workbook, normalFont);
            //greyCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            //greyCellStyle.FillPattern = FillPattern.SolidForeground;

            ISheet Sheet = workbook.CreateSheet("Attendance Report");

            Sheet.CreateRow(0);
            Sheet.CreateRow(1);

            IRow headerRow = Sheet.CreateRow(2);

            Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(2, 2, 0, 5));
            NpoiHelper.CreateCell(headerRow, 0, $"Attendance of {Helper.GetMonthInBs(month ?? nepaliDate.Month)}", headerCellStyle);

            if (company is not null)
            {
                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(2, 2, 6, 12));

                NpoiHelper.CreateCell(headerRow, 6, $"Company: {company.Name}", headerCellStyle);
            }

            if (department is not null)
            {
                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(2, 2, 13, 19));
                NpoiHelper.CreateCell(headerRow, 13, $"Department: {department.Name}", headerCellStyle);
            }

            if (designation is not null)
            {
                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(2, 2, 20, 26));
                NpoiHelper.CreateCell(headerRow, 13, $"Department: {designation.Name}", headerCellStyle);
            }

            Sheet.CreateRow(3);

            int rowIndex = 4;

            if (empDetails.Count == 0)
            {
                return ErrorHelper.ErrorResult("Id", "No Employee for given filter.");
            }

            int serialNumber = 1;

            foreach (var emp in empDetails)
            {
                DateOnly currentDate = firstDayOfMonth;
                double totalWorkingDays = 0;
                double totalPresentDays = 0;
                int lateInCount = 1;
                double totalOnDuty = 0;

                IRow serialNumberRow = Sheet.CreateRow(rowIndex);
                NpoiHelper.CreateCell(serialNumberRow, 0, serialNumber.ToString(), headerCellStyle);

                serialNumber++;

                rowIndex++;

                IRow nameRow = Sheet.CreateRow(rowIndex);
                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, 1, 10));
                NpoiHelper.CreateCell(nameRow, 0, emp.EmpCode, headerCellStyle);
                NpoiHelper.CreateCell(nameRow, 1, emp.Name, headerCellStyle);

                rowIndex++;

                var branchId = await _context.EmpLogs
                    .Where(x => x.EmployeeId == emp.Id)
                    .Join(_context.EmpTransactions,
                        el => el.Id,
                        et => et.Id,
                        (el, et) => et.BranchId
                    ).FirstOrDefaultAsync();

                var attendances = await _context.Attendances
                    .Include(x => x.Regularisation)
                    .ThenInclude(x => x.RegularisationType)
                    .Where(x => x.EmpId == emp.Id &&
                                (x.TransactionDate >= firstDayOfMonth && x.TransactionDate <= lastDayOfMonth))
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

                var weekendDetail = await _context.WeekendDetails
                    .Where(x => x.EmpId == emp.Id || x.EmpId == null && x.ValidFrom <= firstDayOfMonth)
                    .OrderBy(x => x.EmpId)
                    .OrderByDescending(x => x.ValidFrom)
                    .FirstOrDefaultAsync();

                var calendarId = await _context.EmpCalendars.Where(x => x.EmpId == empId).Select(x => x.CalendarId).FirstOrDefaultAsync();

                var holidays = await _context.HolidayCalendars
                    .Include(x => x.Holiday)
                    .Where(x => x.Holiday.Date >= firstDayOfMonth && x.Holiday.Date <= lastDayOfMonth && x.CalendarId == calendarId)
                    .Select(x => new
                    {
                        Name = x.Holiday.Name,
                        Date = x.Holiday.Date,
                    })
                    .ToListAsync();

                var leaves = await _context.LeaveLedgers
                    .Where(x => x.Leave_Date >= firstDayOfMonth && x.Leave_Date <= lastDayOfMonth && x.EmpId == emp.Id)
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
                    .Where(x => x.StartDate <= lastDayOfMonth && x.EndDate >= firstDayOfMonth && x.EmpId == emp.Id && x.Status == "pending")
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
                    .Include(x => x.RegularisationType)
                    .Where(x => x.FromDate <= lastDayOfMonth && x.ToDate >= firstDayOfMonth && x.EmpId == emp.Id && x.Status == "pending")
                    .Select(x => new
                    {
                        x.RegularisationType,
                        x.FromTime,
                        x.ToTime,
                        x.FromDate,
                        x.ToDate
                    })
                    .ToListAsync();

                List<int> weekends = new();

                if (weekendDetail is not null)
                {
                    if (weekendDetail?.Sunday == true)
                    {
                        weekends.Add(1);
                    }

                    if (weekendDetail?.Monday == true)
                    {
                        weekends.Add(2);
                    }

                    if (weekendDetail?.Tuesday == true)
                    {
                        weekends.Add(3);
                    }

                    if (weekendDetail?.Wednesday == true)
                    {
                        weekends.Add(4);
                    }

                    if (weekendDetail?.Thursday == true)
                    {
                        weekends.Add(5);
                    }

                    if (weekendDetail?.Friday == true)
                    {
                        weekends.Add(6);
                    }

                    if (weekendDetail?.Saturday == true)
                    {
                        weekends.Add(7);
                    }
                }

                int dateIndex = rowIndex;
                rowIndex++;
                int inTimeIndex = rowIndex;
                rowIndex++;
                int outTimeIndex = rowIndex;
                rowIndex++;
                int durationIndex = rowIndex;
                rowIndex++;
                int overTimeIndex = rowIndex;
                rowIndex++;
                int onDutyIndex = rowIndex;
                rowIndex++;
                int gatePassIndex = rowIndex;
                rowIndex++;
                int totalIndex = rowIndex;
                rowIndex++;

                IRow dateRow = Sheet.CreateRow(dateIndex);
                IRow inTimeRow = Sheet.CreateRow(inTimeIndex);
                IRow outTimeRow = Sheet.CreateRow(outTimeIndex);
                IRow durationRow = Sheet.CreateRow(durationIndex);
                IRow overTimeRow = Sheet.CreateRow(overTimeIndex);
                IRow onDutyRow = Sheet.CreateRow(onDutyIndex);
                IRow gatePassRow = Sheet.CreateRow(gatePassIndex);
                IRow totalDurationRow = Sheet.CreateRow(totalIndex);

                NpoiHelper.CreateCell(dateRow, 0, "Date", headerCellStyle);
                NpoiHelper.CreateCell(inTimeRow, 0, "In-Time", headerCellStyle);
                NpoiHelper.CreateCell(outTimeRow, 0, "Out-Time", headerCellStyle);
                NpoiHelper.CreateCell(durationRow, 0, "Duration", headerCellStyle);
                NpoiHelper.CreateCell(overTimeRow, 0, "OT-Hours", headerCellStyle);
                NpoiHelper.CreateCell(onDutyRow, 0, "OD-Hours", headerCellStyle);
                NpoiHelper.CreateCell(gatePassRow, 0, "GatePass", headerCellStyle);
                NpoiHelper.CreateCell(totalDurationRow, 0, "Total Duration", headerCellStyle);

                double payingDays = 0;

                do
                {
                    var nepaliDay = NepaliCalendar.Convert_AD2BS(currentDate.ToDateTime(TimeOnly.Parse("00:00"))).Day;

                    TimeOnly? inTime = null;
                    TimeOnly? odInTime = null;
                    TimeOnly? gatePassInTime = null;
                    TimeOnly? outTime = null;
                    TimeOnly? odOutTime = null;
                    TimeOnly? gatePassOutTime = null;
                    TimeSpan? requiredHours = new();
                    TimeSpan? requiredHalfHours = new();
                    TimeSpan? dailyHour = new();
                    TimeSpan? odHour = new();
                    TimeSpan? gatePassHour = new();
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
                        .Include(x => x.RegularisationType)
                        .Where(x => x.Status == "approved" && x.RegularisationType.Name == "gate-pass" && x.EmpId == emp.Id && x.FromDate == currentDate)
                        .ToListAsync();

                    var onDuties = await _context.Regularisations
                        .Include(x => x.RegularisationType)
                        .Where(x => x.Status == "approved" && (x.RegularisationType.Name == "on-duty" || x.RegularisationType.Name == "work-from-home") && x.EmpId == emp.Id && x.FromDate <= currentDate && x.ToDate >= currentDate)
                        .ToListAsync();

                    if (gatePasses.Count > 0)
                    {
                        foreach (var gatePass in gatePasses)
                        {
                            gatePassHour += gatePass.ToTime - gatePass.FromTime;
                        }
                    }

                    totalWorkingDays++;

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

                        totalHour = dailyHour + calculatedOdHour + calculatedGatePassHour;

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

                        var leave = leaves.Where(x => x.Leave_Date == currentDate).FirstOrDefault();

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
                            
                            NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), leaveCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, nepaliDay, ((leave.HLeaveType == 1 || leave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(leave.LeaveName) + "-A", leaveCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, nepaliDay, ((leave.HLeaveType == 1 || leave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(leave.LeaveName) + "-A", leaveCellStyle);
                            NpoiHelper.CreateCell(durationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, nepaliDay, overTime?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, nepaliDay, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), leaveCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, nepaliDay, totalHour?.ToString(@"hh\:mm"), leaveCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        var pendingLeave = pendingLeaves.Where(x => x.StartDate <= currentDate && x.EndDate >= currentDate).FirstOrDefault();

                        if (pendingLeave != null)
                        {
                            NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), leaveCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, nepaliDay, ((pendingLeave.HLeaveType == 1 || pendingLeave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(pendingLeave.LeaveName) + "-P", leaveCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, nepaliDay, ((pendingLeave.HLeaveType == 1 || pendingLeave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(pendingLeave.LeaveName) + "-P", leaveCellStyle);
                            NpoiHelper.CreateCell(durationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, nepaliDay, overTime?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, nepaliDay, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), leaveCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, nepaliDay, totalHour?.ToString(@"hh\:mm"), leaveCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        if (weekends.Contains((int)currentDate.DayOfWeek + 1))
                        {
                            totalWorkingDays--;

                            NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), offPresentCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, nepaliDay, inTime?.ToString("HH:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, nepaliDay, outTime?.ToString("HH:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(durationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, nepaliDay, overTime?.ToString(@"hh\:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, nepaliDay, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), offPresentCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, nepaliDay, totalHour?.ToString(@"hh\:mm"), offPresentCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        var holiday = holidays.Where(x => x.Date == currentDate).FirstOrDefault();

                        if (holiday != null)
                        {
                            totalWorkingDays--;

                            NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), offPresentCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, nepaliDay, inTime?.ToString("HH:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, nepaliDay, outTime?.ToString("HH:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(durationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, nepaliDay, overTime?.ToString(@"hh\:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, nepaliDay, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), offPresentCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), offPresentCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, nepaliDay, totalHour?.ToString(@"hh\:mm"), offPresentCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        if (attendance.Regularisation is not null && attendance.Regularisation.RegularisationType.Name != "gate-pass")
                        {
                            if (attendance.Regularisation.RegularisationType.Name == "in-punch-regularistaion")
                            {
                                NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), regularisationCellStyle);
                                NpoiHelper.CreateCell(inTimeRow, nepaliDay, inTime?.ToString("HH:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(outTimeRow, nepaliDay, outTime?.ToString("HH:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(durationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(overTimeRow, nepaliDay, overTime?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(onDutyRow, nepaliDay, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(totalDurationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), regularisationCellStyle);

                                currentDate = currentDate.AddDays(1);

                                continue;
                            }

                            if (attendance.Regularisation.RegularisationType.Name == "out-punch-regularistaion")
                            {
                                NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), regularisationCellStyle);
                                NpoiHelper.CreateCell(inTimeRow, nepaliDay, inTime?.ToString("HH:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(outTimeRow, nepaliDay, outTime?.ToString("HH:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(durationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(overTimeRow, nepaliDay, overTime?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(onDutyRow, nepaliDay, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(totalDurationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), regularisationCellStyle);

                                currentDate = currentDate.AddDays(1);

                                continue;
                            }

                            if (attendance.Regularisation.RegularisationType.Name == "work-from-home")
                            {
                                NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), regularisationCellStyle);
                                NpoiHelper.CreateCell(inTimeRow, nepaliDay, "WFH-A", regularisationCellStyle);
                                NpoiHelper.CreateCell(outTimeRow, nepaliDay, "WFH-A", regularisationCellStyle);
                                NpoiHelper.CreateCell(durationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(overTimeRow, nepaliDay, overTime?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(onDutyRow, nepaliDay, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), regularisationCellStyle);
                                NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(totalDurationRow, nepaliDay, totalHour?.ToString(@"hh\:mm"), regularisationCellStyle);

                                currentDate = currentDate.AddDays(1);

                                continue;
                            }

                            if (attendance.Regularisation.RegularisationType.Name == "on-duty")
                            {
                                NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), regularisationCellStyle);
                                NpoiHelper.CreateCell(inTimeRow, nepaliDay, "OD-A", regularisationCellStyle);
                                NpoiHelper.CreateCell(outTimeRow, nepaliDay, "OD-A", regularisationCellStyle);
                                NpoiHelper.CreateCell(durationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(overTimeRow, nepaliDay, overTime?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(onDutyRow, nepaliDay, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), regularisationCellStyle);
                                NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(totalDurationRow, nepaliDay, totalHour?.ToString(@"hh\:mm"), regularisationCellStyle);

                                currentDate = currentDate.AddDays(1);

                                continue;
                            }
                        }

                        if (attendanceType == "full-day")
                        {
                            NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), presentCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, nepaliDay, inTime?.ToString("HH:mm"), presentCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, nepaliDay, outTime?.ToString("HH:mm"), presentCellStyle);
                            NpoiHelper.CreateCell(durationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), presentCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, nepaliDay, overTime?.ToString(@"hh\:mm"), presentCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, nepaliDay, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), presentCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), presentCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, nepaliDay, totalHour?.ToString(@"hh\:mm"), presentCellStyle);

                        }
                        else if (attendanceType == "half-day")
                        {
                            NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), halfDayCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, nepaliDay, inTime?.ToString("HH:mm"), halfDayCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, nepaliDay, outTime?.ToString("HH:mm"), halfDayCellStyle);
                            NpoiHelper.CreateCell(durationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), halfDayCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, nepaliDay, overTime?.ToString(@"hh\:mm"), halfDayCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, nepaliDay, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), halfDayCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), halfDayCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, nepaliDay, totalHour?.ToString(@"hh\:mm"), halfDayCellStyle);
                        }
                        else
                        {
                            NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), absentCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, nepaliDay, inTime?.ToString("HH:mm"), absentCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, nepaliDay, outTime?.ToString("HH:mm"), absentCellStyle);
                            NpoiHelper.CreateCell(durationRow, nepaliDay, dailyHour?.ToString(@"hh\:mm"), absentCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, nepaliDay, overTime?.ToString(@"hh\:mm"), absentCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, nepaliDay, (odHour?.ToString(@"hh\:mm") + (middleOd ? "*" : null)), absentCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), absentCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, nepaliDay, totalHour?.ToString(@"hh\:mm"), absentCellStyle);
                        }

                        currentDate = currentDate.AddDays(1);
                    } else
                    {
                        var leave = leaves.Where(x => x.Leave_Date == currentDate).FirstOrDefault();

                        if (leave != null)
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

                            NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), leaveCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, nepaliDay, ((leave.HLeaveType == 1 || leave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(leave.LeaveName) + "-A", leaveCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, nepaliDay, ((leave.HLeaveType == 1 || leave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(leave.LeaveName) + "-A", leaveCellStyle);
                            NpoiHelper.CreateCell(durationRow, nepaliDay, "", leaveCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, nepaliDay, "", leaveCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, nepaliDay, "", leaveCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, nepaliDay, "", leaveCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        var pendingLeave = pendingLeaves.Where(x => x.StartDate <= currentDate && x.EndDate >= currentDate).FirstOrDefault();

                        if (pendingLeave != null)
                        {
                            NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), leaveCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, nepaliDay, ((pendingLeave.HLeaveType == 1 || pendingLeave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(pendingLeave.LeaveName) + "-P", leaveCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, nepaliDay, ((pendingLeave.HLeaveType == 1 || pendingLeave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(pendingLeave.LeaveName) + "-P", leaveCellStyle);
                            NpoiHelper.CreateCell(durationRow, nepaliDay, "", leaveCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, nepaliDay, "", leaveCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, nepaliDay, "", leaveCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), leaveCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, nepaliDay, "", leaveCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        var pendingRegularisation = pendingRegularisations.Where(x => x.FromDate <= currentDate && x.ToDate >= currentDate).FirstOrDefault();

                        if (pendingRegularisation != null)
                        {
                            if (attendance is null && (pendingRegularisation.RegularisationType.Name == "on-duty" || pendingRegularisation.RegularisationType.Name == "work-from-home"))
                            {
                                TimeSpan? totalHours = pendingRegularisation.FromTime - pendingRegularisation.ToTime;

                                NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), regularisationCellStyle);
                                NpoiHelper.CreateCell(inTimeRow, nepaliDay, Helper.GetAbbreviation(pendingRegularisation.RegularisationType.DisplayName) + "-P", regularisationCellStyle);
                                NpoiHelper.CreateCell(outTimeRow, nepaliDay, Helper.GetAbbreviation(pendingRegularisation.RegularisationType.DisplayName) + "-P", regularisationCellStyle);
                                NpoiHelper.CreateCell(durationRow, nepaliDay, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(overTimeRow, nepaliDay, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(onDutyRow, nepaliDay, totalHours?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(totalDurationRow, nepaliDay, totalHours?.ToString(@"hh\:mm"), regularisationCellStyle);

                                currentDate = currentDate.AddDays(1);

                                continue;
                            }

                            if (pendingRegularisation.RegularisationType.Name == "in-punch-regularisation" || pendingRegularisation.RegularisationType.Name == "out-punch-regularisation")
                            {
                                NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), regularisationCellStyle);
                                NpoiHelper.CreateCell(inTimeRow, nepaliDay, Helper.GetAbbreviation(pendingRegularisation.RegularisationType.DisplayName) + "-P", regularisationCellStyle);
                                NpoiHelper.CreateCell(outTimeRow, nepaliDay, Helper.GetAbbreviation(pendingRegularisation.RegularisationType.DisplayName) + "-P", regularisationCellStyle);
                                NpoiHelper.CreateCell(durationRow, nepaliDay, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(overTimeRow, nepaliDay, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(onDutyRow, nepaliDay, "", regularisationCellStyle);
                                NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), regularisationCellStyle);
                                NpoiHelper.CreateCell(totalDurationRow, nepaliDay, "", regularisationCellStyle);

                                currentDate = currentDate.AddDays(1);

                                continue;
                            }
                        }

                        if (weekends.Contains((int)currentDate.DayOfWeek + 1))
                        {
                            totalWorkingDays--;

                            NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), offCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, nepaliDay, "W-OFF", offCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, nepaliDay, "W-OFF", offCellStyle);
                            NpoiHelper.CreateCell(durationRow, nepaliDay, "", offCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, nepaliDay, "", offCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, nepaliDay, "", offCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), offCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, nepaliDay, "", offCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        var holiday = holidays.Where(x => x.Date == currentDate).FirstOrDefault();

                        if (holiday != null)
                        {
                            totalWorkingDays--;

                            NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), offCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, nepaliDay, "H", offCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, nepaliDay, "H", offCellStyle);
                            NpoiHelper.CreateCell(durationRow, nepaliDay, "", offCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, nepaliDay, "", offCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, nepaliDay, "", offCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), offCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, nepaliDay, "", offCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        if (currentDate > DateOnly.FromDateTime(DateTime.Now))
                        {
                            NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), borderedCellStyle);
                            NpoiHelper.CreateCell(inTimeRow, nepaliDay, "", borderedCellStyle);
                            NpoiHelper.CreateCell(outTimeRow, nepaliDay, "", borderedCellStyle);
                            NpoiHelper.CreateCell(durationRow, nepaliDay, "", borderedCellStyle);
                            NpoiHelper.CreateCell(overTimeRow, nepaliDay, "", borderedCellStyle);
                            NpoiHelper.CreateCell(onDutyRow, nepaliDay, "", borderedCellStyle);
                            NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), borderedCellStyle);
                            NpoiHelper.CreateCell(totalDurationRow, nepaliDay, "", borderedCellStyle);

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        NpoiHelper.CreateCell(dateRow, nepaliDay, string.Concat(nepaliDay, currentDate.ToString("(ddd)")), absentCellStyle);
                        NpoiHelper.CreateCell(inTimeRow, nepaliDay, "", absentCellStyle);
                        NpoiHelper.CreateCell(outTimeRow, nepaliDay, "", absentCellStyle);
                        NpoiHelper.CreateCell(durationRow, nepaliDay, "", absentCellStyle);
                        NpoiHelper.CreateCell(overTimeRow, nepaliDay, "", absentCellStyle);
                        NpoiHelper.CreateCell(onDutyRow, nepaliDay, "", absentCellStyle);
                        NpoiHelper.CreateCell(gatePassRow, nepaliDay, gatePassHour?.ToString(@"hh\:mm"), absentCellStyle);
                        NpoiHelper.CreateCell(totalDurationRow, nepaliDay, "", absentCellStyle);

                        currentDate = currentDate.AddDays(1);
                    }
                } while (currentDate <= lastDayOfMonth);

                // Total Payable Days
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

                // Total Working Days, Total Present Days and Total Payable Days
                NpoiHelper.CreateCell(nameRow, 11, "T.W.D", borderedCellStyle);
                NpoiHelper.CreateCell(nameRow, 12, totalWorkingDays.ToString(), borderedCellStyle);
                NpoiHelper.CreateCell(nameRow, 13, "P.D", borderedCellStyle);
                NpoiHelper.CreateCell(nameRow, 14, totalPresentDays.ToString(), borderedCellStyle);

                int leaveIndex = 15;

                var leaveSummary = await (from leave in _context.Leaves
                                          join leaveLedger in _context.LeaveLedgers.Where(x => x.EmpId == emp.Id && x.Leave_Date >= firstDayOfMonth && x.Leave_Date <= lastDayOfMonth)
                                          .GroupBy(x => x.LeaveId)
                                          .Select(x => new
                                          {
                                              LeaveId = x.Key,
                                              TotalTaken = x.Sum(x => x.Taken),
                                          }) on leave.Id equals leaveLedger.LeaveId into leaveLedgers
                                          from leaveLedger in leaveLedgers.DefaultIfEmpty()
                                          select new
                                          {
                                              LeaveId = leave.Id,
                                              Abbreviation = leave.Abbreviation,
                                              Total = leaveLedger != null ? leaveLedger.TotalTaken ?? 0 : 0,
                                          })
                       .ToListAsync();

                foreach (var leave in leaveSummary)
                {
                    NpoiHelper.CreateCell(nameRow, leaveIndex, leave.Abbreviation, borderedCellStyle);
                    leaveIndex++;
                    NpoiHelper.CreateCell(nameRow, leaveIndex, leave.Total.ToString(), borderedCellStyle);
                    leaveIndex++;
                }

                NpoiHelper.CreateCell(nameRow, leaveIndex, "OD", borderedCellStyle);
                leaveIndex++;
                NpoiHelper.CreateCell(nameRow, leaveIndex, totalOnDuty.ToString(), borderedCellStyle);
                leaveIndex++;
                NpoiHelper.CreateCell(nameRow, leaveIndex, "T.P.D", borderedCellStyle);
                leaveIndex++;
                NpoiHelper.CreateCell(nameRow, leaveIndex, payableDays.ToString(), borderedCellStyle);

                rowIndex += 2;

                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, 0, days));

                Sheet.CreateRow(rowIndex);
                NpoiHelper.CreateCell(Sheet.CreateRow(rowIndex), 0, "", greyCellStyle);

                rowIndex += 2;
            }

            //Auto sized all the affected columns
            int lastColumNum = Sheet.GetRow(5).LastCellNum;
            for (int i = 0; i <= lastColumNum; i++)
            {
                Sheet.AutoSizeColumn(i);
                GC.Collect();
            }

            //Write the Workbook to a memory stream
            MemoryStream output = new MemoryStream();
            workbook.Write(output);

            //Return the result to the end user
            return File(output.ToArray(),   //The binary data of the XLS file
             "application/vnd.ms-excel", //MIME type of Excel files
             "Attendance.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        [CustomAuthorize("attendance-calendar")]
        [HttpGet("{empId}")]
        public async Task<IActionResult> GetEmp(int? empId, int? year, int? month)
        {
            DateOnly date = DateOnly.FromDateTime(DateTime.Today);

            var firstDayOfMonth = new DateOnly(year ?? date.Year, month ?? date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            int days = DateTime.DaysInMonth(firstDayOfMonth.Year, firstDayOfMonth.Month);

            if (firstDayOfMonth > date)
            {
                return Ok(new
                {
                    Data = Array.Empty<int>(),
                    Summary = Array.Empty<int>()
                });
            }

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

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Contains(emp?.CompanyId ?? 0))
                {
                    return ErrorHelper.ErrorResult("Id", "Unauthorized");
                };
            }

            List<EmployeeAttendanceData> data = new();

            if (emp == null)
            {
                return ErrorHelper.ErrorResult("Id", "Employee is not registered.");
            }

            DateOnly currentDate = firstDayOfMonth;
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
                .ThenInclude(x => x.RegularisationType)
                .Where(x => x.EmpId == emp.EmployeeId &&
                            (x.TransactionDate >= firstDayOfMonth || x.TransactionDateOut >= firstDayOfMonth) &&
                            (x.TransactionDate <= lastDayOfMonth || x.TransactionDateOut <= lastDayOfMonth))
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

            var calendarId = await _context.EmpCalendars.Where(x => x.EmpId == empId).Select(x => x.CalendarId).FirstOrDefaultAsync();

            var holidays = await _context.HolidayCalendars
                .Include(x => x.Holiday)
                .Where(x => x.Holiday.Date >= firstDayOfMonth && x.Holiday.Date <= lastDayOfMonth && x.CalendarId == calendarId)
                .Select(x => new
                {
                    Name = x.Holiday.Name,
                    Date = x.Holiday.Date,
                })
                .ToListAsync();

            var leaves = await _context.LeaveLedgers
                .Where(x => x.Leave_Date >= firstDayOfMonth && x.Leave_Date <= lastDayOfMonth && x.EmpId == emp.EmployeeId)
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
                .Where(x => x.StartDate <= lastDayOfMonth && x.EndDate >= firstDayOfMonth && x.EmpId == emp.EmployeeId && x.Status == "pending")
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
                .Include(x => x.RegularisationType)
                .Where(x => x.FromDate <= lastDayOfMonth && x.ToDate >= firstDayOfMonth && x.EmpId == emp.EmployeeId && x.Status == "pending")
                .Select(x => new
                {
                    x.RegularisationType,
                    x.FromTime,
                    x.ToTime,
                    x.FromDate,
                    x.ToDate
                })
                .ToListAsync();

            List<DateOnly> weekends = await Helper.GetWeekends(firstDayOfMonth, lastDayOfMonth, empId, _context);

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
                    .Include(x => x.RegularisationType)
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

                    var leave = leaves.Where(x => x.Leave_Date == currentDate).FirstOrDefault();

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

                        data.Add(new EmployeeAttendanceData
                        {
                            Date = currentDate,
                            InTime = attendance.InTime,
                            OutTime = attendance.OutTime,
                            DayStatus = ((leave.HLeaveType == 1 || leave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(leave.LeaveName) + "-A",
                            DayStatusDescription = ((leave.HLeaveType == 1 || leave.HLeaveType == 2) ? "Half - " : "") + leave.LeaveName + " - Approved",
                            DailyHour = dailyHour?.ToString(@"hh\:mm"),
                            OnDutyHour = odHour?.ToString(@"hh\:mm"),
                            GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                            TotalDuration = totalHour?.ToString(@"hh\:mm"),
                            Color = "#c68ee9"
                        });

                        currentDate = currentDate.AddDays(1);

                        continue;
                    }

                    var pendingLeave = pendingLeaves.Where(x => x.StartDate <= currentDate && x.EndDate >= currentDate).FirstOrDefault();

                    if (pendingLeave != null)
                    {
                        data.Add(new EmployeeAttendanceData
                        {
                            Date = currentDate,
                            InTime = attendance.InTime,
                            OutTime = attendance.OutTime,
                            DayStatus = ((pendingLeave.HLeaveType == 1 || pendingLeave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(pendingLeave.LeaveName) + "-P",
                            DayStatusDescription = ((pendingLeave.HLeaveType == 1 || pendingLeave.HLeaveType == 2) ? "Half - " : "") + pendingLeave.LeaveName + " - Pending",
                            DailyHour = dailyHour?.ToString(@"hh\:mm"),
                            OnDutyHour = odHour?.ToString(@"hh\:mm"),
                            GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                            TotalDuration = totalHour?.ToString(@"hh\:mm"),
                            Color = "#c68ee9"
                        });

                        currentDate = currentDate.AddDays(1);

                        continue;
                    }

                    if (weekends.Contains(currentDate))
                    {
                        totalWorkingDays--;

                        data.Add(new EmployeeAttendanceData
                        {
                            Date = currentDate,
                            InTime = attendance.InTime,
                            OutTime = attendance.OutTime,
                            DayStatus = "POW",
                            DayStatusDescription = "Present On Weekly-Off",
                            DailyHour = dailyHour?.ToString(@"hh\:mm"),
                            OnDutyHour = odHour?.ToString(@"hh\:mm"),
                            GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                            TotalDuration = totalHour?.ToString(@"hh\:mm"),
                            Color = "#ADD8E6"
                        });

                        currentDate = currentDate.AddDays(1);

                        continue;
                    }

                    var holiday = holidays.Where(x => x.Date == currentDate).FirstOrDefault();

                    if (holiday != null)
                    {
                        totalWorkingDays--;

                        data.Add(new EmployeeAttendanceData
                        {
                            Date = currentDate,
                            InTime = attendance.InTime,
                            OutTime = attendance.OutTime,
                            DayStatus = "POH",
                            DayStatusDescription = "Present On Holiday",
                            DailyHour = dailyHour?.ToString(@"hh\:mm"),
                            OnDutyHour = odHour?.ToString(@"hh\:mm"),
                            GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                            TotalDuration = totalHour?.ToString(@"hh\:mm"),
                            Color = "#9cd3ea"
                        });

                        currentDate = currentDate.AddDays(1);

                        continue;
                    }

                    if (attendance.Regularisation is not null && attendance.Regularisation.RegularisationType.Name != "gate-pass")
                    {
                        if (attendance.Regularisation.RegularisationType.Name == "in-punch-regularistaion")
                        {
                            data.Add(new EmployeeAttendanceData
                            {
                                Date = currentDate,
                                DayStatus = Helper.GetAbbreviation(attendance.Regularisation.RegularisationType.DisplayName) + "-A",
                                DayStatusDescription = attendance.Regularisation.RegularisationType.DisplayName + " - Approved",
                                InTime = attendance.InTime,
                                OutTime = attendance.OutTime,
                                DailyHour = dailyHour?.ToString(@"hh\:mm"),
                                OnDutyHour = odHour?.ToString(@"hh\:mm"),
                                GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                                TotalDuration = totalHour?.ToString(@"hh\:mm"),
                                Color = "#ECFFDC"
                            });

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        if (attendance.Regularisation.RegularisationType.Name == "out-punch-regularistaion")
                        {
                            data.Add(new EmployeeAttendanceData
                            {
                                Date = currentDate,
                                DayStatus = Helper.GetAbbreviation(attendance.Regularisation.RegularisationType.DisplayName) + "-A",
                                DayStatusDescription = attendance.Regularisation.RegularisationType.DisplayName + " - Approved",
                                InTime = attendance.InTime,
                                OutTime = attendance.OutTime,
                                DailyHour = dailyHour?.ToString(@"hh\:mm"),
                                OnDutyHour = odHour?.ToString(@"hh\:mm"),
                                GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                                TotalDuration = totalHour?.ToString(@"hh\:mm"),
                                Color = "#ECFFDC"
                            });

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        if (attendance.Regularisation.RegularisationType.Name == "work-from-home")
                        {
                            data.Add(new EmployeeAttendanceData
                            {
                                Date = currentDate,
                                DayStatus = Helper.GetAbbreviation(attendance.Regularisation.RegularisationType.DisplayName) + "-A",
                                DayStatusDescription = attendance.Regularisation.RegularisationType.DisplayName + " - Approved",
                                InTime = attendance.InTime,
                                OutTime = attendance.OutTime,
                                DailyHour = dailyHour?.ToString(@"hh\:mm"),
                                OnDutyHour = odHour?.ToString(@"hh\:mm"),
                                GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                                TotalDuration = totalHour?.ToString(@"hh\:mm"),
                                Color = "#ECFFDC"
                            });

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        if (attendance.Regularisation.RegularisationType.Name == "on-duty")
                        {
                            data.Add(new EmployeeAttendanceData
                            {
                                Date = currentDate,
                                DayStatus = Helper.GetAbbreviation(attendance.Regularisation.RegularisationType.DisplayName) + "-A",
                                DayStatusDescription = attendance.Regularisation.RegularisationType.DisplayName + " - Approved",
                                InTime = attendance.InTime,
                                OutTime = attendance.OutTime,
                                DailyHour = dailyHour?.ToString(@"hh\:mm"),
                                OnDutyHour = odHour?.ToString(@"hh\:mm"),
                                GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                                TotalDuration = totalHour?.ToString(@"hh\:mm"),
                                Color = "#ECFFDC"
                            });

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }
                    }

                    if (attendanceType == "full-day")
                    {
                        data.Add(new EmployeeAttendanceData
                        {
                            Date = currentDate,
                            DayStatus = "Present",
                            DayStatusDescription = "Present",
                            InTime = attendance.InTime,
                            OutTime = attendance.OutTime,
                            DailyHour = dailyHour?.ToString(@"hh\:mm"),
                            OnDutyHour = odHour?.ToString(@"hh\:mm"),
                            GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                            TotalDuration = totalHour?.ToString(@"hh\:mm"),
                            Color = "#bcf18a"
                        });

                    }
                    else if (attendanceType == "half-day")
                    {
                        data.Add(new EmployeeAttendanceData
                        {
                            Date = currentDate,
                            DayStatus = "Half-Day",
                            DayStatusDescription = "Half-Day",
                            InTime = attendance.InTime,
                            OutTime = attendance.OutTime,
                            DailyHour = dailyHour?.ToString(@"hh\:mm"),
                            OnDutyHour = odHour?.ToString(@"hh\:mm"),
                            GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                            TotalDuration = totalHour?.ToString(@"hh\:mm"),
                            Color = "#40c119"
                        });
                    }
                    else
                    {
                        data.Add(new EmployeeAttendanceData
                        {
                            Date = currentDate,
                            InTime = attendance.InTime,
                            OutTime = attendance.OutTime,
                            DayStatus = "Absent",
                            DayStatusDescription = "Absent",
                            GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                            Color = "#e93636e0"
                        });
                    }

                    currentDate = currentDate.AddDays(1);
                }
                else
                {
                    var leave = leaves.Where(x => x.Leave_Date == currentDate).FirstOrDefault();

                    if (leave != null)
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

                        data.Add(new EmployeeAttendanceData
                        {
                            Date = currentDate,
                            DayStatus = ((leave.HLeaveType == 1 || leave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(leave.LeaveName) + "-A",
                            DayStatusDescription = ((leave.HLeaveType == 1 || leave.HLeaveType == 2) ? "Half - " : "") + leave.LeaveName + " - Approved",
                            Color = "#c68ee9"
                        });

                        currentDate = currentDate.AddDays(1);

                        continue;
                    }

                    var pendingLeave = pendingLeaves.Where(x => x.StartDate <= currentDate && x.EndDate >= currentDate).FirstOrDefault();

                    if (pendingLeave != null)
                    {
                        data.Add(new EmployeeAttendanceData
                        {
                            Date = currentDate,
                            DayStatus = ((pendingLeave.HLeaveType == 1 || pendingLeave.HLeaveType == 2) ? "H-" : "") + Helper.GetAbbreviation(pendingLeave.LeaveName) + "-P",
                            DayStatusDescription = ((pendingLeave.HLeaveType == 1 || pendingLeave.HLeaveType == 2) ? "Half - " : "") + pendingLeave.LeaveName + " - Pending",
                            Color = "#c68ee9"
                        });

                        currentDate = currentDate.AddDays(1);

                        continue;
                    }

                    var pendingRegularisation = pendingRegularisations.Where(x => x.FromDate <= currentDate && x.ToDate >= currentDate).FirstOrDefault();

                    if (pendingRegularisation != null)
                    {
                        if (attendance is null && (pendingRegularisation.RegularisationType.Name == "on-duty" || pendingRegularisation.RegularisationType.Name == "work-from-home"))
                        {
                            TimeSpan? totalHours = pendingRegularisation.FromTime - pendingRegularisation.ToTime;

                            data.Add(new EmployeeAttendanceData
                            {
                                Date = currentDate,
                                InTime = pendingRegularisation.FromTime?.ToString("HH:mm:ss"),
                                OutTime = pendingRegularisation.ToTime?.ToString("HH:mm:ss"),
                                DayStatus = Helper.GetAbbreviation(pendingRegularisation.RegularisationType.DisplayName) + "-P",
                                DayStatusDescription = pendingRegularisation.RegularisationType.DisplayName + " - Pending",
                                OnDutyHour = totalHours?.ToString(@"hh\:mm"),
                                GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                                Color = "#ECFFDC"
                            });

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }

                        if (pendingRegularisation.RegularisationType.Name == "in-punch-regularisation" || pendingRegularisation.RegularisationType.Name == "out-punch-regularisation")
                        {
                            data.Add(new EmployeeAttendanceData
                            {
                                Date = currentDate,
                                InTime = pendingRegularisation.RegularisationType.Name == "in-punch-regularisation" ? null : attendance?.InTime,
                                OutTime = pendingRegularisation.RegularisationType.Name == "out-punch-regularisation" ? null : attendance?.OutTime,
                                DayStatus = Helper.GetAbbreviation(pendingRegularisation.RegularisationType.DisplayName) + "-P",
                                DayStatusDescription = pendingRegularisation.RegularisationType.DisplayName + " - Pending",
                                DailyHour = null,
                                GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                                Color = "#ECFFDC"
                            });

                            currentDate = currentDate.AddDays(1);

                            continue;
                        }
                    }

                    if (weekends.Contains(currentDate))
                    {
                        totalWorkingDays--;

                        data.Add(new EmployeeAttendanceData
                        {
                            Date = currentDate,
                            DayStatus = "Weekly-Off",
                            DayStatusDescription = "Weekly-Off",
                            Color = "#ADD8E6"
                        });

                        currentDate = currentDate.AddDays(1);

                        continue;
                    }

                    var holiday = holidays.Where(x => x.Date == currentDate).FirstOrDefault();

                    if (holiday != null)
                    {
                        totalWorkingDays--;

                        data.Add(new EmployeeAttendanceData
                        {
                            Date = currentDate,
                            DayStatus = "Holiday",
                            DayStatusDescription = "Holiday",
                            Color = "#9cd3ea"
                        });

                        currentDate = currentDate.AddDays(1);

                        continue;
                    }

                    if (currentDate > DateOnly.FromDateTime(DateTime.Now))
                    {
                        data.Add(new EmployeeAttendanceData
                        {
                            Date = currentDate,
                            DayStatus = "",
                            DayStatusDescription = "",
                            Color = ""
                        });

                        currentDate = currentDate.AddDays(1);

                        continue;
                    }

                    data.Add(new EmployeeAttendanceData
                    {
                        Date = currentDate,
                        DayStatus = "Absent",
                        DayStatusDescription = "Absent",
                        GatePassDuration = gatePassHour?.ToString(@"hh\:mm"),
                        Color = "#e93636e0"
                    });

                    currentDate = currentDate.AddDays(1);
                }
            } while (currentDate <= lastDayOfMonth);

            // Total Payable Days
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

            var summary = data
                .GroupBy(x => new { x.DayStatus, x.DayStatusDescription, x.Color })
                .Select(x => new
                {
                    x.Key.DayStatus,
                    x.Key.DayStatusDescription,
                    x.Key.Color,
                    Total = x.Count()
                })
                .ToList();

            return Ok(new
            {
                Data = data,
                Summary = summary
            });
        }

        [CustomAuthorize("attendance-daily-report")]
        [HttpGet("Daily")]
        public async Task<IActionResult> GetDailyAttendance(int? page, int? limit, int? empId, int? companyId, int? departmentId, int? designationId, int? statusId, string date)
        {
            //if (!await _context.Settings.AnyAsync(x => x.DailyAttendance))
            //{
            //    return Forbid("Feature not available.");
            //}

            int Page = page ?? 1;
            int Limit = limit ?? 10;

            if (empId is null && companyId is null && departmentId is null && designationId is null)
            {
                return BadRequest("No target specified");
            }

            if (string.IsNullOrEmpty(date))
            {
                return ErrorHelper.ErrorResult("date", "Please specify the date");
            }

            if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out DateOnly Date))
            {
                return ErrorHelper.ErrorResult("date", "Invalid date format. Please use " + "yyyy-MM-dd.");
            }

            var empTransactionQuery = _context.EmpTransactions.AsQueryable();
            var empDetailQuery = _context.EmpDetails.AsQueryable();

            var company = await _context.Companies.Where(x => x.Id == companyId).FirstOrDefaultAsync();
            var department = await _context.Departments.Where(x => x.Id == departmentId).FirstOrDefaultAsync();
            var designation = await _context.Designations.Where(x => x.Id == designationId).FirstOrDefaultAsync();
            var status = await _context.Statuses.Where(x => x.Id == statusId).FirstOrDefaultAsync();

            if (empId is not null)
            {
                empDetailQuery = empDetailQuery.Where(x => x.Id == empId);
            }

            if (companyId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.CompanyId == companyId);
            }

            if (departmentId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.DepartmentId == departmentId);
            }

            if (designationId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.DesignationId == designationId);
            }

            if (statusId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.StatusId == statusId);
            } else
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.StatusId == 1);
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                empTransactionQuery = empTransactionQuery.Where(x => companyIds.Contains(x.CompanyId ?? 0));
            }

            var empDetails = await _context.EmpLogs
                .Join(empTransactionQuery,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        el.EmployeeId,
                        et.CompanyId,
                        et.DepartmentId,
                        et.DesignationId,
                    })
                .Join(empDetailQuery,
                    elt => elt.EmployeeId,
                    ed => ed.Id,
                    (elt, ed) => new
                    {
                        Id = ed.Id,
                        EmpCode = ed.CardId,
                        Name = string.Concat(ed.FirstName, " ", !string.IsNullOrEmpty(ed.MiddleName) ? ed.MiddleName + " " : "", ed.LastName),
                        CompanyId = elt.CompanyId,
                        DepartmentId = elt.DepartmentId,
                        DesignationId = elt.DesignationId,
                        
                    }
                )
                .Skip((Page - 1) * Limit)
                .Take(Limit)
                .ToListAsync();

            var count = await _context.EmpLogs
                .Join(empTransactionQuery,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        el.EmployeeId,
                        et.CompanyId,
                        et.DepartmentId,
                        et.DesignationId,
                    })
                .Join(empDetailQuery,
                    elt => elt.EmployeeId,
                    ed => ed.Id,
                    (elt, ed) => new
                    {
                        Id = ed.Id,
                        EmpCode = ed.CardId,
                        Name = string.Concat(ed.FirstName, " ", !string.IsNullOrEmpty(ed.MiddleName) ? ed.MiddleName + " " : "", ed.LastName),
                        CompanyId = elt.CompanyId,
                        DepartmentId = elt.DepartmentId,
                        DesignationId = elt.DesignationId,

                    }
                )
                .CountAsync();

            List<DailyAttendanceData> attendanceData = new();

            foreach (var emp in empDetails)
            {
                var attendance = await _context.Attendances
                    .Include(x => x.Regularisation)
                    .ThenInclude(x => x.RegularisationType)
                    .Where(x => x.EmpId == emp.Id && x.TransactionDate == Date)
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
                    .FirstOrDefaultAsync();

                if (attendance is null)
                {
                    WorkHour? workHour;

                    var roster = await _context.Rosters
                        .Where(x => x.Date == Date && x.EmpId == emp.Id)
                        .Include(x => x.WorkHour)
                        .FirstOrDefaultAsync();

                    workHour = roster?.WorkHour;

                    if (roster is null)
                    {
                        var defaultWorkHour = await _context.DefaultWorkHours
                            .Where(x => (x.EmpId == emp.Id || x.EmpId == null) && x.DayId == ((short)Date.DayOfWeek + 1))
                            .OrderBy(x => x.EmpId)
                            .Include(x => x.WorkHour)
                            .FirstOrDefaultAsync();

                        workHour = defaultWorkHour?.WorkHour;
                    }

                    if (workHour is null)
                    {
                        attendanceData.Add(new DailyAttendanceData
                        {
                            EmpCode = emp.EmpCode,
                            EmpName = emp.Name,
                        });

                        continue;
                    }


                    if (workHour.IsFlexible)
                    {
                        TimeSpan WorkTime = TimeSpan.FromMinutes((double)(workHour?.MinDutyTime ?? 0));

                        attendanceData.Add(new DailyAttendanceData
                        {
                            EmpCode = emp.EmpCode,
                            EmpName = emp.Name,
                            StartTime = workHour.StartTime,
                            EndTime = workHour.EndTime,
                            WorkTime = WorkTime.ToString(@"hh\:mm\:ss"),
                            Remarks = "Absent"
                        });
                    } else
                    {
                        TimeSpan WorkTime = TimeOnly.Parse(workHour.EndTime ?? "00:00:00") - TimeOnly.Parse(workHour.StartTime ?? "00:00:00");

                        attendanceData.Add(new DailyAttendanceData
                        {
                            EmpCode = emp.EmpCode,
                            EmpName = emp.Name,
                            StartTime = workHour.StartTime,
                            EndTime = workHour.EndTime,
                            WorkTime = WorkTime.ToString(@"hh\:mm\:ss"),
                            Remarks = "Absent"
                        });
                    }

                    continue;
                }

                TimeOnly? inTime = null;
                TimeOnly? odInTime = null;
                TimeOnly? outTime = null;
                TimeOnly? odOutTime = null;
                TimeSpan? LateIn = null;
                TimeSpan? LateOut = null;
                TimeSpan? EarlyIn = null;
                TimeSpan? EarlyOut = null;
                TimeSpan? requiredHours = new();
                TimeSpan? requiredHalfHours = new();
                TimeSpan? odHour = new();
                TimeSpan? dailyHour = new();
                TimeSpan? totalHour = new();
                TimeSpan? overTime = new();
                TimeOnly startTime = new();
                TimeOnly endTime = new();
                TimeOnly lateInGrace = new();
                TimeOnly? halfDayEndTime = null;
                TimeOnly? halfDayStartTime = null;
                bool isFlexible = false;
                string attendanceType = "";

                inTime = !string.IsNullOrEmpty(attendance?.InTime) ? TimeOnly.Parse(attendance.InTime) : null;
                outTime = !string.IsNullOrEmpty(attendance?.OutTime) ? TimeOnly.Parse(attendance.OutTime) : null;

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


                if (attendance.Regularisation != null && (attendance.Regularisation.RegularisationType.Name == "on-duty" || attendance.Regularisation.RegularisationType.Name == "work-from-home"))
                {
                    odInTime = attendance.Regularisation.FromTime;
                    odOutTime = attendance.Regularisation.ToTime;

                    odInTime = new TimeOnly(odInTime?.Hour ?? 0, odInTime?.Minute ?? 0);
                    odOutTime = new TimeOnly(odOutTime?.Hour ?? 0, odOutTime?.Minute ?? 0);

                    odHour = (odOutTime - odInTime);
                }


                var gatePass = await _context.Regularisations
                    .Include(x => x.RegularisationType)
                    .Where(x => x.Status == "approved" && x.RegularisationType.Name == "gate-pass" && x.EmpId == empId && x.FromDate == Date)
                    .FirstOrDefaultAsync();

                if (gatePass != null)
                {
                    odInTime = gatePass.FromTime;
                    odOutTime = gatePass.ToTime;

                    odInTime = new TimeOnly(odInTime?.Hour ?? 0, odInTime?.Minute ?? 0);
                    odOutTime = new TimeOnly(odOutTime?.Hour ?? 0, odOutTime?.Minute ?? 0);

                    odHour = (odOutTime - odInTime);
                }

                inTime = attendance.InTime != null ? TimeOnly.ParseExact(attendance.InTime, "HH:mm:ss") : null;
                outTime = attendance.OutTime != null ? TimeOnly.ParseExact(attendance.OutTime, "HH:mm:ss") : null;

                inTime = inTime is not null ? new TimeOnly(inTime?.Hour ?? 0, inTime?.Minute ?? 0) : null;
                outTime = outTime is not null ? new TimeOnly(outTime?.Hour ?? 0, outTime?.Minute ?? 0) : null;

                dailyHour = outTime - inTime;
                dailyHour = dailyHour ?? new();

                if (odHour is not null)
                {
                    if (odInTime == inTime && odOutTime == outTime)
                    {
                        dailyHour = new();
                        totalHour = odHour;
                    }
                    else if (odInTime >= inTime && odOutTime <= outTime)
                    {
                        totalHour = dailyHour;
                        dailyHour = totalHour - odHour;
                    }
                    else
                    {
                        totalHour = dailyHour + odHour;
                    }
                }

                if (isFlexible)
                {
                    if (inTime is not null && outTime is not null)
                    {
                        if (totalHour >= requiredHours)
                        {
                            attendanceType = "Present";
                        }
                        else if (totalHour >= requiredHalfHours)
                        {
                            attendanceType = "Half";
                        }
                    }
                }
                else
                {
                    if (startTime > inTime)
                    {
                        EarlyIn = startTime - inTime;
                    }

                    if (startTime < inTime)
                    {
                        LateIn = inTime - startTime;
                    }

                    if (endTime > outTime)
                    {
                        EarlyOut = endTime - outTime;
                    }

                    if (endTime < outTime)
                    {
                        LateOut = outTime - endTime;
                    }

                    if (inTime is not null && outTime is not null)
                    {
                        if (outTime < endTime)
                        {
                            if (outTime >= halfDayStartTime)
                            {
                                if (inTime <= startTime || odInTime <= startTime)
                                {
                                    attendanceType = "Half";
                                }
                                else if (inTime <= lateInGrace || odInTime <= lateInGrace)
                                {
                                    attendanceType = "Half";
                                }
                            }
                        }
                        else
                        {
                            if (inTime <= startTime || odInTime <= startTime)
                            {
                                attendanceType = "Present";
                            }
                            else if (inTime <= lateInGrace || odInTime <= lateInGrace)
                            {
                                attendanceType = "Present";
                            }
                            else if (inTime <= halfDayEndTime || odInTime <= halfDayStartTime)
                            {
                                attendanceType = "Half";
                            }
                        }
                    }

                    if (totalHour is not null && requiredHours is not null && totalHour > requiredHours)
                    {
                        overTime = totalHour - requiredHours;
                    }

                    attendanceData.Add(new DailyAttendanceData
                    {
                        EmpCode = emp.EmpCode,
                        EmpName = emp.Name,
                        StartTime = attendance.StartTime,
                        EndTime = attendance.EndTime,
                        WorkTime = requiredHours?.ToString(@"hh\:mm\:ss"),
                        InTime = inTime?.ToString("HH:mm:ss"),
                        OutTime = outTime?.ToString("HH:mm:ss"),
                        DailyHour = dailyHour?.ToString(@"hh\:mm\:ss"),
                        OdInTime = odInTime?.ToString("HH:mm:ss"),
                        OdOutTime = odOutTime?.ToString("HH:mm:ss"),
                        OdHour = odHour?.ToString(@"hh\:mm\:ss"),
                        OverTime = overTime?.ToString(@"hh\:mm\:ss"),
                        LateIn = LateIn?.ToString(@"hh\:mm\:ss"),
                        LateOut = LateOut?.ToString(@"hh\:mm\:ss"),
                        EarlyIn = EarlyIn?.ToString(@"hh\:mm\:ss"),
                        EarlyOut = EarlyOut?.ToString(@"hh\:mm\:ss"),
                        Remarks = string.IsNullOrEmpty(attendanceType) ? "Absent" : attendanceType
                    }) ;
                }
            }

            return Ok(new
            {
                Company = company?.Name,
                Department = department?.Name,
                Designation = designation?.Name,
                Date = date,
                AttendanceData = attendanceData,
                Count = count
            });
        }

        [CustomAuthorize("attendance-daily-report")]
        [HttpGet("Daily/Bs")]
        public async Task<IActionResult> GetDailyAttendanceBs(int? page, int? limit, int? empId, int? companyId, int? departmentId, int? designationId, int? statusId, string date)
        {
            //if (!await _context.Settings.AnyAsync(x => x.DailyAttendanceInBs))
            //{
            //    return Forbid("Feature not available.");
            //}

            int Page = page ?? 1;
            int Limit = limit ?? 10;

            if (empId is null && companyId is null && departmentId is null && designationId is null)
            {
                return BadRequest("No target specified");
            }

            if (string.IsNullOrEmpty(date))
            {
                return ErrorHelper.ErrorResult("date", "Please specify the date");
            }

            if (!DateOnly.TryParseExact(date, "yyyy/MM/dd", out _))
            {
                return ErrorHelper.ErrorResult("date", "Invalid date format. Please use " + "yyyy/MM/dd.");
            }

            DateOnly Date = DateOnly.FromDateTime(NepaliCalendar.Convert_BS2AD(string.Concat(date)));

            var empTransactionQuery = _context.EmpTransactions.AsQueryable();
            var empDetailQuery = _context.EmpDetails.AsQueryable();

            var company = await _context.Companies.Where(x => x.Id == companyId).FirstOrDefaultAsync();
            var department = await _context.Departments.Where(x => x.Id == departmentId).FirstOrDefaultAsync();
            var designation = await _context.Designations.Where(x => x.Id == designationId).FirstOrDefaultAsync();
            var status = await _context.Statuses.Where(x => x.Id == statusId).FirstOrDefaultAsync();

            if (empId is not null)
            {
                empDetailQuery = empDetailQuery.Where(x => x.Id == empId);
            }

            if (companyId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.CompanyId == companyId);
            }

            if (departmentId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.DepartmentId == departmentId);
            }

            if (designationId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.DesignationId == designationId);
            }

            if (statusId is not null)
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.StatusId == statusId);
            }
            else
            {
                empTransactionQuery = empTransactionQuery.Where(x => x.StatusId == 1);
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                empTransactionQuery = empTransactionQuery.Where(x => companyIds.Contains(x.CompanyId ?? 0));
            }

            var empDetails = await _context.EmpLogs
                .Join(empTransactionQuery,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        el.EmployeeId,
                        et.CompanyId,
                        et.DepartmentId,
                        et.DesignationId,
                    })
                .Join(empDetailQuery,
                    elt => elt.EmployeeId,
                    ed => ed.Id,
                    (elt, ed) => new
                    {
                        Id = ed.Id,
                        EmpCode = ed.CardId,
                        Name = string.Concat(ed.FirstName, " ", !string.IsNullOrEmpty(ed.MiddleName) ? ed.MiddleName + " " : "", ed.LastName),
                        CompanyId = elt.CompanyId,
                        DepartmentId = elt.DepartmentId,
                        DesignationId = elt.DesignationId,

                    }
                )
                .Skip((Page - 1) * Limit)
                .Take(Limit)
                .ToListAsync();

            var count = await _context.EmpLogs
                .Join(empTransactionQuery,
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        el.EmployeeId,
                        et.CompanyId,
                        et.DepartmentId,
                        et.DesignationId,
                    })
                .Join(empDetailQuery,
                    elt => elt.EmployeeId,
                    ed => ed.Id,
                    (elt, ed) => new
                    {
                        Id = ed.Id,
                        EmpCode = ed.CardId,
                        Name = string.Concat(ed.FirstName, " ", !string.IsNullOrEmpty(ed.MiddleName) ? ed.MiddleName + " " : "", ed.LastName),
                        CompanyId = elt.CompanyId,
                        DepartmentId = elt.DepartmentId,
                        DesignationId = elt.DesignationId,

                    }
                )
                .Skip((Page - 1) * Limit)
                .Take(Limit)
                .CountAsync();

            List<DailyAttendanceData> attendanceData = new();

            foreach (var emp in empDetails)
            {
                var attendance = await _context.Attendances
                    .Include(x => x.Regularisation)
                    .ThenInclude(x => x.RegularisationType)
                    .Where(x => x.EmpId == emp.Id && x.TransactionDate == Date)
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
                    .FirstOrDefaultAsync();

                if (attendance is null)
                {
                    WorkHour? workHour;

                    var roster = await _context.Rosters
                        .Where(x => x.Date == Date && x.EmpId == emp.Id)
                        .Include(x => x.WorkHour)
                        .FirstOrDefaultAsync();

                    workHour = roster?.WorkHour;

                    if (roster is null)
                    {
                        var defaultWorkHour = await _context.DefaultWorkHours
                            .Where(x => (x.EmpId == emp.Id || x.EmpId == null) && x.DayId == ((short)Date.DayOfWeek + 1))
                            .OrderBy(x => x.EmpId)
                            .Include(x => x.WorkHour)
                            .FirstOrDefaultAsync();

                        workHour = defaultWorkHour?.WorkHour;
                    }

                    if (workHour is null)
                    {
                        attendanceData.Add(new DailyAttendanceData
                        {
                            EmpCode = emp.EmpCode,
                            EmpName = emp.Name,
                        });

                        continue;
                    }


                    if (workHour.IsFlexible)
                    {
                        TimeSpan WorkTime = TimeSpan.FromMinutes((double)(workHour?.MinDutyTime ?? 0));

                        attendanceData.Add(new DailyAttendanceData
                        {
                            EmpCode = emp.EmpCode,
                            EmpName = emp.Name,
                            StartTime = workHour.StartTime,
                            EndTime = workHour.EndTime,
                            WorkTime = WorkTime.ToString(@"hh\:mm\:ss"),
                            Remarks = "Absent"
                        });
                    }
                    else
                    {
                        TimeSpan WorkTime = TimeOnly.Parse(workHour.EndTime ?? "00:00:00") - TimeOnly.Parse(workHour.StartTime ?? "00:00:00");

                        attendanceData.Add(new DailyAttendanceData
                        {
                            EmpCode = emp.EmpCode,
                            EmpName = emp.Name,
                            StartTime = workHour.StartTime,
                            EndTime = workHour.EndTime,
                            WorkTime = WorkTime.ToString(@"hh\:mm\:ss"),
                            Remarks = "Absent"
                        });
                    }

                    continue;
                }

                TimeOnly? inTime = null;
                TimeOnly? odInTime = null;
                TimeOnly? outTime = null;
                TimeOnly? odOutTime = null;
                TimeSpan? LateIn = null;
                TimeSpan? LateOut = null;
                TimeSpan? EarlyIn = null;
                TimeSpan? EarlyOut = null;
                TimeSpan? requiredHours = new();
                TimeSpan? requiredHalfHours = new();
                TimeSpan? odHour = new();
                TimeSpan? dailyHour = new();
                TimeSpan? totalHour = new();
                TimeSpan? overTime = new();
                TimeOnly startTime = new();
                TimeOnly endTime = new();
                TimeOnly lateInGrace = new();
                TimeOnly? halfDayEndTime = null;
                TimeOnly? halfDayStartTime = null;
                bool isFlexible = false;
                string attendanceType = "";

                inTime = !string.IsNullOrEmpty(attendance?.InTime) ? TimeOnly.Parse(attendance.InTime) : null;
                outTime = !string.IsNullOrEmpty(attendance?.OutTime) ? TimeOnly.Parse(attendance.OutTime) : null;

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


                if (attendance.Regularisation != null && (attendance.Regularisation.RegularisationType.Name == "on-duty" || attendance.Regularisation.RegularisationType.Name == "work-from-home"))
                {
                    odInTime = attendance.Regularisation.FromTime;
                    odOutTime = attendance.Regularisation.ToTime;

                    odInTime = new TimeOnly(odInTime?.Hour ?? 0, odInTime?.Minute ?? 0);
                    odOutTime = new TimeOnly(odOutTime?.Hour ?? 0, odOutTime?.Minute ?? 0);

                    odHour = (odOutTime - odInTime);
                }


                var gatePass = await _context.Regularisations
                    .Include(x => x.RegularisationType)
                    .Where(x => x.Status == "approved" && x.RegularisationType.Name == "gate-pass" && x.EmpId == empId && x.FromDate == Date)
                    .FirstOrDefaultAsync();

                if (gatePass != null)
                {
                    odInTime = gatePass.FromTime;
                    odOutTime = gatePass.ToTime;

                    odInTime = new TimeOnly(odInTime?.Hour ?? 0, odInTime?.Minute ?? 0);
                    odOutTime = new TimeOnly(odOutTime?.Hour ?? 0, odOutTime?.Minute ?? 0);

                    odHour = (odOutTime - odInTime);
                }

                inTime = attendance.InTime != null ? TimeOnly.ParseExact(attendance.InTime, "HH:mm:ss") : null;
                outTime = attendance.OutTime != null ? TimeOnly.ParseExact(attendance.OutTime, "HH:mm:ss") : null;

                inTime = inTime is not null ? new TimeOnly(inTime?.Hour ?? 0, inTime?.Minute ?? 0) : null;
                outTime = outTime is not null ? new TimeOnly(outTime?.Hour ?? 0, outTime?.Minute ?? 0) : null;

                dailyHour = outTime - inTime;
                dailyHour = dailyHour ?? new();

                if (odHour is not null)
                {
                    if (odInTime == inTime && odOutTime == outTime)
                    {
                        dailyHour = new();
                        totalHour = odHour;
                    }
                    else if (odInTime >= inTime && odOutTime <= outTime)
                    {
                        totalHour = dailyHour;
                        dailyHour = totalHour - odHour;
                    }
                    else
                    {
                        totalHour = dailyHour + odHour;
                    }
                }

                if (isFlexible)
                {
                    if (inTime is not null && outTime is not null)
                    {
                        if (totalHour >= requiredHours)
                        {
                            attendanceType = "Present";
                        }
                        else if (totalHour >= requiredHalfHours)
                        {
                            attendanceType = "Half";
                        }
                    }
                }
                else
                {
                    if (startTime > inTime)
                    {
                        EarlyIn = startTime - inTime;
                    }

                    if (startTime < inTime)
                    {
                        LateIn = inTime - startTime;
                    }

                    if (endTime > outTime)
                    {
                        EarlyOut = endTime - outTime;
                    }

                    if (endTime < outTime)
                    {
                        LateOut = outTime - endTime;
                    }

                    if (inTime is not null && outTime is not null)
                    {
                        if (outTime < endTime)
                        {
                            if (outTime >= halfDayEndTime)
                            {
                                if (inTime <= startTime || odInTime <= startTime)
                                {
                                    attendanceType = "Half";
                                }
                                else if (inTime <= lateInGrace || odInTime <= lateInGrace)
                                {
                                    attendanceType = "Half";
                                }
                            }
                        }
                        else
                        {
                            if (inTime <= startTime || odInTime <= startTime)
                            {
                                attendanceType = "Present";
                            }
                            else if (inTime <= lateInGrace || odInTime <= lateInGrace)
                            {
                                attendanceType = "Present";
                            }
                            else if (inTime <= halfDayStartTime || odInTime <= halfDayStartTime)
                            {
                                attendanceType = "Half";
                            }
                        }
                    }

                    if (totalHour is not null && requiredHours is not null && totalHour > requiredHours)
                    {
                        overTime = totalHour - requiredHours;
                    }

                    attendanceData.Add(new DailyAttendanceData
                    {
                        EmpCode = emp.EmpCode,
                        EmpName = emp.Name,
                        StartTime = attendance.StartTime,
                        EndTime = attendance.EndTime,
                        WorkTime = requiredHours?.ToString(@"hh\:mm\:ss"),
                        InTime = inTime?.ToString("HH:mm:ss"),
                        OutTime = outTime?.ToString("HH:mm:ss"),
                        DailyHour = dailyHour?.ToString(@"hh\:mm\:ss"),
                        OdInTime = odInTime?.ToString("HH:mm:ss"),
                        OdOutTime = odOutTime?.ToString("HH:mm:ss"),
                        OdHour = odHour?.ToString(@"hh\:mm\:ss"),
                        OverTime = overTime?.ToString(@"hh\:mm\:ss"),
                        LateIn = LateIn?.ToString(@"hh\:mm\:ss"),
                        LateOut = LateOut?.ToString(@"hh\:mm\:ss"),
                        EarlyIn = EarlyIn?.ToString(@"hh\:mm\:ss"),
                        EarlyOut = EarlyOut?.ToString(@"hh\:mm\:ss"),
                        Remarks = string.IsNullOrEmpty(attendanceType) ? "Absent" : attendanceType
                    });
                }
            }

            return Ok(new
            {
                Company = company?.Name,
                Department = department?.Name,
                Designation = designation?.Name,
                Date = date,
                AttendanceData = attendanceData,
                Count = count
            });
        }

        [Authorize(Roles = "super-admin")]
        [HttpGet("NoShiftEmp")]
        public async Task<IActionResult> GetEmpWithNoShift()
        {
            var count = await _context.Attendances
                .Where(x => x.WorkHourId == null)
                .CountAsync();

            return Ok(new
            {
                Count = count
            });
        }

        [Authorize(Roles = "super-admin")]
        [HttpGet("SyncShift")]
        public async Task<IActionResult> SyncWorkHour()
        {
            var page = 0;

            //do
            //{
                var attendances = await _context.Attendances.Where(x => x.WorkHourId == null).Skip(page).Take(1000).ToListAsync();

                if (attendances.Any(x => x.WorkHourId is null))
                {
                    page++;

                    //continue;
                };

                foreach(var attendance in attendances)
                {
                    if (attendance.WorkHourId is not null)
                    {
                        continue;
                    }
                    short? WorkHourId;

                    var roster = await _context.Rosters.Where(x => x.Date == attendance.TransactionDate && x.EmpId == attendance.EmpId).FirstOrDefaultAsync();

                    WorkHourId = roster?.WorkHourId;

                    if (roster is null)
                    {
                        var defaultWorkHour = await _context.DefaultWorkHours
                            .Where(x => x.EmpId == attendance.EmpId || x.EmpId == null && x.DayId == ((short)attendance.TransactionDate.DayOfWeek + 1))
                            .OrderBy(x => x.EmpId)
                            .FirstOrDefaultAsync();

                        WorkHourId = defaultWorkHour?.WorkHourId;
                    }

                    attendance.WorkHourId = WorkHourId;
                }

                await _context.SaveChangesAsync();

            //    page++;

            //} while (await _context.Attendances.Skip(page).Take(1000).AnyAsync());

            return Ok();
        }

        [HttpGet("Data")]
        public async Task<IActionResult> GetAttendanceData(int? year, int? month, int? day)
        {
            DateOnly date = new DateOnly(year ?? DateTime.Now.Year, month ?? DateTime.Now.Month, day ?? DateTime.Now.Day);

            var attendances = await _context.AttendanceLogNoDirections.Where(x => x.Date == date).ToListAsync();

            List<Data> data = new();

            foreach(var attendance in attendances)
            {
                var empCode = await _context.EmpDeviceCodes
                    .Where(x => x.DeviceId == attendance.DeviceId && x.DeviceCode == attendance.DeviceCode)
                    .Include(x => x.Emp)
                    .Select(x => x.Emp.CardId)
                    .FirstOrDefaultAsync();

                if (empCode is null)
                {
                    continue;
                }

                data.Add(new Data
                {
                    EmpCode = empCode,
                    Date = date.ToString("dd-MM-yyyy"),
                    Time = attendance.Time.ToString("hh:mm")
                });
            }

            return Ok(data);
        }

        public static string LineBreak(string a, string b)
        {
            return a + "\n" + b;
        }

        public class Data
        {
            public string EmpCode { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
        }

        public class AttendanceData
        {
            [Name("Date")]
            public DateOnly Date { get; set; }

            [Name("Emp code")]
            public string EmpCode { get; set; }

            [Name("Emp Name")]
            public string EmpName { get; set; }

            [Name("Days")]
            public string Days { get; set; }

            [Name("Shift Name")]
            public string Shift { get; set; }

            [Name("In Time")]
            public string? InTime { get; set; }

            [Name("Out Time")]
            public string? OutTime { get; set; }

            [Name("Working Hours")]
            public TimeSpan? DailyHour { get; set; }

            [Name("Attendance Status")]
            public string DayStatus { get; set; }

        }

        public class EmployeeAttendanceData
        {
            public DateOnly Date { get; set; }
            public string DayStatus { get; set; }
            public string DayStatusDescription { get; set; }
            public string? InTime { get; set; }
            public string? OutTime { get; set; }
            public string? DailyHour { get; set; }
            public string? OnDutyHour { get; set; }
            public string? GatePassDuration { get; set; }
            public string? TotalDuration { get; set; } 
            public string Color { get; set; }
        }

        public class Summary
        {
            public string DayStatus { get; set; }
            public string DayStatusDescription { get; set; }
            public int Total { get; set; }
            public string Color { get; set; }
        }

        public class DailyAttendanceData
        {
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string WorkTime { get; set; }
            public string InTime { get; set; }
            public string OutTime { get; set; }
            public string DailyHour { get; set; }
            public string OdInTime { get; set; }
            public string OdOutTime { get; set; }
            public string OdHour { get; set; }
            public string OverTime { get; set; }
            public string LateIn { get; set; }
            public string LateOut { get; set; }
            public string EarlyIn { get; set; }
            public string EarlyOut { get; set; }
            public string Remarks { get; set; }
        }

        public class AttendanceInputData
        {
            public string Date { get; set; }
            public string Status { get; set; }
            public int? LeaveId { get; set; }
        }
    }
}

