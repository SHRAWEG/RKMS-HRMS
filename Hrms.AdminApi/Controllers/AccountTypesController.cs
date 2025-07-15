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
    public class AccountTypesController : Controller
    {
        private readonly DataContext _context;

        public AccountTypesController(DataContext context)
        {
            _context = context;
        }

        // GET: AccountTypes
        [CustomAuthorize("list-accounttype")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name)
        {
            var query = _context.AccountTypes.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(g => g.Name.ToLower().Contains(name.ToLower()));
            }

            Expression<Func<AccountType, object>> field = sortColumn switch
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

            var data = await PagedList<AccountType>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: AccountTypes/All
        [CustomAuthorize("search-accounttype")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.AccountTypes.ToListAsync();
            return Ok(new { Data = data });
        }

        // GET: AccountTypes/5
        [CustomAuthorize("view-accounttype")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.AccountTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            return Ok(new { AccountType = data });
        }

        // POST: AccountTypes
        [CustomAuthorize("write-accounttype")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            AccountType data = new()
            {
                Name = input.Name
            };

            _context.AccountTypes.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: AccountTypes/5
        [CustomAuthorize("update-accounttype")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.AccountTypes.FirstOrDefaultAsync(c => c.Id == id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            data.Name = input.Name;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: AccountTypes/5
        [CustomAuthorize("delete-accounttype")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.AccountTypes.FindAsync(id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            _context.AccountTypes.Remove(data);
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
                    .MustBeUnique(context.AccountTypes.AsQueryable(), "Name");
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
                    .MustBeUnique(_context.AccountTypes.Where(x => x.Id != int.Parse(_id)), "Name");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.AccountTypes.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
