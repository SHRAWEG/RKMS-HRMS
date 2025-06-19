using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.XSSF.UserModel.Helpers;
using static Hrms.AdminApi.Controllers.AttendancesController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class DepartmentsController : Controller
    {
        private readonly DataContext _context;

        public DepartmentsController(DataContext context)
        {
            _context = context;
        }

        [CustomAuthorize("list-department")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection,
                string name, string code)
        {
            var query = _context.Departments
                .Where(x => x.FLDType == "D")
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name != null && x.Name.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrEmpty(code))
            {
                query = query.Where(x => x.Code!.ToLower().Contains(code.ToLower()));
            }

            Expression<Func<Department, object>> field = sortColumn switch
            {
            
                "Name" => x => x.Name!,
                "Code" => x => x.Code!,
                "Parent" => x => x.Parent!,
                "Level" => x => x.Level!,
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

            var data = await PagedList<Department>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Parent = x.Parent,
                    Name = x.Name,
                    Code = x.Code,
                    Level = x.Level
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: Departments/all
        [CustomAuthorize("search-department")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll(string parentName)
        {
            var query = _context.Departments
                .AsQueryable();

            if (!string.IsNullOrEmpty(parentName))
            {
                query = query.Where(x => x.Parent == parentName);
            }

            var data = await query.Where(x => x.FLDType == "D").ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Parent = x.Parent,
                    Name = x.Name,
                    Code = x.Code,
                    Level = x.Level
                })
            });
        }

        [CustomAuthorize("search-department")]
        [HttpGet("All/Section")]
        public async Task<IActionResult> GetAllSections(int? id)
        {
            var query = _context.Departments
                .AsQueryable();

            if (id is not null)
            {
                var department = await _context.Departments.Where(x => x.Id == id).FirstOrDefaultAsync();

                if (department is null)
                {
                    return ErrorHelper.ErrorResult("Id", "Department does not exists.");
                }

                query = query.Where(x => x.Parent == department.Name);
            }

            var data = await query.Where(x => x.FLDType == "S").ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
               //     CompanyId = x.CompanyId,
                 //   CompanyName = x.Company?.Name,
                    Parent = x.Parent,
                    Name = x.Name,
                    Code = x.Code,
                    Level = x.Level
                })
            }); 
        }

        // GET: Departments/5
        [CustomAuthorize("view-department")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Departments
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                Department = new
                {
                    Id = data.Id,
                    Parent = data.Parent,
                    Name = data.Name,
                    Code = data.Code,
                    FLDType = data.FLDType,
                    Level = data.Level
                }
            });
        }

        // Post: Departments
        [CustomAuthorize("write-department")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            if(!string.IsNullOrEmpty(input.ParentName))
            {
                var departmentParent = await _context.Departments.FirstOrDefaultAsync(x => x.Name == input.ParentName);

                if(departmentParent == null)
                {
                    return ErrorHelper.ErrorResult("ParentName", "Department Parent is invalid.");
                }

                if(await _context.Departments.Where(x => x.Parent == input.ParentName && x.Name == input.Name).AnyAsync())
                {
                    return ErrorHelper.ErrorResult("Name", "Name already exists on this department.");
                }

                Department subDepartment = new()
                {
                    Parent = input.ParentName,
                    Name = input.Name,
                    Code = input.Code,
                    Level = departmentParent.Level + 1,
                    FLDType = input.IsSection ? "S" : "D",
                    TotalStaffs = 0,
                };

                _context.Departments.Add(subDepartment);
                await _context.SaveChangesAsync();

                return Ok();
            }

            Department department = new()
            {
                Parent = "Main",
                Name = input.Name,
                Code = input.Code,
                Level = 1,
                FLDType = "D",
                TotalStaffs = 0,
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: Departments/5
        [CustomAuthorize("update-department")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Departments.FirstOrDefaultAsync(c => c.Id == id);

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();
            }

            if (data.Name != input.Name)
            {
                var subDepartments = await _context.Departments.Where(x => x.Parent == data.Name).ToListAsync();

                subDepartments.ForEach(x => x.Parent = input.Name);
            }

            if (data.Parent != input.ParentName)
            {
                var subDepartments = await _context.Departments.Where(x => x.Parent == data.Name).ToListAsync();
            }

            if (!string.IsNullOrEmpty(input.ParentName))
            {
                var departmentParent = await _context.Departments.FirstOrDefaultAsync(x => x.Name == input.ParentName);

                if (departmentParent == null)
                {
                    return ErrorHelper.ErrorResult("ParentName", "Department Parent is invalid.");
                }

                if (await _context.Departments.Where(x => x.Parent == input.ParentName && x.Name == input.Name && x.Id != id).AnyAsync())
                {
                    return ErrorHelper.ErrorResult("Name", "Name already exists on this department.");
                }

                data.Parent = input.ParentName;
            }

            data.Name = input.Name;
            data.Code = input.Code;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Departments/5    
        [CustomAuthorize("delete-department")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.Departments.AnyAsync(x => x.Parent == department.Name))
            {
                return ErrorHelper.ErrorResult("Id", "Please delete sub-departments first.");
            }

            if (await _context.EmpTransactions.AnyAsync(x => x.DepartmentId == id || x.SubDepartmentId == id) )
            
            {
                return ErrorHelper.ErrorResult("Id", "Department is already in use.");
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            
            public string Name { get; set; }
            public string Code { get; set; }
            public string ParentName { get; set; }
            public bool IsSection { get; set; }
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
                    .MustBeUnique(_context.Departments.AsQueryable(), "Name")
                    .Unless(x => !string.IsNullOrEmpty(x.ParentName))
                    .NotEmpty();

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.Departments.AsQueryable(), "Code");
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
                    .MustBeUnique(_context.Departments.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name")
                    .Unless(x => !string.IsNullOrEmpty(x.ParentName))
                    .NotEmpty();

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.Departments.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Code");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Departments.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
