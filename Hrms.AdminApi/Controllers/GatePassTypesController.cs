using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class GatePassTypesController : Controller
    {
        private readonly DataContext _context;

        public GatePassTypesController(DataContext context)
        {
            _context = context;
        }

        // GET: GatePassTypes
        [CustomAuthorize("list-gate-pass-type")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string gradeType)
        {
            var query = _context.GatePassTypes.AsQueryable();

            if (!string.IsNullOrEmpty(gradeType))
            {
                query = query.Where(b => b.Name.Contains(gradeType));
            }

            Expression<Func<GatePassType, object>> field = sortColumn switch
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

            var data = await PagedList<GatePassType>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: GatePassTypes/5
        [CustomAuthorize("view-gate-pass-type")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.GatePassTypes
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                GatePassType = data
            });
        }

        // GETALL: GatePassTypes
        [CustomAuthorize("search-gate-pass-type")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.GatePassTypes.ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        // Post: GatePassTypes/Create
        [CustomAuthorize("write-gate-pass-type")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            GatePassType data= new()
            {
                Name = input.Name,
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: GatePassTypes/5
        [CustomAuthorize("update-gate-pass-type")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.GatePassTypes.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: GatePassTypes/5
        [CustomAuthorize("delete-gate-pass-type")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.GatePassTypes.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.Regularisations.AnyAsync(x => x.GatePassTypeId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Gate type is already in use.");
            }
            
            _context.GatePassTypes.Remove(data);
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
                    .MustBeUnique(_context.GatePassTypes.AsQueryable(), "Name");
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
                    .MustBeUnique(_context.GatePassTypes.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.GatePassTypes.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
