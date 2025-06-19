using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class CalendarsController : Controller
    {
        private readonly DataContext _context;

        public CalendarsController(DataContext context)
        {
            _context = context;
        }

        // GET: Calendars
        [CustomAuthorize("list-calendar")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name)
        {
            var query = _context.Calendars.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name.Contains(name));
            }

            Expression<Func<Calendar, object>> field = sortColumn switch
            {
                "Name" => x => x.Name!,
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

            var data = await PagedList<Calendar>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: Calendars/5
        [CustomAuthorize("view-calendar")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Calendars
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                Category = data 
            });
        }

        // GET: Calendars/5
        [CustomAuthorize("search-calendar")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Calendars.ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        // Post: Calendars/Create
        [CustomAuthorize("write-calendar")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            Calendar holidayList = new()
            {
                Name = input.Name
            };

            _context.Add(holidayList);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: Calendars/5
        [CustomAuthorize("update-calendar")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Calendars.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Calendars/5
        [CustomAuthorize("delete-calendar")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Calendars.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.EmpCalendars.AnyAsync(x => x.CalendarId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Category already assigned to some employees.");
            }
            
            _context.Calendars.Remove(data);
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
                    .MustBeUnique(_context.Calendars.AsQueryable(), "Name");
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
                    .MustBeUnique(_context.Calendars.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Calendars.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
