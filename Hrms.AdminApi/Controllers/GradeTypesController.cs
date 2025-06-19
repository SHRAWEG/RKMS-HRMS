using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "super-admin, admin")]

    public class GradeTypesController : Controller
    {
        private readonly DataContext _context;

        public GradeTypesController(DataContext context)
        {
            _context = context;
        }

        // GET: GradeTypes
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string gradeType)
        {
            var query = _context.GradeTypes.AsQueryable();

            if (!string.IsNullOrEmpty(gradeType))
            {
                query = query.Where(b => b.GType.Contains(gradeType));
            }

            Expression<Func<GradeType, object>> field = sortColumn switch
            {
                "GType" => x => x.GType,
                _ => x => x.GType
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.GType);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<GradeType>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // Post: GradeTypes/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            GradeType data= new()
            {
                GType = input.GType,
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: GradeTypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, UpdateInputModel input)
        {
            var data = await _context.GradeTypes.FirstOrDefaultAsync(c => c.GType == id);

            data.GType = input.GType;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: GradeTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var data = await _context.GradeTypes.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }
            
            _context.GradeTypes.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string GType { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context; 

                Transform(x => x.GType, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.GradeTypes.AsQueryable(), "GType");
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

                Transform(x => x.GType, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.GradeTypes.Where(x => x.GType != _id).AsQueryable(), "GType");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.GradeTypes.Find(_id) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
