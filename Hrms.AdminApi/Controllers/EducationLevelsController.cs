using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class EducationLevelsController : Controller
    {
        private readonly DataContext _context;

        public EducationLevelsController(DataContext context)
        {
            _context = context;
        }

        // GET: EducationLevels
        [CustomAuthorize("list-education-level")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name, string code)
        {
            var query = _context.EducationLevels.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            Expression<Func<EducationLevel, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                _ => x => x.Id,
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Id);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<EducationLevel>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: EducationLevels/all
        [CustomAuthorize("search-education-level")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.EducationLevels.ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name
                })
            });
        }

        // GET: EducationLevels/5
        [CustomAuthorize("view-education-level")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(short id)
        {
            var data = await _context.EducationLevels
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                EducationLevel = data
            });
        }

        // Post: EducationLevels/Create
        [CustomAuthorize("write-education-level")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            EducationLevel data= new()
            {
                Name = input.Name
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: EducationLevels/5
        [CustomAuthorize("update-education-level")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(short id, UpdateInputModel input)
        {
            var data = await _context.EducationLevels.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: EducationLevels/5
        [CustomAuthorize("delete-education-level")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(short id)
        {
            var data = await _context.EducationLevels.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.Educations.AnyAsync(x => x.EducationLevelId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Education Level is already in use.");
            }

            _context.EducationLevels.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                Transform(x => x.Name, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.EducationLevels.AsQueryable(), "Name");
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

                Transform(x => x.Name, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.EducationLevels.Where(x => x.Id != short.Parse(_id)).AsQueryable(), "Name");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.EducationLevels.Find(short.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
