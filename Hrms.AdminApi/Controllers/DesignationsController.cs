using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Hrms.AdminApi.Controllers.AttendancesController;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class DesignationsController : Controller
    {
        private readonly DataContext _context;

        public DesignationsController(DataContext context)
        {
            _context = context;
        }

        // GET: Deisgnations
        [CustomAuthorize("list-designation")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name, string code)
        {
            var query = _context.Designations
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name!.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrEmpty(code))
            {
                query = query.Where(x => x.Code!.ToLower().Contains(code.ToLower()));
            }

            Expression<Func<Designation, object>> field = sortColumn switch
            {
                "Name" => x => x.Name!,
                "Code" => x => x.Code!,
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

            var data = await PagedList<Designation>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: Designations/all
        [CustomAuthorize("search-designation")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var query = _context.Designations
                .AsQueryable();
            var data = await query.ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code
                }),
            });
        }

        // GET: Designations/5
        [CustomAuthorize("view-designation")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(short id)
        {
            var data = await _context.Designations
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                Designation = new
                {
                    Id = data.Id,
                    Name = data.Name,
                    Code = data.Code
                }
            });
        }

        // Post: Designations
        [CustomAuthorize("write-designation")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {

            Designation designation = new()
            {
                //DepartmentId = input.DepartmentId,
                Name = input.Name,
                Code = input.Code,
                Rank = 0,
                JobDescription = "N/A",
                TotalStaffs = 0
            };

            _context.Designations.Add(designation);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: Designations/5
        [CustomAuthorize("update-designation")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(short id, UpdateInputModel input)
        {
            var data = await _context.Designations.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.Code = input.Code;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Designations/5
        [CustomAuthorize("delete-designation")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(short id)
        {
            var designation = await _context.Designations.FindAsync(id);

            if (designation == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.EmpTransactions.AnyAsync(x => x.DesignationId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Designation is already in use.");
            }


            _context.Designations.Remove(designation);
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
                    .MustBeUnique(_context.Designations.AsQueryable(), "Name");

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.Designations.AsQueryable(), "Code");
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
                    .MustBeUnique(_context.Designations.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name");

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.Designations.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Code");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Designations.Find(short.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
