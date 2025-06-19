
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mysqlx.Notice;
using NPOI.OpenXmlFormats.Shared;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Security.Policy;
using static Hrms.AdminApi.Controllers.AttendancesController;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "super-admin")]
    public class SalaryAnnexuresController : ControllerBase
    {
        private readonly DataContext _context;
        private static readonly List<string> ShDataTypes = new()
        {
            "PERCENT",
            "AMOUNT",
            "PERUNIT"
        };
        private readonly DbHelper _dbHelper;

        public SalaryAnnexuresController(DataContext context, DbHelper dbHelper)
        {
            _context = context;
            _dbHelper = dbHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name)
        {
            var query = _context.SalaryAnnexures
                .Include(x => x.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name == name);
            }

            Expression<Func<SalaryAnnexure, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "CreatedByUserName" => x => x.User.UserName,
                "CreatedAt" => x => x.CreatedAt,
                "UpdatedAt" => x => x.UpdatedAt,
                _ => x => x.AnxId
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.AnxId);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<SalaryAnnexure>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    AnxId = x.AnxId,
                    Name = x.Name,
                    CreatedByUserId = x.User.Id,
                    CreatedByUserName = x.User.UserName,
                    IsDraft = x.IsDraft,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.SalaryAnnexures
                .Include(x => x.User)
                .Where(x => !x.IsDraft)
                .ToListAsync();

            return Ok(new
            {
                SalaryAnnexures = data.Select(x => new
                {
                    AnxId = x.AnxId,
                    Name = x.Name,
                    CreatedByUserId = x.User.Id,
                    CreatedByUserName = x.User.UserName,
                    IsDraft = x.IsDraft,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
            });
        }

        [HttpGet("{anxId}")]
        public async Task<IActionResult> GetDetail(int anxId)
        {
            var salaryAnnexure = await _context.SalaryAnnexures
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.AnxId == anxId);

            if (salaryAnnexure is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            var earnings = await _context.SalaryAnnexureHeads
                .Include(x => x.SalaryHead)
                .ThenInclude(x => x.ShCategory)
                .Include(x => x.SalaryAnnexureHeadDetails)
                .ThenInclude(x => x.ReferenceSalaryAnnexureHead)
                .ThenInclude(x => x.SalaryHead)
                .Where(x => x.AnxId == anxId && x.SalaryHead.ShCategory.Shc_Type == "EARNING")
                .ToListAsync();

            var deductions = await _context.SalaryAnnexureHeads
                .Include(x => x.SalaryHead)
                .ThenInclude(x => x.ShCategory)
                .Include(x => x.SalaryAnnexureHeadDetails)
                .Where(x => x.AnxId == anxId && x.SalaryHead.ShCategory.Shc_Type == "DEDUCTION")
                .ToListAsync();

            List<ParticularsDTO> earningParticulars = new();

            foreach (var earning in earnings)
            {
                if (earning.ShDataType == "PERCENT")
                {
                    string value = "";

                    foreach(var data in earning.SalaryAnnexureHeadDetails)
                    {
                        if (value != "")
                        {
                            value += " + ";
                        }

                        if (data.IsPercentageOfMonthlySalary)
                        {
                            value += string.Concat(data.Percent, "% of Monthly Salary");
                        } else
                        {
                            value += string.Concat(data.Percent, "% of ", data.ReferenceSalaryAnnexureHead.SalaryHead.Name);
                        }
                    }

                    earningParticulars.Add(new ParticularsDTO
                    {
                        SalaryAnnexureHeadId = earning.SalaryAnnexureHeadId,
                        Particular = earning.SalaryHead.Name,
                        Type = earning.ShDataType,
                        Value = value,
                    });

                    continue;
                }

                earningParticulars.Add(new ParticularsDTO
                {
                    SalaryAnnexureHeadId = earning.SalaryAnnexureHeadId,
                    Particular = earning.SalaryHead.Name,
                    Type = earning.ShDataType,
                });
            }

            List<ParticularsDTO> contributionParticulars = new();
            List<ParticularsDTO> deductionParticulars = new();

            foreach (var deduction in deductions)
            {
                if (deduction.ShDataType == "PERCENT")
                {
                    string value = "";

                    foreach (var data in deduction.SalaryAnnexureHeadDetails)
                    {
                        if (data.IsPercentageOfMonthlySalary)
                        {
                            value += string.Concat(data.Percent, "% of Monthly Salary + ");
                        }
                        else
                        {
                            value += string.Concat(data.Percent, "% of ", data.ReferenceSalaryAnnexureHead.SalaryHead.Name, " + ");
                        }
                    }

                    deductionParticulars.Add(new ParticularsDTO
                    {
                        Particular = deduction.SalaryHead.Name,
                        Type = deduction.ShDataType,
                        Value = value,
                    });
                } else
                {
                    deductionParticulars.Add(new ParticularsDTO
                    {
                        SalaryAnnexureHeadId = deduction.SalaryAnnexureHeadId,
                        Particular = deduction.SalaryHead.Name,
                        Type = deduction.ShDataType,
                    });
                }

                if (deduction.HasOfficeContribution == true)
                {
                    if (deduction.ContributionType == 1)
                    {
                        contributionParticulars.Add(new ParticularsDTO
                        {
                            SalaryAnnexureHeadId = deduction.SalaryAnnexureHeadId,
                            Particular = deduction.SalaryHead.Name,
                            Type = "Office Contribution",
                            Value = deduction.OfficeContribution + "% of Basic Salary",
                        });
                    }
                    else if (deduction.ContributionType == 4)
                    {
                        contributionParticulars.Add(new ParticularsDTO
                        {
                            SalaryAnnexureHeadId = deduction.SalaryAnnexureHeadId,
                            Particular = deduction.SalaryHead.Name,
                            Type = "Office Contribution (Deduction)",
                            Value = deduction.OfficeContribution + "% of Deduction"
                        });
                    }
                }
            }

            return Ok(new
            {
                Data = new
                {
                    AnxId = salaryAnnexure.AnxId,
                    Name = salaryAnnexure.Name,
                    IsDraft = salaryAnnexure.IsDraft,
                    CreatedByUserId = salaryAnnexure.CreatedByUserId,
                    CreatedByUserName = salaryAnnexure.User.UserName,
                    AnnualSalaryEstimate = salaryAnnexure.AnnualSalaryEstimate,
                    CreatedAt = salaryAnnexure.CreatedAt,
                    UpdatedAt = salaryAnnexure.UpdatedAt,
                    Earnings = earningParticulars,
                    OfficeContributions = contributionParticulars,
                    Deductions = deductionParticulars,
                }
            });
        }

        [HttpGet("AnnexureDetail/{salaryAnnexureHeadId}")]
        public async Task<IActionResult> GetAnnexureDetail(int salaryAnnexureHeadId)
        {
            var data = await _context.SalaryAnnexureHeads
                .Include(x => x.SalaryHead)
                .Include(x => x.SalaryAnnexureHeadDetails)
                .ThenInclude(x => x.ReferenceSalaryAnnexureHead)
                .ThenInclude(x => x.SalaryHead)
                .FirstOrDefaultAsync(x => x.SalaryAnnexureHeadId == salaryAnnexureHeadId);

            if (data is null)
            {
                return ErrorHelper.ErrorResult("Id", "salaryAnnexureHeadId is invalid.");
            }

            return Ok(new
            {
                SalaryAnnexureHead = new
                {
                    SalaryAnnexureHeadId = data.SalaryAnnexureHeadId,
                    ShId = data.ShId,
                    ShName = data.SalaryHead.Name,
                    HasOfficeContribution = data.HasOfficeContribution,
                    ContributionType = data.ContributionType,
                    OfficeContribution = data.OfficeContribution,
                    ShDataType = data.ShDataType,
                    SalaryAnnexureDetails = data.SalaryAnnexureHeadDetails.Select(x => new
                    {
                        ReferenceSalaryAnnexureHeadId = x.IsPercentageOfMonthlySalary == true ? 0 : x.ReferenceSalaryAnnexureHeadId,
                        ReferenceSalaryAnnexureHeadName = x.IsPercentageOfMonthlySalary == true ? "Monthly Salary" : x.ReferenceSalaryAnnexureHead.SalaryHead.Name,
                        Percent = x.Percent
                    }),
                    CreatedAt = data.CreatedAt,
                    UpdatedAt = data.UpdatedAt
                }
            });
        }

        [HttpGet("GetFormFields")]
        public async Task<IActionResult> GetFormFields()
        {
            var data = await _context.SalaryHeadCategories
                .Include(x => x.SalaryHeads)
                .Where(x =>
                    x.FlgAssign == true ||
                    x.ShowCategory == true ||
                    x.FlgUse == true
                )
                .Select(
                x => new
                {
                    ShcId = x.ShcId,
                    Name = x.Name,
                    Catgory = x.Category,
                    SNO = x.SNO,
                    SalaryHeads = x.SalaryHeads.Select(sh => new
                    {
                        ShId = sh.ShId,
                        //Name = sh.Name,
                        //ShDataType = sh.ShDataType,
                        //ShCalcType = sh.ShCalcType,
                        //UnitName = sh.UnitName,
                        //PerUnitRate = sh.DefValue
                    })
                })
                .ToListAsync();

            // eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwidW5pcXVlX25hbWUiOiJhZG1pbiIsInJvbGUiOiJzdXBlci1hZG1pbiIsIm5iZiI6MTcyMjMyOTk4NSwiZXhwIjoxNzIyNDE2Mzg0LCJpYXQiOjE3MjIzMjk5ODV9.sgU7Fub1MfeN-4FGz6RBG-Z3bysYaNjXODJqjqIYy41HxRfnyO6SWA1j-b8sFzf4rezSPY8w5PPaIrmJXCu0ug

            return Ok(data);
        }

        [HttpGet("ReferenceSalaryAnnexureHeads/{anxId}")]
        public async Task<IActionResult> GetReferenceSalaryHeads(int anxId)
        {
            var annexure = await _context.SalaryAnnexures.FindAsync(anxId);

            if (annexure is null)
            {
                return ErrorHelper.ErrorResult("Id", "AnxId is invalid.");
            }

            var salaryAnnexureHeads = await _context.SalaryAnnexureHeads
                .Include(x => x.SalaryHead)
                .Where(x => x.AnxId == anxId)
                .Select(x => new ReferenceSalaryAnnexureHeadDTO
                {
                    SalaryAnnexureHeadId = x.SalaryAnnexureHeadId,
                    SalaryAnnexureHeadName = x.SalaryHead.Name
                })
                .ToListAsync();

            salaryAnnexureHeads.Add(new ReferenceSalaryAnnexureHeadDTO
            {
                SalaryAnnexureHeadId = 0,
                SalaryAnnexureHeadName = "Monthly Salary"
            });

            return Ok(new
            {
                Data = salaryAnnexureHeads
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalaryAnnexure(AddInputModel input)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                SalaryAnnexure data = new SalaryAnnexure
                {
                    Name = input.Name,
                    CreatedByUserId = User.GetUserId(),
                    IsDraft = true
                };

                _context.Add(data);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    AnxId = data.AnxId
                });

            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                return ErrorHelper.ErrorResult("Id", "Internal system error. Please contact administrator.");
            }
        }

        [HttpPut("{anxId}")]
        public async Task<IActionResult> UpdateSalaryAnnexure(int anxId, UpdateInputModel input)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var salaryAnnexure = await _context.SalaryAnnexures.FirstOrDefaultAsync(x => x.AnxId == anxId);

            try
            {
                salaryAnnexure.IsDraft = true;

                salaryAnnexure.Name = input.Name;
                salaryAnnexure.IsDraft = true;
                salaryAnnexure.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                return ErrorHelper.ErrorResult("Id", "Internal system error. Please contact administrator.");
            }
        }

        [HttpPost("AddSalaryAnnexureHead/{anxId}")]
        public async Task<IActionResult> AddSalaryHead(int anxId, AddSalaryAnnexureHeadInputModel input)
        {
            var salaryAnnexure = await _context.SalaryAnnexures.FirstOrDefaultAsync(x => x.AnxId == anxId);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                salaryAnnexure.IsDraft = true;

                SalaryAnnexureHead data = new()
                {
                    AnxId = anxId,
                    ShId = input.ShId,
                    ShDataType = input.ShDataType,
                    HasOfficeContribution = input.HasOfficeContribution,
                    ContributionType = input.ContributionType,
                    OfficeContribution = input.OfficeContribution
                };

                _context.Add(data);
                await _context.SaveChangesAsync();

                if (input.ShDataType == "PERCENT")
                {
                    foreach (var salaryAnnexureHeadDetail in input.SalaryAnnexureDetails)
                    {
                        _context.Add(new SalaryAnnexureHeadDetail
                        {
                            SalaryAnnexureHeadId = data.SalaryAnnexureHeadId,
                            IsPercentageOfMonthlySalary = salaryAnnexureHeadDetail.ReferenceSalaryAnnexureHeadId == 0,
                            ReferenceSalaryAnnexureHeadId = salaryAnnexureHeadDetail.ReferenceSalaryAnnexureHeadId == 0 ? null : salaryAnnexureHeadDetail.ReferenceSalaryAnnexureHeadId,
                            Percent = salaryAnnexureHeadDetail.Percent
                        });
                    }
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

        [HttpPut("UpdateSalaryAnnexureHead/{salaryAnnexureHeadId}")]
        public async Task<IActionResult> UpdateSalaryHead(int salaryAnnexureHeadId, UpdateSalaryAnnexureHeadInputModel input)
        {
            var data = await _context.SalaryAnnexureHeads.FirstOrDefaultAsync(X => X.SalaryAnnexureHeadId == salaryAnnexureHeadId);

            var salaryAnnexure = await _context.SalaryAnnexures.FirstOrDefaultAsync(x => x.AnxId == data.AnxId);

            var salaryAnnexureHeads = await _context.SalaryAnnexureHeads
                .Include(x => x.SalaryAnnexureHeadDetails)
                .Where(x => x.AnxId == data.AnxId)
                .ToListAsync();

            if (input.ShDataType == "PERCENT")
            {
                foreach (var sahDetail in input.SalaryAnnexureDetails)
                {
                    if (sahDetail.ReferenceSalaryAnnexureHeadId == 0)
                    {
                        continue;
                    }
                    else
                    {
                        bool isCircular = _dbHelper.CompareAnxCircularDependency(salaryAnnexureHeads, salaryAnnexureHeadId, sahDetail.ReferenceSalaryAnnexureHeadId ?? 0);

                        if (isCircular)
                        {
                            return ErrorHelper.ErrorResult("Id", "Circular dependency in reference.");
                        }
                    }
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                salaryAnnexure.IsDraft = true;
                
                data.ShDataType = input.ShDataType;
                data.HasOfficeContribution = input.HasOfficeContribution;
                data.ContributionType = input.ContributionType;
                data.OfficeContribution = input.OfficeContribution;
                data.UpdatedAt = DateTime.UtcNow;

                var salaryAnnexureHeadDetails = await _context.SalaryAnnexureHeadDetails.Where(x => x.SalaryAnnexureHeadId == data.SalaryAnnexureHeadId).ToListAsync();

                _context.RemoveRange(salaryAnnexureHeadDetails);

                if (input.ShDataType == "PERCENT")
                {
                    foreach(var detail in input.SalaryAnnexureDetails)
                    {
                        _context.Add(new SalaryAnnexureHeadDetail
                        {
                            SalaryAnnexureHeadId = data.SalaryAnnexureHeadId,
                            IsPercentageOfMonthlySalary = detail.ReferenceSalaryAnnexureHeadId == 0,
                            ReferenceSalaryAnnexureHeadId = detail.ReferenceSalaryAnnexureHeadId == 0 ? null : detail.ReferenceSalaryAnnexureHeadId,
                            Percent = detail.Percent
                        });
                    }

                    await _context.SaveChangesAsync();
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

        [HttpPut("Draft/{anxId}")]
        public async Task<IActionResult> Draft(int anxId)
        {
            var salaryAnnexure = await _context.SalaryAnnexures.FirstOrDefaultAsync(x => x.AnxId == anxId);

            if (salaryAnnexure is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            salaryAnnexure.IsDraft = true;
            salaryAnnexure.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            return Ok();
        }

        [HttpPut("Publish/{anxId}")]
        public async Task<IActionResult> Publish(int anxId)
        {
            var salaryAnnexure = await _context.SalaryAnnexures.FirstOrDefaultAsync(x => x.AnxId == anxId);

            if (salaryAnnexure is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            salaryAnnexure.IsDraft = false;
            salaryAnnexure.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{anxId}")]
        public async Task<IActionResult> Delete(int anxId)
        {
            var salaryAnnexure = await _context.SalaryAnnexures.FirstOrDefaultAsync(x => x.AnxId == anxId);

            if (salaryAnnexure is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            var salaryAnnexureHeads = await _context.SalaryAnnexureHeads.Where(x => x.AnxId == anxId).ToListAsync();

            List<int> ids = salaryAnnexureHeads.Select(x => x.SalaryAnnexureHeadId).ToList();

            var salaryAnnexureHeadDetails = await _context.SalaryAnnexureHeadDetails.Where(x => ids.Contains(x.SalaryAnnexureHeadId)).ToListAsync();

            _context.RemoveRange(salaryAnnexureHeadDetails);
            _context.RemoveRange(salaryAnnexureHeads);
            _context.Remove(salaryAnnexure);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("RemoveSalaryAnnexureHead/{salaryAnnexureHeadId}")]
        public async Task<IActionResult> DeleteSalaryAnnexureHead(int salaryAnnexureHeadId)
        {
            var salaryAnnexureHead = await _context.SalaryAnnexureHeads.Where(x => x.SalaryAnnexureHeadId == salaryAnnexureHeadId).FirstOrDefaultAsync();

            if (salaryAnnexureHead == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            var salaryAnnexureHeadDetails = await _context.SalaryAnnexureHeadDetails.Where(x => x.SalaryAnnexureHeadId == salaryAnnexureHeadId).ToListAsync();

            _context.RemoveRange(salaryAnnexureHeadDetails);
            _context.Remove(salaryAnnexureHead);

            await _context.SaveChangesAsync();

            return Ok();
        }

        public class ReferenceSalaryAnnexureHeadDTO
        {
            public int SalaryAnnexureHeadId { get; set; }
            public string SalaryAnnexureHeadName { get; set; }
        }

        public class ParticularsDTO
        {
            public int SalaryAnnexureHeadId { get; set; }
            public string Particular { get; set; }
            public string Type { get; set; }
            public string? Value { get; set; }
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
            //public int? AnnualSalaryEstimate { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;

                Transform(x => x.Name, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.SalaryAnnexures.AsQueryable(), "Name");
            }
        }

        public class UpdateInputModelValidator : AbstractValidator<UpdateInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public UpdateInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["anxId"]?.ToString();

                Transform(x => x.Name, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.SalaryAnnexures.Where(x => x.AnxId != int.Parse(_id)).AsQueryable(), "Name");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.SalaryAnnexures.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "AnxId is invalid."));
                    return false;
                }

                return true;
            }
        }

        public class SalaryAnnexureHeadDetailModel
        {
            public int? ReferenceSalaryAnnexureHeadId { get; set; }
            public decimal? Percent { get; set; }
        }

        public class BaseSalaryAnnexureHeadInputModel
        {
            public bool? HasOfficeContribution { get; set; }
            public int? ContributionType { get; set; }
            public decimal? OfficeContribution { get; set; }
            public string ShDataType { get; set; }
            public List<SalaryAnnexureHeadDetailModel> SalaryAnnexureDetails { get; set; }
            //public decimal? AnnualPercent { get; set; }
        }

        public class AddSalaryAnnexureHeadInputModel : BaseSalaryAnnexureHeadInputModel
        {
            public int ShId { get; set; }
        }

        public class UpdateSalaryAnnexureHeadInputModel : BaseSalaryAnnexureHeadInputModel { }

        public class AddSalaryAnnexureHeadInputModelValidator : AbstractValidator<AddSalaryAnnexureHeadInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public AddSalaryAnnexureHeadInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["anxId"]?.ToString();
                List<int?> contributionTypes = new List<int?> { 1, 4 };

                RuleFor(x => x.ShId)
                    .IdMustExist(_context.SalaryHeads.AsQueryable(), "ShId")
                    .MustBeUnique(_context.SalaryAnnexureHeads.Where(x => x.AnxId == int.Parse(_id)).AsQueryable(), "ShId");

                RuleFor(x => x.ShDataType)
                    .NotEmpty()
                    .MustBeIn(ShDataTypes);

                When(x => x.ShDataType == "PERCENT", () => {
                    RuleFor(x => x.SalaryAnnexureDetails)
                        .NotEmpty();

                    RuleForEach(x => x.SalaryAnnexureDetails)
                       .ChildRules((salaryAnnexureDetail) =>
                       {
                           salaryAnnexureDetail.RuleFor(x => x.ReferenceSalaryAnnexureHeadId)
                                .NotNull()
                                .WithMessage("Please specify the reference to the percentage.");

                           salaryAnnexureDetail.RuleFor(x => x.ReferenceSalaryAnnexureHeadId)
                               .IdMustExist(_context.SalaryAnnexureHeads.AsQueryable(), "SalaryAnnexureHeadId")
                               .Unless(x => x.ReferenceSalaryAnnexureHeadId == 0);

                           salaryAnnexureDetail.RuleFor(x => x.Percent)
                               .NotEmpty()
                               .GreaterThan(0)
                               .LessThanOrEqualTo(100);
                       });
                });

                //RuleFor(x => x.AnnualPercent)
                //    .Empty()
                //    .When(x => !CompareSalaryHeadType(x.ShId, "AMOUNT"))
                //    .When(x => !CompareSalaryHeadCategory(x.ShId, "ALLOWANCE"))
                //    .WithMessage("Annual percent can be set only when type is Amount and is an Allowance.");

                //RuleFor(x => x.AnnualPercent)
                //    .GreaterThanOrEqualTo(0)
                //    .Must((x, y) => !IsAnnualPercentExceeded(_context, int.Parse(_id), x.AnnualPercent))
                //    .WithMessage("The total annual percent for this annexure exceeds 100 percent.")
                //    .Unless(x => x.AnnualPercent is null);


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

            protected override bool PreValidate(ValidationContext<AddSalaryAnnexureHeadInputModel> context, ValidationResult result)
            {
                if (_context.SalaryAnnexures.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "AnxId is invalid."));
                    return false;
                }

                return true;
            }
        }

        public class UpdateSalaryAnnexureHeadInputModelValidator : AbstractValidator<UpdateSalaryAnnexureHeadInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public UpdateSalaryAnnexureHeadInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["salaryAnnexureHeadId"]?.ToString();
                List<int?> contributionTypes = new List<int?> { 1, 4 };

                RuleFor(x => x.ShDataType)
                    .NotEmpty()
                    .MustBeIn(ShDataTypes);

                When(x => x.ShDataType == "PERCENT", () => {
                    RuleFor(x => x.SalaryAnnexureDetails)
                        .NotEmpty();

                    RuleForEach(x => x.SalaryAnnexureDetails)
                       .ChildRules((salaryAnnexureDetail) =>
                       {
                           salaryAnnexureDetail.RuleFor(x => x.ReferenceSalaryAnnexureHeadId)
                                .NotNull()
                                .WithMessage("Please specify the reference to the percentage.");

                           salaryAnnexureDetail.RuleFor(x => x.ReferenceSalaryAnnexureHeadId)
                               .IdMustExist(_context.SalaryAnnexureHeads.AsQueryable(), "SalaryAnnexureHeadId")
                               .Unless(x => x.ReferenceSalaryAnnexureHeadId == 0);

                           salaryAnnexureDetail.RuleFor(x => x.Percent)
                               .NotEmpty()
                               .GreaterThan(0)
                               .LessThanOrEqualTo(100);
                       });
                });

                //RuleFor(x => x.AnnualPercent)
                //    .Empty()
                //    .When(x => !CompareSalaryHeadType(x.ShId, "AMOUNT"))
                //    .When(x => !CompareSalaryHeadCategory(x.ShId, "ALLOWANCE"))
                //    .WithMessage("Annual percent can be set only when type is Amount and is an Allowance.");

                //RuleFor(x => x.AnnualPercent)
                //    .GreaterThanOrEqualTo(0)
                //    .Must((x, y) => !IsAnnualPercentExceeded(_context, int.Parse(_id), x.AnnualPercent))
                //    .WithMessage("The total annual percent for this annexure exceeds 100 percent.")
                //    .Unless(x => x.AnnualPercent is null);


                When(x => !CompareSalaryHeadCategory("DEDUCTION"), () =>
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

                When(x => CompareSalaryHeadCategory("DEDUCTION"), () =>
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

            private bool IsAnnualPercentExceededUpdate(decimal? percent)
            {
                int anxId = _context.SalaryAnnexureHeads.Where(x => x.SalaryAnnexureHeadId == int.Parse(_id)).Select(x => x.AnxId).FirstOrDefault();

                var annualPercent = _context.SalaryAnnexureHeads.Where(x => x.AnxId == anxId && x.SalaryAnnexureHeadId != int.Parse(_id)).Sum(x => x.AnnualPercent);


                if (percent + annualPercent > 100)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private bool CompareSalaryHeadCategory(string category)
            {
                // Assuming you have a method to get the type of SalaryHead by its ShId
                var salaryAnnexureHead = _context.SalaryAnnexureHeads
                    .Include(x => x.SalaryHead)
                    .ThenInclude(x => x.ShCategory)
                    .FirstOrDefault(sh => sh.SalaryAnnexureHeadId == int.Parse(_id));

                return salaryAnnexureHead?.SalaryHead.ShCategory.Category == category; // Assuming Type is a string indicating "Amount" or "Percent"
            }

            protected override bool PreValidate(ValidationContext<UpdateSalaryAnnexureHeadInputModel> context, ValidationResult result)
            {
                if (_context.SalaryAnnexureHeads.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "SalaryAnnexureHeadId is invalid."));
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
