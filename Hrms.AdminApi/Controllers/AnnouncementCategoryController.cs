using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "super-admin, admin")]
    public class AnnouncementCategoryController : ControllerBase
    {
        private readonly DataContext _context;

        public AnnouncementCategoryController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name, string code)
        {
            var query = _context.AnnouncementCategories.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrEmpty(code))
            {
                query = query.Where(b => b.Code!.ToLower().Contains(code.ToLower()));
            }

            Expression<Func<AnnouncementCategory, object>> field = sortColumn switch
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

            var data = await PagedList<AnnouncementCategory>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.AnnouncementCategories.ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.AnnouncementCategories
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                AnnouncementCategory = data
            });
        }

        
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            AnnouncementCategory data = new()
            {
                Name = input.Name,
                Code = input.Code
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.AnnouncementCategories.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.Code = input.Code;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.AnnouncementCategories.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.Announcements.AnyAsync(x => x.AnnouncementCategoryId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Announcement Category is already in use.");
            }

            _context.AnnouncementCategories.Remove(data);
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
                    .MustBeUnique(_context.AnnouncementCategories.AsQueryable(), "Name");

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.AnnouncementCategories.AsQueryable(), "Code");
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
                    .MustBeUnique(_context.AnnouncementCategories.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name");

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.AnnouncementCategories.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Code");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.AnnouncementCategories.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
