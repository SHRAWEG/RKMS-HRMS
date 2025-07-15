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
    public class AccountCodesController : Controller
    {
        private readonly DataContext _context;

        public AccountCodesController(DataContext context)
        {
            _context = context;
        }

        // GET: AccountCodes
        [CustomAuthorize("list-accountcode")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name)
        {
            var query = _context.AccountCodes.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name.ToLower().Contains(name.ToLower()));
            }

            Expression<Func<AccountCode, object>> field = sortColumn switch
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

            var data = await PagedList<AccountCode>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: AccountCodes/All
        [CustomAuthorize("search-accountcode")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.AccountCodes.ToListAsync();
            return Ok(new { Data = data });
        }

        // GET: AccountCodes/{id}
        [CustomAuthorize("view-accountcode")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.AccountCodes.FirstOrDefaultAsync(x => x.Id == id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            return Ok(new { AccountCode = data });
        }

        // POST: AccountCodes
        [CustomAuthorize("write-accountcode")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            AccountCode data = new()
            {
                Name = input.Name
            };

            _context.AccountCodes.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: AccountCodes/{id}
        [CustomAuthorize("update-accountcode")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.AccountCodes.FirstOrDefaultAsync(x => x.Id == id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            data.Name = input.Name;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE: AccountCodes/{id}
        [CustomAuthorize("delete-accountcode")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.AccountCodes.FindAsync(id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            _context.AccountCodes.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // ========= Input Models and Validators =========

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
                    .MustBeUnique(context.AccountCodes.AsQueryable(), "Name");
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
                    .MustBeUnique(_context.AccountCodes.Where(x => x.Id != int.Parse(_id)), "Name");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.AccountCodes.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }
                return true;
            }
        }
    }
}
