using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class CostCentersController : Controller
    {
        private readonly DataContext _context;

        public CostCentersController(DataContext context)
        {
            _context = context;
        }

        // GET: CostCenters
        [CustomAuthorize("list-cost-center")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name, string code)
        {
            var query = _context.CostCenters.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrEmpty(code))
            {
                query = query.Where(b => b.Code!.ToLower().Contains(code.ToLower()));
            }

            Expression<Func<CostCenter, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "Code" => x => x.Code,
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

            var data = await PagedList<CostCenter>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GETALL: CostCenters
        [CustomAuthorize("search-cost-center")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.CostCenters.ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        // GET: CostCenters/5
        [CustomAuthorize("view-cost-center")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.CostCenters
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                CostCenter = data
            });
        }

        // Post: CostCenters/Create
        [CustomAuthorize("write-cost-center")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            CostCenter data= new()
            {
                Name = input.Name,
                Code = input.Code
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: CostCenters/5
        [CustomAuthorize("update-cost-center")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.CostCenters.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.Code = input.Code;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: CostCenters/5
        [CustomAuthorize("delete-cost-center")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.CostCenters.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.EmpTransactions.AnyAsync(x => x.CostCenterId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Cost Center is already in use.");
            }

            _context.CostCenters.Remove(data);
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
                    .MustBeUnique(_context.CostCenters.AsQueryable(), "Name");

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.CostCenters.AsQueryable(), "Code");
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
                    .MustBeUnique(_context.CostCenters.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name");

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.CostCenters.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Code");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.CostCenters.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
