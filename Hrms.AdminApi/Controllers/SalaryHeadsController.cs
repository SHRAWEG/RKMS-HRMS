using Hrms.Common.Models;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "super-admin, admin")]
    public class SalaryHeadsController : Controller
    {
        private readonly DataContext _context;

        public SalaryHeadsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string shName, int? shcId)
        {
            var query = _context.SalaryHeads
                .Include(x => x.ShCategory)
                .AsQueryable();

            if (!string.IsNullOrEmpty(shName))
            {
                query = query.Where(x => x.Name!.ToLower().Contains(shName.ToLower()));
            }

            if (shcId != null)
            {
                query = query.Where(x => x.ShcId == shcId);
            }

            Expression<Func<SalaryHead, object>> field = sortColumn switch
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

            var data = await PagedList<SalaryHead>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.ShId,
                    x.Name,
                    x.ShcId,
                    ShCategoryName = x.ShCategory.Name,
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.SalaryHeads
                .Include(x => x.ShCategory)
                .ToListAsync();

            return Ok(new
            {
                SalaryHeads = data.Select(x => new
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
            var data = await _context.SalaryHeads
                .Include(x => x.ShCategory)
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
                    data.Name,
                    data.ShcId,
                    ShCategory = data.ShCategory.Category,
                    ShCategoryName = data.ShCategory.Name,
                    data.CreatedAt,
                    data.UpdatedAt,
                }
            });
        }

        // Post: SalaryHeads/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var shCategory = await _context.SalaryHeadCategories.Where(x => x.ShcId == input.ShcId).FirstOrDefaultAsync();

            SalaryHead data = new()
            {
                ShcId = input.ShcId,
                Name = input.Name,
                CreatedByUserId = User.GetUserId()
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: SalaryHeads/:id
        [HttpPut("{shId}")]
        public async Task<IActionResult> Edit(int shId, UpdateInputModel input)
        {
            var shCategory = await _context.SalaryHeadCategories.Where(x => x.ShcId == input.ShcId).FirstOrDefaultAsync();
            var data = await _context.SalaryHeads.FirstOrDefaultAsync(c => c.ShId == shId);

            if (data.ShcId != input.ShcId 
                && (await _context.EmpSalaryHeads.AnyAsync(x => x.ShId == data.ShId) 
                    || await _context.SalaryAnnexureHeads.AnyAsync(x => x.ShId == data.ShId))) 
            {
                return ErrorHelper.ErrorResult("ShcId", "Salary head is already in use. Cannot change category.");
            }

            data.ShcId = input.ShcId;
            data.Name = input.Name;

            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE: SalaryHeads/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.SalaryHeads.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.SalaryAnnexureHeads.AnyAsync(x => x.ShId == id) || await _context.EmpSalaryHeads.AnyAsync(x => x.ShId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Salary head is already in use.");
            }

            _context.SalaryHeads.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
            public int ShcId { get; set; }
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
                    .MustBeUnique(_context.SalaryHeads.AsQueryable(), "Name");
            }

            protected override bool PreValidate(ValidationContext<AddInputModel> context, ValidationResult result)
            {
                AddInputModel input = context.InstanceToValidate;

                if (input.ShcId > 0)
                {
                    _salaryHeadCategory = _context.SalaryHeadCategories.Where(x => x.ShcId == input.ShcId).FirstOrDefault();
                }

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
                        .MustBeUnique(_context.SalaryHeads.Where(x => x.ShId != int.Parse(_id)).AsQueryable(), "Name");
            }


            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                UpdateInputModel input = context.InstanceToValidate;

                if (_context.SalaryHeads.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                if (input.ShcId > 0)
                {
                    _salaryHeadCategory = _context.SalaryHeadCategories.Where(x => x.ShcId == input.ShcId).FirstOrDefault();

                    Console.WriteLine(_salaryHeadCategory.Category);
                }

                return true;
            }
        }
    }
}
