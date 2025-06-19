using CsvHelper.Configuration.Attributes;
using Hrms.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.HSSF.UserModel;
using NPOI.OpenXmlFormats.Dml.Chart;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "super-admin")]

    public class PayBillsController : Controller
    {
        private readonly DataContext _context;
        private readonly IAttendnaceService _attendancenaceService;

        public PayBillsController(DataContext context, IAttendnaceService attendnaceService)
        {
            _context = context;
            _attendancenaceService = attendnaceService;
        }

        [HttpGet()]
        public async Task<IActionResult> Get(int? page, int? limit, int? year, int? month, string empCode, string name)
        {
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);

            DateOnly date = new DateOnly(year ?? currentDate.Year, month ?? currentDate.Month, 1);
            DateOnly endOfMonth = date.AddMonths(1).AddDays(-1);

            if (!await _context.PayBills.AnyAsync(x => x.FromDate == date))
            {
                return ErrorHelper.ErrorResult("Id", "Paybill not generated for this month.");
            }

            var salaryHeads = await _context.SalaryHeads
                .Include(x => x.ShCategory)
                .ToListAsync();

            var query = _context.PayBills
                .Include(x => x.Emp)
                .Where(x => x.FromDate == date)
                .Join(_context.EmpLogs,
                    pb => pb.EmpId,
                    el => el.EmployeeId,
                    (pb, el) => new
                    {
                        PayBillId = pb.PayBillId,
                        EmpId = pb.EmpId,
                        PId = el.Id,
                        EmpCode = pb.Emp.CardId,
                        FirstName = pb.Emp.FirstName,
                        MiddleName = pb.Emp.MiddleName,
                        LastName = pb.Emp.LastName,
                        EmpName = Helper.FullName(pb.Emp.FirstName, pb.Emp.MiddleName, pb.Emp.LastName),
                        JoinDate = pb.Emp.JoinDate,
                        PanNumber = pb.Emp.PanNumber,
                        AadharNumber = pb.Emp.AadharNumber,
                        PayBillSalaryHeads = pb.PayBillSalaryHeads,
                        IsRun = pb.IsRun
                    })
                .Join(_context.EmpTransactions,
                    x => x.PId,
                    et => et.Id,
                    (x, et) => new
                    {
                        PayBillId = x.PayBillId,
                        CompanyId = et.CompanyId,
                        EmpId = x.EmpId,
                        EmpCode = x.EmpCode,
                        FirstName = x.FirstName,
                        MiddleName = x.MiddleName,
                        LastName = x.LastName,
                        EmpName = x.EmpName,
                        JoinDate = x.JoinDate,
                        PanNumber = x.PanNumber,
                        AadharNumber = x.AadharNumber,
                        AccountNumber = et.AccountNumber,
                        PayBillSalaryHeads = x.PayBillSalaryHeads,
                        IsRun = x.IsRun
                    })
                .AsQueryable();

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                query = query.Where(x => companyIds.Contains(x.CompanyId ?? 0));
            }

            if (!string.IsNullOrEmpty(empCode))
            {
                query = query.Where(x => x.EmpCode!.ToLower().Contains(empCode.ToLower()));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => string.Concat(x.FirstName, x.MiddleName, x.LastName).ToLower().Contains(name.Replace(" ", string.Empty).ToLower()));
            }

            page ??= 1;
            limit ??= 10;

            var TotalCount = await query.CountAsync();
            var TotalPages = (int)Math.Ceiling(TotalCount / (double)page);
            var paybills = await query.Skip((page - 1) * limit ?? 10).Take(limit ?? 10).ToListAsync();

            List<PayBillDTO> data = new();

            foreach (var payBill in paybills)
            {
                double payableDays = await _attendancenaceService.CalculateAttendance(payBill.EmpId, date, endOfMonth);

                var payBillSalaryHeads = await _context.PayBillSalaryHeads
                    .Include(x => x.EmpSalaryHead)
                    .Where(x => x.PayBillId == payBill.PayBillId)
                    .ToListAsync();

                List<ShDTO> earnings = new();
                List<ShDTO> deductions = new();
                List<ShDTO> contributions = new();

                foreach(var salaryHead in salaryHeads)
                {
                    var sh = payBillSalaryHeads.Where(x => x.EmpSalaryHead.ShId == salaryHead.ShId).FirstOrDefault();

                    if (salaryHead.ShCategory.Shc_Type == "EARNING")
                    {

                        earnings.Add(new ShDTO
                        {
                            PayBillShId = sh?.PayBillShId,
                            Name = salaryHead.Name,
                            TotalAmount = sh?.TotalAmount,
                            TotalUnit = sh?.TotalUnit,
                            IsPerUnit = sh?.EmpSalaryHead.ShDataType == "PERUNIT",
                            IsAssigned = sh is not null
                        });
                    }

                    if (salaryHead.ShCategory.Shc_Type == "DEDUCTION")
                    {
                        deductions.Add(new ShDTO
                        {
                            PayBillShId = sh?.PayBillShId,
                            Name = salaryHead.Name,
                            TotalAmount = sh?.TotalAmount,
                            TotalUnit = sh?.TotalUnit,
                            IsPerUnit = sh?.EmpSalaryHead.ShDataType == "PERUNIT",
                            IsAssigned = sh is not null
                        });

                        contributions.Add(new ShDTO
                        {
                            PayBillShId = sh?.PayBillShId,
                            Name = salaryHead.Name,
                            TotalAmount = sh?.OfficeContributionAmount,
                            TotalUnit = null,
                            IsPerUnit = false,
                            IsAssigned = sh?.EmpSalaryHead.HasOfficeContribution ?? false,
                        });
                    }
                }

                decimal? totalEarning = earnings.Sum(x => x.TotalAmount);
                decimal? totalDeduction = deductions.Sum(x => x.TotalAmount);
                decimal? totalContribution = contributions.Sum(x => x.TotalAmount);
                decimal grossSalary = (totalEarning ?? 0) + (totalContribution ?? 0);
                decimal netSalary = grossSalary - (totalDeduction ?? 0);

                data.Add(new PayBillDTO
                {
                    PayBillId = payBill.PayBillId,
                    IsRun = payBill.IsRun,
                    EmpCode = payBill.EmpCode,
                    EmpName = payBill.EmpName,
                    JoinDate = payBill.JoinDate,
                    PanNumber = payBill.PanNumber,
                    AadharNumber = payBill.AadharNumber,
                    AccountNumber = payBill.AccountNumber,
                    PayableDays = payableDays,
                    Earnings = earnings,
                    TotalEarning = totalEarning,
                    Deductions = deductions,
                    TotalDeduction = totalDeduction,
                    Contributions = contributions,
                    TotalContribution = totalContribution,
                    GrossSalary = grossSalary,
                    NetSalary = netSalary,
                });
            }

            return Ok(new
            {
                Data = data,
                TotalCount,
                TotalPages
            });

        }

        [HttpPost()]
        public async Task<IActionResult> Generate(GenerateInputModel input)
        {
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly date = new DateOnly(input.Year, input.Month, 1);
            int days = DateTime.DaysInMonth(input.Year, input.Month);
            DateOnly toDate = date.AddDays(days - 1);

            if (await _context.PayBills.AnyAsync(x => x.FromDate == date))
            {
                return ErrorHelper.ErrorResult("Month", "Pay bill already generated.");
            }

            if (date > currentDate)
            {
                return ErrorHelper.ErrorResult("Month", "Cannot generate for future date.");
            }

            var empLogs = await _context.EmpLogs
                .Join(_context.EmpTransactions.Where(x => x.StatusId == 1),
                    el => el.Id,
                    et => et.Id,
                    (el, et) => new
                    {
                        EmpId = el.EmployeeId,
                    })
                .ToListAsync();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach(var emp in empLogs)
                {
                    double payableDays = await _attendancenaceService.CalculateAttendance(emp.EmpId ?? 0, date, toDate);

                    PayBill payBill = new PayBill()
                    {
                        EmpId = emp.EmpId ?? 0,
                        FromDate = date,
                        ToDate = toDate,
                        CreatedByUserId = User.GetUserId(),
                    };

                    _context.Add(payBill);
                    await _context.SaveChangesAsync();

                    var empSalaryRecord = await _context.EmpSalaryRecords
                        .Include(x => x.EmpSalaryHeads)
                        .Where(x => x.EmpId == emp.EmpId && x.FromDate <= date)
                        .OrderByDescending(x => x.FromDate)
                        .FirstOrDefaultAsync();

                    if (empSalaryRecord == null)
                    {
                        continue;
                    }

                    foreach(var empSh in empSalaryRecord.EmpSalaryHeads)
                    {
                        decimal totalAmount = ((empSh.Amount / days) * (decimal)payableDays) ?? 0;
                        decimal officeContribution = ((empSh.OfficeContribution / days) * (decimal)payableDays) ?? 0;

                        _context.Add(new PayBillSalaryHead
                        {
                            EmpShId = empSh.EmpShId,
                            PayBillId = payBill.PayBillId,
                            OfficeContributionAmount = officeContribution,
                            TotalUnit= empSh.ShDataType== "PERUNIT"?(decimal)payableDays:null,
                            TotalAmount = totalAmount
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok();
            } catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
                //return ErrorHelper.ErrorResult("Id", "System error. Please contact administrator.");
            }
        }

        [HttpPost("RunPayroll")]
        public async Task<IActionResult> RunPayRoll(RunPayrollInputModel input)
        {
            DateOnly date = new DateOnly(input.Year, input.Month, 1);

            var payBill = await _context.PayBills.FirstOrDefaultAsync(x => x.FromDate == date);

            if (payBill == null) 
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            payBill.IsRun = true;
            payBill.RunByUserId = User.GetUserId();

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("UpdateAmount/{payBillShId}")]
        public async Task<IActionResult> UpdateAmount(int payBillShId, UpdateAmountInputModel input)
        {
            var payBillSh = await _context.PayBillSalaryHeads
                .Include(x => x.PayBill)
                .FirstOrDefaultAsync(x => x.PayBillId == payBillShId);

            if (payBillSh is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (payBillSh.PayBill.IsRun)
            {
                return ErrorHelper.ErrorResult("Id", "Payroll already run.");
            }

            payBillSh.TotalAmount = input.TotalAmount;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("UpdateUnit/{payBillShId}")]
        public async Task<IActionResult> UpdateUnit(int payBillShId, UpdateUnitInputModel input)
        {
            var payBillSh = await _context.PayBillSalaryHeads
                .Include(x => x.EmpSalaryHead)
                .Include(x => x.PayBill)
                .FirstOrDefaultAsync(x => x.PayBillShId == payBillShId);

            if (payBillSh is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (payBillSh.PayBill.IsRun)
            {
                return ErrorHelper.ErrorResult("Id", "Payroll already run.");
            }

            if (payBillSh.EmpSalaryHead.ShDataType != "PERUNIT")
            {
                return ErrorHelper.ErrorResult("TotalUnit", "Invlid salary head type.");
            }

            payBillSh.TotalUnit = input.TotalUnit;
            payBillSh.TotalAmount = payBillSh.EmpSalaryHead.PerUnitRate * input.TotalUnit;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("Export")]
        public async Task<IActionResult> ExportExcel(int? year, int? month)
        {
            DateTime currentDate = DateTime.Now;

            DateOnly date = new DateOnly(year ?? currentDate.Year, month ?? currentDate.Month, 1);
            DateOnly endOfMonth = date.AddMonths(1).AddDays(-1);

            var paybills = await _context.PayBills
                .Include(x => x.Emp)
                .Where(x => x.FromDate == date)
                .Join(_context.EmpLogs,
                    pb => pb.EmpId,
                    el => el.EmployeeId,
                    (pb, el) => new
                    {
                        PayBillId = pb.PayBillId,
                        EmpId = pb.EmpId,
                        PId = el.Id,
                        EmpCode = pb.Emp.CardId,
                        EmpName = Helper.FullName(pb.Emp.FirstName, pb.Emp.MiddleName, pb.Emp.LastName),
                        JoinDate = pb.Emp.JoinDate,
                        PanNumber = pb.Emp.PanNumber,
                        AadharNumber = pb.Emp.AadharNumber,
                        PayBillSalaryHeads = pb.PayBillSalaryHeads
                    })
                .Join(_context.EmpTransactions,
                    x => x.PId,
                    et => et.Id,
                    (x, et) => new
                    {
                        PayBillId = x.PayBillId,
                        EmpId = x.EmpId,
                        EmpCode = x.EmpCode,
                        EmpName = x.EmpName,
                        JoinDate = x.JoinDate,
                        PanNumber = x.PanNumber,
                        AadharNumber = x.AadharNumber,
                        AccountNumber = et.AccountNumber,
                        PayBillSalaryHeads = x.PayBillSalaryHeads
                    })
            .ToListAsync();

            if (!paybills.Any())
            {
                return ErrorHelper.ErrorResult("Id", "No records found.");
            }

            var salaryHeads = await _context.SalaryHeads
                .Include(x => x.ShCategory)
                .ToListAsync();

            List<PayBillDTO> data = new();

            HSSFWorkbook workbook = new();

            HSSFFont headerFont = (HSSFFont)workbook.CreateFont();
            HSSFFont normalFont = (HSSFFont)workbook.CreateFont();

            headerFont.IsBold = true;
            headerFont.FontHeightInPoints = 10;
            headerFont.FontName = "Tahoma";

            normalFont.FontHeightInPoints = 10;

            HSSFCellStyle headerCellStyle = NpoiHelper.BorderedCellStyle(workbook, headerFont);
            HSSFCellStyle normalCellStyle = NpoiHelper.NormalCellStyle(workbook, normalFont);

            ISheet Sheet = workbook.CreateSheet("Pay Bill");

            IRow headerRow = Sheet.CreateRow(0);

            Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 0, 6));
            NpoiHelper.CreateCell(headerRow, 0, "Employee Detail", headerCellStyle);

            int earningShCount = salaryHeads.Count(x => x.ShCategory.Shc_Type == "EARNING");
            int deductionShCount = salaryHeads.Count(x => x.ShCategory.Shc_Type == "DEDUCTION");
            int startingCell = 7;

            Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, startingCell, startingCell + earningShCount));
            NpoiHelper.CreateCell(headerRow, startingCell, "Earnings", headerCellStyle);

            startingCell = startingCell + earningShCount + 1;

            if (deductionShCount != 0)
            {
                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, startingCell, startingCell + deductionShCount));
                NpoiHelper.CreateCell(headerRow, startingCell, "Deductions", headerCellStyle);

                startingCell = startingCell + deductionShCount + 1;

                Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, startingCell, startingCell + deductionShCount));
                NpoiHelper.CreateCell(headerRow, startingCell, "Contributions", headerCellStyle);

                startingCell = startingCell + deductionShCount + 1;
            }

            Sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, startingCell, startingCell + 1));
            NpoiHelper.CreateCell(headerRow, startingCell, "Summary", headerCellStyle);

            var cra = new NPOI.SS.Util.CellRangeAddress(0, 0, 0, startingCell + 1);
            RegionUtil.SetBorderBottom(1, cra, Sheet);//Bottom border  
            RegionUtil.SetBorderLeft(1, cra, Sheet);//Left border  
            RegionUtil.SetBorderRight(1, cra, Sheet);//Right border  
            RegionUtil.SetBorderTop(1, cra, Sheet);

            IRow subHeaderRow = Sheet.CreateRow(1);

            NpoiHelper.CreateCell(subHeaderRow, 0, "Emp Code", headerCellStyle);
            NpoiHelper.CreateCell(subHeaderRow, 1, "Emp Name", headerCellStyle);
            NpoiHelper.CreateCell(subHeaderRow, 2, "Join Date", headerCellStyle);
            NpoiHelper.CreateCell(subHeaderRow, 3, "Pan Number", headerCellStyle);
            NpoiHelper.CreateCell(subHeaderRow, 4, "Aadhar Number", headerCellStyle);
            NpoiHelper.CreateCell(subHeaderRow, 5, "Account Number", headerCellStyle);
            NpoiHelper.CreateCell(subHeaderRow, 6, "Payable Days", headerCellStyle);

            int columnCount = 7;

            foreach (var salaryHead in salaryHeads.Where(x => x.ShCategory.Shc_Type == "EARNING"))
            {
                NpoiHelper.CreateCell(subHeaderRow, columnCount, salaryHead.Name, headerCellStyle);
                columnCount++;
            }

            NpoiHelper.CreateCell(subHeaderRow, columnCount, "Total Earnings", headerCellStyle);
            columnCount++;

            if (deductionShCount != 0)
            {
                foreach (var salaryHead in salaryHeads.Where(x => x.ShCategory.Shc_Type == "DEDUCTION"))
                {
                    NpoiHelper.CreateCell(subHeaderRow, columnCount, salaryHead.Name, headerCellStyle);
                    columnCount++;
                }

                NpoiHelper.CreateCell(subHeaderRow, columnCount, "Total Deductions", headerCellStyle);
                columnCount++;

                foreach (var salaryHead in salaryHeads.Where(x => x.ShCategory.Shc_Type == "DEDUCTION"))
                {
                    NpoiHelper.CreateCell(subHeaderRow, columnCount, salaryHead.Name, headerCellStyle);
                    columnCount++;
                }

                NpoiHelper.CreateCell(subHeaderRow, columnCount, "Total Conributions", headerCellStyle);
                columnCount++;
            }

            NpoiHelper.CreateCell(subHeaderRow, columnCount, "Gross Salary", headerCellStyle);
            columnCount++;

            NpoiHelper.CreateCell(subHeaderRow, columnCount, "Net Salary", headerCellStyle);

            int rowCount = 2;

            foreach (var payBill in paybills)
            {
                columnCount = 7;

                IRow row = Sheet.CreateRow(rowCount);

                double payableDays = await _attendancenaceService.CalculateAttendance(payBill.EmpId, date, endOfMonth);

                NpoiHelper.CreateCell(row, 0, payBill.EmpCode, normalCellStyle);
                NpoiHelper.CreateCell(row, 1, payBill.EmpName, normalCellStyle);
                NpoiHelper.CreateCell(row, 2, payBill.JoinDate.ToString(), normalCellStyle);
                NpoiHelper.CreateCell(row, 3, payBill.PanNumber, normalCellStyle);
                NpoiHelper.CreateCell(row, 4, payBill.AadharNumber, normalCellStyle);
                NpoiHelper.CreateCell(row, 5, payBill.AccountNumber, normalCellStyle);
                NpoiHelper.CreateCell(row, 6, payableDays.ToString(), normalCellStyle);

                var payBillSalaryHeads = await _context.PayBillSalaryHeads
                    .Include(x => x.EmpSalaryHead)
                    .Where(x => x.PayBillId == payBill.PayBillId)
                    .ToListAsync();

                decimal totalEarnings = 0;
                decimal totalDeductions = 0;
                decimal totalContirbutions = 0;
                decimal grossSalary = 0;
                decimal netSalary = 0;

                foreach (var salaryHead in salaryHeads.Where(x => x.ShCategory.Shc_Type == "EARNING"))
                {
                    var sh = payBillSalaryHeads.Where(x => x.EmpSalaryHead.ShId == salaryHead.ShId).FirstOrDefault();

                    NpoiHelper.CreateCell(row, columnCount, sh?.TotalAmount, normalCellStyle);
                    columnCount++;

                    totalEarnings += (sh?.TotalAmount ?? 0);
                }

                NpoiHelper.CreateCell(row, columnCount, totalEarnings, normalCellStyle);
                columnCount++;

                if (deductionShCount != 0)
                {
                    foreach (var salaryHead in salaryHeads.Where(x => x.ShCategory.Shc_Type == "DEDUCTION"))
                    {
                        var sh = payBillSalaryHeads.Where(x => x.EmpSalaryHead.ShId == salaryHead.ShId).FirstOrDefault();

                        NpoiHelper.CreateCell(row, columnCount, sh?.TotalAmount, normalCellStyle);
                        columnCount++;

                        totalDeductions += (sh?.TotalAmount ?? 0);
                    }

                    NpoiHelper.CreateCell(row, columnCount, totalDeductions, normalCellStyle);
                    columnCount++;

                    foreach (var salaryHead in salaryHeads.Where(x => x.ShCategory.Shc_Type == "DEDUCTION"))
                    {
                        var sh = payBillSalaryHeads.Where(x => x.EmpSalaryHead.ShId == salaryHead.ShId).FirstOrDefault();

                        NpoiHelper.CreateCell(row, columnCount, sh?.OfficeContributionAmount, normalCellStyle);
                        columnCount++;

                        totalContirbutions += (sh?.OfficeContributionAmount ?? 0);
                    }

                    NpoiHelper.CreateCell(row, columnCount, totalContirbutions, normalCellStyle);
                    columnCount++;
                }

                grossSalary = totalEarnings + totalContirbutions;
                netSalary = grossSalary - totalDeductions;

                NpoiHelper.CreateCell(row, columnCount, grossSalary, normalCellStyle);
                columnCount++;
                NpoiHelper.CreateCell(row, columnCount, netSalary, normalCellStyle);
                columnCount++;

                rowCount++;
            }

            //Auto sized all the affected columns
            int lastColumNum = Sheet.GetRow(1).LastCellNum;
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
             "PayBill.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        public class ShDTO
        {
            public int? PayBillShId { get; set; }
            public string Name { get; set; }
            public decimal? TotalAmount { get; set; }
            public decimal? TotalUnit { get; set; }
            public bool IsPerUnit { get; set; }
            public bool IsAssigned { get; set; }
        }

        public class PayBillDTO
        {
            public int PayBillId { get; set; }
            public bool IsRun { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public DateOnly? JoinDate { get; set; }
            public string PanNumber { get; set; }
            public string AadharNumber { get; set; }
            public string AccountNumber { get; set; }
            public double PayableDays { get; set; }
            public List<ShDTO> Earnings { get; set; }
            public decimal? TotalEarning { get; set; }
            public List<ShDTO> Deductions { get; set; }
            public decimal? TotalDeduction { get; set; }
            public List<ShDTO> Contributions { get; set; }
            public decimal? TotalContribution { get; set; }
            public decimal GrossSalary { get; set; }
            public decimal NetSalary { get; set; }
        }

        public class GenerateInputModel
        {
            public int Year { get; set; }
            public int Month { get; set; }
        }

        public class UpdateUnitInputModel
        {
            public decimal? TotalUnit { get; set; }
        }

        public class UpdateAmountInputModel
        {
            public decimal? TotalAmount { get; set; }
        }

        public class RunPayrollInputModel
        {
            public int Year { get; set; }
            public int Month { get; set; }
        }
    }
}
