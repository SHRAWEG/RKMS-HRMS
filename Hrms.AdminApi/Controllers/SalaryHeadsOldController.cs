using Hrms.Common.Models;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "super-admin, admin")]
    public class SalaryHeadsOldController : Controller
    {
        private readonly DataContext _context;

        public SalaryHeadsOldController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string shName, int? shcId)
        {
            var query = _context.SalaryHeadOlds
                .Include(x => x.ShCategory)
                .AsQueryable()
                .Where(x => x.IsLocked == false);

            if (!string.IsNullOrEmpty(shName))
            {
                query = query.Where(x => x.Name!.ToLower().Contains(shName.ToLower()));
            }

            if (shcId != null)
            {
                query = query.Where(x => x.ShcId == shcId);
            }

            Expression<Func<SalaryHeadOld, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                _ => x => x.ShId,
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.ShId);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<SalaryHeadOld>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.ShId,
                    x.Name,
                    x.ShcId,
                    ShCategoryName = x.ShCategory.Name,
                    ShCalcType = x.ShCalcType,
                    ShDataType = x.ShDataType,
                    x.TrnCode,
                    x.RefId,
                    x.DrCr,
                    x.ShCalcMode,
                    x.ShCalcCategory,
                    x.IsTaxable,
                    x.IsActive,
                    x.MinHours,
                    x.MaxNos,
                    x.DefValue,
                    x.UnitName,
                    x.OfficeContribution,
                    x.ContributionType,
                    x.DedTaxFreeAmount,
                    x.DedTaxFreeLimitCheck,
                    x.IsLocked
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.SalaryHeadOlds
                .Include(x => x.ShCategory)
                .Where(x => !x.IsLocked)
                .ToListAsync();

            return Ok(new
            {
                SalaryHeadOlds = data.Select(x => new
                {
                    ShId = x.ShId,
                    ShCategory = x.ShCategory.Category,
                    Name = x.Name
                })
            });
        }

        [HttpGet("{shId}")]
        public async Task<IActionResult> Get(int shId)
        {
            var data = await _context.SalaryHeadOlds
                .Include(x => x.ShCategory)
                .Where(x => x.IsLocked == false)
                .FirstOrDefaultAsync(x => x.ShId == shId);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                SalaryHead = new
                {
                    data.ShId,
                    data.TrnCode,
                    data.Name,
                    data.ShcId,
                    ShCategory = data.ShCategory.Category,
                    ShCategoryName = data.ShCategory.Name,
                    data.RefId,
                    data.DrCr,
                    data.ShDataType,
                    data.ShCalcType,
                    data.ShCalcMode,
                    data.ShCalcCategory,
                    data.IsTaxable,
                    data.IsActive,
                    data.MinHours,
                    data.MaxNos,
                    data.DefValue,
                    data.UnitName,
                    data.OfficeContribution,
                    data.ContributionType,
                    data.DedTaxFreeLimitCheck,
                    data.DedTaxFreeAmount,
                    data.EstimatePostMonths,
                    data.IsLocked,
                    data.CreatedAt,
                    data.UpdatedAt,
                    data.CreatedBy
                }
            });
        }

        // Post: SalaryHeadOlds/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var shCategory = await _context.SalaryHeadCategories.Where(x => x.ShcId == input.ShcId).FirstOrDefaultAsync();

            switch (shCategory.Category)
            {
                case "ALLOWANCE":
                    SalaryHeadOld data = new()
                    {
                        ShcId = input.ShcId,
                        Name = input.Name,
                        ShDataType = input.ShDataType,
                        ShCalcType = input.ShCalcType,
                        ShCalcMode = input.ShCalcMode, // Old - Auto/ Manual || New - fixed/ variable
                        ShCalcCategory = input.ShCalcCategory,
                        IsTaxable = input.IsTaxable,
                        DefValue = input.DefValue,
                        UnitName = input.UnitName,
                        MinHours = input.MinHours,
                        MaxNos = input.MaxNos,
                        //OfficeContribution = input.OfficeContribution,
                        //ContributionType = input.ContributionType,
                        IsActive = true,
                        IsLocked = false,
                    };
                    _context.Add(data);
                    await _context.SaveChangesAsync();
                    return Ok();
                    break;

                case "DEDUCTION":
                    SalaryHeadOld deductionData = new()
                    {
                        ShcId = input.ShcId,
                        Name = input.Name,
                        ShDataType = input.ShDataType,
                        ShCalcType = input.ShCalcType,
                        ShCalcMode = input.ShCalcMode,
                        ShCalcCategory = input.ShCalcCategory,
                        IsActive = true,
                        MaxNos = input.MaxNos,
                        IsTaxable = input.IsTaxable,
                        DefValue = input.DefValue,
                        UnitName = input.UnitName,
                        OfficeContribution = input.OfficeContribution,
                        ContributionType = input.ContributionType ?? 0,
                        //DedTaxFreeLimitCheck = input.DedTaxFreeLimitCheck,
                        //DedTaxFreeAmount = input.DedTaxFreeAmount,
                        IsLocked = false,
                    };
                    _context.Add(deductionData);
                    await _context.SaveChangesAsync();
                    return Ok();
                    break;

                case "TAXABLEPAYMENT":
                    SalaryHeadOld taxablePaymentData = new()
                    {
                        ShcId = input.ShcId,
                        Name = input.Name,
                        ShDataType = input.ShDataType,
                        ShCalcType = input.ShCalcType,
                        DefValue = input.DefValue,
                        ShCalcCategory = input.ShCalcCategory,
                        IsActive = true,
                        IsLocked = false,
                    };
                    _context.Add(taxablePaymentData);
                    await _context.SaveChangesAsync();
                    return Ok();
                    break;

                case "TAXABLEREDUCTION":
                    SalaryHeadOld taxableReductionData = new()
                    {
                        ShcId = input.ShcId,
                        Name = input.Name,
                        ShDataType = input.ShDataType,
                        ShCalcType = input.ShCalcType,
                        ShCalcMode = input.ShCalcMode,
                        DefValue = input.DefValue,
                        ShCalcCategory = input.ShCalcCategory,
                        IsActive = true,
                        IsLocked = false,
                    };
                    _context.Add(taxableReductionData);
                    await _context.SaveChangesAsync();
                    return Ok();
                    break;
            }

            return ErrorHelper.ErrorResult("ShCategoryId", "Id is Invalid.");
        }

        // PUT: SalaryHeadOlds/:id
        [HttpPut("{shId}")]
        public async Task<IActionResult> Edit(int shId, UpdateInputModel input)
        {
            var shCategory = await _context.SalaryHeadCategories.Where(x => x.ShcId == input.ShcId).FirstOrDefaultAsync();
            var data = await _context.SalaryHeadOlds.FirstOrDefaultAsync(c => c.ShId == shId);

            switch (shCategory.Category)
            {
                case "ALLOWANCE":
                    data.ShcId = input.ShcId;
                    data.Name = input.Name;
                    data.ShDataType = input.ShDataType;
                    data.ShCalcType = input.ShCalcType;
                    data.ShCalcMode = input.ShCalcMode; // Old - Auto/ Manual || New - fixed/ variable
                    data.ShCalcCategory = input.ShCalcCategory;
                    data.IsTaxable = input.IsTaxable;
                    data.DefValue = input.DefValue;
                    data.UnitName = input.UnitName;
                    data.MinHours = input.MinHours;
                    data.MaxNos = input.MaxNos;
                    data.OfficeContribution = input.OfficeContribution;
                    data.ContributionType = input.ContributionType ?? 0 ;
                    data.IsActive = true;
                    data.IsLocked = false;

                    await _context.SaveChangesAsync();
                    return Ok();
                    break;

                case "DEDUCTION":
                    data.ShcId = input.ShcId;
                    data.Name = input.Name;
                    data.ShDataType = input.ShDataType;
                    data.ShCalcType = input.ShCalcType;
                    data.ShCalcMode = input.ShCalcMode;
                    data.IsTaxable = input.IsTaxable;
                    data.DefValue = input.DefValue;
                    data.UnitName = input.UnitName;
                    data.ShCalcCategory = input.ShCalcCategory;
                    data.OfficeContribution = input.OfficeContribution;
                    data.ContributionType = input.ContributionType ?? 0;
                    data.DedTaxFreeLimitCheck = input.DedTaxFreeLimitCheck;
                    //data.DedTaxFreeAmount = input.DedTaxFreeAmount;
                    data.IsActive = true;
                    data.IsLocked = false;

                    await _context.SaveChangesAsync();
                    return Ok();
                    break;

                case "TAXABLEPAYMENT":
                    data.ShcId = input.ShcId;
                    data.Name = input.Name;
                    data.ShDataType = input.ShDataType;
                    data.ShCalcType = input.ShCalcType;
                    data.DefValue = input.DefValue;
                    data.ShCalcCategory = input.ShCalcCategory;
                    data.IsActive = true;
                    data.IsLocked = false;

                    await _context.SaveChangesAsync();
                    return Ok();
                    break;

                case "TAXABLEREDUCTION":
                    data.ShcId = input.ShcId;
                    data.Name = input.Name;
                    data.ShDataType = input.ShDataType;
                    data.ShCalcType = input.ShCalcType;
                    data.ShCalcMode = input.ShCalcMode;
                    data.DefValue = input.DefValue;
                    data.ShCalcCategory = input.ShCalcCategory;
                    data.IsActive = true;
                    data.IsLocked = false;

                    await _context.SaveChangesAsync();
                    return Ok();
                    break;
            }

            return ErrorHelper.ErrorResult("ShCategoryId", "Id is Invalid.");
        }

        // DELETE: SalaryHeadOlds/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.SalaryHeadOlds.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            _context.SalaryHeadOlds.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
            public int ShcId { get; set; }
            public string ShDataType { get; set; }
            public string ShCalcType { get; set; }
            public string ShCalcMode { get; set; }
            public int ShCalcCategory { get; set; }
            public bool IsTaxable { get; set; }
            public bool IsActive { get; set; }
            public decimal MinHours { get; set; }
            public int MaxNos { get; set; }
            public decimal DefValue { get; set; }
            public string UnitName { get; set; } = "";
            public decimal OfficeContribution { get; set; }
            public int? ContributionType { get; set; }
            public bool DedTaxFreeLimitCheck { get; set; }
            public decimal DedTaxFreeAmount { get; set; }
            public int EstimatePostMonths { get; set; }
        }
        public class AddInputModel : BaseInputModel { };
        public class UpdateInputModel : BaseInputModel { };

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;
            private SalaryHeadCategory? _salaryHeadCategory;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                // COMMON FIELDS

                RuleFor(x => x.ShcId)
                    .NotEmpty()
                    .IdMustExist(_context.SalaryHeadCategories.AsQueryable(), "ShcId");

                RuleFor(x => x.Name)
                    .NotEmpty()
                    .MustBeUnique(_context.SalaryHeadOlds.AsQueryable(), "Name");

                RuleFor(x => x.ShDataType)
                    .NotEmpty();

                RuleFor(x => x.ShCalcType)
                    .NotEmpty();

                // ALLOWANCE || DEDUCTION || TAXABLEREDUCTION
                //RuleFor(x => x.ShCalcCategory)
                //       .NotEmpty()
                //       .When(x =>
                //       _salaryHeadCategory?.Category == "ALLOWANCE" ||
                //       _salaryHeadCategory?.Category == "DEDUCTION" ||
                //       _salaryHeadCategory?.Category == "TAXABLEREDUCTION"
                //       );

                RuleFor(x => x.ShCalcMode)
                       .NotEmpty()
                       .When(x =>
                       _salaryHeadCategory?.Category == "ALLOWANCE" ||
                       _salaryHeadCategory?.Category == "DEDUCTION" ||
                       _salaryHeadCategory?.Category == "TAXABLEREDUCTION"
                       );

                RuleFor(x => x.DefValue)
                    .NotEmpty();
                       //.When(x =>
                       //(_salaryHeadCategory?.Category == "ALLOWANCE" ||
                       //_salaryHeadCategory?.Category == "DEDUCTION" ||
                       //_salaryHeadCategory?.Category == "TAXABLEREDUCTION") && (x.ShDataType == "PERUNIT")
                       //);

                RuleFor(x => x.UnitName)
                    .NotEmpty()
                       .When(x =>
                       (_salaryHeadCategory?.Category == "ALLOWANCE" ||
                       _salaryHeadCategory?.Category == "DEDUCTION" ||
                       _salaryHeadCategory?.Category == "TAXABLEREDUCTION") &&
                       (x.ShDataType == "PERUNIT") &&
                       (x.DefValue > 0)
                       );

                // ALLOWANCE || DEDUCTION
                RuleFor(x => x.IsTaxable)
                    .Must(x => x == false || x == true)
                    .When(x =>
                    _salaryHeadCategory?.Category == "ALLOWANCE" ||
                    _salaryHeadCategory?.Category == "DEDUCTION");

                RuleFor(x => x.MaxNos)
                    .NotEmpty()
                    .When(x =>
                    _salaryHeadCategory?.Category == "ALLOWANCE" ||
                    _salaryHeadCategory?.Category == "DEDUCTION");

                // ALLOWANCE

                RuleFor(x => x.MinHours)
                    .NotEmpty()
                    .When(x => _salaryHeadCategory?.Category == "ALLOWANCE" && x.ShCalcType == "DAILY" && x.ShCalcCategory == 2);

                // DEDUCTUON
                RuleFor(x => x.OfficeContribution)
                    .NotEmpty()
                    .When(x => _salaryHeadCategory?.Category == "DEDUCTION");

                RuleFor(x => x.ContributionType)
                    .NotEmpty()
                    .When(x => _salaryHeadCategory?.Category == "DEDUCTION");

                //RuleFor(x => x.DedTaxFreeLimitCheck)
                //    .NotEmpty()
                //    .When(x => _salaryHeadCategory?.Category == "DEDUCTION");

            }

            protected override bool PreValidate(ValidationContext<AddInputModel> context, ValidationResult result)
            {
                AddInputModel input = context.InstanceToValidate;

                if (input.ShcId > 0)
                {
                    _salaryHeadCategory = _context.SalaryHeadCategories.Where(x => x.ShcId == input.ShcId).FirstOrDefault();
                }

                //if (input.ShDataTypeId > 0)
                //{
                //    _salaryHeadDataType = _context.SalaryHeadDataTypes.Where(x => x.Id == input.ShDataTypeId).FirstOrDefault();
                //}

                return true;
            }
        }

        public class UpdateInputModelValidator : AbstractValidator<UpdateInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;
            private SalaryHeadCategory? _salaryHeadCategory;
            private string? _salaryHeadDataType;

            public UpdateInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["shId"]?.ToString();

                RuleFor(x => x.ShcId)
                        .NotEmpty()
                        .IdMustExist(_context.SalaryHeadCategories.AsQueryable(),"ShcId");

                RuleFor(x => x.Name)
                        .NotEmpty()
                        .MustBeUnique(_context.SalaryHeadOlds.Where(x => x.ShId != int.Parse(_id)).AsQueryable(), "Name");

                RuleFor(x => x.ShDataType)
                    .NotEmpty();

                RuleFor(x => x.ShCalcType)
                    .NotEmpty();

                // ALLOWANCE || DEDUCTION || TAXABLEREDUCTION
                //RuleFor(x => x.ShCalcCategory)
                //       .NotEmpty()
                //       .When(x =>
                //       _salaryHeadCategory?.Category == "ALLOWANCE" ||
                //       _salaryHeadCategory?.Category == "DEDUCTION" ||
                //       _salaryHeadCategory?.Category == "TAXABLEREDUCTION"
                //       );

                RuleFor(x => x.ShCalcMode)
                       .NotEmpty()
                       .When(x =>
                       _salaryHeadCategory?.Category == "ALLOWANCE" ||
                       _salaryHeadCategory?.Category == "DEDUCTION" ||
                       _salaryHeadCategory?.Category == "TAXABLEREDUCTION"
                       );

                RuleFor(x => x.DefValue)
                    .NotEmpty();
                       //.When(x =>
                       //(_salaryHeadCategory?.Category == "ALLOWANCE" ||
                       //_salaryHeadCategory?.Category == "DEDUCTION" ||
                       //_salaryHeadCategory?.Category == "TAXABLEREDUCTION") && (x.ShDataType == "PERUNIT")
                       //);

                RuleFor(x => x.UnitName)
                    .NotEmpty()
                       .When(x =>
                       (_salaryHeadCategory?.Category == "ALLOWANCE" ||
                       _salaryHeadCategory?.Category == "DEDUCTION" ||
                       _salaryHeadCategory?.Category == "TAXABLEREDUCTION") &&
                       (x.ShDataType == "PERUNIT") &&
                       (x.DefValue > 0)
                       );

                // ALLOWANCE || DEDUCTION
                RuleFor(x => x.IsTaxable)
                    .Must(x => x == false || x == true)
                    .When(x =>
                    _salaryHeadCategory?.Category == "ALLOWANCE" ||
                    _salaryHeadCategory?.Category == "DEDUCTION");

                RuleFor(x => x.MaxNos)
                    .NotEmpty()
                    .When(x =>
                    _salaryHeadCategory?.Category == "ALLOWANCE" ||
                    _salaryHeadCategory?.Category == "DEDUCTION");

                // ALLOWANCE

                RuleFor(x => x.MinHours)
                    .NotEmpty()
                    .When(x => _salaryHeadCategory?.Category == "ALLOWANCE" && x.ShCalcType == "DAILY" && x.ShCalcCategory == 2);

                // DEDUCTUON
                RuleFor(x => x.OfficeContribution)
                    .NotEmpty()
                    .When(x => _salaryHeadCategory?.Category == "DEDUCTION");

                RuleFor(x => x.ContributionType)
                    .NotEmpty()
                    .When(x => _salaryHeadCategory?.Category == "DEDUCTION");

                //RuleFor(x => x.DedTaxFreeAmount)
                //    .NotEmpty()
                //    .When(x => _salaryHeadCategory?.Category == "DEDUCTION");

                //RuleFor(x => x.DedTaxFreeLimitCheck)
                //    .NotEmpty()
                //    .When(x => _salaryHeadCategory?.Category == "DEDUCTION");
            }


            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                UpdateInputModel input = context.InstanceToValidate;

                if (_context.SalaryHeadOlds.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                if (input.ShcId > 0)
                {
                    _salaryHeadCategory = _context.SalaryHeadCategories.Where(x => x.ShcId == input.ShcId).FirstOrDefault();

                    Console.WriteLine(_salaryHeadCategory.Category);
                }

                if (!string.IsNullOrEmpty(input.ShDataType))
                {
                    _salaryHeadDataType = input.ShDataType;
                }

                return true;
            }
        }
    }
}
