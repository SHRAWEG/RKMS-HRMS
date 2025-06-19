using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class BranchesController : Controller
    {
        private readonly DataContext _context;

        public BranchesController(DataContext context)
        {
            _context = context;
        }

        // GET: Branches
        [CustomAuthorize("list-location")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, 
            string name, int? stateId, int? cityId)
        {
            var query = _context.Branches
                .Include(x => x.State)
                .Include(x => x.City)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name!.ToLower().Contains(name.ToLower()));
            }

            if (stateId != null)
            {
                query = query.Where(x => x.StateId == stateId);
            }

            if (cityId != null)
            {
                query = query.Where(x => x.CityId == cityId);
            }

            Expression<Func<Branch, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "StateName" => x => x.State.Name,
                "CityName" => x => x.City.Name,
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

            var data = await PagedList<Branch>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.StateId,
                    StateName = x.State.Name,
                    x.CityId,
                    CityName = x.City.Name
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // GETALL: Branches
        [CustomAuthorize("search-location")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Branches.ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name
                })
            });
        }

        // GET: Branches/5
        [CustomAuthorize("view-location")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(short id)
        {
            var data = await _context.Branches
                .Include(x => x.State)
                .Include(x => x.City)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                Branch = new
                {
                    data.Id,
                    data.Name,
                    data.StateId,
                    StateName = data.State.Name,
                    data.CityId,
                    CityName = data.City.Name
                }
            });
        }

        // Post: Branches/Create
        [CustomAuthorize("write-location")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            Branch data = new()
            {
                Name = input.Name,
                StateId = input.StateId,
                CityId = input.CityId,
                Address = "N/A",
                Contact = "N/A"
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: Branches/5
        [CustomAuthorize("update-location")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(short id, UpdateInputModel input)
        {
            var data = await _context.Branches.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.StateId = input.StateId;
            data.CityId = input.CityId;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Branches/5
        [CustomAuthorize("delete-location")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(short id)
        {
            var data = await _context.Branches.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.EmpTransactions.AnyAsync(x => x.BranchId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Location is already in use.");
            }

            _context.Branches.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
            public int StateId { get; set; }
            public int CityId { get; set; }
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
                    .MustBeUnique(_context.Branches.AsQueryable(), "Name");

                RuleFor(x => x.StateId)
                    .NotEmpty()
                    .IdMustExist(_context.States.AsQueryable());

                RuleFor(x => x.CityId)
                    .NotEmpty()
                    .IdMustExist(_context.Cities.AsQueryable());
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
                    .MustBeUnique(_context.Branches.Where(x => x.Id != short.Parse(_id)).AsQueryable(), "Name");

                RuleFor(x => x.StateId)
                    .NotEmpty()
                    .IdMustExist(_context.States.AsQueryable());

                RuleFor(x => x.CityId)
                    .NotEmpty()
                    .IdMustExist(_context.Cities.AsQueryable());
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Branches.Find(short.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
