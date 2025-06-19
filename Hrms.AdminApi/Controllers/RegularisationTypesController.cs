using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class RegularisationTypesController : Controller
    {
        private readonly DataContext _context;

        public RegularisationTypesController(DataContext context)
        {
            _context = context;
        }

        [CustomAuthorize("list-regularization-type")]
        // GET: RegularisationTypes
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string displayName)
        {
            var query = _context.RegularisationTypes.AsQueryable();

            if (!string.IsNullOrEmpty(displayName))
            {
                query = query.Where(b => b.DisplayName.Contains(displayName));
            }

            Expression<Func<RegularisationType, object>> field = sortColumn switch
            {
                "DisplayName" => x => x.DisplayName,
                "Name" => x => x.Name,
                _ => x => x.Id
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.DisplayName);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<RegularisationType>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        [CustomAuthorize("search-regularization-type")]
        [HttpGet("All")]
        public async Task<IActionResult> Get()
        {
            var data = await _context.RegularisationTypes.ToListAsync();

            return Ok(new
            {
                Data = data,
            });
        }

        [CustomAuthorize("update-regularization-type")]
        // PUT: RegularisationTypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.RegularisationTypes.FirstOrDefaultAsync(c => c.Id == id);

            data.DisplayName = input.DisplayName;

            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string DisplayName { get; set; }
        }

        public class UpdateInputModel : BaseInputModel { }

        public class UpdateInputModelValidator : AbstractValidator<UpdateInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public UpdateInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

                Transform(x => x.DisplayName, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.RegularisationTypes.Where(x => x.DisplayName != _id).AsQueryable(), "DisplayName");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.RegularisationTypes.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
