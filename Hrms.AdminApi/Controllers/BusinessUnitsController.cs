using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class BusinessUnitsController : Controller
    {
        private readonly DataContext _context;

        public BusinessUnitsController(DataContext context)
        {
            _context = context;
        }

        // GET: BusinessUnits
        [CustomAuthorize("list-business-unit")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name, string code)
        {
            var query = _context.BusinessUnits.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrEmpty(code))
            {
                query = query.Where(b => b.Code!.ToLower().Contains(code.ToLower()));
            }

            Expression<Func<BusinessUnit, object>> field = sortColumn switch
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

            var data = await PagedList<BusinessUnit>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GETALL: BusinessUnits
        [CustomAuthorize("search-business-unit")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.BusinessUnits.ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        // GET: BusinessUnits/5
        [CustomAuthorize("view-business-unit")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.BusinessUnits
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                BusinessUnit = data
            });
        }

        // Post: BusinessUnits/Create
        [CustomAuthorize("write-business-unit")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            BusinessUnit data= new()
            {
                Name = input.Name,
                Code = input.Code
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: BusinessUnits/5
        [CustomAuthorize("update-business-unit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.BusinessUnits.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.Code = input.Code;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: BusinessUnits/5
        [CustomAuthorize("delete-business-unit")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.BusinessUnits.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.EmpTransactions.AnyAsync(x => x.BusinessUnitId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Business Unit is already in use.");
            }

            _context.BusinessUnits.Remove(data);
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
                    .MustBeUnique(_context.BusinessUnits.AsQueryable(), "Name");

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.BusinessUnits.AsQueryable(), "Code");
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
                    .MustBeUnique(_context.BusinessUnits.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name");

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.BusinessUnits.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Code");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.BusinessUnits.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
