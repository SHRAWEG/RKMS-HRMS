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
    public class DocumentTypesController : Controller
    {
        private readonly DataContext _context;

        public DocumentTypesController(DataContext context)
        {
            _context = context;
        }

        // GET: DocumentTypes
        [CustomAuthorize("list-document-type")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name)
        {
            var query = _context.DocumentTypes.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(d => d.Name.ToLower().Contains(name.ToLower()));
            }

            Expression<Func<DocumentType, object>> field = sortColumn switch
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

            var data = await PagedList<DocumentType>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: DocumentTypes/All
        [CustomAuthorize("search-document-type")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.DocumentTypes.ToListAsync();
            return Ok(new { Data = data });
        }

        // GET: DocumentTypes/5
        [CustomAuthorize("view-document-type")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.DocumentTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            return Ok(new { DocumentType = data });
        }

        // POST: DocumentTypes
        [CustomAuthorize("write-document-type")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            DocumentType data = new()
            {
                Name = input.Name
            };

            _context.DocumentTypes.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: DocumentTypes/5
        [CustomAuthorize("update-document-type")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.DocumentTypes.FirstOrDefaultAsync(c => c.Id == id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            data.Name = input.Name;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: DocumentTypes/5
        [CustomAuthorize("delete-document-type")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.DocumentTypes.FindAsync(id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            _context.DocumentTypes.Remove(data);
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
                    .MustBeUnique(context.DocumentTypes.AsQueryable(), "Name");
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
                    .MustBeUnique(_context.DocumentTypes.Where(x => x.Id != int.Parse(_id)), "Name");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.DocumentTypes.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
