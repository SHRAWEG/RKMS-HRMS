using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hrms.Common.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using System.Collections.Generic;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DepartmentForLevelController : Controller
    {
        private readonly DataContext _context;

        public DepartmentForLevelController(DataContext context)
        {
            _context = context;
        }

        [CustomAuthorize("list-department")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string departmentName)
        {
            var query = _context.DepartmentForLevels.AsQueryable();

            if (!string.IsNullOrEmpty(departmentName))
            {
                query = query.Where(d => d.Name!.ToLower().Contains(departmentName.ToLower()));
            }

            Expression<Func<DepartmentForLevel, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
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

            var data = await PagedList<DepartmentForLevel>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        [CustomAuthorize("search-department")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.DepartmentForLevels.ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        [CustomAuthorize("view-department")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.DepartmentForLevels
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                Department = data
            });
        }

        [CustomAuthorize("write-department")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            DepartmentForLevel data = new()
            {
                Name = input.Name,
                CreateAt = DateTime.UtcNow
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [CustomAuthorize("update-department")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.DepartmentForLevels.FirstOrDefaultAsync(c => c.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            data.Name = input.Name;
            data.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [CustomAuthorize("delete-department")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.DepartmentForLevels.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            _context.DepartmentForLevels.Remove(data);
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
                    .MustBeUnique(_context.DepartmentForLevels.AsQueryable(), "Name");
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
                    .MustBeUnique(_context.DepartmentForLevels.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.DepartmentForLevels.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
