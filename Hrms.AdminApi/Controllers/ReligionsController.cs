using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class ReligionsController : Controller
    {
        private readonly DataContext _context;

        public ReligionsController(DataContext context)
        {
            _context = context;
        }

        // GET: Religions
        [CustomAuthorize("list-religion")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name)
        {
            var query = _context.Religions.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            Expression<Func<Religion, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                _ => x => x.Id
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

            var data = await PagedList<Religion>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GETALL: Religions
        [CustomAuthorize("search-religion")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Religions.ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        // GET: Religions/5
        [CustomAuthorize("view-religion")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Religions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                Religion = data
            });
        }

        // Post: Religions/Create
        [CustomAuthorize("write-religion")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            Religion data= new()
            {
                Name = input.Name,
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: Religions/5
        [CustomAuthorize("update-religion")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Religions.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Religions/5
        [CustomAuthorize("delete-religion")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Religions.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.EmpDetails.AnyAsync(x => x.ReligionId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Religion is already in use.");
            }

            _context.Religions.Remove(data);
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
                    .MustBeUnique(_context.Religions.AsQueryable(), "Name");
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
                    .MustBeUnique(_context.Religions.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Religions.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
