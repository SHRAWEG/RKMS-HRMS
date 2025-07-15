using Hrms.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.Results;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GuestsController : Controller
    {
        private readonly DataContext _context;

        public GuestsController(DataContext context)
        {
            _context = context;
        }

        // GET: Guests
        [CustomAuthorize("list-guest")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name)
        {
            var query = _context.Guests.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(g => g.Name.ToLower().Contains(name.ToLower()));
            }

            Expression<Func<Guest, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                _ => x => x.Id
            };

            query = sortDirection switch
            {
                "asc" => query.OrderBy(field),
                "desc" => query.OrderByDescending(field),
                _ => query.OrderByDescending(x => x.Id)
            };

            var data = await PagedList<Guest>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: Guests/All
        [CustomAuthorize("search-guest")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Guests.ToListAsync();
            return Ok(new { Data = data });
        }

        // GET: Guests/5
        [CustomAuthorize("view-guest")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Guests.FirstOrDefaultAsync(x => x.Id == id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            return Ok(new { Guest = data });
        }

        // POST: Guests
        [CustomAuthorize("write-guest")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            Guest data = new()
            {
                Name = input.Name
            };

            _context.Guests.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: Guests/5
        [CustomAuthorize("update-guest")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Guests.FirstOrDefaultAsync(c => c.Id == id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            data.Name = input.Name;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Guests/5
        [CustomAuthorize("delete-guest")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Guests.FindAsync(id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            _context.Guests.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // ======== Input Models and Validators ========

        public class BaseInputModel
        {
            public string Name { get; set; }
        }

        public class AddInputModel : BaseInputModel { }
        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            public AddInputModelValidator(DataContext context)
            {
                Transform(x => x.Name, v => v?.Trim())
                    .NotEmpty().WithMessage("Name is required.")
                    .MustBeUnique(context.Guests.AsQueryable(), "Name");
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
                    .NotEmpty().WithMessage("Name is required.")
                    .MustBeUnique(_context.Guests.Where(x => x.Id != int.Parse(_id)), "Name");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Guests.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
