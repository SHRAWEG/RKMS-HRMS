using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class StatesController : Controller
    {
        private readonly DataContext _context;

        public StatesController(DataContext context)
        {
            _context = context;
        }

        // GET: States
        [CustomAuthorize("list-state")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name, string code)
        {
            var query = _context.States.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrEmpty(code))
            {
                query = query.Where(b => b.Code!.ToLower().Contains(code.ToLower()));
            }

            Expression<Func<State, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "Code" => x => x.Code,
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

            var data = await PagedList<State>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: States/all
        [CustomAuthorize("search-state")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.States.ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code
                })
            });
        }

        // GET: States/5
        [CustomAuthorize("view-state")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.States
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                State = data
            });
        }

        // Post: States/Create
        [CustomAuthorize("write-state")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            State data= new()
            {
                Name = input.Name,
                Code = input.Code
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: States/5
        [CustomAuthorize("update-state")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.States.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.Code = input.Code;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: States/5
        [CustomAuthorize("delete-state")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.States.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.Cities.AnyAsync(x => x.StateId == id) || await _context.EmpDetails.AnyAsync(x => x.BirthStateId == id))
            {
                return ErrorHelper.ErrorResult("Id", "State is already in use.");
            }

            _context.States.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
            public string Code { get; set; }
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
                    .MustBeUnique(_context.States.AsQueryable(), "Name");

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.States.AsQueryable(), "Code");
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
                    .MustBeUnique(_context.States.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name");

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.States.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Code");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.States.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
