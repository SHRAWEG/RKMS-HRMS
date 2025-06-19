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
    public class HierarchyLevelController : Controller
    {
        private readonly DataContext _context;

        public HierarchyLevelController(DataContext context)
        {
            _context = context;
        }

        [CustomAuthorize("list-hierarchy-level")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, int? level, int? departmentId)
        {
            var query = _context.HierarchyLeves.Include(h => h.Department).AsQueryable();

            if (level.HasValue)
            {
                query = query.Where(h => h.Level == level);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(h => h.DepartmentId == departmentId);
            }

            Expression<Func<HierarchyLevel, object>> field = sortColumn switch
            {
                "Level" => x => x.Level,
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

            var data = await PagedList<HierarchyLevel>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        [CustomAuthorize("search-hierarchy-level")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.HierarchyLeves.Include(h => h.Department)
                                                    .OrderBy(h => h.Level)
                                                    .ThenBy(h => h.Id)
                                                    .ToListAsync();

            if (data == null || !data.Any())
            {
                return Ok(new
                {
                    Data = new List<HierarchyLevelDto>()
                });
            }

            var result = BuildHierarchy(data);

            return Ok(new
            {
                Data = result
            });
        }

        private List<HierarchyLevelDto> BuildHierarchy(List<HierarchyLevel> data)
        {
            var hierarchyLevels = data.Select(level => new HierarchyLevelDto
            {
                Id = level.Id,
                Level = level.Level,
                LevelName = level.LevelName,
                ParentId = level.ParentId ?? 0,
                ChildLevels = new List<HierarchyLevelDto>(),
                DepartmentName = level.Department?.Name
            }).ToList();

            var lookup = hierarchyLevels.ToLookup(x => x.ParentId);

            foreach (var level in hierarchyLevels)
            {
                level.ChildLevels = lookup[level.Id].ToList();
            }

            return hierarchyLevels.Where(l => l.ParentId == 0).ToList();
        }

        [CustomAuthorize("view-hierarchy-level")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.HierarchyLeves.Include(h => h.Department)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                HierarchyLevel = data
            });
        }

        [CustomAuthorize("write-hierarchy-level")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            if (input.ParentId < 1)
            {
                input.ParentId = null;
            }
            HierarchyLevel data = new()
            {
                ParentId = input.ParentId,
                DepartmentId = input.DepartmentId,
                Level = input.Level,
                LevelName = input.LevelName
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [CustomAuthorize("update-hierarchy-level")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.HierarchyLeves.FirstOrDefaultAsync(c => c.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (input.ParentId < 1)
            {
                data.ParentId = null;
            }
            else
            {
                data.ParentId = input.ParentId;
            }

            data.DepartmentId = input.DepartmentId;
            data.Level = input.Level;
            data.LevelName = input.LevelName;
            data.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [CustomAuthorize("delete-hierarchy-level")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.HierarchyLeves.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            _context.HierarchyLeves.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public int? ParentId { get; set; }
            public int? DepartmentId { get; set; }
            public int Level { get; set; }
            public string LevelName { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            public AddInputModelValidator()
            {
                RuleFor(x => x.Level).NotEmpty().WithMessage("Level is required.");
                RuleFor(x => x.LevelName).NotEmpty().WithMessage("LevelName is required.");
            }
        }

        public class UpdateInputModelValidator : AbstractValidator<UpdateInputModel>
        {
            private readonly DataContext _context;
            private readonly string _id;

            public UpdateInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

                RuleFor(x => x.Level).NotEmpty().WithMessage("Level is required.");
                RuleFor(x => x.LevelName).NotEmpty().WithMessage("LevelName is required.");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.HierarchyLeves.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
