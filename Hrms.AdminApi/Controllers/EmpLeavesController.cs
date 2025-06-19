using CsvHelper;
using CsvHelper.Configuration.Attributes;
using FluentValidation;
using Hrms.Common.Helpers;
using Hrms.Common.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Ocsp;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Security.Cryptography;
using static Hrms.AdminApi.Controllers.EmpTransactionsController;
using static Hrms.AdminApi.Controllers.LeaveManagementsController;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [CustomAuthorize("employee-leave-management")]

    public class EmpLeavesController : Controller
    {
        private readonly DataContext _context;

        public EmpLeavesController(DataContext context)
        {
            _context = context;
        }

        // Get all leaves assigned to the employee
        [HttpGet("All/{empId}")]
        public async Task<IActionResult> Get(int empId)
        {
            var empLog = await _context.EmpLogs.Where(x => x.EmployeeId == empId).FirstOrDefaultAsync();

            if (empLog is null)
            {
                return ErrorHelper.ErrorResult("Id", "Emp is not registered.");
            }

            var empTran = await _context.EmpTransactions.FirstOrDefaultAsync(x => x.Id == empLog.Id);

            if (empTran is null)
            {
                return ErrorHelper.ErrorResult("Id", "Transaction record does not exist.");
            }

            if (empTran.CompanyId == null)
            {
                return ErrorHelper.ErrorResult("Id", "No company assigned to the employee.");
            }

            var leaveYear = await _context.LeaveYearCompanies
                .Where(x => x.CompanyId == empTran.CompanyId || x.CompanyId == null)
                .OrderByDescending(x => x.CompanyId)
                .FirstOrDefaultAsync();

            if (leaveYear is null)
            {
                return ErrorHelper.ErrorResult("Id", "No leave year defined.");
            }

            var empLeaves = await _context.EmpLeaves
                .Where(x => x.PId == empLog.Id)
                .Include(x => x.Leave)
                .Select(x => new
                {
                    Id = x.Id,
                    PId = x.PId,
                    LeaveId = x.LeaveId,
                    LeaveName = x.Leave.Name,
                    LeaveEarnType = x.Leave.LeaveEarnType,
                    MaxDays = x.MaxDays,
                    Days = x.Days,
                })
                .ToListAsync();

            var leaveLedgers = await _context.LeaveLedgers
                .Where(x => x.EmpId == empId && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && !x.IsClosed)
                .GroupBy(x => x.LeaveId)
                .Select(x => new
                {
                    LeaveId = x.Key,
                    Credited = x.Sum(x => x.Given),
                    Availed = x.Sum(x => x.Taken)
                })
                .ToListAsync();

            var pendingLeaves = await _context.LeaveApplicationHistories
                .Where(x => x.EmpId == empId && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && !x.IsClosed && x.Status == "pending")
                .GroupBy(x => x.LeaveId)
                .Select(x => new
                {
                    LeaveId = x.Key,
                    TotalPending = x.Sum(y => y.TotalDays)
                })
                .ToListAsync();

            List<LeaveSummary> data = new();

            foreach(var empLeave in empLeaves)
            {
                var leaveLedger = leaveLedgers.FirstOrDefault(x => x.LeaveId == empLeave.LeaveId);
                var pendingLeave = pendingLeaves.FirstOrDefault(x => x.LeaveId == empLeave.LeaveId);
                var yearlyCredited = await _context.LeaveLedgers
                    .Where(x => x.LeaveId == empLeave.LeaveId && x.EmpId == empId && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && x.IsYearly)
                    .Select(x => x.Given)
                    .FirstOrDefaultAsync();

                data.Add(new LeaveSummary
                {
                    Id = empLeave.Id,
                    PId = empLeave.PId,
                    LeaveId = empLeave.LeaveId,
                    LeaveName = empLeave.LeaveName,
                    LeaveEarnType = empLeave.LeaveEarnType,
                    Days = empLeave.Days,
                    LeaveMax = empLeave.MaxDays,
                    YearlyCredited = yearlyCredited,
                    Credited = leaveLedger?.Credited,
                    Availed = leaveLedger?.Availed,
                    Pending = pendingLeave?.TotalPending,
                    Balance = leaveLedger?.Credited - leaveLedger?.Availed - (pendingLeave != null ? pendingLeave.TotalPending : 0)
                });
            }

            return Ok(new
            {
                Data = data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmpLEave(int id)
        {
            var empLeave = await _context.EmpLeaves
                .Include(x => x.Leave)
                .Select(x => new
                {
                    Id = x.Id,
                    PId = x.PId,
                    LeaveId = x.LeaveId,
                    LeaveName = x.Leave.Name,
                    Days = x.Days,
                    LeaveMax = x.MaxDays,
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            if (empLeave is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            var empLog = await _context.EmpLogs.FirstOrDefaultAsync(x => x.Id == empLeave.PId);

            if (empLog is null)
            {
                return ErrorHelper.ErrorResult("Id", "Cannot update old records.");
            }

            var empTran = await _context.EmpTransactions.FirstOrDefaultAsync(x => x.Id == empLog.Id);

            if (empTran is null)
            {
                return ErrorHelper.ErrorResult("Id", "Transaction record does not exist.");
            }

            if (empTran.CompanyId == null)
            {
                return ErrorHelper.ErrorResult("Id", "No company assigned to the employee.");
            }

            var leaveYear = await _context.LeaveYearCompanies
                .Where(x => x.CompanyId == empTran.CompanyId || x.CompanyId == null)
                .OrderByDescending(x => x.CompanyId)
                .FirstOrDefaultAsync();

            if (leaveYear is null)
            {
                return ErrorHelper.ErrorResult("Id", "No leave year defined.");
            }

            var leaveLedger = await _context.LeaveLedgers
                .Where(x => x.EmpId == empLog.EmployeeId && x.LeaveId == empLeave.LeaveId && x.IsYearly == true && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && !x.IsClosed)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                EmpLeave = new
                {
                    Id = empLeave.Id,
                    PId = empLeave.PId,
                    LeaveId = empLeave.LeaveId,
                    LeaveName = empLeave.LeaveName,
                    Days = empLeave.Days,
                    LeaveMax = empLeave.LeaveMax,
                    YearlyBalance = leaveLedger?.Given,
                }
            });
        }

        [HttpPost("CalculateLeave")]
        public async Task<IActionResult> CalculateLeaves(CalculateLeaveModel input)
        {
            var empLog = await _context.EmpLogs.FirstOrDefaultAsync(x => x.Id == input.EmpId);

            if (empLog is null)
            {
                return ErrorHelper.ErrorResult("Id", "Cannot update old records.");
            }

            var empTran = await _context.EmpTransactions.FirstOrDefaultAsync(x => x.Id == empLog.Id);

            if (empTran is null)
            {
                return ErrorHelper.ErrorResult("Id", "Transaction record does not exist.");
            }

            if (empTran.CompanyId == null)
            {
                return ErrorHelper.ErrorResult("Id", "No company assigned to the employee.");
            }

            var leaveYear = await _context.LeaveYearCompanies
                .Where(x => x.CompanyId == empTran.CompanyId || x.CompanyId == null)
                .OrderByDescending(x => x.CompanyId)
                .FirstOrDefaultAsync();

            if (leaveYear is null)
            {
                return ErrorHelper.ErrorResult("Id", "No leave year defined.");
            }

            var leaveYearMonth = await _context.LeaveYearMonths.Where(x => x.LeaveYearId == leaveYear.LeaveYearId && x.Month == DateTime.Now.Month).SingleOrDefaultAsync();

            decimal balance = (13 - leaveYearMonth.MonthSequence) * ((decimal)input.Days / (decimal)12);

            return Ok(new
            {
                Balance = balance,
            });
        }

        // Post: EmpLeaves/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var empLog = await _context.EmpLogs.Where(x => x.EmployeeId == input.EmpId).FirstOrDefaultAsync();

            if (empLog is null)
            {
                return ErrorHelper.ErrorResult("EmpId", "Employee is not registered");
            }

            if (await _context.EmpLeaves.AnyAsync(x => x.PId == empLog.Id && x.LeaveId == input.LeaveId))
            {
                return ErrorHelper.ErrorResult("LeaveId", "Leave already assigned to the employee.");
            }

            var empTran = await _context.EmpTransactions.FirstOrDefaultAsync(x => x.Id == empLog.Id);

            if (empTran is null)
            {
                return ErrorHelper.ErrorResult("Id", "Transaction record does not exist.");
            }

            if (empTran.CompanyId == null)
            {
                return ErrorHelper.ErrorResult("Id", "No company assigned to the employee.");
            }

            var leaveYear = await _context.LeaveYearCompanies
                .Where(x => x.CompanyId == empTran.CompanyId || x.CompanyId == null)
                .OrderByDescending(x => x.CompanyId)
                .FirstOrDefaultAsync();

            if (leaveYear is null)
            {
                return ErrorHelper.ErrorResult("Id", "No leave year defined.");
            }

            var leave = await _context.Leaves.FindAsync(input.LeaveId);

            EmpLeave data = new()
            {
                PId = empLog.Id,
                LeaveId = (short)input.LeaveId,
                Days = (short)input.Days,
                MaxDays = leave.LeaveMax
            };

            LeaveLedger leaveLedger = new()
            {
                EmpId = input.EmpId,
                LeaveId = input.LeaveId,
                Given = input.Balance,
                GivenMonth = (short)DateTime.Now.Month,
                GivenYear = (short)DateTime.Now.Year,
                Remarks = "Initial Balance",
                ApprovedById = User.GetUserId(),
                TransactionUser = User.GetUsername(),
                LeaveYearId = leaveYear.LeaveYearId,
                CompanyId = empTran.CompanyId,
                IsYearly = true,

                //Default
                IsRegular = 1,
                Adjusted = 0,
                NoHrs = 0,
                HLeaveType = 0,
            };

            _context.Add(data);
            _context.Add(leaveLedger);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateInputModel input)
        {
            var empLeave = await _context.EmpLeaves
                .Include(x => x.Leave)
                .FirstOrDefaultAsync(x => x.Id == id);

            var empLog = await _context.EmpLogs.FirstOrDefaultAsync(x => x.Id == empLeave.PId);

            if (empLog == null)
            {
                return ErrorHelper.ErrorResult("Id", "Cannot update old records.");
            }

            var empTran = await _context.EmpTransactions.FirstOrDefaultAsync(x => x.Id == empLog.Id);

            if (empTran is null)
            {
                return ErrorHelper.ErrorResult("Id", "Transaction record does not exist.");
            }

            if (empTran.CompanyId == null)
            {
                return ErrorHelper.ErrorResult("Id", "No company assigned to the employee.");
            }

            var leaveYear = await _context.LeaveYearCompanies
                .Where(x => x.CompanyId == empTran.CompanyId || x.CompanyId == null)
                .OrderByDescending(x => x.CompanyId)
                .FirstOrDefaultAsync();

            if (leaveYear is null)
            {
                return ErrorHelper.ErrorResult("Id", "No leave year defined.");
            }

            empLeave.Days = input.Days;
            empLeave.UpdatedAt = DateTime.Now;

            var leaveLedger = await _context.LeaveLedgers
                .Where(x => x.EmpId == empLog.EmployeeId && x.LeaveId == empLeave.LeaveId && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && !x.IsClosed)
                .GroupBy(x => x.LeaveId)
                .Select(x => new
                {
                    LeaveId = x.Key,
                    Credited = x.Sum(x => x.Given),
                    Availed = x.Sum(x => x.Taken)
                })
                .FirstOrDefaultAsync();

            var pendingLeave = await _context.LeaveApplicationHistories
                .Where(x => x.EmpId == empLog.EmployeeId && x.LeaveId == empLeave.LeaveId 
                    && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && !x.IsClosed 
                    && x.Status == "pending")
                .GroupBy(x => x.LeaveId)
                .Select(x => new
                {
                    LeaveId = x.Key,
                    TotalPending = x.Sum(y => y.TotalDays)
                })
                .FirstOrDefaultAsync();

            var yearlyLeaveLedger = await _context.LeaveLedgers
                .FirstOrDefaultAsync(x => x.EmpId == empLog.EmployeeId && x.LeaveId == empLeave.LeaveId 
                    && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && !x.IsClosed 
                    && x.IsYearly);

            if (yearlyLeaveLedger != null)
            {
                if (yearlyLeaveLedger.Given > input.Balance)
                {
                    decimal? balance = leaveLedger.Credited - leaveLedger.Availed - (pendingLeave is not null ? pendingLeave.TotalPending : 0);

                    if (balance - (yearlyLeaveLedger.Given - input.Balance) < 0)
                    {
                        return ErrorHelper.ErrorResult("Balance", "Not Enough Balance. Employee has taken more leaves than calculated balance.");
                    }
                }

                yearlyLeaveLedger.Given = input.Balance;
            } else
            {
                LeaveLedger newLeaveLedger = new()
                {
                    EmpId = empLog.EmployeeId ?? 0,
                    LeaveId = empLeave.LeaveId,
                    Given = input.Balance,
                    GivenMonth = (short)DateTime.Now.Month,
                    GivenYear = (short)DateTime.Now.Year,
                    Remarks = "Initial Balance",
                    ApprovedById = User.GetUserId(),
                    TransactionUser = User.GetUsername(),
                    LeaveYearId = leaveYear.LeaveYearId,
                    CompanyId = empTran.CompanyId,
                    IsYearly = true,

                    //Default
                    IsRegular = 1,
                    Adjusted = 0,
                    NoHrs = 0,
                    HLeaveType = 0,
                };

                _context.Add(newLeaveLedger);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        // IMPORT: LeaveManagements/import
        [HttpPost("Import")]
        public async Task<IActionResult> Import([FromForm] ImportModel input)
        {
            if (Path.GetExtension(input.File.FileName).ToLower() != ".csv")
            {
                return BadRequest("Invalid file type.");
            }

            List<Error> errors = new();

            using (var reader = new StreamReader(input.File.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<EmpLeaveHeader>().ToList();

                if (!records.Any())
                {
                    return BadRequest("No records found.");
                }

                bool isError = false;

                try
                {
                    int i = 2;

                    foreach (var record in records)
                    {
                        EmpLog? empLog = new();
                        EmpTransaction? empTran = new();
                        LeaveYearCompany? leaveYear = new();

                        isError = false;
                        List<string> errorsData = new();

                        var emp = await _context.EmpDetails.Where(x => x.CardId == record.EmpCode.Trim()).FirstOrDefaultAsync();
                        var leave = await _context.Leaves.Where(x => x.Abbreviation == record.LeaveAbbreviation.Trim()).FirstOrDefaultAsync();

                        if (leave is null)
                        {
                            errorsData.Add("Leave does not exist.");
                            isError = true;
                        }

                        if (record.YearlyDays is null or 0)
                        {
                            errorsData.Add("Please specify number of Yearly Days.");
                            isError = true;
                        }

                        if (record.Balance is null or 0)
                        {
                            errorsData.Add("Please specify leave balance.");
                            isError = true;
                        }

                        if (emp is null)
                        {
                            errorsData.Add("Employee does not exist.");
                            isError = true;
                        } else
                        {
                            empLog = await _context.EmpLogs.Where(x => x.EmployeeId == emp.Id).FirstOrDefaultAsync();

                            if (empLog is null)
                            {
                                errorsData.Add("Employee is not registered.");
                                isError = true;
                            } else
                            {
                                empTran = await _context.EmpTransactions.FirstOrDefaultAsync(x => x.Id == empLog.Id);

                                if (empTran is null)
                                {
                                    errorsData.Add("Transaction record does not exist.");
                                    isError = true;
                                } else
                                {
                                    if (empTran.CompanyId == null)
                                    {
                                        errorsData.Add("No company assigned to the employee.");
                                        isError = true;
                                    } else
                                    {
                                        leaveYear = await _context.LeaveYearCompanies
                                            .Where(x => x.CompanyId == empTran.CompanyId || x.CompanyId == null)
                                            .OrderByDescending(x => x.CompanyId)
                                            .FirstOrDefaultAsync();

                                        if (leaveYear is null)
                                        {
                                            errorsData.Add("No leave year defined.");
                                            isError = true;
                                        }
                                    }
                                }

                                if (User.GetUserRole() != "super-admin")
                                {
                                    var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                                    if (!companyIds.Contains(empTran?.CompanyId ?? 0))
                                    {
                                        errorsData.Add("Forbidden");
                                        isError = true;
                                    }
                                }
                            }
                        }

                        if (isError)
                        {
                            errors.Add(new Error
                            {
                                Record = i,
                                Errors = errorsData
                            });

                            i++;

                            continue;
                        }

                        var empLeave = await _context.EmpLeaves.Where(x => x.PId == empLog.Id && x.LeaveId == leave.Id).FirstOrDefaultAsync(); 

                        if (empLeave is not null)
                        {
                            empLeave.Days = record.YearlyDays ?? 0;
                            empLeave.UpdatedAt = DateTime.UtcNow;

                            var leaveLedger = await _context.LeaveLedgers
                                .Where(x => x.EmpId == empLog.EmployeeId && x.LeaveId == empLeave.LeaveId 
                                    && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && !x.IsClosed)
                                .GroupBy(x => x.LeaveId)
                                .Select(x => new
                                {
                                    LeaveId = x.Key,
                                    Credited = x.Sum(x => x.Given),
                                    Availed = x.Sum(x => x.Taken)
                                })
                                .FirstOrDefaultAsync();

                            var pendingLeave = await _context.LeaveApplicationHistories
                                .Where(x => x.EmpId == empLog.EmployeeId && x.LeaveId == empLeave.LeaveId 
                                    && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && !x.IsClosed
                                    && x.Status == "pending")
                                .GroupBy(x => x.LeaveId)
                                .Select(x => new
                                {
                                    LeaveId = x.Key,
                                    TotalPending = x.Sum(y => y.TotalDays)
                                })
                                .FirstOrDefaultAsync();

                            var yearlyLeaveLedger = await _context.LeaveLedgers
                                .FirstOrDefaultAsync(x => x.EmpId == empLog.EmployeeId && x.LeaveId == empLeave.LeaveId 
                                    && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && !x.IsClosed
                                    && x.IsYearly);

                            if (yearlyLeaveLedger is not null)
                            {
                                if (yearlyLeaveLedger.Given > record.Balance)
                                {
                                    decimal? balance = leaveLedger.Credited - leaveLedger.Availed - (pendingLeave is not null ? pendingLeave.TotalPending : 0);

                                    if (balance - (yearlyLeaveLedger.Given - record.Balance) < 0)
                                    {
                                        errorsData.Add("User has already taken more leaves than calculated balance.");
                                        errors.Add(new Error
                                        {
                                            Record = i,
                                            Errors = errorsData
                                        });

                                        i++;
                                        continue;
                                    }
                                }

                                yearlyLeaveLedger.Given = record.Balance;

                                i++;
                                continue;
                            }

                            _context.Add(new LeaveLedger
                            {
                                EmpId = emp.Id,
                                LeaveId = leave.Id,
                                Given = record.Balance,
                                GivenMonth = (short)DateTime.Now.Month,
                                GivenYear = (short)DateTime.Now.Year,
                                Remarks = record.Remarks,
                                ApprovedById = User.GetUserId(),
                                TransactionUser = User.GetUsername(),
                                LeaveYearId = leaveYear.LeaveYearId,
                                CompanyId = empTran.CompanyId,
                                IsYearly = true,

                                //Default
                                IsRegular = 1,
                                Adjusted = 0,
                                NoHrs = 0,
                                HLeaveType = 0,
                            });

                            i++;

                            continue;
                        }

                        _context.Add(new LeaveLedger
                        {
                            EmpId = emp.Id,
                            LeaveId = leave.Id,
                            Given = record.Balance,
                            GivenMonth = (short)DateTime.Now.Month,
                            GivenYear = (short)DateTime.Now.Year,
                            Remarks = record.Remarks,
                            ApprovedById = User.GetUserId(),
                            TransactionUser = User.GetUsername(),
                            LeaveYearId = leaveYear.LeaveYearId,
                            CompanyId = empTran.CompanyId,
                            IsYearly = true,

                            //Default
                            IsRegular = 1,
                            Adjusted = 0,
                            NoHrs = 0,
                            HLeaveType = 0,
                        });

                        _context.Add(new EmpLeave
                        {
                            PId = empLog.Id,
                            Days = record.YearlyDays ?? 0,
                            MaxDays = leave.LeaveMax,
                            LeaveId = leave.Id
                        });

                        i++;
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Errors = errors
            });
        }

        [HttpGet("DownloadFormat")]
        public async Task<IActionResult> DownloadFormat()
        {
            Type table = typeof(EmpLeaveHeader);

            byte[] data;

            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader(table);

                    csv.NextRecord();
                }

                data = stream.ToArray();
            }

            return File(data, "text/csv", "EmpLeaves.csv");
        }

        // DELETE: EmpTransactions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.EmpLeaves
                .Include(x => x.Leave)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            var empLog = await _context.EmpLogs.FindAsync(data.PId);

            if (empLog is null)
            {
                return ErrorHelper.ErrorResult("Id", "Cannot delete old records.");
            }

            var empTran = await _context.EmpTransactions.FirstOrDefaultAsync(x => x.Id == empLog.Id);

            if (empTran is null)
            {
                return ErrorHelper.ErrorResult("Id", "Transaction record does not exist.");
            }

            if (empTran.CompanyId == null)
            {
                return ErrorHelper.ErrorResult("Id", "No company assigned to the employee.");
            }

            var leaveYear = await _context.LeaveYearCompanies
                .Where(x => x.CompanyId == empTran.CompanyId || x.CompanyId == null)
                .OrderByDescending(x => x.CompanyId)
                .FirstOrDefaultAsync();

            if (leaveYear is null)
            {
                return ErrorHelper.ErrorResult("Id", "No leave year defined.");
            }

            if (await _context.LeaveLedgers.AnyAsync(x => x.EmpId == empLog.EmployeeId && x.LeaveId == data.LeaveId && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId && x.Taken > 0))
            {
                return ErrorHelper.ErrorResult("Id", "Employee has already taken this leave.");
            }

            if (data.Leave.LeaveEarnType != 0)
            {
                var leaveEarns = await _context.EarnedLeaves.Where(x => x.EmpId == empLog.EmployeeId && x.LeaveId == data.LeaveId).ToListAsync();

                _context.RemoveRange(leaveEarns);
            }

            var empLeaveLedger = await _context.LeaveLedgers
                .Where(x => x.EmpId == empLog.EmployeeId && x.LeaveId == data.LeaveId && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == empTran.CompanyId)
                .ToListAsync();

            _context.EmpLeaves.Remove(data);
            _context.LeaveLedgers.RemoveRange(empLeaveLedger);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class LeaveSummary
        {
            public int Id { get; set; }
            public int PId { get; set; }
            public short LeaveId { get; set; }
            public string LeaveName { get; set; }
            public byte LeaveEarnType { get; set; }
            public short? LeaveMax { get; set; }
            public short Days { get; set; }
            public decimal? Credited { get; set; }
            public decimal? Availed { get; set; }
            public decimal? Pending { get; set; }
            public decimal? YearlyCredited { get; set; }
            public decimal? Balance { get; set; }
        }

        public class BaseInputModel
        {
            public short Days { get; set; }
            public decimal Balance { get; set; }
        }

        public class AddInputModel : BaseInputModel 
        {
            public int EmpId { get; set; }
            public short LeaveId { get; set; }
        }

        public class UpdateInputModel : BaseInputModel { }

        public class CalculateLeaveModel
        {
            public int EmpId { get; set; }
            public short LeaveId { get; set; }
            public int Days { get; set; }
        }

        public class EmpLeaveHeader
        {
            [Name("EMPCODE")]
            public string EmpCode { get; set; }

            [Name("LEAVEABBREVIATION")]
            public string LeaveAbbreviation { get; set; }

            [Name("YEARLYDAYS")]
            public short? YearlyDays { get; set; }

            [Name("BALANCE")]
            public decimal? Balance { get; set; }

            [Name("REMARKS")]
            public string Remarks { get; set; }
        }

        public class ImportModel
        {
            public IFormFile File { get; set; }
        }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.LeaveId)
                    .NotEmpty()
                    .IdMustExist(_context.Leaves.AsQueryable());

                RuleFor(x => x.Days)
                    .NotEmpty();

                RuleFor(x => x.Balance)
                    .NotEmpty();
            }
        }

        public class UpdateInputModelValidator : AbstractValidator<UpdateInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public UpdateInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

                RuleFor(x => x.Days)
                    .NotEmpty();

                RuleFor(x => x.Balance)
                    .NotEmpty();

            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.EmpLeaves.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }

        public class CalculateLeaveModelValidator : AbstractValidator<CalculateLeaveModel>
        {
            private readonly DataContext _context;

            public CalculateLeaveModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.LeaveId)
                    .NotEmpty()
                    .IdMustExist(_context.Leaves.AsQueryable());

                RuleFor(x => x.Days)
                    .NotEmpty()
                    .GreaterThan(0);
            }
        }
    }
}
