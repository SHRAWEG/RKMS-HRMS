 using CsvHelper;
using CsvHelper.Configuration.Attributes;
using DocumentFormat.OpenXml.Wordprocessing;
using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using NPOI.SS.Formula.Functions;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using static Hrms.AdminApi.Controllers.AttendancesController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class EmployeesController : Controller
    {
        private readonly DataContext _context;
        private readonly List<char> _genders = new() { 'M', 'F', 'O' };
        private readonly List<char> _maritalStatus = new() { 'M', 'S' };

        public EmployeesController(DataContext context)
        {
            _context = context;
        }

        // GET: EmpDetails
        [CustomAuthorize("list-employee")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection,
            string empCode, string name, int? branchId, int? companyId, int? departmentId, short? designationId, int? gradeId, int? statusId)
        {
            var query = (from detail in _context.EmpDetails
                         join log in _context.EmpLogs on detail.Id equals log.EmployeeId into logs
                         from log in logs.DefaultIfEmpty()
                         join tran in _context.EmpTransactions on log.Id equals tran.Id into trans
                         from tran in trans.DefaultIfEmpty()
                         select new
                         {
                             PId = log != null ? log.Id : 0,
                             EmpId = detail.Id,
                             EmpCode = detail.CardId,
                             detail.Title,
                             detail.FirstName,
                             detail.MiddleName,
                             detail.LastName,
                             ContactNumber = detail.ContactNumber,
                             Email = detail.Email,
                             Name = Helper.FullName(detail.FirstName, detail.MiddleName, detail.LastName),
                             IsRegistered = tran == null ? false : true,
                             BranchId = tran != null ? tran.BranchId : 0,
                             BranchName = tran != null ? (tran.Branch != null ? tran.Branch.Name : "") : "",
                             CompanyId = tran != null ? tran.CompanyId : 0,
                             CompanyName = tran != null ? (tran.Company != null ? tran.Company.Name : "") : "",
                             DepartmentId = tran != null ? tran.DepartmentId : 0,
                             DepartmentName = tran != null ? (tran.Department != null ? tran.Department.Name : "") : "",
                             DesignationId = tran != null ? tran.DesignationId : 0,
                             DesignationName = tran != null ? (tran.Designation != null ? tran.Designation.Name : "") : "",
                             GradeId = tran != null ? tran.GradeId : 0,
                             GradeName = tran != null ? (tran.Grade != null ? tran.Grade.Name : "") : "",
                             StatusId = tran != null ? tran.StatusId : 0,
                             StatusName = tran != null ? (tran.Status != null ? tran.Status.Name : "") : "",
                             TransactionDate = log != null ? log.TransactionDate : null
                         }
                       ).AsQueryable();

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                query = query.Where(x => companyIds.Contains(x.CompanyId ?? 0) || x.CompanyId == 0);
            }

            if (!string.IsNullOrEmpty(empCode))
            {
                query = query.Where(x => x.EmpCode!.ToLower().Contains(empCode.ToLower()));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => string.Concat(x.FirstName, x.MiddleName, x.LastName).ToLower().Contains(name.Replace(" ", string.Empty).ToLower()));
            }

            if (branchId != null)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (companyId != null)
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            if (departmentId != null)
            {
                query = query.Where(x => x.DepartmentId == departmentId);
            }

            if (designationId != null)
            {
                query = query.Where(x => x.DesignationId == designationId);
            }

            if (gradeId != null)
            {
                query = query.Where(x => x.GradeId == gradeId);
            }

            if (statusId != null)
            {
                query = query.Where(x => x.StatusId == statusId);
            }

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.EmpId);
            }
            else if (sortDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderBy(x => x.EmpCode);
                        break;

                    case "Name":
                        query.OrderBy(x => x.FirstName);
                        break;

                    case "BranchName":
                        query.OrderBy(x => x.BranchId);
                        break;

                    case "DepartmentName":
                        query.OrderBy(x => x.DepartmentId);
                        break;

                    case "DesignationName":
                        query.OrderBy(x => x.DesignationId);
                        break;

                    case "GradeName":
                        query.OrderBy(x => x.GradeId);
                        break;

                    case "StatusName":
                        query.OrderBy(x => x.StatusId);
                        break;

                    case "TransactionDate":
                        query.OrderBy(x => x.TransactionDate);
                        break;

                    default:
                        query.OrderBy(x => x.EmpId);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderByDescending(x => x.EmpCode);
                        break;

                    case "Name":
                        query.OrderByDescending(x => x.Name);
                        break;

                    case "BranchName":
                        query.OrderByDescending(x => x.BranchId);
                        break;

                    case "DepartmentName":
                        query.OrderByDescending(x => x.DepartmentId);
                        break;

                    case "DesignationName":
                        query.OrderByDescending(x => x.DesignationId);
                        break;

                    case "GradeName":
                        query.OrderByDescending(x => x.GradeId);
                        break;

                    case "StatusName":
                        query.OrderByDescending(x => x.StatusId);
                        break;

                    case "TransactionDate":
                        query.OrderByDescending(x => x.TransactionDate);
                        break;

                    default:
                        query.OrderByDescending(x => x.EmpId);
                        break;
                }
            }

            var TotalCount = await query.CountAsync();
            var TotalPages = (int)Math.Ceiling(TotalCount / (double)page);
            var data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return Ok(new
            {
                Data = data,
                TotalCount,
                TotalPages
            });
        }


        // GET: EmpDetails/Search
        [CustomAuthorize("search-employee")]
        [HttpGet("Search")]
        public async Task<IActionResult> Search(string term,string empCode, int? branchId, int? departmentId, short? designationId, int? gradeId, int? statusId)
        {
            var query = (from detail in _context.EmpDetails
                         join log in _context.EmpLogs on detail.Id equals log.EmployeeId into logs
                         from log in logs.DefaultIfEmpty()
                         join tran in _context.EmpTransactions on log.Id equals tran.Id into trans
                         from tran in trans.DefaultIfEmpty()
                         select new
                         {
                             PId = log != null ? log.Id : 0,
                             EmpId = detail.Id,
                             EmpCode = detail.CardId,
                             FirstName = detail.FirstName,
                             MiddleName = detail.MiddleName,
                             LastName = detail.LastName,
                             Name = FullName(detail.FirstName, detail.MiddleName, detail.LastName),
                             CompanyId = tran != null ? tran.CompanyId : 0,
                             CompanyName = tran != null ? (tran.Company != null ? tran.Company.Name : "") : "",
                             BranchId = tran != null ? tran.BranchId : 0,
                             BranchName = tran != null ? (tran.Branch != null ? tran.Branch.Name : "") : "",
                             DepartmentId = tran != null ? tran.DepartmentId : 0,
                             DepartmentName = tran != null ? (tran.Department != null ? tran.Department.Name : "") : "",
                             DesignationId = tran != null ? tran.DesignationId : 0,
                             DesignationName = tran != null ? (tran.Designation != null ? tran.Designation.Name : "") : "",
                             GradeId = tran != null ? tran.GradeId : 0,
                             GradeName = tran != null ? (tran.Grade != null ? tran.Grade.Name : "") : "",
                             StatusId = tran != null ? tran.StatusId : 0,
                             StatusName = tran != null ? (tran.Status != null ? tran.Status.Name : "") : "",
                             TransactionDate = log != null ? log.TransactionDate : null,
                         }
                       ).AsQueryable();

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                query = query.Where(x => companyIds.Contains(x.CompanyId ?? 0));
            }

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(x => x.EmpCode.Trim().ToLower().Contains(term.Trim().ToLower()) ||
                            string.Concat(x.FirstName, x.MiddleName, x.LastName).Trim().ToLower().Contains(term.Replace(" ", string.Empty).Trim().ToLower())
                );
            }

            if (!string.IsNullOrEmpty(empCode))
            {
                query = query.Where(x => x.EmpCode == empCode);
            }

            if (branchId != null)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (departmentId != null)
            {
                query = query.Where(x => x.DepartmentId == departmentId);
            }

            if (designationId != null)
            {
                query = query.Where(x => x.DesignationId == designationId);
            }

            if (gradeId != null)
            {
                query = query.Where(x => x.GradeId == gradeId);
            }

            if (statusId != null)
            {
                query = query.Where(x => x.StatusId == statusId);
            } else
            {
                query = query.Where(x => x.StatusId == 1);
            }

            var data = await query
                .Take(20)
                .ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        // GET: EmpDetails/Search
        [CustomAuthorize("search-employee")]
        [HttpGet("FilteredList")]
        public async Task<IActionResult> FilteredList(string term, string empCode, int? branchId, int? departmentId, short? designationId, int? gradeId, int? statusId)
        {
            var query = (from detail in _context.EmpDetails
                         join log in _context.EmpLogs on detail.Id equals log.EmployeeId into logs
                         from log in logs.DefaultIfEmpty()
                         join tran in _context.EmpTransactions on log.Id equals tran.Id into trans
                         from tran in trans.DefaultIfEmpty()
                         select new
                         {
                             PId = log != null ? log.Id : 0,
                             EmpId = detail.Id,
                             EmpCode = detail.CardId,
                             FirstName = detail.FirstName,
                             MiddleName = detail.MiddleName,
                             LastName = detail.LastName,
                             Name = FullName(detail.FirstName, detail.MiddleName, detail.LastName),
                             CompanyId = tran != null ? tran.CompanyId : 0,
                             CompanyName = tran != null ? (tran.Company != null ? tran.Company.Name : "") : "",
                             BranchId = tran != null ? tran.BranchId : 0,
                             BranchName = tran != null ? (tran.Branch != null ? tran.Branch.Name : "") : "",
                             DepartmentId = tran != null ? tran.DepartmentId : 0,
                             DepartmentName = tran != null ? (tran.Department != null ? tran.Department.Name : "") : "",
                             DesignationId = tran != null ? tran.DesignationId : 0,
                             DesignationName = tran != null ? (tran.Designation != null ? tran.Designation.Name : "") : "",
                             GradeId = tran != null ? tran.GradeId : 0,
                             GradeName = tran != null ? (tran.Grade != null ? tran.Grade.Name : "") : "",
                             StatusId = tran != null ? tran.StatusId : 0,
                             StatusName = tran != null ? (tran.Status != null ? tran.Status.Name : "") : "",
                             TransactionDate = log != null ? log.TransactionDate : null,
                         }
                       ).AsQueryable();

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                query = query.Where(x => companyIds.Contains(x.CompanyId ?? 0));
            }

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(x => x.EmpCode.Trim().ToLower().Contains(term.Trim().ToLower()) ||
                            string.Concat(x.FirstName, x.MiddleName, x.LastName).Trim().ToLower().Contains(term.Replace(" ", string.Empty).Trim().ToLower())
                );
            }

            if (!string.IsNullOrEmpty(empCode))
            {
                query = query.Where(x => x.EmpCode == empCode);
            }

            if (branchId != null)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (departmentId != null)
            {
                query = query.Where(x => x.DepartmentId == departmentId);
            }

            if (designationId != null)
            {
                query = query.Where(x => x.DesignationId == designationId);
            }

            if (gradeId != null)
            {
                query = query.Where(x => x.GradeId == gradeId);
            }

            if (statusId != null)
            {
                query = query.Where(x => x.StatusId == statusId);
            }
            else
            {
                query = query.Where(x => x.StatusId == 1);
            }

            var data = await query.ToListAsync();

            DateOnly date = DateOnly.FromDateTime(DateTime.Now);

            List<AllEmpDTO> emps = new();

            foreach (var item in data)
            {
                WorkHour? workHour = null;

                var roster = await _context.Rosters
                    .Where(x => x.Date == date && x.EmpId == item.EmpId)
                    .Include(x => x.WorkHour)
                    .FirstOrDefaultAsync();

                workHour = roster?.WorkHour;

                if (roster is null)
                {
                    var defaultWorkHour = await _context.DefaultWorkHours
                        .Where(x => (x.EmpId == item.EmpId || x.EmpId == null) && x.DayId == ((short)date.DayOfWeek + 1))
                        .OrderBy(x => x.EmpId)
                        .Include(x => x.WorkHour)
                        .FirstOrDefaultAsync();

                    workHour = defaultWorkHour?.WorkHour;
                }

                emps.Add(new AllEmpDTO
                {
                    EmpId = item.EmpId,
                    EmpCode = item.EmpCode,
                    Name = item.Name,
                    BranchName = item.BranchName,
                    CompanyName = item.CompanyName,
                    DepartmentName = item.DepartmentName,
                    DesignationName = item.DesignationName,
                    GradeName = item.GradeName,
                    StatusName = item.StatusName,
                    WorkHourName = workHour?.Name ?? ""
                });
            }

            return Ok(new
            {
                Data = emps
            });
        }

        [CustomAuthorize("view-employee")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await (from detail in _context.EmpDetails where detail.Id == id
                         join log in _context.EmpLogs on detail.Id equals log.EmployeeId into logs
                         from log in logs.DefaultIfEmpty()
                         join tran in _context.EmpTransactions on log.Id equals tran.Id into trans
                         from tran in trans.DefaultIfEmpty()
                         select new
                         {
                             PId = log != null ? log.Id : 0,
                             EmpId = detail.Id,
                             EmpCode = detail.CardId,
                             detail.Title,
                             detail.FirstName,
                             Name = FullName(detail.FirstName, detail.MiddleName, detail.LastName),
                             RmEmpId = tran != null ? tran.RmEmpId: 0,
                             RmEmpCode = tran != null ? (tran.RmEmp != null ? tran.RmEmp.CardId : "") : "", 
                             RmEmpName = tran != null ? (tran.RmEmp != null ? FullName(tran.RmEmp.FirstName, tran.RmEmp.MiddleName, tran.RmEmp.LastName) : "") : "",
                             HodEmpId = tran != null ? tran.HodEmpId : 0,
                             HodEmpCode = tran != null ? (tran.HodEmp != null ? tran.HodEmp.CardId : "") : "",
                             HodEmpName = tran != null ? (tran.HodEmp != null ? FullName(tran.HodEmp.FirstName, tran.HodEmp.MiddleName, tran.HodEmp.LastName) : "") : "",
                             BranchId = tran != null ? tran.BranchId : 0,
                             BranchName = tran != null ? (tran.Branch != null ? tran.Branch.Name : "") : "",
                             DepartmentId = tran != null ? tran.DepartmentId : 0,
                             DepartmentName = tran != null ? (tran.Department != null ? tran.Department.Name : "") : "",
                             DesignationId = tran != null ? tran.DesignationId : 0,
                             DesignationName = tran != null ? (tran.Designation != null ? tran.Designation.Name : "") : "",
                             GradeId = tran != null ? tran.GradeId : 0,
                             GradeName = tran != null ? (tran.Grade != null ? tran.Grade.Name : "") : "",
                             StatusId = tran != null ? tran.StatusId : 0,
                             StatusName = tran != null ? (tran.Status != null ? tran.Status.Name : ""): "",
                             TransactionDate = log != null ? log.TransactionDate : null,
                             detail.PermanentAddress,
                             detail.PermanentAddress2,
                             detail.PermanentCity,
                             detail.PermanentPincode,
                             detail.PermanentState,
                             detail.PermanentDistrict,
                             detail.CorrespondanceAddress,
                             detail.CorrespondanceAddress2,
                             detail.CorrespondanceCity,
                             detail.CorrespondancePincode,
                             detail.CorrespondanceState,
                             detail.CorrespondanceDistrict,
                             detail.Nationality,
                             detail.ContactNumber,
                             detail.DateOfBirth,
                             detail.MarriageDate,
                             detail.JoinDate,
                             detail.AppointedDate,
                             detail.RelevingDate,
                             detail.BirthCountryId,
                             BirthCountryName = detail.BirthCountry != null ? detail.BirthCountry.Name : "",
                             detail.ReligionId,
                             ReligionName = detail.Religion != null ? detail.Religion.Name : "",
                             detail.BirthStateId,
                             BirthStateName = detail.BirthState != null ? detail.BirthState.Name : "",
                             detail.BirthPlace,
                             detail.Education,
                             detail.PassportNumber,
                             detail.AadharNumber,
                             detail.CitizenshipNumber,
                             detail.PanNumber,
                             detail.Email,
                             TransactionUser = tran != null ? tran.TransactionUser : "",
                             EmpTypeId = tran != null ? tran.ModeId : 0,
                             EmpTypeName = tran != null ? (tran.Mode != null ? tran.Mode.Name : "") : "",
                             CompanyId = tran != null ? tran.CompanyId : 0,
                             CompanyName = tran != null ? (tran.Company != null ? tran.Company.Name : "") : "",
                             BusinessUnitId = tran != null ? tran.BusinessUnitId : 0,
                             BusinessUnitName = tran != null ? (tran.BusinessUnit != null ? tran.BusinessUnit.Name : "") : "",
                             PlantId = tran != null ? tran.PlantId : 0,
                             PlantName = tran != null ? (tran.Plant != null ? tran.Plant.Name : "") : "",
                             RegionId = tran != null ? tran.RegionId : 0,
                             RegionName = tran != null ? (tran.Region != null ? tran.Region.Name : "") : "",
                             DivisionId = tran != null ? tran.DivisionId : 0,
                             DivisionName = tran != null ? (tran.Division != null ? tran.Division.Name : "") : "",
                             PersonalArea = tran != null ? tran.PersonalArea : "",
                             SubArea = tran != null ? tran.SubArea : "",
                             CostCenterId = tran != null ? tran.CostCenterId : 0,
                             CostCenterName = tran != null ? (tran.CostCenter != null ? tran.CostCenter.Name : "") : "",
                             PositionCode = tran != null ? tran.PositionCode : "",
                             PersonType = tran != null ? tran.PersonType : "",
                             UniformStatus = tran != null ? tran.UniformStatus : null,
                             UniformTypeId = tran != null ? tran.UniformTypeId : 0,
                             UniformTypeName = tran != null ? (tran.UniformType != null ? tran.UniformType.Name : "") : "",
                             ExtraUniform = tran != null ? tran.ExtraUniform : null,
                             EsiNumber = tran != null ? tran.EsiNumber : "",
                             UanNumber = tran != null ? tran.UanNumber : "",
                             SalaryBankId = tran != null ? tran.SalaryBankId : 0,
                             BankName = tran != null ? (tran.SalaryBank != null ? tran.SalaryBank.Name : "") : "",
                             PfAccountNumber = tran != null ? tran.PfAccountNumber : "",
                         }
           ).FirstOrDefaultAsync();

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

            return Ok(new
            {
                EmpDetail = data
            });
        }

        [CustomAuthorize("update-employee-status")]
        [HttpPut("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(UpdateStatusModel input)
        {
            var empLogs = await _context.EmpLogs.Where(x => x.EmployeeId == input.EmpId).FirstOrDefaultAsync();

            if (empLogs is null)
            {
                return ErrorHelper.ErrorResult("EmpId", "Employee is not registered.");
            }

            var empTransaction = await _context.EmpTransactions.Where(x => x.Id == empLogs.Id).FirstOrDefaultAsync();

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Any(x => x == empTransaction?.CompanyId))
                {
                    return Forbid();
                }
            }

            EmpTransaction data = new EmpTransaction
            {
                EmployeeId = empTransaction.EmployeeId,
                OfficialEmail = empTransaction.OfficialEmail,
                OfficialContactNumber = empTransaction.OfficialContactNumber,
                CompanyId = empTransaction.CompanyId,
                RmEmpId = empTransaction.RmEmpId,
                HodEmpId = empTransaction.HodEmpId,
                BusinessUnitId = empTransaction.BusinessUnitId,
                PlantId = empTransaction.PlantId,
                RegionId = empTransaction.RegionId,
                DivisionId = empTransaction.DivisionId,
                BranchId = empTransaction.BranchId,
                DepartmentId = empTransaction.DepartmentId,
                SubDepartmentId = empTransaction.SubDepartmentId,
                ModeId = empTransaction.ModeId,
                DesignationId = empTransaction.DesignationId,
                GradeId = empTransaction.GradeId,
                StatusId = input.StatusId,
                PersonalArea = empTransaction.PersonalArea,
                SubArea = empTransaction.SubArea,
                CostCenterId = empTransaction.CostCenterId,
                PositionCode = empTransaction.PositionCode,
                SubType = empTransaction.SubType,
                PersonType = empTransaction.PersonType,
                UniformStatus = empTransaction.UniformStatus,
                UniformTypeId = empTransaction.UniformTypeId,
                ExtraUniform = empTransaction.ExtraUniform,
                EsiNumber = empTransaction.EsiNumber,
                UanNumber = empTransaction.UanNumber,
                SalaryBankId = empTransaction.SalaryBankId,
                AccountNumber = empTransaction.AccountNumber,
                PfApplicable = empTransaction.PfApplicable,
                PfAccountNumber = empTransaction.PfAccountNumber,
                IsCeiling = empTransaction.IsCeiling,
                EpsApplicable = empTransaction.EpsApplicable,
                IfscCode = empTransaction.IfscCode,
                VpfApplicable = empTransaction.VpfApplicable,
                VpfAmount = empTransaction.VpfAmount,

                //Default Values
                TransactionUser = "N/A",
                TransactionMode = "N/A",
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
            await _context.SaveChangesAsync();

            _context.Remove(empLogs);
            _context.Add(new EmpLog
            {
                EmployeeId = data.EmployeeId,
                Id = data.Id,
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        [CustomAuthorize("view-employee-calendar")]
        [HttpGet("EmpCalendar/{empId}")]
        public async Task<IActionResult> ViewEmpCalendar(int empId)
        {
            if (!await _context.EmpDetails.AnyAsync(x => x.Id == empId))
            {
                return ErrorHelper.ErrorResult("Id", "Employee does not exist.");
            }

            var empCalendar = await _context.EmpCalendars
                .Include(x => x.Calendar)
                .Include(x => x.Emp)
                .FirstOrDefaultAsync(x => x.EmpId == empId);

            return Ok(new
            {
                Data = new
                {
                    Id = empCalendar?.Id,
                    CalendarId = empCalendar?.CalendarId,
                    CalendarName = empCalendar?.Calendar?.Name,
                    EmpId = empCalendar?.EmpId,
                    EmpName = FullName(empCalendar?.Emp?.FirstName, empCalendar?.Emp?.MiddleName, empCalendar?.Emp?.LastName),
                    EmpCode = empCalendar?.Emp?.CardId,
                }
            });
        }

        [CustomAuthorize("assign-employee-calendar")]
        [HttpPost("AssignCalendar")]
        public async Task<IActionResult> AssignCalendar(AssignCalendarInputModel input)
        {
            var statusId = await _context.EmpLogs.Where(x => x.EmployeeId == input.EmpId)
                .Join(_context.EmpTransactions, el => el.Id, et => et.Id,
                (el, et) => et.StatusId)
                .FirstOrDefaultAsync();

            if (statusId != 1)
            {
                return ErrorHelper.ErrorResult("EmpId", "Employee is not active.");
            }

            var empCalendar = await _context.EmpCalendars.FirstOrDefaultAsync(x => x.EmpId == input.EmpId);

            if (empCalendar is null)
            {
                EmpCalendar data = new EmpCalendar
                {
                    EmpId = input.EmpId,
                    CalendarId = input.CalendarId,
                };

                _context.Add(data);
            } else
            {
                empCalendar.CalendarId = input.CalendarId;
                empCalendar.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET ALL EMP
        [CustomAuthorize("search-employee")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var query = _context.EmpLogs
                        .Join(_context.EmpDetails,
                            el => el.EmployeeId,
                            ed => ed.Id,
                            (el, ed) => new
                            {
                                EmpId = ed.Id,
                                PId = el.Id,
                                Name = FullName(ed.FirstName, ed.MiddleName, ed.LastName),
                                EmpCode = ed.CardId,
                                ed.Email,
                                ed.ContactNumber
                            }
                        )
                        .Join(_context.EmpTransactions,
                            eld => eld.PId,
                            et => et.Id,
                            (eld, et) => new
                            {
                                eld.EmpId,
                                eld.PId,
                                eld.Name,
                                eld.EmpCode,
                                eld.Email,
                                eld.ContactNumber,
                                et.CompanyId,
                            }
                         )
                        .AsQueryable();

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                query = query.Where(x => companyIds.Contains(x.CompanyId ?? 0));
            }

            var data = await query
                .Take(20)
                .ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        [HttpGet("DownloadFormat")]
        public async Task<IActionResult> Format()
        {
            Type table = typeof(EmpHeader);

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

            return File(data, "text/csv", "Employees.csv");
        }

        // POST: Employees/import
        [CustomAuthorize("import-employee")]
        [HttpPost("Import")]
        public async Task<IActionResult> Import([FromForm] ImportInputModel input)
        {
            if (Path.GetExtension(input.File.FileName).ToLower() != ".csv")
            {
                return BadRequest("Invalid file type.");
            }

            List<Error> errors = new();

            using (var reader = new StreamReader(input.File.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<EmpHeader>().ToList();

                if (!records.Any())
                {
                    return BadRequest("No records found.");
                }

                bool isError = false;

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    int i = 2;

                    foreach (var record in records)
                    {
                        List<string> errorData = new();
                        isError = false;

                        var birthCountry = await _context.Countries.Where(x => x.Name.ToLower().Trim() == record.BirthCountryName.ToLower().Trim()).FirstOrDefaultAsync();
                        var birthState = await _context.States.Where(x => x.Name.ToLower().Trim() == record.BirthStateName.ToLower().Trim()).FirstOrDefaultAsync();
                        var religion = await _context.Religions.Where(x => x.Name.ToLower().Trim() == record.ReligionName.ToLower().Trim()).FirstOrDefaultAsync();
                        var device = await _context.DeviceSettings.Where(x => x.DeviceModel.ToLower().Trim() == record.Device.ToLower().Trim()).FirstOrDefaultAsync();
                        var rmEmp = await _context.EmpDetails.Where(x => x.CardId == record.RmEmpCode).FirstOrDefaultAsync();
                        var hodEmp = await _context.EmpDetails.Where(x => x.CardId == record.HodEmpCode).FirstOrDefaultAsync();
                        var company = await _context.Companies.Where(x => x.Name.ToLower().Trim() == record.CompanyName.ToLower().Trim()).FirstOrDefaultAsync();
                        var businessUnit = await _context.BusinessUnits.Where(x => x.Name.ToLower().Trim() == record.BusinessUnitName.ToLower().Trim()).FirstOrDefaultAsync();
                        var plant = await _context.Plants.Where(x => x.Name.ToLower().Trim() == record.PlantName.ToLower().Trim()).FirstOrDefaultAsync();
                        var region = await _context.Regions.Where(x => x.Name.ToLower().Trim() == record.RegionName.ToLower().Trim()).FirstOrDefaultAsync();
                        var division = await _context.Divisions.Where(x => x.Name.ToLower().Trim() == record.DivisionName.ToLower().Trim()).FirstOrDefaultAsync();
                        var branch = await _context.Branches.Where(x => x.Name.ToLower().Trim() == record.BranchName.ToLower().Trim()).FirstOrDefaultAsync();
                        var department = await _context.Departments.Where(x => x.Name.ToLower().Trim() == record.DepartmentName.ToLower().Trim()).FirstOrDefaultAsync();
                        var subDepartment = await _context.Departments.Where(x => x.Name.ToLower().Trim() == record.SubDepartmentName.ToLower().Trim()).FirstOrDefaultAsync();
                        var designation = await _context.Designations.Where(x => x.Name.ToLower().Trim() == record.DesignationName.ToLower().Trim()).FirstOrDefaultAsync();
                        var grade = await _context.Grades.Where(x => x.Name.ToLower().Trim() == record.GradeName.ToLower().Trim()).FirstOrDefaultAsync();
                        var mode = await _context.Modes.Where(x => x.Name.ToLower().Trim() == record.ModeName.ToLower().Trim()).FirstOrDefaultAsync();
                        var costCenter = await _context.CostCenters.Where(x => x.Name.ToLower().Trim() == record.CostCenterName.ToLower().Trim()).FirstOrDefaultAsync();
                        var uniformType = await _context.UniformTypes.Where(x => x.Name.ToLower().Trim() == record.UniformTypeName.ToLower().Trim()).FirstOrDefaultAsync();
                        var salaryBank = await _context.Banks.Where(x => x.Name.ToLower().Trim() == record.SalaryBankName.ToLower().Trim()).FirstOrDefaultAsync();
                        var workHour = await _context.WorkHours.Where(x => x.Name.ToLower().Trim() == record.WorkHourName.ToLower().Trim()).FirstOrDefaultAsync();

                        if (string.IsNullOrEmpty(record.EmpCode) || string.IsNullOrEmpty(record.Title) || string.IsNullOrEmpty(record.FirstName) || string.IsNullOrEmpty(record.LastName) || string.IsNullOrEmpty(record.ContactNumber)
                                || string.IsNullOrEmpty(record.Gender) || string.IsNullOrEmpty(record.MaritalStatus)
                            )
                        {
                            errorData.Add("EmpCode, Title, First Name, Last Name, Contact Number, Gender and Marital Status are required fields.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.EmpCode) && (await _context.EmpDetails.Where(x => x.CardId.Trim() == record.EmpCode.Trim()).AnyAsync()))
                        {
                            errorData.Add("EmpCode already exists");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.ContactNumber))
                        {
                            if (record.ContactNumber.Trim().Length != 10)
                            {
                                errorData.Add("Contact Number should be of 10 digits");
                                isError = true;
                            }

                            if (await _context.EmpDetails.AnyAsync(x => x.ContactNumber == record.ContactNumber.Trim()))
                            {
                                errorData.Add("Contact Number should be unique.");
                                isError = true;
                            }
                        }

                        if (!string.IsNullOrEmpty(record.OfficialContactNumber) && record.OfficialContactNumber.Length != 10)
                        {
                            errorData.Add("Official Contact Number should be of 10 digits");
                            isError = true;
                        }

                        char gender = 'M';
                        char maritalStatus = 'M';

                        if (!string.IsNullOrEmpty(record.Gender))
                        {
                            bool isGender = char.TryParse(record.Gender, out gender);

                            if (!isGender)
                            {
                                errorData.Add("Gender should be a character: M, F or O.");
                                isError = true;
                            }
                            else
                            {
                                if (!_genders.Contains(gender))
                                {
                                    errorData.Add("Invalid Gender Character. Please choose M, F or O.");
                                    isError = true;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(record.MaritalStatus))
                        {
                            bool isMaritalStatus = char.TryParse(record.MaritalStatus, out maritalStatus);

                            if (!isMaritalStatus)
                            {
                                errorData.Add("Marital Status should be a character: S or M.");
                                isError = true;
                            }
                            else
                            {
                                if (!_maritalStatus.Contains(maritalStatus))
                                {
                                    errorData.Add("Invalid Marital Status Character. Please choose S or M.");
                                    isError = true;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(record.Email) && await _context.EmpDetails.AnyAsync(x => x.Email.Trim().ToLower() == record.Email.Trim().ToLower()))
                        {
                            errorData.Add("Email must be unique.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.BirthCountryName) && birthCountry is null)
                        {
                            errorData.Add("Invalid Birth Country Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.BirthStateName) && birthState is null)
                        {
                            errorData.Add("Invalid Birth State Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.ReligionName) && religion is null)
                        {
                            errorData.Add("Invalid Religion Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.RmEmpCode) && rmEmp is null)
                        {
                            errorData.Add("Invalid RM EmpCode.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.HodEmpCode) && hodEmp is null)
                        {
                            errorData.Add("Invalid HOD EmpCode.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.CompanyName) && company is null)
                        {
                            errorData.Add("Invalid Company Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.BusinessUnitName) && businessUnit is null)
                        {
                            errorData.Add("Invalid Business Unit Name");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.PlantName) && plant is null)
                        {
                            errorData.Add("Invalid Plant Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.RegionName) && region is null)
                        {
                            errorData.Add("Invalid Region Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.DivisionName) && division is null)
                        {
                            errorData.Add("Invalid Division Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.BranchName) && branch is null)
                        {
                            errorData.Add("Invalid Location Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.DepartmentName) && department is null)
                        {
                            errorData.Add("Invalid Department Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.SubDepartmentName) && subDepartment is null)
                        {
                            errorData.Add("Invalid Sub Department Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.DesignationName) && designation is null)
                        {
                            errorData.Add("Invalid Designation Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.GradeName) && grade is null)
                        {
                            errorData.Add("Invalid Grade Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.ModeName) && mode is null)
                        {
                            errorData.Add("Invalid Mode Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.CostCenterName) && costCenter is null)
                        {
                            errorData.Add("Invalid Cost Center Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.UniformTypeName) && uniformType is null)
                        {
                            errorData.Add("Invalid Uniform Type Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.SalaryBankName) && salaryBank is null)
                        {
                            errorData.Add("Invalid Salary Bank Name.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.WorkHourName) && workHour is null)
                        {
                            errorData.Add("Invalid Shift Name.");
                            isError = true;
                        }

                        DateOnly dateOfBirth;
                        DateOnly joinDate;
                        DateOnly marriageDate;
                        DateOnly relevingDate;
                        DateOnly appointedDate;

                        if (!string.IsNullOrEmpty(record.DateOfBirth))
                        {
                            if(!DateOnly.TryParse(record.DateOfBirth, out dateOfBirth))
                            {
                                errorData.Add("Invalid Date of Birth. Please use format yyyy-mm-dd.");
                                isError = true;
                            };
                        }

                        if (!string.IsNullOrEmpty(record.JoinDate))
                        {
                            if (!DateOnly.TryParse(record.JoinDate, out joinDate))
                            {
                                errorData.Add("Invalid Join Date. Please use format yyyy-mm-dd.");
                                isError = true;
                            };
                        }

                        if (!string.IsNullOrEmpty(record.MarriageDate))
                        {
                            if (!DateOnly.TryParse(record.MarriageDate, out marriageDate))
                            {
                                errorData.Add("Invalid Marriage Date. Please use format yyyy-mm-dd.");
                                isError = true;
                            };
                        }  

                        if (!string.IsNullOrEmpty(record.RelevingDate))
                        {
                            if (!DateOnly.TryParse(record.RelevingDate, out relevingDate))
                            {
                                errorData.Add("Invalid Releving Date. Please use format yyyy-mm-dd.");
                                isError = true;
                            };
                        }

                        if (!string.IsNullOrEmpty(record.AppointedDate))
                        {
                            if (!DateOnly.TryParse(record.AppointedDate, out appointedDate))
                            {
                                errorData.Add("Invalid Appointed Date. Please use format yyyy-mm-dd.");
                                isError = true;
                            };
                        }

                        if (!string.IsNullOrEmpty(record.Device) && device is null)
                        {
                            errorData.Add("Invalid Device.");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.DeviceCode) && !string.IsNullOrEmpty(record.Device) && device is not null)
                        {
                            if(await _context.EmpDeviceCodes.AnyAsync(x => x.DeviceId == device.Id && x.DeviceCode == record.DeviceCode))
                            {
                                errorData.Add("Device Code linked with the device already exists.");
                                isError = true;
                            }
                        }

                        if (!string.IsNullOrEmpty(record.PermanentPincode))
                        {
                            if (record.PermanentPincode.Length != 6)
                            {
                                errorData.Add("Permanent Pincode should be of 6 digits.");
                                isError = true;
                            }
                        }

                        if (!string.IsNullOrEmpty(record.CorrespondancePincode))
                        {
                            if (record.CorrespondancePincode.Length != 6)
                            {
                                errorData.Add("Correspondance pin code should be of 6 digits.");
                                isError = true;
                            }
                        }

                        if (isError)
                        {
                            errors.Add(new Error
                            {
                                Record = i,
                                Errors = errorData
                            });

                            i++;

                            continue;
                        }

                        EmpDetail empDetail = new()
                        {
                            CardId = record.EmpCode.Trim(),
                            Title = record.Title,
                            FirstName = record.FirstName.Trim(),
                            MiddleName = record.MiddleName.Trim(),
                            LastName = record.LastName.Trim(),
                            Email = record.Email.Trim().ToLower(),
                            ContactNumber = record.ContactNumber,
                            Gender = gender,
                            MaritalStatus = maritalStatus,
                            BloodGroup = record.BloodGroup ?? "",
                            Nationality = record.Nationality,
                            BirthCountryId = birthCountry?.Id,
                            ReligionId = religion?.Id,
                            BirthStateId = birthState?.Id,
                            BirthPlace = record.BirthPlace,
                            DateOfBirth = !string.IsNullOrEmpty(record.DateOfBirth) ? dateOfBirth : null,
                            JoinDate = !string.IsNullOrEmpty(record.JoinDate) ? joinDate : null,
                            MarriageDate = !string.IsNullOrEmpty(record.MarriageDate) ? marriageDate : null,
                            RelevingDate = !string.IsNullOrEmpty(record.RelevingDate) ? relevingDate : null,
                            AppointedDate = !string.IsNullOrEmpty(record.AppointedDate) ? appointedDate : null,
                            PermanentAddress = record.PermanentAddress,
                            PermanentAddress2 = record.PermanentAddress2,
                            PermanentCity = record.PermanentCity,
                            PermanentPincode = record.PermanentPincode,
                            PermanentState = record.PermanentState,
                            PermanentDistrict = record.PermanentDistrict,
                            CorrespondanceAddress = record.CorrespondanceAddress,
                            CorrespondanceAddress2 = record.CorrespondanceAddress2,
                            CorrespondanceCity = record.CorrespondanceCity,
                            CorrespondancePincode = record.CorrespondancePincode,
                            CorrespondanceState = record.CorrespondanceState,
                            CorrespondanceDistrict = record.CorrespondanceDistrict,
                            PanNumber = record.PanNumber,
                            PassportNumber = record.PassportNumber,
                            AadharNumber = record.AadharNumber,
                            DrivingLicenseNumber = record.DrivingLicenseNumber ?? "",

                            //DefaultValues

                            Appointed = 0,
                            EmergencyContactPerson = "N/A",
                            EmergencyContactNumber = "N/A",
                            FatherName = "N/A",
                            MotherName = "N/A",
                            GrandFatherName = "N/A",
                            TransactionUser = "N/A",
                        };

                        _context.Add(empDetail);
                        await _context.SaveChangesAsync();

                        if (!string.IsNullOrEmpty(record.DeviceCode) && !string.IsNullOrEmpty(record.Device))
                        {
                            _context.Add(new EmpDeviceCode
                            {
                                EmpId = empDetail.Id,
                                DeviceCode = record.DeviceCode,
                                DeviceId = device.Id
                            });
                        }

                        if (!string.IsNullOrEmpty(record.WorkHourName))
                        {
                            List<DefaultWorkHour> defaultWorkHourData = new();

                            for (short day = 1; day < 8; day++)
                            {
                                defaultWorkHourData.Add(new DefaultWorkHour
                                {
                                    EmpId = empDetail.Id,
                                    DayId = day,
                                    WorkHourId = workHour.Id,
                                });
                            }

                            _context.AddRange(defaultWorkHourData);
                        }

                        EmpTransaction data = new()
                        {
                            EmployeeId = empDetail.Id,
                            OfficialEmail = record.OfficialEmail.Trim().ToLower(),
                            OfficialContactNumber = record.OfficialContactNumber,
                            CompanyId = company?.Id,
                            RmEmpId = rmEmp?.Id,
                            HodEmpId = hodEmp?.Id,
                            BusinessUnitId = businessUnit?.Id,
                            PlantId = plant?.Id,
                            RegionId = region?.Id,
                            DivisionId = division?.Id,
                            BranchId = branch?.Id,
                            DepartmentId = department?.Id,
                            SubDepartmentId = subDepartment?.Id,
                            ModeId = mode?.Id,
                            StatusId = 1,
                            DesignationId = designation?.Id,
                            GradeId = grade?.Id,
                            PersonalArea = record.PersonalArea,
                            SubArea = record.SubArea,
                            CostCenterId = costCenter?.Id,
                            PositionCode = record.PositionCode,
                            SubType = record.SubType,
                            PersonType = record.PersonType,
                            UniformStatus = record.UniformStatus,
                            UniformTypeId = uniformType?.Id,
                            ExtraUniform = record.ExtraUniform,
                            EsiNumber = record.EsiNumber,
                            UanNumber = record.UanNumber,
                            SalaryBankId = salaryBank?.Id,
                            AccountNumber = record.AccountNumber,
                            PfApplicable = record.PfApplicable,
                            PfAccountNumber = record.PfAccountNumber,
                            IsCeiling = record.IsCeiling,
                            EpsApplicable = record.EpsApplicable,
                            IfscCode = record.IfscCode,
                            VpfApplicable = record.VpfApplicable,
                            VpfAmount = record.VpfAmount,

                            //Default Values
                            TransactionUser = "N/A",
                            TransactionMode = "N/A",
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
                        await _context.SaveChangesAsync();

                        EmpLog empLog = new()
                        {
                            Id = data.Id,
                            EmployeeId = empDetail.Id
                        };

                        _context.Add(empLog);
                        await _context.SaveChangesAsync();

                        i++;
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    throw;
                }
            }

            return Ok(new
            {
                Errors = errors
            });
        }

        [HttpGet("DownloadFormat/UpdateEmp")]
        public async Task<IActionResult> UpdateEmpFormat()
        {
            Type table = typeof(UpdateEmpHeader);

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

            return File(data, "text/csv", "Employees.csv");
        }

        [CustomAuthorize("update-employee-through-import")]
        [HttpPost("Import/UpdateEmp")]
        public async Task<IActionResult> UpdateEmpImport([FromForm] ImportInputModel input)
        {
            if (Path.GetExtension(input.File.FileName).ToLower() != ".csv")
            {
                return BadRequest("Invalid file type.");
            }

            List<Error> errors = new();

            using (var reader = new StreamReader(input.File.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<UpdateEmpHeader>().ToList();

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
                        List<string> errorData = new();
                        isError = false;

                        var empDetail = await _context.EmpDetails.Where(x => x.CardId.Trim() == record.EmpCode.Trim()).FirstOrDefaultAsync();
                        var device = await _context.DeviceSettings.Where(x => x.DeviceModel.ToLower().Trim() == record.Device.ToLower().Trim()).FirstOrDefaultAsync();
                        var workHour = await _context.WorkHours.Where(x => x.Name.ToLower().Trim() == record.WorkHourName.ToLower().Trim()).FirstOrDefaultAsync();

                        if (string.IsNullOrEmpty(record.EmpCode))
                        {
                            errorData.Add("EMPCODE is required fields.");
                            isError = true;
                        }

                        if (string.IsNullOrEmpty(record.Device) && string.IsNullOrEmpty(record.DeviceCode) && string.IsNullOrEmpty(record.WorkHourName))
                        {
                            errorData.Add("Nothing to update.");
                            isError = true;
                        }

                        if ((!string.IsNullOrEmpty(record.Device) && string.IsNullOrEmpty(record.DeviceCode)) || 
                            (string.IsNullOrEmpty(record.Device) && !string.IsNullOrEmpty(record.DeviceCode)))
                        {
                            errorData.Add("DEVICE and DEVICECODE both are required to update device information.");
                            isError = true;
                        }

                        if (empDetail is null)
                        {
                            errorData.Add("EMPCODE does not exist.");
                            isError = true;
                        } else
                        {
                            var companyId = await _context.EmpLogs.Where(x => x.EmployeeId == empDetail.Id).Join(_context.EmpTransactions, el => el.Id, et => et.Id, (el, et) => new { et.CompanyId }).Select(x => x.CompanyId).FirstOrDefaultAsync();

                            if (User.GetUserRole() != "super-admin")
                            {
                                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                                if (!companyIds.Contains(companyId ?? 0))
                                {
                                    errorData.Add("Forbidden");
                                }
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(record.Device) && device is null)
                        {
                            errorData.Add("Device does not exist");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.WorkHourName) && workHour is null)
                        {
                            errorData.Add("Shift does not exist");
                            isError = true;
                        }

                        if (isError)
                        {
                            errors.Add(new Error
                            {
                                Record = i,
                                Errors = errorData
                            });

                            i++;

                            continue;
                        }

                        if (!string.IsNullOrEmpty(record.Device) && !string.IsNullOrEmpty(record.DeviceCode))
                        {
                            var empDevice = await _context.EmpDeviceCodes.Where(x => x.EmpId == empDetail.Id).FirstOrDefaultAsync();

                            if (empDevice != null)
                            {
                                empDevice.DeviceId = device.Id;
                                empDevice.DeviceCode = record.DeviceCode;
                            } else
                            {
                                _context.Add(new EmpDeviceCode
                                {
                                    EmpId = empDetail.Id,
                                    DeviceId = device.Id,
                                    DeviceCode = record.DeviceCode
                                });
                            }
                        }

                        if (!string.IsNullOrEmpty(record.WorkHourName))
                        {
                            var defaultWorkHours = await _context.DefaultWorkHours.Where(x => x.EmpId == empDetail.Id).ToListAsync();

                            if (defaultWorkHours.Count > 0)
                            {
                                foreach (var defaultWorkHour in defaultWorkHours)
                                {
                                    defaultWorkHour.WorkHourId = workHour.Id;
                                    defaultWorkHour.UpdatedAt = DateTime.UtcNow;
                                }
                            }
                            else
                            {
                                List<DefaultWorkHour> defaultWorkHourData = new();

                                for (short day = 1; day < 8; day++)
                                {
                                    defaultWorkHourData.Add(new DefaultWorkHour
                                    {
                                        EmpId = empDetail.Id,
                                        DayId = day,
                                        WorkHourId = workHour.Id,
                                    });
                                }

                                _context.AddRange(defaultWorkHourData);
                            }
                        }

                        await _context.SaveChangesAsync();

                        i++;
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return Ok(new
            {
                Errors = errors
            });

        }

        [HttpGet("DownloadFormat/UpdateManager")]
        public async Task<IActionResult> UpdateManagerFormat()
        {
            Type table = typeof(UpdateManagerHeader);

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

            return File(data, "text/csv", "Employees.csv");
        }

        [CustomAuthorize("update-employee-through-import")]
        [HttpPost("Import/UpdateManager")]
        public async Task<IActionResult> UpdateManagerImport([FromForm] ImportInputModel input)
        {
            if (Path.GetExtension(input.File.FileName).ToLower() != ".csv")
            {
                return BadRequest("Invalid file type.");
            }

            List<Error> errors = new();

            using (var reader = new StreamReader(input.File.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<UpdateManagerHeader>().ToList();

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
                        List<string> errorData = new();
                        isError = false;

                        var empDetail = await _context.EmpDetails.Where(x => x.CardId.Trim() == record.EmpCode.Trim()).FirstOrDefaultAsync();
                        var rmEmp = await _context.EmpDetails.Where(x => x.CardId.ToLower().Trim() == record.RmEmpCode.ToLower().Trim()).FirstOrDefaultAsync();
                        var hodEmp = await _context.EmpDetails.Where(x => x.CardId.ToLower().Trim() == record.HodEmpCode.ToLower().Trim()).FirstOrDefaultAsync();

                        if (string.IsNullOrEmpty(record.EmpCode))
                        {
                            errorData.Add("EMPCODE is required fields.");
                            isError = true;
                        }

                        if (string.IsNullOrEmpty(record.RmEmpCode) && string.IsNullOrEmpty(record.HodEmpCode))
                        {
                            errorData.Add("Nothing to update.");
                            isError = true;
                        }

                        if (empDetail is null)
                        {
                            errorData.Add("EMPCODE does not exist.");
                            isError = true;
                        }
                        else
                        {
                            var companyId = await _context.EmpLogs.Where(x => x.EmployeeId == empDetail.Id).Join(_context.EmpTransactions, el => el.Id, et => et.Id, (el, et) => new { et.CompanyId }).Select(x => x.CompanyId).FirstOrDefaultAsync();

                            if (User.GetUserRole() != "super-admin")
                            {
                                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                                if (!companyIds.Contains(companyId ?? 0))
                                {
                                    errorData.Add("Forbidden");
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(record.RmEmpCode) && rmEmp is null)
                        {
                            errorData.Add("RMEMPCODE does not exist");
                            isError = true;
                        }

                        if (!string.IsNullOrEmpty(record.HodEmpCode) && hodEmp is null)
                        {
                            errorData.Add("HODEMPCODE does not exist");
                            isError = true;
                        }

                        if (isError)
                        {
                            errors.Add(new Error
                            {
                                Record = i,
                                Errors = errorData
                            });

                            i++;

                            continue;
                        }

                        int? pId = await _context.EmpLogs.Where(x => x.EmployeeId == empDetail.Id).Select(x => x.Id).FirstOrDefaultAsync();

                        if (pId is null)
                        {
                            errorData.Add("Employee is not registered.");

                            errors.Add(new Error
                            {
                                Record = i,
                                Errors = errorData,
                            });

                            i++;

                            continue;
                        }

                        var empTransaction = await _context.EmpTransactions.Where(x => x.Id == pId).FirstOrDefaultAsync();

                        if (rmEmp is not null)
                        {
                            empTransaction.RmEmpId = rmEmp.Id;
                        }

                        if (hodEmp is not null)
                        {
                            empTransaction.HodEmpId = hodEmp.Id;
                        }

                        await _context.SaveChangesAsync();

                        i++;
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return Ok(new
            {
                Errors = errors
            });

        }


        [HttpGet("Birthdays")]
        public async Task<IActionResult> GetAllBirthdays()
        {
            var data = await _context.EmpDetails
                .Where(x => x.DateOfBirth != null ?
                                    x.DateOfBirth.Value.Month == DateTime.UtcNow.Month && x.DateOfBirth.Value.Day == DateTime.UtcNow.Day
                                    : false)
                .Join(_context.EmpTransactions,
                    ed => ed.Id,
                    et => et.Id,
                    (ed, et) => new
                    {
                        EmpId = ed.Id,
                        Name = FullName(ed.FirstName, ed.MiddleName, ed.LastName),
                        DesignationName = et != null ? (et.Designation != null ? et.Designation.Name : "") : ""
                    }
                ).Select(e => new
                {
                    e.EmpId,
                    e.Name,
                    e.DesignationName
                })
                .ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        [HttpGet("MarriageAnniversary")]
        public async Task<IActionResult> GetAllAnniversary()
        {
            var data = await _context.EmpDetails
                .Where(x => x.MarriageDate != null ?
                                    x.MarriageDate.Value.Month == DateTime.UtcNow.Month && x.MarriageDate.Value.Day == DateTime.UtcNow.Day
                                    : false)
                .Join(_context.EmpTransactions,
                    ed => ed.Id,
                    et => et.Id,
                    (ed, et) => new
                    {
                        EmpId = ed.Id,
                        Name = FullName(ed.FirstName, ed.MiddleName, ed.LastName),
                        DesignationName = et != null ? (et.Designation != null ? et.Designation.Name : "") : ""
                    }
                ).Select(e => new
                {
                    e.EmpId,
                    e.Name,
                    e.DesignationName
                })
                .ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        [CustomAuthorize("export-employee")]
        [HttpGet("Export")]
        public async Task<IActionResult> Export()
        {
            var defaultShift = await _context.DefaultWorkHours
                .Include(x => x.WorkHour)
                .Where(x => x.EmpId == null)
                .FirstOrDefaultAsync();

            List<int> companyIds = new();
            List<EmpHeader> data = new();

            if (User.GetUserRole() != "super-admin")
            {
                companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                data = await (from detail in _context.EmpDetails
                                  join empDevice in _context.EmpDeviceCodes on detail.Id equals empDevice.EmpId into empDevices
                                  from empDevice in empDevices.DefaultIfEmpty()
                                  join workHour in _context.DefaultWorkHours.Where(x => x.DayId == 1) on detail.Id equals workHour.EmpId into workHours
                                  from workHour in workHours.DefaultIfEmpty()
                                  join log in _context.EmpLogs on detail.Id equals log.EmployeeId into logs
                                  from log in logs.DefaultIfEmpty()
                                  join tran in _context.EmpTransactions.Where(x => companyIds.Contains(x.CompanyId ?? 0)) on log.Id equals tran.Id into trans
                                  from tran in trans.DefaultIfEmpty()
                                  select new EmpHeader
                                  {
                                      EmpCode = detail.CardId,
                                      Title = detail.Title,
                                      FirstName = detail.FirstName,
                                      MiddleName = detail.MiddleName,
                                      LastName = detail.LastName,
                                      Email = detail.Email,
                                      ContactNumber = detail.ContactNumber,
                                      Gender = detail.Gender.ToString(),
                                      MaritalStatus = detail.MaritalStatus.ToString(),
                                      BloodGroup = detail.BloodGroup,
                                      Nationality = detail.Nationality,
                                      BirthCountryName = detail.BirthCountry != null ? detail.BirthCountry.Name : "",
                                      ReligionName = detail.Religion != null ? detail.Religion.Name : "",
                                      BirthStateName = detail.BirthState != null ? detail.BirthState.Name : "",
                                      BirthPlace = detail.BirthPlace,
                                      DateOfBirth = detail.DateOfBirth.ToString(),
                                      JoinDate = detail.JoinDate.ToString(),
                                      MarriageDate = detail.MarriageDate.ToString(),
                                      RelevingDate = detail.RelevingDate.ToString(),
                                      AppointedDate = detail.AppointedDate.ToString(),
                                      PermanentAddress = detail.PermanentAddress,
                                      PermanentAddress2 = detail.PermanentAddress2,
                                      PermanentCity = detail.PermanentCity,
                                      PermanentPincode = detail.PermanentPincode,
                                      PermanentState = detail.PermanentState,
                                      PermanentDistrict = detail.PermanentDistrict,
                                      CorrespondanceAddress = detail.CorrespondanceAddress,
                                      CorrespondanceAddress2 = detail.CorrespondanceAddress2,
                                      CorrespondanceCity = detail.CorrespondanceCity,
                                      CorrespondancePincode = detail.CorrespondancePincode,
                                      CorrespondanceState = detail.CorrespondanceState,
                                      CorrespondanceDistrict = detail.CorrespondanceDistrict,
                                      PanNumber = detail.PanNumber,
                                      PassportNumber = detail.PassportNumber,
                                      AadharNumber = detail.AadharNumber,
                                      DrivingLicenseNumber = detail.DrivingLicenseNumber,
                                      OfficialContactNumber = tran != null ? tran.OfficialContactNumber : "",
                                      OfficialEmail = tran != null ? tran.OfficialEmail : "",
                                      RmEmpCode = tran != null ? (tran.RmEmp != null ? tran.RmEmp.CardId : "") : "",
                                      HodEmpCode = tran != null ? (tran.HodEmp != null ? tran.HodEmp.CardId : "") : "",
                                      BranchName = tran != null ? (tran.Branch != null ? tran.Branch.Name : "") : "",
                                      DepartmentName = tran != null ? (tran.Department != null ? tran.Department.Name : "") : "",
                                      SubDepartmentName = tran != null ? (tran.SubDepartment != null ? tran.SubDepartment.Name : "") : "",
                                      DesignationName = tran != null ? (tran.Designation != null ? tran.Designation.Name : "") : "",
                                      GradeName = tran != null ? (tran.Grade != null ? tran.Grade.Name : "") : "",
                                      CompanyName = tran != null ? (tran.Company != null ? tran.Company.Name : "") : "",
                                      BusinessUnitName = tran != null ? (tran.BusinessUnit != null ? tran.BusinessUnit.Name : "") : "",
                                      PlantName = tran != null ? (tran.Plant != null ? tran.Plant.Name : "") : "",
                                      RegionName = tran != null ? (tran.Region != null ? tran.Region.Name : "") : "",
                                      DivisionName = tran != null ? (tran.Division != null ? tran.Division.Name : "") : "",
                                      PersonalArea = tran != null ? tran.PersonalArea : "",
                                      SubArea = tran != null ? tran.SubArea : "",
                                      CostCenterName = tran != null ? (tran.CostCenter != null ? tran.CostCenter.Name : "") : "",
                                      PositionCode = tran != null ? tran.PositionCode : "",
                                      PersonType = tran != null ? tran.PersonType : "",
                                      UniformStatus = tran != null ? tran.UniformStatus : null,
                                      UniformTypeName = tran != null ? (tran.UniformType != null ? tran.UniformType.Name : "") : "",
                                      ExtraUniform = tran != null ? tran.ExtraUniform : null,
                                      EsiNumber = tran != null ? tran.EsiNumber : "",
                                      UanNumber = tran != null ? tran.UanNumber : "",
                                      SalaryBankName = tran != null ? (tran.SalaryBank != null ? tran.SalaryBank.Name : "") : "",
                                      PfAccountNumber = tran != null ? tran.PfAccountNumber : "",
                                      SubType = tran != null ? tran.SubType : "",
                                      AccountNumber = tran != null ? tran.AccountNumber : "",
                                      EpsApplicable = tran != null ? tran.EpsApplicable : null,
                                      IfscCode = tran != null ? tran.IfscCode : "",
                                      IsCeiling = tran != null ? tran.IsCeiling : null,
                                      ModeName = tran != null ? (tran.Mode != null ? tran.Mode.Name : "") : "",
                                      PfApplicable = tran != null ? tran.PfApplicable : null,
                                      VpfAmount = tran != null ? tran.VpfAmount : null,
                                      VpfApplicable = tran != null ? tran.VpfApplicable : null,
                                      Device = empDevice != null ? (empDevice.Device != null ? empDevice.Device.DeviceModel : "") : "",
                                      DeviceCode = empDevice != null ? empDevice.DeviceCode : "",
                                      WorkHourName = workHour != null ? workHour.WorkHour.Name : defaultShift != null ? defaultShift.WorkHour.Name : "",
                                      StatusName = tran != null ? (tran.Status != null ? tran.Status.Name : "") : ""
                                  }).ToListAsync();
            } else
            {
                data = await (from detail in _context.EmpDetails
                                  join empDevice in _context.EmpDeviceCodes on detail.Id equals empDevice.EmpId into empDevices
                                  from empDevice in empDevices.DefaultIfEmpty()
                                  join workHour in _context.DefaultWorkHours.Where(x => x.DayId == 1) on detail.Id equals workHour.EmpId into workHours
                                  from workHour in workHours.DefaultIfEmpty()
                                  join log in _context.EmpLogs on detail.Id equals log.EmployeeId into logs
                                  from log in logs.DefaultIfEmpty()
                                  join tran in _context.EmpTransactions on log.Id equals tran.Id into trans
                                  from tran in trans.DefaultIfEmpty()
                                  select new EmpHeader
                                  {
                                      EmpCode = detail.CardId,
                                      Title = detail.Title,
                                      FirstName = detail.FirstName,
                                      MiddleName = detail.MiddleName,
                                      LastName = detail.LastName,
                                      Email = detail.Email,
                                      ContactNumber = detail.ContactNumber,
                                      Gender = detail.Gender.ToString(),
                                      MaritalStatus = detail.MaritalStatus.ToString(),
                                      BloodGroup = detail.BloodGroup,
                                      Nationality = detail.Nationality,
                                      BirthCountryName = detail.BirthCountry != null ? detail.BirthCountry.Name : "",
                                      ReligionName = detail.Religion != null ? detail.Religion.Name : "",
                                      BirthStateName = detail.BirthState != null ? detail.BirthState.Name : "",
                                      BirthPlace = detail.BirthPlace,
                                      DateOfBirth = detail.DateOfBirth.ToString(),
                                      JoinDate = detail.JoinDate.ToString(),
                                      MarriageDate = detail.MarriageDate.ToString(),
                                      RelevingDate = detail.RelevingDate.ToString(),
                                      AppointedDate = detail.AppointedDate.ToString(),
                                      PermanentAddress = detail.PermanentAddress,
                                      PermanentAddress2 = detail.PermanentAddress2,
                                      PermanentCity = detail.PermanentCity,
                                      PermanentPincode = detail.PermanentPincode,
                                      PermanentState = detail.PermanentState,
                                      PermanentDistrict = detail.PermanentDistrict,
                                      CorrespondanceAddress = detail.CorrespondanceAddress,
                                      CorrespondanceAddress2 = detail.CorrespondanceAddress2,
                                      CorrespondanceCity = detail.CorrespondanceCity,
                                      CorrespondancePincode = detail.CorrespondancePincode,
                                      CorrespondanceState = detail.CorrespondanceState,
                                      CorrespondanceDistrict = detail.CorrespondanceDistrict,
                                      PanNumber = detail.PanNumber,
                                      PassportNumber = detail.PassportNumber,
                                      AadharNumber = detail.AadharNumber,
                                      DrivingLicenseNumber = detail.DrivingLicenseNumber,
                                      OfficialContactNumber = tran != null ? tran.OfficialContactNumber : "",
                                      OfficialEmail = tran != null ? tran.OfficialEmail : "",
                                      RmEmpCode = tran != null ? (tran.RmEmp != null ? tran.RmEmp.CardId : "") : "",
                                      HodEmpCode = tran != null ? (tran.HodEmp != null ? tran.HodEmp.CardId : "") : "",
                                      BranchName = tran != null ? (tran.Branch != null ? tran.Branch.Name : "") : "",
                                      DepartmentName = tran != null ? (tran.Department != null ? tran.Department.Name : "") : "",
                                      SubDepartmentName = tran != null ? (tran.SubDepartment != null ? tran.SubDepartment.Name : "") : "",
                                      DesignationName = tran != null ? (tran.Designation != null ? tran.Designation.Name : "") : "",
                                      GradeName = tran != null ? (tran.Grade != null ? tran.Grade.Name : "") : "",
                                      CompanyName = tran != null ? (tran.Company != null ? tran.Company.Name : "") : "",
                                      BusinessUnitName = tran != null ? (tran.BusinessUnit != null ? tran.BusinessUnit.Name : "") : "",
                                      PlantName = tran != null ? (tran.Plant != null ? tran.Plant.Name : "") : "",
                                      RegionName = tran != null ? (tran.Region != null ? tran.Region.Name : "") : "",
                                      DivisionName = tran != null ? (tran.Division != null ? tran.Division.Name : "") : "",
                                      PersonalArea = tran != null ? tran.PersonalArea : "",
                                      SubArea = tran != null ? tran.SubArea : "",
                                      CostCenterName = tran != null ? (tran.CostCenter != null ? tran.CostCenter.Name : "") : "",
                                      PositionCode = tran != null ? tran.PositionCode : "",
                                      PersonType = tran != null ? tran.PersonType : "",
                                      UniformStatus = tran != null ? tran.UniformStatus : null,
                                      UniformTypeName = tran != null ? (tran.UniformType != null ? tran.UniformType.Name : "") : "",
                                      ExtraUniform = tran != null ? tran.ExtraUniform : null,
                                      EsiNumber = tran != null ? tran.EsiNumber : "",
                                      UanNumber = tran != null ? tran.UanNumber : "",
                                      SalaryBankName = tran != null ? (tran.SalaryBank != null ? tran.SalaryBank.Name : "") : "",
                                      PfAccountNumber = tran != null ? tran.PfAccountNumber : "",
                                      SubType = tran != null ? tran.SubType : "",
                                      AccountNumber = tran != null ? tran.AccountNumber : "",
                                      EpsApplicable = tran != null ? tran.EpsApplicable : null,
                                      IfscCode = tran != null ? tran.IfscCode : "",
                                      IsCeiling = tran != null ? tran.IsCeiling : null,
                                      ModeName = tran != null ? (tran.Mode != null ? tran.Mode.Name : "") : "",
                                      PfApplicable = tran != null ? tran.PfApplicable : null,
                                      VpfAmount = tran != null ? tran.VpfAmount : null,
                                      VpfApplicable = tran != null ? tran.VpfApplicable : null,
                                      Device = empDevice != null ? (empDevice.Device != null ? empDevice.Device.DeviceModel : "") : "",
                                      DeviceCode = empDevice != null ? empDevice.DeviceCode : "",
                                      WorkHourName = workHour != null ? workHour.WorkHour.Name : defaultShift != null ? defaultShift.WorkHour.Name : "",
                                      StatusName = tran != null ? (tran.Status != null ? tran.Status.Name : "") : ""
                                  }).ToListAsync();
            }

            Response.ContentType = "text/csv";
            Response.Headers.Add("Content-Disposition", $"attachment; filename=Employees");

            await using (var writer = new StreamWriter(Response.Body))
            await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var record in data)
                {
                    await csv.WriteRecordsAsync(new EmpHeader[] { record });
                    await csv.FlushAsync();
                }
            }

            await Response.CompleteAsync();

            return new EmptyResult();
        }

        private static string FullName(string firstName, string middleName, string lastName)
        {
            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
        }

        public class AllEmpDTO
        {
            public int EmpId { get; set; }
            public string EmpCode { get; set; }
            public string Name { get; set; }
            public string CompanyName { get; set; }
            public string BranchName { get; set; }
            public string DepartmentName { get; set; }
            public string DesignationName { get; set; }
            public string GradeName { get; set; }
            public string StatusName { get; set; }
            public string WorkHourName { get; set; }
        }

        public class EmpHeader
        {
            [Name("EmpCode")]
            public string EmpCode { get; set; }

            [Name("Title")]
            public string Title { get; set; }
            
            [Name("First Name")]
            public string FirstName { get; set; }
            
            [Name("Middle Name")]
            public string MiddleName { get; set; }
            
            [Name("Last Name")]
            public string LastName { get; set; }
            
            [Name("Email")]
            public string Email { get; set; }
            
            [Name("Contact Number")]
            public string ContactNumber { get; set; }

            [Name("Gender")]
            public string Gender { get; set; }

            [Name("Marital Status")]
            public string MaritalStatus { get; set; }

            [Name("Blood Group")]
            public string BloodGroup { get; set; }
            
            [Name("Nationality")]
            public string Nationality { get; set; }

            [Name("Birth country")]
            public string BirthCountryName { get; set; }
            
            [Name("Religion")]
            public string ReligionName { get; set; }

            [Name("Birth State")]
            public string BirthStateName { get; set; }
            
            [Name("Birth Place")]
            public string BirthPlace { get; set; }

            [Name("Date of Birth")]
            public string DateOfBirth { get; set; }

            [Name("Join Date")]
            public string JoinDate { get; set; }

            [Name("Marriage Date")]
            public string MarriageDate { get; set; }

            [Name("Releving Date")]
            public string RelevingDate { get; set; }

            [Name("Appointed Date")]
            public string AppointedDate { get; set; }

            [Name("Permanent Address")]
            public string PermanentAddress { get; set; }

            [Name("Permanent Address 2")]
            public string PermanentAddress2 { get; set; }

            [Name("Permanent City")]
            public string PermanentCity { get; set; }

            [Name("Permanent Pincode")]
            public string PermanentPincode { get; set; }

            [Name("Permanent State")]
            public string PermanentState { get; set; }

            [Name("Permanent District")]
            public string PermanentDistrict { get; set; }

            [Name("Correspondance Address")]
            public string CorrespondanceAddress { get; set; }
            
            [Name("Correspondance Address 2")]
            public string CorrespondanceAddress2 { get; set; }
            
            [Name("Correspondance City")]
            public string CorrespondanceCity { get; set; }
            
            [Name("Correspondnace Pincode")]
            public string CorrespondancePincode { get; set; }
            
            [Name("Correspondance State")]
            public string CorrespondanceState { get; set; }
            
            [Name("Correspondance District")]
            public string CorrespondanceDistrict { get; set; }
            
            [Name("Pan Number")]
            public string PanNumber { get; set; }
            
            [Name("Passport Number")]
            public string PassportNumber { get; set; }
            
            [Name("Aadhar Number")]
            public string AadharNumber { get; set; }
            
            [Name("Driving License Number")]
            public string DrivingLicenseNumber { get; set; }

            [Name("Official Contact Number")]
            public string OfficialContactNumber { get; set; }

            [Name("Official Email")]
            public string OfficialEmail { get; set; }

            [Name("Device Code")]
            public string DeviceCode { get; set; }

            [Name("Device")]
            public string Device { get; set; }

            [Name("RM EmpCode")]
            public string RmEmpCode { get; set; }

            [Name("HOD EmpCode")]
            public string HodEmpCode { get; set; }

            [Name("Company")]
            public string? CompanyName { get; set; }

            [Name("Business Unit")]
            public string BusinessUnitName { get; set; }

            [Name("Plant")]
            public string PlantName { get; set; }

            [Name("Region")]
            public string RegionName { get; set; }

            [Name("Division")]
            public string DivisionName { get; set; }

            [Name("Location")]
            public string BranchName { get; set; }

            [Name("Department")]
            public string DepartmentName { get; set; }

            [Name("Sub Department")]
            public string SubDepartmentName { get; set; }

            [Name("Designation")]
            public string DesignationName { get; set; }

            [Name("Grade")]
            public string GradeName { get; set; }

            [Name("Mode")]
            public string ModeName { get; set; }

            [Name("Personal Area")]
            public string PersonalArea { get; set; }

            [Name("Sub Area")]
            public string SubArea { get; set; }

            [Name("Cost Center")]
            public string CostCenterName { get; set; }

            [Name("Position Code")]
            public string PositionCode { get; set; }

            [Name("Sub Type")]
            public string SubType { get; set; }

            [Name("Person Type")]
            public string PersonType { get; set; }

            [Name("Uniform Status")]
            public bool? UniformStatus { get; set; }

            [Name("Uniform Type")]
            public string UniformTypeName { get; set; }

            [Name("Extra Uniform")]
            public bool? ExtraUniform { get; set; }

            [Name("Esi Number")]
            public string EsiNumber { get; set; }

            [Name("Uan Number")]
            public string UanNumber { get; set; }

            [Name("Salary Bank")]
            public string SalaryBankName { get; set; }

            [Name("Account Number")]
            public string AccountNumber { get; set; }

            [Name("PF Account Number")]
            public string PfAccountNumber { get; set; }

            [Name("Shift")]
            public string WorkHourName { get; set; }

            [Name("PF Applicable?")]
            public bool? PfApplicable { get; set; }

            [Name("Is Ceiling?")]
            public bool? IsCeiling { get; set; }

            [Name("Eps Applicable?")]
            public bool? EpsApplicable { get; set; }

            [Name("IFS Code")]
            public string IfscCode { get; set; }

            [Name("VPF Applicable?")]
            public bool? VpfApplicable { get; set; }

            [Name("VPF Amount")]
            public decimal? VpfAmount { get; set; }

            [Name("Status")]
            public string StatusName { get; set; }
        } 

        public class UpdateEmpHeader
        {
            [Name("EMPCODE")]
            public string EmpCode { get; set; }

            [Name("DEVICE")]
            public string? Device { get; set; }

            [Name("DEVICECODE")]
            public string? DeviceCode { get; set; }

            [Name("SHIFTNAME")]
            public string? WorkHourName { get; set; }
        }

        public class UpdateManagerHeader
        {
            [Name("EMPCODE")]
            public string EmpCode { get; set; }

            [Name("RMEMPCODE")]
            public string RmEmpCode { get; set; }

            [Name("HODEMPCODE")]
            public string HodEmpCode { get; set; }
        }

        public class Error
        {
            public int Record { get; set; }
            public List<string> Errors { get; set; }
        }

        public class ImportInputModel
        {
            public IFormFile File { get; set; }
        }

        public class AssignCalendarInputModel
        {
            public int EmpId { get; set; }
            public int CalendarId { get; set; }
        }

        public class UpdateStatusModel
        {
            public int EmpId { get; set; }
            public short StatusId { get; set; }
        }

        public class ImportInputModelValidator : AbstractValidator<ImportInputModel>
        {
            public ImportInputModelValidator()
            {
                RuleFor(x => x.File)
                    .NotEmpty();
            }
        }

        public class UpdateStatusModelValidator : AbstractValidator<UpdateStatusModel>
        {
            private readonly DataContext _context;

            public UpdateStatusModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.StatusId)
                    .NotEmpty()
                    .IdMustExist(_context.Statuses.AsQueryable());
            }
        }

        public class AssignCalendarInputModelValidator : AbstractValidator<AssignCalendarInputModel>
        {
            private readonly DataContext _context;

            public AssignCalendarInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.CalendarId)
                    .NotEmpty()
                    .IdMustExist(_context.Calendars.AsQueryable());
            }
        }
    }
}
