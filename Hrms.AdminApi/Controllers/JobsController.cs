using Hrms.Common.Enumerations;
using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities.Bzip2;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class JobsController : Controller
    {
        private readonly DataContext _context;

        public JobsController(DataContext context)
        {
            _context = context;
        }

        // GET: Jobs
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, 
            string title, int? branchId, int? departmentId, int? employmentTypeId, string status)
        {
            var statusName = "";
            var query = _context.Jobs
                .Include(x => x.EmploymentType)
                .Include(x => x.Branch)
                .Include(x => x.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(b => b.Title!.ToLower().Contains(title.ToLower()));
            }

            if (branchId is not null)
            {
                query = query.Where(b => b.BranchId == branchId);
            }

            if (departmentId is not null)
            {
                query = query.Where(b => b.DepartmentId == departmentId);
            }

            if (employmentTypeId is not null)
            {
                query = query.Where(b => b.EmploymentTypeId == employmentTypeId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (!Enumeration.GetAll<JobStatus>().Select(x => x.Id).ToList().Contains(status))
                {
                    return ErrorHelper.ErrorResult("status", "Status does not exists.");
                }
                
                statusName = Enumeration.GetAll<JobStatus>().FirstOrDefault(y => y.Id == status).Name;

                query = query.Where(b => b.Status!.ToLower().Contains(status.ToLower()));
            }


            Expression<Func<Job, object>> field = sortColumn switch
            {
                "JobTitle" => x => x.Title,
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

            var data = await PagedList<Job>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Title = x.Title,
                    Status = x.Status,
                    EmploymentTypeId = x.EmploymentTypeId,
                    EmploymentTypeName = x.EmploymentType?.Name,
                    BranchId = x.BranchId,
                    BranchName = x.Branch?.Name,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department?.Name,
                    Quantity = x.Quantity,
                    Description = x.Description,
                    EstimatedDate = x.EstimatedDate,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // GETALL: Jobs
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Jobs.Where(x => x.Status == "active").ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Title = x.Title,
                    Status = x.Status,
                    Quantity = x.Quantity
                })
            });
        }

        // GET: Jobs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Jobs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                Job = data
            });
        }

        // Post: Jobs/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            DateOnly estimatedDate = DateOnlyHelper.ParseDateOrNow(input.EstimatedDate);

            Job data= new()
            {
                Title = input.Title,
                EmploymentTypeId = input.EmploymentTypeId,
                BranchId = input.BranchId,
                DepartmentId = input.DepartmentId,
                Quantity = input.Quantity,
                Description = input.Description,
                EstimatedDate = estimatedDate,
                Status = "active"
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: Jobs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            DateOnly estimatedDate = DateOnlyHelper.ParseDateOrNow(input.EstimatedDate);

            var data = await _context.Jobs.FirstOrDefaultAsync(c => c.Id == id);

            data.Title = input.Title;
            data.EmploymentTypeId = input.EmploymentTypeId;
            data.BranchId = input.BranchId;
            data.DepartmentId = input.DepartmentId;
            data.Quantity = input.Quantity;
            data.Description = input.Description;
            data.EstimatedDate = estimatedDate;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Jobs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Jobs.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.Candidates.AnyAsync(x => x.JobId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Job is already in use.");
            }

            _context.Jobs.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: Jobs/updateStatus/5
        [HttpPut("UpdateStatus/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateStatusInputModel input)
        {
            var data = await _context.Jobs.FirstOrDefaultAsync(c => c.Id == id);

            data.Status = input.Status;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Title { get; set; }
            public int EmploymentTypeId { get; set; }
            public short BranchId { get; set; }
            public int DepartmentId { get; set; }
            public int Quantity { get; set; }
            public string EstimatedDate { get; set; }
            public string Description { get; set; }
            
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class UpdateStatusInputModel
        {
            public string Status { get; set; }
        }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                Transform(x => x.Title, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.Jobs.AsQueryable(), "Title");

                RuleFor(x => x.EmploymentTypeId)
                    .NotEmpty()
                    .IdMustExist(_context.EmploymentTypes.AsQueryable());

                RuleFor(x => x.BranchId)
                    .NotEmpty()
                    .IdMustExist(_context.Branches.AsQueryable());

                RuleFor(x => x.DepartmentId)
                    .NotEmpty()
                    .IdMustExist(_context.Departments.AsQueryable());

                RuleFor(x => x.Quantity)
                    .NotEmpty();

                RuleFor(x => x.EstimatedDate)
                    .MustBeDate()
                    .MustBeDateAfterNow()
                    .Unless(x => string.IsNullOrEmpty(x.EstimatedDate));

                RuleFor(x => x.Description)
                    .NotEmpty();


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

                Transform(x => x.Title, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.Jobs.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Title");

                RuleFor(x => x.EmploymentTypeId)
                    .NotEmpty()
                    .IdMustExist(_context.EmploymentTypes.AsQueryable());

                RuleFor(x => x.BranchId)
                    .NotEmpty()
                    .IdMustExist(_context.Branches.AsQueryable());

                RuleFor(x => x.DepartmentId)
                    .NotEmpty()
                    .IdMustExist(_context.Departments.AsQueryable());

                RuleFor(x => x.Quantity)
                    .NotEmpty();

                RuleFor(x => x.EstimatedDate)
                    .MustBeDate()
                    .MustBeDateAfterNow()
                    .Unless(x => string.IsNullOrEmpty(x.EstimatedDate));

                RuleFor(x => x.Description)
                    .NotEmpty();
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Jobs.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }

        public class UpdateStatusInputModelValidator : AbstractValidator<UpdateStatusInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public UpdateStatusInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor) 
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

                RuleFor(x => x.Status)
                    .NotEmpty()
                    .MustBeIn(Enumeration.GetAll<JobStatus>().Select(x => x.Id).ToList());
            }

            protected override bool PreValidate(ValidationContext<UpdateStatusInputModel> context, ValidationResult result)
            {
                if (_context.Jobs.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
