using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class ModesController : Controller
    {
        private readonly DataContext _context;

        public ModesController(DataContext context)
        {
            _context = context;
        }

        // GET: Modes
        [CustomAuthorize("list-emp-category")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, 
            string name, string abbreviation)
        {
            var query = _context.Modes.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(abbreviation))
            {
                query = query.Where(x => x.Abbreviation.Contains(abbreviation));
            }

            Expression<Func<Mode, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "Abbreviation" => x => x.Abbreviation,
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

            var data = await PagedList<Mode>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        //Get All
        [CustomAuthorize("search-emp-category")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Modes.ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        // GET: Modes/5
        [CustomAuthorize("view-emp-category")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(short id)
        {
            var data = await _context.Modes
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                Mode = data
            });
        }

        // Post: Modes/Create
        [CustomAuthorize("write-emp-category")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            Mode data = new()
            {
                Name = input.Name,
                Abbreviation = input.Abbreviation,
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: Modes/5
        [CustomAuthorize("update-emp-category")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(short id, UpdateInputModel input)
        {
            var data = await _context.Modes.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.Abbreviation = input.Abbreviation;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Modes/5
        [CustomAuthorize("delete-emp-category")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(short id)
        {
            var data = await _context.Modes.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.EmpTransactions.AnyAsync(x => x.ModeId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Employee Category is already in use.");
            }

            _context.Modes.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
            public string Abbreviation { get; set; }
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
                    .MustBeUnique(_context.Modes.AsQueryable(), "Name");

                Transform(x => x.Abbreviation, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.Modes.AsQueryable(), "Abbreviation");
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
                    .MustBeUnique(_context.Modes.Where(x => x.Id != short.Parse(_id)).AsQueryable(), "Name");

                Transform(x => x.Abbreviation, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.Modes.Where(x => x.Id != short.Parse(_id)).AsQueryable(), "Abbreviation");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Modes.Find(short.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
