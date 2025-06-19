using FluentValidation;
using Hrms.Common.Data.Migrations;
using Hrms.Common.Helpers;
using Hrms.Common.Models;
using MathNet.Numerics.Providers.LinearAlgebra;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using System.Security.Cryptography.X509Certificates;
using static Hrms.AdminApi.Controllers.AttendancesController;
using static NPOI.HSSF.Util.HSSFColor;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [CustomAuthorize("employee-transaction")]
    public class EmpTransactionsController : Controller
    {
        private readonly DataContext _context;

        public EmpTransactionsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var pId = await _context.EmpLogs.Where(x => x.EmployeeId == id).Select(x => x.Id).SingleOrDefaultAsync();

            var data = await _context.EmpTransactions
                .Include(x => x.Employee)
                .Include(x => x.RmEmp)
                .Include(x => x.HodEmp)
                .Include(x => x.Company)
                .Include(x => x.BusinessUnit)
                .Include(x => x.Plant)
                .Include(x => x.Region)
                .Include(x => x.Division)
                .Include(x => x.Branch)
                .Include(x => x.Department)
                .Include(x => x.SubDepartment)
                .Include(x => x.Grade)
                .Include(x => x.Designation)
                .Include(x => x.Mode)
                .Include(x => x.CostCenter)
                .Include(x => x.UniformType)
                .Include(x => x.SalaryBank)
                .FirstOrDefaultAsync(x => x.Id == pId);

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Any(x => x == data?.CompanyId) && data?.CompanyId != null)
                {
                    return Forbid();
                }
            }

            string? empCode;
            string? deviceCode;
            int? deviceId;
            string? deviceName;
            int? workHourId;
            string? workHourName;

            if (data == null)
            {
                empCode = await _context.EmpDetails.Where(x => x.Id == id).Select(x => x.CardId).FirstOrDefaultAsync();
                deviceCode = null;
                deviceId = null;
                deviceName = null;
                workHourId = null;
                workHourName = null;
            }
            else
            {
                var empDevice = await _context.EmpDeviceCodes
                    .Include(x => x.Device)
                    .Where(x => x.EmpId == data.EmployeeId)
                    .FirstOrDefaultAsync();

                var empWorkHour = await _context.DefaultWorkHours
                    .Include(x => x.WorkHour)
                    .Where(x => x.EmpId == data.EmployeeId)
                    .FirstOrDefaultAsync();

                empCode = data?.Employee?.CardId;
                deviceCode = empDevice?.DeviceCode;
                deviceId = empDevice?.DeviceId;
                deviceName = empDevice?.Device?.DeviceModel;
                workHourId = empWorkHour?.WorkHourId;
                workHourName = empWorkHour?.WorkHour?.Name;
            }

            return Ok(new
            {
                EmpTransaction = new
                {
                    data?.Id,
                    data?.EmployeeId,
                    EmpCode = empCode,
                    DeviceCode = deviceCode,
                    DeviceId = deviceId,
                    DeviceName = deviceName,
                    data?.OfficialEmail,
                    data?.OfficialContactNumber,
                    data?.RmEmpId,
                    RmEmpCode = data?.RmEmp?.CardId,
                    RmName = FullName(data?.RmEmp?.FirstName!, data?.RmEmp?.MiddleName!, data?.RmEmp?.LastName!),
                    data?.HodEmpId,
                    HodEmpCode = data?.HodEmp?.CardId,
                    HodName = FullName(data?.HodEmp?.FirstName!, data?.HodEmp?.MiddleName!, data?.HodEmp?.LastName!),
                    data?.CompanyId,
                    CompanyName = data?.Company?.Name,
                    data?.BusinessUnitId,
                    BusinessUnitName = data?.BusinessUnit?.Name,
                    data?.PlantId,
                    PlantName = data?.Plant?.Name,
                    data?.RegionId,
                    RegionName = data?.Region?.Name,
                    data?.DivisionId,
                    DivisionName = data?.Division?.Name,
                    data?.BranchId,
                    BranchName = data?.Branch?.Name,
                    data?.DepartmentId,
                    DepartmentName = data?.Department?.Name,
                    data?.SubDepartmentId,
                    SubDepartmentName = data?.SubDepartment?.Name,
                    data?.GradeId,
                    GradeName = data?.Grade?.Name,
                    data?.DesignationId,
                    DesignationName = data?.Designation?.Name,
                    data?.ModeId,
                    ModeName = data?.Mode?.Name,
                    data?.PersonalArea,
                    data?.SubArea,
                    data?.CostCenterId,
                    CostCenterName = data?.CostCenter?.Name,
                    data?.PositionCode,
                    data?.SubType,
                    data?.PersonType,
                    data?.UniformStatus,
                    data?.UniformTypeId,
                    UniformTypeName = data?.UniformType?.Id,
                    data?.ExtraUniform,
                    data?.EsiNumber,
                    data?.UanNumber,
                    data?.SalaryBankId,
                    data?.AccountNumber,
                    data?.PfApplicable,
                    data?.PfAccountNumber,
                    data?.IsCeiling,
                    data?.EpsApplicable,
                    data?.IfscCode,
                    data?.VpfApplicable,
                    data?.VpfAmount,
                    WorkHourId = workHourId,
                    WorkHourName = workHourName,
                }
            });
        }

        [HttpGet("TransactionMode")]
        public async Task<IActionResult> GetTransactionModes()
        {
            var data = Enumeration.GetAll<TransactionMode>().ToList();

            return Ok(new
            {
                Data = data
            });
        }

        // Post: EmpTransactions/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var settings = await _context.Settings.SingleOrDefaultAsync();

            var setting = await _context.Settings.FirstOrDefaultAsync();

            if (setting is null)
            {
                return ErrorHelper.ErrorResult("Id", "No settings found.");
            }

            var empLog = await _context.EmpLogs.Where(x => x.EmployeeId == input.EmployeeId).FirstOrDefaultAsync();

            //DateOnly transactionDate = DateOnlyHelper.ParseDateOrNow(input.TransactionDate);

            if (empLog is not null)
            {
                //if (input.TransactionMode == "JOIN")
                //{
                //    return ErrorHelper.ErrorResult("TransactionMode", "Employee has already joined");
                //}

                var empTran = await _context.EmpTransactions.Where(x => x.Id == empLog.Id).FirstOrDefaultAsync();

                if (User.GetUserRole() != "super-admin")
                {
                    var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                    if (!companyIds.Any(x => x == empTran?.CompanyId))
                    {
                        return Forbid();
                    }
                }

                //if (transactionDate >= empTran.LastTransactionDate)
                //{
                //    return ErrorHelper.ErrorResult("TransactionDate", "Transaction date cannot be greater than last transaction date, " + empTran.LastTransactionDate);
                //}
            }

            EmpTransaction data = new()
            {
                TransactionMode = "JOIN",
                //LastTransactionDate = transactionDate,
                EmployeeId = input.EmployeeId,
                OfficialEmail = input.OfficialEmail,
                OfficialContactNumber = input.OfficialContactNumber,
                CompanyId = input.CompanyId,
                RmEmpId = input.RmEmpId,
                HodEmpId = input.HodEmpId,
                BusinessUnitId = input.BusinessUnitId,
                PlantId = input.PlantId,
                RegionId = input.RegionId,
                DivisionId = input.DivisionId,
                BranchId = input.BranchId,
                DepartmentId = input.DepartmentId,
                SubDepartmentId = input.SubDepartmentId,
                ModeId = input.ModeId,
                DesignationId = input.DesignationId,
                GradeId = input.GradeId,
                PersonalArea = input.PersonalArea,
                SubArea = input.SubArea,
                CostCenterId = input.CostCenterId,
                PositionCode = input.PositionCode,
                SubType = input.SubType,
                PersonType = input.PersonType,
                UniformStatus = input.UniformStatus,
                UniformTypeId = input.UniformTypeId,
                ExtraUniform = input.ExtraUniform,
                EsiNumber = input.EsiNumber,
                UanNumber = input.UanNumber,
                SalaryBankId = input.SalaryBankId,
                AccountNumber = input.AccountNumber,
                PfApplicable = input.PfApplicable,
                PfAccountNumber = input.PfAccountNumber,
                IsCeiling = input.IsCeiling,
                EpsApplicable = input.EpsApplicable,
                IfscCode = input.IfscCode,
                VpfApplicable = input.VpfApplicable,
                VpfAmount = input.VpfAmount,
                TransactionUser = User.GetUsername(),

                //Default Values
                StatusId = 1,
                TDSType = 0,
                CheckInMode = 0,
                DefCalcDutyHour = 0,
                GradeAmount = 0,
                GradeAmountDaily = 0,
                DailyWage = 0,
                PayRollMode = "N/A",
                Terminate = 0,
                TerminateMonth = 0,
                TerminateYear = 0,
            };

            _context.Add(data);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.SaveChangesAsync();

                if (empLog != null)
                {
                    var empLeaves = await _context.EmpLeaves.Where(x => x.PId == empLog.Id).ToListAsync();

                    _context.AddRange(empLeaves.Select(x => new EmpLeave
                    {
                        PId = data.Id,
                        LeaveId = x.LeaveId,
                        Days = x.Days,
                        MaxDays = x.MaxDays
                    }));

                    var companyId = await _context.EmpTransactions.Where(x => x.Id == empLog.Id).Select(x => x.CompanyId).FirstOrDefaultAsync();

                    if (companyId != input.CompanyId)
                    {
                        var leaveYear = await _context.LeaveYearCompanies
                            .Where(x => x.CompanyId == companyId || x.CompanyId == null)
                            .OrderBy(x => x.CompanyId)
                            .FirstOrDefaultAsync();

                        if (leaveYear is null)
                        {
                            return ErrorHelper.ErrorResult("Id", "No leave year defined for the previous company. Please contact adminstrator.");
                        }

                        if (await _context.LeaveApplicationHistories.AnyAsync(x => x.EmpId == input.EmployeeId && x.Status == "pending"))
                        {
                            return ErrorHelper.ErrorResult("Id", "Cannot transfer company. The employee has pending leaves.");
                        }

                        foreach (var empLeave in empLeaves)
                        {
                            var closeLeaveLedgers = await _context.LeaveLedgers
                                .Where(x => x.EmpId == input.EmployeeId && x.LeaveYearId == leaveYear.LeaveYearId && x.CompanyId == companyId && x.LeaveId == empLeave.LeaveId && x.IsClosed == false)
                                .ToListAsync();

                            if (closeLeaveLedgers.Count == 0)
                            {
                                continue;
                            }

                            var balance = closeLeaveLedgers.Sum(x => x.Given) - closeLeaveLedgers.Sum(x => x.Taken);

                            foreach (var closeLeaveLedger in closeLeaveLedgers)
                            {
                                closeLeaveLedger.IsClosed = true;
                            }

                            var closeLeaveAppplications = await _context.LeaveApplicationHistories
                                .Where(x => x.EmpId == input.EmployeeId && !x.IsClosed)
                                .ToListAsync();

                            foreach (var closeLeaveApplication in closeLeaveAppplications)
                            {
                                closeLeaveApplication.IsClosed = true;
                            }

                            _context.Add(new LeaveLedger
                            {
                                EmpId = input.EmployeeId,
                                LeaveId = empLeave.LeaveId,
                                Remarks = "Closing",
                                ApprovedById = User.GetUserId(),
                                TransactionUser = User.GetUsername(),
                                LeaveYearId = leaveYear.LeaveYearId,
                                CompanyId = companyId,
                                HLeaveType = 0,
                                Taken = balance,
                                IsClosed = true,

                                //Default
                                IsRegular = 0,
                                Adjusted = 0,
                                NoHrs = 0,
                            });

                            var newLeaveYear = await _context.LeaveYearCompanies
                                .Where(x => x.CompanyId == input.CompanyId || x.CompanyId == null)
                                .OrderByDescending(x => x.CompanyId)
                                .FirstOrDefaultAsync();

                            if (newLeaveYear is null)
                            {
                                return ErrorHelper.ErrorResult("CompanyId", "No leave year defined for this company.");
                            }

                            _context.Add(new LeaveLedger
                            {
                                EmpId = input.EmployeeId,
                                LeaveId = empLeave.LeaveId,
                                Given = balance,
                                GivenMonth = (short)DateTime.Now.Month,
                                GivenYear = (short)DateTime.Now.Year,
                                Remarks = "Initial Opening Balance",
                                ApprovedById = User.GetUserId(),
                                TransactionUser = User.GetUsername(),
                                LeaveYearId = newLeaveYear.LeaveYearId,
                                CompanyId = input.CompanyId,
                                IsYearly = true,

                                //Default
                                IsRegular = 1,
                                Adjusted = 0,
                                NoHrs = 0,
                                HLeaveType = 0,
                            });
                        }
                    }
                }

                var empDeviceCode = await _context.EmpDeviceCodes.Where(x => x.EmpId == input.EmployeeId).FirstOrDefaultAsync();

                if (empDeviceCode is null)
                {
                    _context.EmpDeviceCodes.Add(new EmpDeviceCode
                    {
                        EmpId = input.EmployeeId,
                        DeviceCode = input.DeviceCode,
                        DeviceId = input.DeviceId
                    });
                }
                else
                {
                    empDeviceCode.DeviceCode = input.DeviceCode;
                    empDeviceCode.DeviceId = input.DeviceId;
                }

                if (input.WorkHourId is not null)
                {
                    var defaultWorkHours = await _context.DefaultWorkHours.Where(x => x.EmpId == input.EmployeeId).ToListAsync();

                    if (defaultWorkHours.Count > 0)
                    {
                        if (defaultWorkHours.FirstOrDefault()?.WorkHourId != input.WorkHourId)
                        {
                            foreach (var defaultWorkHour in defaultWorkHours)
                            {
                                defaultWorkHour.WorkHourId = input.WorkHourId ?? 0;
                                defaultWorkHour.UpdatedAt = DateTime.UtcNow;
                            }
                        }
                    }
                    else
                    {
                        List<DefaultWorkHour> defaultWorkHourData = new();

                        for (short i = 1; i < 8; i++)
                        {
                            defaultWorkHourData.Add(new DefaultWorkHour
                            {
                                EmpId = input.EmployeeId,
                                DayId = i,
                                WorkHourId = input.WorkHourId ?? 0,
                            });
                        }

                        _context.AddRange(defaultWorkHourData);
                    }
                }

                await _context.SaveChangesAsync();

                if (empLog is not null)
                {
                    _context.Remove(empLog);
                }

                _context.Add(new EmpLog
                {
                    Id = data.Id,
                    EmployeeId = data.EmployeeId
                });

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok();
            } catch (Exception)
            {
                await transaction.RollbackAsync();

                return ErrorHelper.ErrorResult("Id", "Internal Server error. Please contact administrator.");
            }
        }

        // PUT: EmpTransactions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.EmpTransactions.FirstOrDefaultAsync(c => c.EmployeeId == id);
            var setting = await _context.Settings.FirstOrDefaultAsync();

            if (setting is null)
            {
                return ErrorHelper.ErrorResult("Id", "No settings found.");
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Any(x => x == data?.CompanyId))
                {
                    return Forbid();
                }
            }

            data.EmployeeId = input.EmployeeId;
            data.CompanyId = input.CompanyId;
            data.RmEmpId = input.RmEmpId;
            data.HodEmpId = input.HodEmpId;
            data.BusinessUnitId = input.BusinessUnitId;
            data.PlantId = input.PlantId;
            data.RegionId = input.RegionId;
            data.DivisionId = input.DivisionId;
            data.BranchId = input.BranchId;
            data.DepartmentId = input.DepartmentId;
            data.SubDepartmentId = input.SubDepartmentId;
            data.DesignationId = input.DesignationId;
            data.GradeId = input.GradeId;
            data.ModeId = input.ModeId;
            data.PersonalArea = input.PersonalArea;
            data.SubArea = input.SubArea;
            data.CostCenterId = input.CostCenterId;
            data.PositionCode = input.PositionCode;
            data.SubType = input.SubType;
            data.PersonType = input.PersonType;
            data.UniformStatus = input.UniformStatus;
            data.UniformTypeId = input.UniformTypeId;
            data.ExtraUniform = input.ExtraUniform;
            data.EsiNumber = input.EsiNumber;
            data.UanNumber = input.UanNumber;
            data.SalaryBankId = input.SalaryBankId;
            data.AccountNumber = input.AccountNumber;
            data.PfAccountNumber = input.PfAccountNumber;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("Transfer/{empId}")]
        public async Task<IActionResult> Transfer(int empId, TransferInputModel input)
        {
            var empLog = await _context.EmpLogs.Where(x => x.EmployeeId == empId).FirstOrDefaultAsync();

            if (empLog is null)
            {
                return ErrorHelper.ErrorResult("Id", "Employee is not registered.");
            }

            var empTran = await _context.EmpTransactions.Where(x => x.Id == empLog.Id).FirstOrDefaultAsync();

            if (empTran.CompanyId == input.CompanyId)
            {
                return ErrorHelper.ErrorResult("CompanyId", "Cannot transfer to the same company.");
            }

            var leaveYear = await _context.LeaveYearCompanies
                .Where(x => x.CompanyId == empTran.CompanyId || x.CompanyId == null)
                .OrderBy(x => x.CompanyId)
                .FirstOrDefaultAsync();

            if (leaveYear is null)
            {
                return ErrorHelper.ErrorResult("Id", "No leave year defined.");
            }

            if (await _context.LeaveApplicationHistories.AnyAsync(x => x.EmpId == empId && x.Status == "pending"))
            {
                return ErrorHelper.ErrorResult("Id", "The employee has pending leaves.");
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var empLeaves = await _context.EmpLeaves.Where(x => x.PId == empTran.Id).ToListAsync();

                    foreach (var empLeave in empLeaves)
                    {
                        var closeLeaveLedgers = await _context.LeaveLedgers
                            .Where(x => x.EmpId == empId && x.LeaveYearId == leaveYear.Id && x.CompanyId == empTran.CompanyId && x.LeaveId == empLeave.Id && x.IsClosed == false)
                            .ToListAsync();

                        if (closeLeaveLedgers.Count == 0)
                        {
                            continue;
                        }

                        var balance = closeLeaveLedgers.Sum(x => x.Given) - closeLeaveLedgers.Sum(x => x.Taken);

                        foreach (var closeLeaveLedger in closeLeaveLedgers)
                        {
                            closeLeaveLedger.IsClosed = true;
                        }

                        var closeLeaveAppplications = await _context.LeaveApplicationHistories
                            .Where(x => x.EmpId == empId && !x.IsClosed)
                            .ToListAsync();

                        foreach(var closeLeaveApplication in closeLeaveAppplications)
                        {
                            closeLeaveApplication.IsClosed = true;
                        }

                        _context.Add(new LeaveLedger
                        {
                            EmpId = empId,
                            LeaveId = empLeave.LeaveId,
                            Remarks = "Closing",
                            ApprovedById = User.GetUserId(),
                            TransactionUser = User.GetUsername(),
                            LeaveYearId = leaveYear.LeaveYearId,
                            CompanyId = empTran.CompanyId,
                            HLeaveType = 0,
                            Taken = balance,
                            IsClosed = true,

                            //Default
                            IsRegular = 0,
                            Adjusted = 0,
                            NoHrs = 0,
                        });
                    }

                    EmpTransaction newEmpTran = empTran;

                    empTran.Id = new();
                    newEmpTran.CompanyId = input.CompanyId;

                    _context.Add(newEmpTran);
                    _context.Remove(empLog);

                    await _context.SaveChangesAsync();

                    _context.Add(new EmpLog
                    {
                        Id = newEmpTran.Id,
                        EmployeeId = empId
                    });

                    var newLeaveYear = await _context.LeaveYearCompanies
                        .Where(x => x.CompanyId == input.CompanyId || x.CompanyId == null)
                        .OrderByDescending(x => x.CompanyId)
                        .FirstOrDefaultAsync();

                    if (newLeaveYear is null)
                    {
                        return ErrorHelper.ErrorResult("Id", "No leave year defined.");
                    }

                    if (input.Leaves is not null && input.Leaves.Count > 0)
                    {
                        foreach (var leave in input.Leaves)
                        {
                            var leaveDetail = await _context.Leaves.FindAsync(leave.LeaveId);

                            _context.Add(new EmpLeave
                            {
                                PId = newEmpTran.Id,
                                LeaveId = (short)leave.LeaveId,
                                Days = (short)leave.Days,
                                MaxDays = leaveDetail.LeaveMax
                            });

                            _context.Add(new LeaveLedger
                            {
                                EmpId = empId,
                                LeaveId = leave.LeaveId,
                                Given = leave.Balance,
                                GivenMonth = (short)DateTime.Now.Month,
                                GivenYear = (short)DateTime.Now.Year,
                                Remarks = "Initial Balance",
                                ApprovedById = User.GetUserId(),
                                TransactionUser = User.GetUsername(),
                                LeaveYearId = newLeaveYear.LeaveYearId,
                                CompanyId = input.CompanyId,
                                IsYearly = true,

                                //Default
                                IsRegular = 1,
                                Adjusted = 0,
                                NoHrs = 0,
                                HLeaveType = 0,
                            });
                        }

                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok();
                } catch (Exception ex )
                {
                    await transaction.RollbackAsync();

                    return ErrorHelper.ErrorResult("Id", ex.StackTrace);
                }
            }
        }

        // DELETE: EmpTransactions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.EmpTransactions.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Any(x => x == data?.CompanyId))
                {
                    return Forbid();
                }
            }

            _context.EmpTransactions.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private static string FullName(string firstName, string middleName, string lastName)
        {
            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
        }

        public class BaseInputModel
        {
            public int EmployeeId { get; set; }
            public string DeviceCode { get; set; }
            public int? DeviceId { get; set; }
            public string OfficialContactNumber { get; set; }
            public string OfficialEmail { get; set; }
            public int? RmEmpId { get; set; }
            public int? HodEmpId { get; set; }
            public int? CompanyId { get; set; }
            public int? BusinessUnitId { get; set; }
            public int? PlantId { get; set; }
            public int? RegionId { get; set; }
            public int? DivisionId { get; set; }
            public short? BranchId { get; set; }
            public int? DepartmentId { get; set; }
            public int? SubDepartmentId { get; set; }
            public short? DesignationId { get; set; }
            public int? GradeId { get; set; }
            public short? ModeId { get; set; }
            public string PersonalArea { get; set; }
            public string SubArea { get; set; }
            public int? CostCenterId { get; set; }
            public string PositionCode { get; set; }
            public string SubType { get; set; }
            public string PersonType { get; set; }
            public bool? UniformStatus { get; set; }
            public int? UniformTypeId { get; set; }
            public bool? ExtraUniform { get; set; }
            public string EsiNumber { get; set; }
            public string UanNumber { get; set; }
            public int? SalaryBankId { get; set; }
            public string AccountNumber { get; set; }
            public string PfAccountNumber { get; set; }
            public short? WorkHourId { get; set; }
            public bool? PfApplicable { get; set; }
            public bool? IsCeiling { get; set; }
            public bool? EpsApplicable { get; set; }
            public string? IfscCode { get; set; }
            public bool? VpfApplicable { get; set; }
            public decimal? VpfAmount { get; set; }
        }

        public class AddInputModel : BaseInputModel
        {
            public string TransactionDate { get; set; }
            public string TransactionMode { get; set; }
        }

        public class UpdateInputModel : BaseInputModel { }

        public class LeaveInputModel
        {
            public short LeaveId { get; set; }
            public short Days { get; set; }
            public decimal Balance { get; set; }
        }

        public class TransferInputModel
        {
            public int CompanyId { get; set; }
            public List<LeaveInputModel> Leaves { get; set; }
        }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                //RuleFor(x => x.TransactionDate)
                //    .NotEmpty()
                //    .MustBeDate();

                //RuleFor(x => x.TransactionMode)
                //    .NotEmpty()
                //    .MustBeIn(Enumeration.GetAll<TransactionMode>().Select(x => x.Id).ToList());

                RuleFor(x => x.EmployeeId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.DeviceCode)
                    .NotEmpty();

                RuleFor(x => x.DeviceId)
                    .NotEmpty()
                    .When(x => _context.Settings.Select(x => x.UniqueDeviceCode).FirstOrDefault() == false);

                RuleFor(x => x.OfficialEmail)
                    .EmailAddress()
                    .Unless(x => string.IsNullOrEmpty(x.OfficialEmail));

                RuleFor(x => x.OfficialContactNumber)
                    .MustBeDigits(10)
                    .Unless(x => string.IsNullOrEmpty(x.OfficialContactNumber));

                RuleFor(x => x.RmEmpId)
                    .IdMustExist(_context.EmpDetails.AsQueryable())
                    .Unless(x => x.RmEmpId is null);

                RuleFor(x => x.HodEmpId)
                    .IdMustExist(_context.EmpDetails.AsQueryable())
                    .Unless(x => x.HodEmpId is null);

                RuleFor(x => x.CompanyId)
                    .NotEmpty()
                    .IdMustExist(_context.Companies.AsQueryable());

                RuleFor(x => x.BusinessUnitId)
                    .IdMustExist(_context.BusinessUnits.AsQueryable())
                    .Unless(x => x.BusinessUnitId is null);

                RuleFor(x => x.PlantId)
                    .IdMustExist(_context.Plants.AsQueryable())
                    .Unless(x => x.PlantId is null);

                RuleFor(x => x.RegionId)
                    .IdMustExist(_context.Regions.AsQueryable())
                    .Unless(x => x.RegionId is null);

                RuleFor(x => x.DivisionId)
                    .IdMustExist(_context.Divisions.AsQueryable())
                    .Unless(x => x.DivisionId is null);

                RuleFor(x => x.BranchId)
                    .IdMustExist(_context.Branches.AsQueryable())
                    .Unless(x => x.BranchId is null);

                RuleFor(x => x.DepartmentId)
                    .IdMustExist(_context.Departments.AsQueryable())
                    .Unless(x => x.DepartmentId is null);

                RuleFor(x => x.SubDepartmentId)
                    .IdMustExist(_context.Departments.AsQueryable())
                    .Unless(x => x.SubDepartmentId is null);

                RuleFor(x => x.GradeId)
                    .IdMustExist(_context.Grades.AsQueryable())
                    .Unless(x => x.GradeId is null);

                RuleFor(x => x.DesignationId)
                    .IdMustExist(_context.Designations.AsQueryable())
                    .Unless(x => x.DesignationId is null);

                RuleFor(x => x.ModeId)
                    .IdMustExist(_context.Modes.AsQueryable())
                    .Unless(x => x.ModeId is null);

                RuleFor(x => x.CostCenterId)
                    .IdMustExist(_context.CostCenters.AsQueryable())
                    .Unless(x => x.CostCenterId is null);

                RuleFor(x => x.PersonType)
                    .MustBeIn(Enumeration.GetAll<EmpPersonType>().Select(x => x.Id).ToList())
                    .Unless(x => string.IsNullOrEmpty(x.PersonType));

                RuleFor(x => x.UniformTypeId)
                    .IdMustExist(_context.UniformTypes.AsQueryable())
                    .Unless(x => x.UniformTypeId is null);

                RuleFor(x => x.SalaryBankId)
                    .IdMustExist(_context.Banks.AsQueryable())
                    .Unless(x => x.SalaryBankId is null);

                RuleFor(x => x.WorkHourId)
                    .IdMustExist(_context.WorkHours.AsQueryable())
                    .Unless(x => x.WorkHourId is null);
            }

            public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result)
            {
                AddInputModel model = (AddInputModel)validationContext.InstanceToValidate;

                List<string> errorFields = ValidationHelper.GetErrorFields(result);

                if (!errorFields.Contains(nameof(model.DeviceCode)) && !errorFields.Contains(nameof(model.DeviceId)))
                {
                    if (_context.EmpDeviceCodes.Where(x => x.DeviceId == model.DeviceId && x.DeviceCode == model.DeviceCode && x.EmpId != model.EmployeeId).Any())
                    {
                        string message = "Employee with this " + nameof(model.DeviceCode) + " and this device already exists.";

                        result.Errors.Add(new ValidationFailure(nameof(model.DeviceCode), message));
                    }
                }

                return result;
            }

            public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
            {
                return commonContext;
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

                RuleFor(x => x.EmployeeId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.DeviceCode)
                    .NotEmpty();

                RuleFor(x => x.DeviceId)
                    .NotEmpty()
                    .When(x => _context.Settings.Select(x => x.UniqueDeviceCode).FirstOrDefault() == false);

                RuleFor(x => x.OfficialEmail)
                    .EmailAddress()
                    .Unless(x => string.IsNullOrEmpty(x.OfficialEmail));

                RuleFor(x => x.OfficialContactNumber)
                    .MustBeDigits(10)
                    .Unless(x => string.IsNullOrEmpty(x.OfficialContactNumber));

                RuleFor(x => x.CompanyId)
                    .NotEmpty()
                    .IdMustExist(_context.Companies.AsQueryable());

                RuleFor(x => x.BusinessUnitId)
                    .IdMustExist(_context.BusinessUnits.AsQueryable())
                    .Unless(x => x.BusinessUnitId is not null);

                RuleFor(x => x.PlantId)
                    .IdMustExist(_context.Plants.AsQueryable())
                    .Unless(x => x.PlantId is not null);

                RuleFor(x => x.RegionId)
                    .IdMustExist(_context.Regions.AsQueryable())
                    .Unless(x => x.RegionId is not null);

                RuleFor(x => x.DivisionId)
                    .IdMustExist(_context.Divisions.AsQueryable())
                    .Unless(x => x.DivisionId is not null);

                RuleFor(x => x.BranchId)
                    .IdMustExist(_context.Branches.AsQueryable())
                    .Unless(x => x.BranchId is not null);

                RuleFor(x => x.DepartmentId)
                    .IdMustExist(_context.Departments.AsQueryable())
                    .Unless(x => x.DepartmentId is not null);

                RuleFor(x => x.SubDepartmentId)
                    .IdMustExist(_context.Departments.AsQueryable())
                    .Unless(x => x.SubDepartmentId is null);

                RuleFor(x => x.GradeId)
                    .IdMustExist(_context.Grades.AsQueryable())
                    .Unless(x => x.GradeId is null);

                RuleFor(x => x.DesignationId)
                    .IdMustExist(_context.Designations.AsQueryable())
                    .Unless(x => x.DesignationId is null);

                RuleFor(x => x.ModeId)
                    .IdMustExist(_context.Modes.AsQueryable())
                    .Unless(x => x.ModeId is null);

                RuleFor(x => x.CostCenterId)
                    .IdMustExist(_context.CostCenters.AsQueryable())
                    .Unless(x => x.CostCenterId is null);

                RuleFor(x => x.PersonType)
                    .MustBeIn(Enumeration.GetAll<EmpPersonType>().Select(x => x.Id).ToList());

                RuleFor(x => x.UniformTypeId)
                    .IdMustExist(_context.UniformTypes.AsQueryable())
                    .Unless(x => x.UniformTypeId is null);

                RuleFor(x => x.SalaryBankId)
                    .IdMustExist(_context.Banks.AsQueryable())
                    .Unless(x => x.SalaryBankId is not null);

                RuleFor(x => x.WorkHourId)
                    .IdMustExist(_context.WorkHours.AsQueryable())
                    .Unless(x => x.WorkHourId is null);
            }

            public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result)
            {
                AddInputModel model = (AddInputModel)validationContext.InstanceToValidate;

                List<string> errorFields = ValidationHelper.GetErrorFields(result);

                if (!errorFields.Contains(nameof(model.DeviceCode)) && !errorFields.Contains(nameof(model.DeviceId)))
                {
                    if (_context.EmpDeviceCodes.Where(x => x.DeviceId == model.DeviceId && x.DeviceCode == model.DeviceCode && x.EmpId != model.EmployeeId).Any())
                    {
                        string message = "Employee with this " + nameof(model.DeviceCode) + " and this device already exists.";

                        result.Errors.Add(new ValidationFailure(nameof(model.DeviceCode), message));
                    }
                }

                return result;
            }

            public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
            {
                return commonContext;
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.EmpTransactions.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }

        public class TransferInputModelValidator : AbstractValidator<TransferInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _empId;

            public TransferInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _empId = contextAccessor.HttpContext?.Request?.RouteValues["empId"]?.ToString();

                RuleFor(x => x.CompanyId)
                    .NotEmpty()
                    .IdMustExist(_context.Companies.AsQueryable());

                RuleForEach(x => x.Leaves)
                    .ChildRules(leave =>
                    {
                        leave.RuleFor(x => x.LeaveId)
                            .NotEmpty()
                            .IdMustExist(_context.Leaves.AsQueryable());

                        leave.RuleFor(x => x.Days)
                            .NotEmpty();

                        leave.RuleFor(x => x.Balance)
                            .NotEmpty();
                    });
            }

            protected override bool PreValidate(ValidationContext<TransferInputModel> context, ValidationResult result)
            {
                if (_context.EmpDetails.Find(int.Parse(_empId)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "EmpId is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
