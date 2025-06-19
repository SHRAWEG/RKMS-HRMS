using Hrms.Common.Data.Migrations;
using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using ZstdNet;
using static Hrms.AdminApi.Controllers.AttendancesController;
using static Hrms.AdminApi.Controllers.SalaryAnnexuresController;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "super-admin")]

    public class EmpSalaryDetailController : Controller
    {
        private readonly DataContext _context;
        private readonly DbHelper _dbHelper;
        private static readonly List<string> ShDataTypes = new()
        {
            "PERCENT",
            "AMOUNT",
            "PERUNIT"
        };

        public EmpSalaryDetailController(DataContext context, DbHelper dbHelper)
        {
            _context = context;
            _dbHelper = dbHelper;
        }

        [HttpGet("{empId}")]
        public async Task<IActionResult> Get(int empId)
        {
            var data = await _context.EmpSalaryRecords
                .Where(x => x.EmpId == empId)
                .OrderByDescending(x => x.FromDate)
                .ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        [HttpGet("ReferenceEmpSalaryHeads/{empSalaryRecordId}")]
        public async Task<IActionResult> GetReferenceSalaryHeads(int empSalaryRecordId)
        {
            var empSalaryRecord = await _context.EmpSalaryRecords.FindAsync(empSalaryRecordId);

            if (empSalaryRecord is null)
            {
                return ErrorHelper.ErrorResult("Id", "AnxId is invalid.");
            }

            if (empSalaryRecord.ToDate != null)
            {
                return ErrorHelper.ErrorResult("Id", "Old Record.");
            }

            var empSalaryHeads = await _context.EmpSalaryHeads
                .Include(x => x.SalaryHead)
                .Where(x => x.EmpSalaryRecordId == empSalaryRecordId)
                .Select(x => new GetAllReferenceDTO
                {
                    EmpShId = x.EmpShId,
                    EmpShName = x.SalaryHead.Name
                })
                .ToListAsync();

            empSalaryHeads.Add(new GetAllReferenceDTO
            {
                EmpShId = 0,
                EmpShName = "Monthly Salary"
            });

            return Ok(new
            {
                Data = empSalaryHeads
            });
        }

        [HttpGet("Particular/{empSalaryRecordId}")]
        public async Task<IActionResult> GetDetail(int empSalaryRecordId)
        {
            var empSalaryRecord = await _context.EmpSalaryRecords.FirstOrDefaultAsync(x => x.EmpSalaryRecordId == empSalaryRecordId);

            if (empSalaryRecord is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            var earnings = await _context.EmpSalaryHeads
                .Include(x => x.SalaryHead)
                .ThenInclude(x => x.ShCategory)
                .Include(x => x.EmpSalaryHeadDetails)
                .ThenInclude(x => x.ReferenceEmpSh)
                .ThenInclude(x => x.SalaryHead)
                .Where(x => x.EmpSalaryRecordId == empSalaryRecordId && x.SalaryHead.ShCategory.Shc_Type == "EARNING")
                .ToListAsync();

            var deductions = await _context.EmpSalaryHeads
                .Include(x => x.SalaryHead)
                .ThenInclude(x => x.ShCategory)
                .Include(x => x.EmpSalaryHeadDetails)
                .ThenInclude(x => x.ReferenceEmpSh)
                .ThenInclude(x => x.SalaryHead)
                .Where(x => x.EmpSalaryRecordId == empSalaryRecordId && x.SalaryHead.ShCategory.Shc_Type == "DEDUCTION")
                .ToListAsync();

            var basicSalary = earnings.Where(x => x.ShId == 1).FirstOrDefault();

            decimal? basicSalaryAmount = basicSalary?.Amount ?? 0;

            List<ParticularsDTO> earningParticulars = new();

            decimal grossSalaryAmount = 0;
            decimal totalEarning = 0;
            decimal totalDeduction = 0;
            decimal totalContribution = 0;

            foreach (var earning in earnings)
            {
                if (earning.ShDataType == "PERCENT")
                {
                    string references = "";

                    foreach (var data in earning.EmpSalaryHeadDetails)
                    {
                        if (references != "")
                        {
                            references += " + ";
                        }

                        if (data.IsPercentageOfMonthlySalary)
                        {
                            references += string.Concat(data.Percent, "% of Monthly Salary");
                        }
                        else
                        {
                            references += string.Concat(data.Percent, "% of ", data.ReferenceEmpSh?.SalaryHead.Name);
                        }
                    }

                    earningParticulars.Add(new ParticularsDTO
                    {
                        EmpShId = earning.EmpShId,
                        Particular = earning.SalaryHead.Name,
                        Type = earning.ShDataType,
                        References = references,
                        Value = earning.Amount,
                    });
                } 
                else if (earning.ShDataType == "PERUNIT")
                {
                    earningParticulars.Add(new ParticularsDTO
                    {
                        EmpShId = earning.EmpShId,
                        Particular = earning.SalaryHead.Name,
                        Type = earning.ShDataType,
                        Value = earning.PerUnitRate
                    });
                } 
                else
                {
                    earningParticulars.Add(new ParticularsDTO
                    {
                        EmpShId = earning.EmpShId,
                        Particular = earning.SalaryHead.Name,
                        Type = earning.ShDataType,
                        Value = earning.Amount
                    });
                }

                totalEarning += (earning.Amount ?? 0);
            }

            List<ParticularsDTO> contributionParticulars = new();
            List<ParticularsDTO> deductionParticulars = new();

            foreach (var deduction in deductions)
            {
                if (deduction.ShDataType == "PERCENT")
                {
                    string references = "";

                    foreach (var data in deduction.EmpSalaryHeadDetails)
                    {
                        if (data.IsPercentageOfMonthlySalary)
                        {
                            references += string.Concat(data.Percent, "% of Monthly Salary + ");
                        }
                        else
                        {
                            references += string.Concat(data.Percent, "% of ", data.ReferenceEmpSh?.SalaryHead.Name, " + ");
                        }
                    }

                    deductionParticulars.Add(new ParticularsDTO
                    {
                        EmpShId = deduction.EmpShId,
                        Particular = deduction.SalaryHead.Name,
                        Type = deduction.ShDataType,
                        References = references,
                        Value = deduction.Amount
                    });
                }
                else if(deduction.ShDataType == "PERUNIT")
                {
                    deductionParticulars.Add(new ParticularsDTO
                    {
                        EmpShId = deduction.EmpShId,
                        Particular = deduction.SalaryHead.Name,
                        Type = deduction.ShDataType,
                        Value = deduction.PerUnitRate
                    });
                }
                else
                {
                    deductionParticulars.Add(new ParticularsDTO
                    {
                        EmpShId = deduction.EmpShId,
                        Particular = deduction.SalaryHead.Name,
                        Type = deduction.ShDataType,
                        Value = deduction.Amount
                    });
                }

                totalDeduction += deduction.Amount ?? 0;

                if (deduction.HasOfficeContribution == true)
                {
                    decimal? contributionAmount = 0;
                    if (deduction.ContributionType == 1)
                    {
                        contributionAmount = deduction.OfficeContribution * basicSalaryAmount / 100;

                        contributionParticulars.Add(new ParticularsDTO
                        {
                            EmpShId = deduction.EmpShId,
                            Particular = deduction.SalaryHead.Name,
                            Type = "Office Contribution",
                            References = deduction.OfficeContribution + "% of Basic Salary",
                            Value = contributionAmount,
                        });
                    }
                    else if (deduction.ContributionType == 4)
                    {
                        contributionAmount = deduction.OfficeContribution * deduction.Amount / 100;

                        contributionParticulars.Add(new ParticularsDTO
                        {
                            EmpShId = deduction.EmpShId,
                            Particular = deduction.SalaryHead.Name,
                            Type = "Office Contribution",
                            References = deduction.OfficeContribution + "% of " + deduction.SalaryHead.Name,
                            Value = contributionAmount
                        });
                    }

                    totalContribution += contributionAmount ?? 0;
                }
            }

            grossSalaryAmount = totalEarning + totalContribution;

            return Ok(new
            {
                Data = new
                {
                    EmpSalaryRecordId = empSalaryRecordId,
                    
                    FromDate = empSalaryRecord.FromDate,
                    MonthlySalary = empSalaryRecord.MonthlySalary,
                    Earnings = earningParticulars,
                    TotalEarningAmount = totalEarning,
                    OfficeContributions = contributionParticulars,
                    TotalOfficeContributionAmount = totalContribution,
                    Deductions = deductionParticulars,
                    TotalDeductionAmount = totalDeduction,
                    GrossSalaryAmount = grossSalaryAmount
                }
            });
        }

        [HttpGet("EmpShDetail/{empShId}")]
        public async Task<IActionResult> GetAnnexureDetail(int empShId)
        {
            var data = await _context.EmpSalaryHeads
                .Include(x => x.SalaryHead)
                .Include(x => x.EmpSalaryHeadDetails)
                .ThenInclude(x => x.ReferenceEmpSh)
                .ThenInclude(x => x.SalaryHead)
                .FirstOrDefaultAsync(x => x.EmpShId == empShId);

            if (data is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                EmpSalaryHead = new
                {
                    EmpShId = data.EmpShId,
                    ShId = data.ShId,
                    ShName = data.SalaryHead.Name,
                    HasOfficeContribution = data.HasOfficeContribution,
                    ContributionType = data.ContributionType,
                    OfficeContribution = data.OfficeContribution,
                    ShDataType = data.ShDataType,
                    Amount = data.Amount,
                    PerUnitRate = data.PerUnitRate,
                    EmpSalaryHeadDetails = data.EmpSalaryHeadDetails.Select(x => new
                    {
                        ReferenceEmpShId = x.IsPercentageOfMonthlySalary == true ? 0 : x.ReferenceEmpShId,
                        ReferenceSalaryAnnexureHeadName = x.IsPercentageOfMonthlySalary == true ? "Monthly Salary" : x.ReferenceEmpSh.SalaryHead.Name,
                        Percent = x.Percent,
                        Amount = x.Amount
                    }),
                    CreatedAt = data.CreatedAt,
                    UpdatedAt = data.UpdatedAt
                }
            });
        }

        [HttpPost()]
        public async Task<IActionResult> Add(AddInputModel input)
        {
            DateOnly fromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);

            EmpSalaryRecord empSalaryRecord = new()
            {
                EmpId = input.EmpId,
                FromDate = fromDate,
                MonthlySalary = input.MonthlySalary,
            };

            _context.Add(empSalaryRecord);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Data = empSalaryRecord
            });
        }

        [HttpPut("{empSalaryRecordId}")]
        public async Task<IActionResult> Update(int empSalaryRecordId, UpdateInputModel input)
        {
            var data = await _context.EmpSalaryRecords.Where(x => x.EmpSalaryRecordId == empSalaryRecordId).FirstOrDefaultAsync();

            DateOnly fromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);

            if (await _context.EmpSalaryRecords.AnyAsync(x => x.EmpId == data.EmpId && x.FromDate > fromDate && x.ToDate != null))
            {
                return ErrorHelper.ErrorResult("FromDate", "Invalid Date.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                data.FromDate = fromDate;
                data.MonthlySalary = input.MonthlySalary;

                var empSalaryHeads = await _context.EmpSalaryHeads.Where(x => x.EmpSalaryRecordId == empSalaryRecordId).ToListAsync();

                var empShIds = empSalaryHeads.Select(x => x.EmpShId).ToList();

                var empSalaryHeadDetails = await _context.EmpSalaryHeadDetails.Where(x => empShIds.Contains(x.EmpShId)).ToListAsync();

                var updateEmpShIds = empSalaryHeadDetails.Where(x => x.IsPercentageOfMonthlySalary).Select(x => x.EmpShId).ToList();

                var updateEmpShs = empSalaryHeads.Where(x => updateEmpShIds.Contains(x.EmpShId)).ToList();

                if (updateEmpShs.Count > 0)
                {
                    await _dbHelper.RecurringUpdate(updateEmpShs, empSalaryHeadDetails, input.MonthlySalary);
                }

                await _context.SaveChangesAsync();

                await _context.Database.CommitTransactionAsync();

                return Ok();
            }
            catch (Exception)
            {
                await _context.Database.RollbackTransactionAsync();

                return ErrorHelper.ErrorResult("Id", "System Error. Please contact administrator.");
            }
    
            
        }

        [HttpPost("Revise")]
        public async Task<IActionResult> ReviseSalary(ReviseInputModel input)
        {
            var empSalaryRecord = await _context.EmpSalaryRecords
                .Include(x => x.EmpSalaryHeads)
                .ThenInclude(x => x.EmpSalaryHeadDetails)
                .Where(x => x.EmpId == input.EmpId && x.ToDate == null)
                .FirstOrDefaultAsync();

            if (empSalaryRecord is null)
            {
                return ErrorHelper.ErrorResult("Id", "No previous salary record.");
            }

            DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);

            if (empSalaryRecord.FromDate >= FromDate)
            {
                return ErrorHelper.ErrorResult("FromDate", "FromDate should be greater than previous salary date.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                empSalaryRecord.ToDate = FromDate;
                empSalaryRecord.UpdatedAt = DateTime.UtcNow;

                EmpSalaryRecord data = new()
                {
                    FromDate = FromDate,
                    EmpId = input.EmpId,
                    MonthlySalary = input.MonthlySalary
                };

                _context.Add(data);
                await _context.SaveChangesAsync();

                List<EmpSalaryHead> empSalaryHeads = new();
                List<EmpSalaryHeadDetail> empSalaryHeadDetails = new();

                foreach (var empSalaryHead in empSalaryRecord.EmpSalaryHeads)
                {
                    empSalaryHeads.Add(new()
                    {
                        EmpSalaryRecordId = data.EmpSalaryRecordId,
                        ShId = empSalaryHead.ShId,
                        ShDataType = empSalaryHead.ShDataType,
                        Amount = empSalaryHead.Amount,
                        PerUnitRate = empSalaryHead.PerUnitRate,
                        HasOfficeContribution = empSalaryHead.HasOfficeContribution,
                        ContributionType = empSalaryHead.ContributionType,
                        OfficeContribution = empSalaryHead.OfficeContribution,
                    });
                }

                _context.AddRange(empSalaryHeads);
                await _context.SaveChangesAsync();

                foreach (var empSalaryHead in empSalaryRecord.EmpSalaryHeads.Where(x => x.ShDataType == "PERCENT"))
                {
                    int EmpShId = empSalaryHeads.FirstOrDefault(x => x.ShId == empSalaryHead.ShId).EmpShId;

                    foreach (var empShDetail in empSalaryHead.EmpSalaryHeadDetails)
                    {
                        int? referenceEmpShId = empSalaryHeads.FirstOrDefault(x => x.ShId == empShDetail.ReferenceEmpSh?.ShId)?.EmpShId;

                        empSalaryHeadDetails.Add(new EmpSalaryHeadDetail
                        {
                            EmpShId = EmpShId,
                            IsPercentageOfMonthlySalary = empShDetail.IsPercentageOfMonthlySalary,
                            ReferenceEmpShId = referenceEmpShId,
                            Percent = empShDetail.Percent,
                        });
                    }
                }

                _context.AddRange(empSalaryHeadDetails);
                await _context.SaveChangesAsync();

                var updateEmpShs = empSalaryHeads.Where(x => x.ShDataType == "PERCENT").ToList();

                if (updateEmpShs.Count > 0)
                {
                    await _dbHelper.RecurringUpdate(updateEmpShs, empSalaryHeadDetails, input.MonthlySalary);
                }

                await _context.Database.CommitTransactionAsync();

                return Ok(new
                {
                    Data = data
                });
            }
            catch (Exception)
            {
                await _context.Database.RollbackTransactionAsync();

                return ErrorHelper.ErrorResult("Id", "System error. Please contact administration.");
            }
        }

        [HttpPost("AddSalaryHead/{empSalaryRecordId}")]
        public async Task<IActionResult> AddSalaryHead(int empSalaryRecordId, AddShInputModel input)
        {
            var empSalaryRecord = await _context.EmpSalaryRecords.FindAsync(empSalaryRecordId);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Common.Models.EmpSalaryHead data = new()
                {
                    EmpSalaryRecordId = empSalaryRecordId,
                    ShId = input.ShId,
                    ShDataType = input.ShDataType,
                    Amount = input.ShDataType == "AMOUNT" ? input.Amount : null,
                    PerUnitRate = input.ShDataType == "PERUNIT" ? input.PerUnitRate : null,
                    HasOfficeContribution = input.HasOfficeContribution,
                    ContributionType = input.ContributionType,
                    OfficeContribution = input.OfficeContribution
                };

                _context.Add(data);
                await _context.SaveChangesAsync();

                decimal amount = 0;

                if (input.ShDataType == "PERCENT")
                {
                    foreach (var salaryHeadDetail in input.SalaryHeadDetails)
                    {
                        decimal? refAmount = 0;

                        if (salaryHeadDetail.ReferenceEmpShId == 0)
                        {
                            refAmount = empSalaryRecord.MonthlySalary * salaryHeadDetail.Percent / 100;
                        }
                        else
                        {
                            refAmount = await _context.EmpSalaryHeads.Where(x => x.EmpShId == salaryHeadDetail.ReferenceEmpShId).Select(x => x.Amount).FirstOrDefaultAsync()
                                        * salaryHeadDetail.Percent
                                        / 100;
                        }

                        amount += refAmount ?? 0;

                        _context.Add(new EmpSalaryHeadDetail
                        {
                            EmpShId = data.EmpShId,
                            IsPercentageOfMonthlySalary = salaryHeadDetail.ReferenceEmpShId == 0,
                            ReferenceEmpShId = salaryHeadDetail.ReferenceEmpShId == 0 ? null : salaryHeadDetail.ReferenceEmpShId,
                            Percent = salaryHeadDetail.Percent,
                            Amount = refAmount
                        });
                    }

                    data.Amount = amount;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                return ErrorHelper.ErrorResult("Id", "Internal server error. Please contact administrator.");
            }
        }

        [HttpPut("UpdateShType/{empShId}")]
        public async Task<IActionResult> UpdateShType(int empShId, UpdateTypeInputModel input)
        {
            var empSh = await _context
                .EmpSalaryHeads
                .Include(x => x.EmpSalaryHeadDetails)
                .FirstOrDefaultAsync(x => x.EmpShId == empShId);


            if (empSh is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            var empSalaryRecord = await _context.EmpSalaryRecords.Where(x => x.EmpSalaryRecordId == empSh.EmpSalaryRecordId).FirstOrDefaultAsync();

            if (empSalaryRecord is null)
            {
                return ErrorHelper.ErrorResult("Id", "Salary Record is empty.");
            }

            if (empSalaryRecord.ToDate != null)
            {
                return ErrorHelper.ErrorResult("Id", "Cannot update old record.");
            }

            if (!ShDataTypes.Contains(input.ShDataType))
            {
                return ErrorHelper.ErrorResult("ShDataType", "Invalid data type.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                empSh.ShDataType = input.ShDataType;

                if (input.ShDataType != "AMOUNT")
                {
                    empSh.Amount = null;
                } 

                if (input.ShDataType != "PERUNIT")
                {
                    empSh.PerUnitRate = null;
                }

                if (input.ShDataType != "PERCENT")
                {
                    _context.RemoveRange(empSh.EmpSalaryHeadDetails);
                }

                await _context.SaveChangesAsync();

                var empSalaryHeads = await _context.EmpSalaryHeads
                    .Include(x => x.EmpSalaryHeadDetails)
                    .Where(x => x.EmpSalaryRecordId == empSh.EmpSalaryRecordId)
                    .ToListAsync();

                var empShIds = empSalaryHeads.Select(x => x.EmpShId).ToList();

                var empSalaryHeadDetails = await _context.EmpSalaryHeadDetails.Where(x => empShIds.Contains(x.EmpShId)).ToListAsync();

                var updateEmpShIds = empSalaryHeadDetails.Where(x => x.ReferenceEmpShId == empSh.EmpShId).Select(x => x.EmpShId).ToList();

                var updateEmpShs = empSalaryHeads.Where(x => updateEmpShIds.Contains(x.EmpShId)).ToList();

                if (updateEmpShs.Count > 0)
                {
                    await _dbHelper.RecurringUpdate(updateEmpShs, empSalaryHeadDetails, empSalaryRecord.MonthlySalary);
                }

                await transaction.CommitAsync();

                return Ok();
            } catch (Exception)
            {
                await transaction.RollbackAsync();

                return ErrorHelper.ErrorResult("Id", "System error. Please contact administrator.");
            }
        }

        [HttpPut("UpdateValue/{empShId}")]
        public async Task<IActionResult> UpdateValue(int empShId, UpdateValueInputModel input)
        {
            var empSh = await _context
                .EmpSalaryHeads
                .Include(x => x.EmpSalaryHeadDetails)
                .FirstOrDefaultAsync(x => x.EmpShId == empShId);


            if (empSh is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            var empSalaryRecord = await _context.EmpSalaryRecords.Where(x => x.EmpSalaryRecordId == empSh.EmpSalaryRecordId).FirstOrDefaultAsync();

            if (empSalaryRecord is null)
            {
                return ErrorHelper.ErrorResult("Id", "Salary Record is empty.");
            }

            if (empSalaryRecord.ToDate != null)
            {
                return ErrorHelper.ErrorResult("Id", "Cannot update old record.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (empSh.ShDataType == "AMOUNT")
                {
                    empSh.Amount = input.Value;
                }

                if (empSh.ShDataType == "PERUNIT")
                {
                    empSh.PerUnitRate = input.Value;
                }

                await _context.SaveChangesAsync();

                var empSalaryHeads = await _context.EmpSalaryHeads
                    .Include(x => x.EmpSalaryHeadDetails)
                    .Where(x => x.EmpSalaryRecordId == empSh.EmpSalaryRecordId)
                    .ToListAsync();

                var empShIds = empSalaryHeads.Select(x => x.EmpShId).ToList();

                var empSalaryHeadDetails = await _context.EmpSalaryHeadDetails.Where(x => empShIds.Contains(x.EmpShId)).ToListAsync();

                var updateEmpShIds = empSalaryHeadDetails.Where(x => x.ReferenceEmpShId == empSh.EmpShId).Select(x => x.EmpShId).ToList();

                var updateEmpShs = empSalaryHeads.Where(x => updateEmpShIds.Contains(x.EmpShId)).ToList();

                if (updateEmpShs.Count > 0)
                {
                    await _dbHelper.RecurringUpdate(updateEmpShs, empSalaryHeadDetails, empSalaryRecord.MonthlySalary);
                }

                await transaction.CommitAsync();

                return Ok();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                return ErrorHelper.ErrorResult("Id", "System error. Please contact administrator.");
            }
        }

        [HttpPut("UpdateReference/{empShId}")]
        public async Task<IActionResult> UpdateSalaryHead(int empShId, UpdateReferenceInputModel input)
        {
            var data = await _context.EmpSalaryHeads.FirstOrDefaultAsync(x => x.EmpShId == empShId);

            var empSalaryRecord = await _context.EmpSalaryRecords.FirstOrDefaultAsync(x => x.EmpSalaryRecordId == data.EmpSalaryRecordId);

            var empSalaryHeads = await _context.EmpSalaryHeads
                .Include(x => x.EmpSalaryHeadDetails)
                .Where(x => x.EmpSalaryRecordId == data.EmpSalaryRecordId)
                .ToListAsync();

            foreach(var empShDetail in input.SalaryHeadDetails)
            {
                if (empShDetail.ReferenceEmpShId == 0)
                {
                    continue;
                } else
                {
                    bool isCircular = _dbHelper.CompareCircularDependency(empSalaryHeads, empShId, empShDetail.ReferenceEmpShId ?? 0);

                    if (isCircular)
                    {
                        return ErrorHelper.ErrorResult("Id", "Circular dependency in reference.");
                    }
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                data.UpdatedAt = DateTime.UtcNow;

                var empSalaryHeadDetail = await _context.EmpSalaryHeadDetails.Where(x => x.EmpShId == data.EmpShId).ToListAsync();

                _context.RemoveRange(empSalaryHeadDetail);

                decimal totalAmount = 0;

                foreach (var detail in input.SalaryHeadDetails)
                {
                    decimal? amount = 0;

                    if (detail.ReferenceEmpShId == 0)
                    {
                        amount = detail.Percent * empSalaryRecord.MonthlySalary / 100;
                    }
                    else
                    {
                        var referenceEmpSh = await _context.EmpSalaryHeads.Where(x => x.EmpShId == detail.ReferenceEmpShId).FirstOrDefaultAsync();

                        amount = detail.Percent * referenceEmpSh.Amount / 100;
                    }

                    totalAmount += amount ?? 0;

                    _context.Add(new EmpSalaryHeadDetail
                    {
                        EmpShId = data.EmpShId,
                        IsPercentageOfMonthlySalary = detail.ReferenceEmpShId == 0,
                        ReferenceEmpShId = detail.ReferenceEmpShId == 0 ? null : detail.ReferenceEmpShId,
                        Percent = detail.Percent,
                        Amount = amount
                    });
                }

                data.Amount = totalAmount;

                await _context.SaveChangesAsync();

                var empShIds = empSalaryHeads.Select(x => x.EmpShId).ToList();

                var empSalaryHeadDetails = await _context.EmpSalaryHeadDetails.Where(x => empShIds.Contains(x.EmpShId)).ToListAsync();

                var updateEmpShIds = empSalaryHeadDetails.Where(x => x.ReferenceEmpShId == data.EmpShId).Select(x => x.EmpShId).ToList();

                var updateEmpShs = empSalaryHeads.Where(x => updateEmpShIds.Contains(x.EmpShId)).ToList();

                if (updateEmpShs.Count > 0)
                {
                    await _dbHelper.RecurringUpdate(updateEmpShs, empSalaryHeadDetails, empSalaryRecord.MonthlySalary);
                }

                await transaction.CommitAsync();

                return Ok();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                return ErrorHelper.ErrorResult("Id", "Internal server error. Please contact administrator.");
            }
        }

        [HttpPut("UpdateContribution/{empShId}")]
        public async Task<IActionResult> UpdateContribution(int empShId, UpdateContributionInputModel input)
        {
            var empSh = await _context.EmpSalaryHeads.FindAsync(empShId);

            if (empSh.SalaryHead.ShCategory.Category != "DEDUCTION")
            {
                return ErrorHelper.ErrorResult("Id", "Salary head must be deduction.");
            }

            empSh.HasOfficeContribution = input.HasOfficeContribution;
            empSh.ContributionType = input.ContributionType;
            empSh.OfficeContribution = input.OfficeContribution;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("AddSalaryAnnexure")]
        public async Task<IActionResult> AssignSalaryAnnexure(AssignSAInputModel input)
        {
            var empSalary = await _context.EmpSalaryRecords
                .Include(x => x.EmpSalaryHeads)
                .Where(x => x.EmpSalaryRecordId == input.EmpSalaryRecordId)
                .SingleOrDefaultAsync();

            if (empSalary.ToDate != null)
            {
                return ErrorHelper.ErrorResult("EmpSalaryRecordId", "Cannot update old records.");
            }

            var salaryAnnexureHeads = await _context.SalaryAnnexureHeads
                .Include(x => x.SalaryAnnexureHeadDetails)
                .ThenInclude(x => x.ReferenceSalaryAnnexureHead)
                .Where(x => x.AnxId == input.AnxId)
                .ToListAsync();

            List<int> existingSalaryHeads = empSalary.EmpSalaryHeads.Select(x => x.ShId).ToList();

            if (salaryAnnexureHeads.Any(x => existingSalaryHeads.Contains(x.ShId))) 
            {
                return ErrorHelper.ErrorResult("AnxId", "Some of the salary heads already exists for the employee.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                List<Common.Models.EmpSalaryHead> empSalaryHeads = new();
                List<EmpSalaryHeadDetail> empSalaryHeadDetails = new();

                foreach (var salaryAnnexureHead in salaryAnnexureHeads)
                {
                    empSalaryHeads.Add(new Common.Models.EmpSalaryHead
                    {
                        EmpSalaryRecordId = empSalary.EmpSalaryRecordId,
                        ShId = salaryAnnexureHead.ShId,
                        ShDataType = salaryAnnexureHead.ShDataType,
                        HasOfficeContribution = salaryAnnexureHead.HasOfficeContribution,
                        ContributionType = salaryAnnexureHead.ContributionType,
                        OfficeContribution = salaryAnnexureHead.OfficeContribution,
                    });
                }

                _context.AddRange(empSalaryHeads);
                await _context.SaveChangesAsync();

                foreach(var salaryAnnexureHead in salaryAnnexureHeads.Where(x => x.ShDataType == "PERCENT"))
                {
                    int EmpShId = empSalaryHeads.FirstOrDefault(x => x.ShId == salaryAnnexureHead.ShId).EmpShId;

                    foreach (var salaryAnnexureHeadDetail in salaryAnnexureHead.SalaryAnnexureHeadDetails)
                    {
                        int? referenceEmpShId = empSalaryHeads.FirstOrDefault(x => x.ShId == salaryAnnexureHeadDetail.ReferenceSalaryAnnexureHead?.ShId)?.EmpShId;

                        empSalaryHeadDetails.Add(new EmpSalaryHeadDetail
                        {
                            EmpShId = EmpShId,
                            IsPercentageOfMonthlySalary = salaryAnnexureHeadDetail.IsPercentageOfMonthlySalary,
                            ReferenceEmpShId = referenceEmpShId,
                            Percent = salaryAnnexureHeadDetail.Percent,
                        });
                    }
                }

                _context.AddRange(empSalaryHeadDetails);
                await _context.SaveChangesAsync();

                var empShIds = empSalaryHeadDetails.Where(x => x.IsPercentageOfMonthlySalary).Select(x => x.EmpShId).ToList();

                var updateEmpShs = empSalaryHeads.Where(x => empShIds.Contains(x.EmpShId)).ToList();

                if (updateEmpShs.Count > 0)
                {
                    await _dbHelper.RecurringUpdate(updateEmpShs, empSalaryHeadDetails, empSalary.MonthlySalary);
                }

                await _context.Database.CommitTransactionAsync();

                return Ok();
            } catch (Exception ex)
            {
                await _context.Database.RollbackTransactionAsync();

                return BadRequest(ex.Message);

                //return ErrorHelper.ErrorResult("AnxId", "System Error. Please contact administrator.");
            }
        }

        [HttpDelete("{empShId}")]
        public async Task<IActionResult> DeleteSalaryHead(int empShId)
        {
            var empSh = await _context.EmpSalaryHeads.FirstOrDefaultAsync(x => x.EmpShId == empShId);

            if (empSh == null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id.");
            }

            if (await _context.EmpSalaryHeadDetails.AnyAsync(x => x.ReferenceEmpShId == empShId))
            {
                return ErrorHelper.ErrorResult("Id", "There are references to this salary head.");
            }

            var empShDetail = await _context.EmpSalaryHeadDetails.Where(x => x.EmpShId == empShId).ToListAsync();

            _context.RemoveRange(empShDetail);
            _context.Remove(empSh);

            await _context.SaveChangesAsync();

            return Ok();
        }

        public class GetAllReferenceDTO
        {
            public int EmpShId { get; set; }
            public string EmpShName { get; set; }
        }

        public class ReferenceSalaryHeadDTO
        {
            public int SalaryAnnexureHeadId { get; set; }
            public int? ReferenceSalaryAnnexureHeadId { get; set; }
            public int? ReferenceShId { get; set; }
            public bool IsOfMonthlyPercentage { get; set; }
            public decimal Percentage { get; set; }

        }

        public class ParticularsDTO
        {
            public int EmpShId { get; set; }
            public string Particular { get; set; }
            public string Type { get; set; }
            public string? References { get; set; }
            public decimal? Value { get; set; }
        }

        public class BaseInputModel
        {
            public string FromDate { get; set; }
            public int MonthlySalary { get; set; }
        }

        public class AddInputModel : BaseInputModel 
        { 
            public int EmpId { get; set; }
        }

        public class UpdateInputModel : BaseInputModel { }

        public class ReviseInputModel : BaseInputModel
        {
            public int EmpId { get; set; }
        }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable())
                    .MustBeUnique(_context.EmpSalaryRecords.AsQueryable(), "EmpId");

                RuleFor(x => x.FromDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.MonthlySalary)
                    .NotEmpty()
                    .GreaterThan(0);
            }
        }

        public class UpdateInputModelValidator : AbstractValidator<UpdateInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public UpdateInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["empSalaryRecordId"]?.ToString();

                RuleFor(x => x.FromDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.MonthlySalary)
                    .NotEmpty()
                    .GreaterThan(0);
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.EmpSalaryRecords.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                if (_context.EmpSalaryRecords.Any(x => x.EmpSalaryRecordId == int.Parse(_id) && x.ToDate != null))
                {
                    result.Errors.Add(new ValidationFailure("Id", "Cannot update old records"));
                    return false;
                }

                return true;
            }
        }

        public class ReviseInputModelValidator : AbstractValidator<ReviseInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public ReviseInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.FromDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.MonthlySalary)
                    .NotEmpty()
                    .GreaterThan(0);
            }
        }

        public class AssignSAInputModel
        {
            public int EmpSalaryRecordId { get; set; }
            public int AnxId { get; set; }
        }

        public class  AssignSAInputModelValidator : AbstractValidator<AssignSAInputModel>
        {
            private readonly DataContext _context;

            public AssignSAInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.EmpSalaryRecordId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpSalaryRecords.AsQueryable(), "EmpSalaryRecordId");

                RuleFor(x => x.AnxId)
                    .NotEmpty()
                    .IdMustExist(_context.SalaryAnnexures.AsQueryable(), "AnxId");
            }
        }

        public class ReferenceSh
        {
            public int? ReferenceEmpShId { get; set; }
            public decimal? Percent { get; set; }
        }

        public class BaseShInputModel
        {
            public List<ReferenceSh> SalaryHeadDetails { get; set; }
        }

        public class AddShInputModel : BaseShInputModel
        {
            public int ShId { get; set; }
            public string ShDataType { get; set; }
            public decimal? Amount { get; set; }
            public decimal? PerUnitRate { get; set; }
            public bool? HasOfficeContribution { get; set; }
            public int? ContributionType { get; set; }
            public decimal? OfficeContribution { get; set; }
        }

        public class UpdateReferenceInputModel : BaseShInputModel { }

        public class UpdateTypeInputModel 
        { 
            public string ShDataType { get; set; }
        }

        public class UpdateValueInputModel
        {
            public decimal Value { get; set; }
        }

        public class UpdateContributionInputModel 
        {
            public bool? HasOfficeContribution { get; set; }
            public int? ContributionType { get; set; }
            public decimal? OfficeContribution { get; set; }
        }

        public class AddShInputModelValidator : AbstractValidator<AddShInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public AddShInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["empSalaryRecordId"]?.ToString();
                List<int?> contributionTypes = new List<int?> { 1, 4 };

                RuleFor(x => x.ShId)
                    .IdMustExist(_context.SalaryHeads.AsQueryable(), "ShId")
                    .MustBeUnique(_context.EmpSalaryHeads.Where(x => x.EmpSalaryRecordId == int.Parse(_id)).AsQueryable(), "ShId");

                RuleFor(x => x.ShDataType)
                    .NotEmpty()
                    .MustBeIn(ShDataTypes);

                When(x => x.ShDataType == "AMOUNT", () =>
                {
                    RuleFor(x => x.Amount)
                        .NotEmpty()
                        .GreaterThan(0);
                });

                When(x => x.ShDataType == "PERUNIT", () =>
                {
                    RuleFor(x => x.PerUnitRate)
                        .NotEmpty()
                        .GreaterThan(0);
                });

                When(x => x.ShDataType == "PERCENT", () => {
                    RuleFor(x => x.SalaryHeadDetails)
                        .NotEmpty();

                    RuleForEach(x => x.SalaryHeadDetails)
                       .ChildRules((salaryAnnexureDetail) =>
                       {
                           salaryAnnexureDetail.RuleFor(x => x.ReferenceEmpShId)
                                .NotNull()
                                .WithMessage("Please specify the reference to the percentage.");

                           salaryAnnexureDetail.RuleFor(x => x.ReferenceEmpShId)
                               .IdMustExist(_context.EmpSalaryHeads.AsQueryable(), "EmpShId")
                               .Unless(x => x.ReferenceEmpShId == 0);

                           salaryAnnexureDetail.RuleFor(x => x.Percent)
                               .NotEmpty()
                               .GreaterThan(0)
                               .LessThanOrEqualTo(100);
                       });
                });

                When(x => !CompareSalaryHeadCategory(x.ShId, "DEDUCTION"), () =>
                {
                    RuleFor(x => x.HasOfficeContribution)
                        .Null()
                        .WithMessage("Should be empty when salary head category is not 'Deduction'.");

                    RuleFor(x => x.ContributionType)
                        .Empty()
                        .WithMessage("Should be empty when salary head category is not 'Deduction'.");

                    RuleFor(x => x.OfficeContribution)
                        .Empty()
                        .WithMessage("Should be empty when salary head category is not 'Deduction'.");
                });

                When(x => CompareSalaryHeadCategory(x.ShId, "DEDUCTION"), () =>
                {
                    RuleFor(x => x.HasOfficeContribution)
                        .NotNull();

                    When(x => x.HasOfficeContribution == false, () =>
                    {
                        RuleFor(x => x.ContributionType)
                            .Empty()
                            .WithMessage("There is no office contribution");

                        RuleFor(x => x.OfficeContribution)
                            .Empty()
                            .WithMessage("There is no office contribution");
                    });

                    When(x => x.HasOfficeContribution == true, () =>
                    {
                        RuleFor(x => x.ContributionType)
                            .NotEmpty()
                            .MustBeIn(contributionTypes);

                        RuleFor(x => x.OfficeContribution)
                            .NotEmpty()
                            .GreaterThanOrEqualTo(0)
                            .LessThanOrEqualTo(100);
                    });
                });
            }

            private bool CompareSalaryHeadCategory(int shId, string category)
            {
                // Assuming you have a method to get the type of SalaryHead by its ShId
                var salaryHead = _context.SalaryHeads
                    .Include(x => x.ShCategory)
                    .FirstOrDefault(sh => sh.ShId == shId);

                return salaryHead?.ShCategory.Category == category;
            }

            protected override bool PreValidate(ValidationContext<AddShInputModel> context, ValidationResult result)
            {
                if (_context.EmpSalaryRecords.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                if (_context.EmpSalaryRecords.Any(x => x.EmpSalaryRecordId == int.Parse(_id) && x.ToDate != null))
                {
                    result.Errors.Add(new ValidationFailure("Id", "Cannot update old records"));
                    return false;
                }

                return true;
            }
        }

        public class UpdateReferenceInputModelValidator : AbstractValidator<UpdateReferenceInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public UpdateReferenceInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["empShId"]?.ToString();
                List<int?> contributionTypes = new List<int?> { 1, 4 };

                RuleFor(x => x.SalaryHeadDetails)
                    .NotEmpty();

                RuleForEach(x => x.SalaryHeadDetails)
                    .ChildRules((salaryAnnexureDetail) =>
                    {
                        salaryAnnexureDetail.RuleFor(x => x.ReferenceEmpShId)
                            .NotNull()
                            .WithMessage("Please specify the reference to the percentage.");

                        salaryAnnexureDetail.RuleFor(x => x.ReferenceEmpShId)
                            .IdMustExist(_context.EmpSalaryHeads.AsQueryable(), "EmpShId")
                            .Unless(x => x.ReferenceEmpShId == 0);

                        salaryAnnexureDetail.RuleFor(x => x.Percent)
                            .NotEmpty()
                            .GreaterThan(0)
                            .LessThanOrEqualTo(100);
                    });
            }

            private bool CompareSalaryHeadCategory(string category)
            {
                // Assuming you have a method to get the type of SalaryHead by its ShId
                var salaryAnnexureHead = _context.EmpSalaryHeads
                    .Include(x => x.SalaryHead)
                    .ThenInclude(x => x.ShCategory)
                    .FirstOrDefault(sh => sh.EmpShId == int.Parse(_id));

                return salaryAnnexureHead?.SalaryHead.ShCategory.Category == category; // Assuming Type is a string indicating "Amount" or "Percent"
            }

            protected override bool PreValidate(ValidationContext<UpdateReferenceInputModel> context, ValidationResult result)
            {
                if (_context.EmpSalaryHeads.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "EmpShId is invalid."));
                    return false;
                }

                return true;
            }
        }

        public class UpdateContributionInputModelValidator : AbstractValidator<UpdateContributionInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public UpdateContributionInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["empShId"]?.ToString();
                List<int?> contributionTypes = new List<int?> { 1, 4 };

                RuleFor(x => x.HasOfficeContribution)
                    .NotNull();

                When(x => x.HasOfficeContribution == false, () =>
                {
                    RuleFor(x => x.ContributionType)
                        .Empty()
                        .WithMessage("There is no office contribution");

                    RuleFor(x => x.OfficeContribution)
                        .Empty()
                        .WithMessage("There is no office contribution");
                });

                When(x => x.HasOfficeContribution == true, () =>
                {
                    RuleFor(x => x.ContributionType)
                        .NotEmpty()
                        .MustBeIn(contributionTypes);

                    RuleFor(x => x.OfficeContribution)
                        .NotEmpty()
                        .NotNull()
                        .GreaterThan(0)
                        .LessThanOrEqualTo(100);
                });
            }

            protected override bool PreValidate(ValidationContext<UpdateContributionInputModel> context, ValidationResult result)
            {
                if (_context.EmpSalaryHeads.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "EmpShId is invalid."));
                    return false;
                }

                return true;
            }
        }

        private static bool IsAnnualPercentExceeded(DataContext _context, int anxId, decimal? percent)
        {
            var annualPercent = _context.SalaryAnnexureHeads.Where(x => x.AnxId == anxId).Sum(x => x.AnnualPercent);

            if (percent + annualPercent > 100)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
