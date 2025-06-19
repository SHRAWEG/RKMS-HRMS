using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Cryptography;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class WeekendsController : Controller
    {
        private readonly DataContext _context;

        public WeekendsController(DataContext context)
        {
            _context = context;
        }

        // GET: WeekendDetails
        [CustomAuthorize("list-weekend")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name, int? empId)
        {
            var query = (from weekend in _context.WeekendDetails
                         join emp in _context.EmpDetails on weekend.EmpId equals emp.Id into emps
                         from emp in emps.DefaultIfEmpty()
                         select new
                         {
                             weekend.Id,
                             EmpId = emp != null ? emp.Id : 0,
                             EmpCode = emp != null ? emp.CardId : "",
                             Name = emp != null ? emp.FirstName + " " + (!string.IsNullOrEmpty(emp.MiddleName) ? emp.MiddleName + " " : "") + emp.LastName : "",
                             weekend.BranchId,
                             BranchName = weekend.Branch != null ? weekend.Branch.Name : "",
                             weekend.ValidFrom,
                             weekend.Sunday,
                             weekend.Monday,
                             weekend.Tuesday,
                             weekend.Wednesday,
                             weekend.Thursday,
                             weekend.Friday,
                             weekend.Saturday
                         }).AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            if (empId is not null)
            {
                query = query.Where(b => b.EmpId == empId);
            }

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Id);
            }
            else if (sortDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "ValidFrom":
                        query.OrderBy(x => x.ValidFrom);
                        break;

                    case "EmpCode":
                        query.OrderBy(x => x.EmpCode);
                        break;

                    case "BranchId":
                        query.OrderBy(x => x.BranchId);
                        break;

                    case "Name":
                        query.OrderBy(x => x.Name);
                        break;

                    case "Sunday":
                        query.OrderBy(x => x.Sunday);
                        break;

                    case "Monday":
                        query.OrderBy(x => x.Monday);
                        break;

                    case "Tuesday":
                        query.OrderBy(x => x.Tuesday);
                        break;

                    case "Wednesday":
                        query.OrderBy(x => x.Wednesday);
                        break;

                    case "Thursday":
                        query.OrderBy(x => x.Thursday);
                        break;

                    case "Friday":
                        query.OrderByDescending(x => x.Friday);
                        break;

                    case "Saturday":
                        query.OrderByDescending(x => x.Saturday);
                        break;

                    default:
                        query.OrderBy(x => x.Id);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "ValidFrom":
                        query.OrderBy(x => x.ValidFrom);
                        break;

                    case "EmpCode":
                        query.OrderByDescending(x => x.EmpCode);
                        break;

                    case "BranchId":
                        query.OrderByDescending(x => x.BranchId);
                        break;

                    case "Name":
                        query.OrderByDescending(x => x.Name);
                        break;

                    case "Sunday":
                        query.OrderByDescending(x => x.Sunday);
                        break;

                    case "Monday":
                        query.OrderByDescending(x => x.Monday);
                        break;

                    case "Tuesday":
                        query.OrderByDescending(x => x.Tuesday);
                        break;

                    case "Wednesday":
                        query.OrderByDescending(x => x.Wednesday);
                        break;

                    case "Thursday":
                        query.OrderByDescending(x => x.Thursday);
                        break;

                    case "Friday":
                        query.OrderByDescending(x => x.Friday);
                        break;

                    case "Saturday":
                        query.OrderByDescending(x => x.Saturday);
                        break;

                    default:
                        query.OrderByDescending(x => x.Id);
                        break;
                }
            }

            var TotalCount = await query.CountAsync();
            var TotalPages = (int)Math.Ceiling(TotalCount / (double)page);
            var data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return Ok(new
            {
                Data = data,
                TotalCount,
                TotalPages
            });
        }

        // GET: Weekend/Detail
        [CustomAuthorize("view-weekend")]
        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> GetWeekendDetail(int id)
        {
            var data = await _context.WeekendDetails.Where(x => x.EmpId == id).SingleOrDefaultAsync();

            if(data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id");
            }

            return Ok(new
            {
                Data = new
                {
                    data.Id,
                    data.EmpId,
                    data.BranchId,
                    BranchName = data.Branch?.Name,
                    data.ValidFrom,
                    data.Sunday,
                    data.Monday,
                    data.Tuesday,
                    data.Wednesday,
                    data.Thursday,
                    data.Friday,
                    data.Saturday,
                }
            });
        }

        // GET: WeekendDetails/5
        [CustomAuthorize("view-weekend")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = (from weekend in _context.WeekendDetails where weekend.Id == id
                         join emp in _context.EmpDetails on weekend.EmpId equals emp.Id into emps
                         from emp in emps.DefaultIfEmpty()
                         select new
                         {
                             weekend.Id,
                             EmpId = emp != null ? emp.Id : 0,
                             EmpCode = emp != null ? emp.CardId : "",
                             Name = emp != null ? emp.FirstName + " " + (!string.IsNullOrEmpty(emp.MiddleName) ? emp.MiddleName + " " : "") + emp.LastName : "",
                             weekend.ValidFrom,
                             weekend.Sunday,
                             weekend.Monday,
                             weekend.Tuesday,
                             weekend.Wednesday,
                             weekend.Thursday,
                             weekend.Friday,
                             weekend.Saturday
                         }).SingleOrDefault();

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                Weekend = data
            });
        }

        // Post: WeekendDetails/Create
        [CustomAuthorize("write-weekend")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            DateOnly? validFrom = null;

            if (!string.IsNullOrEmpty(input.ValidFrom))
            {
                validFrom = DateOnlyHelper.ParseDateOrNow(input.ValidFrom);
            }

            if (await _context.WeekendDetails.AnyAsync(x => x.EmpId == input.EmpId && x.ValidFrom == validFrom))
            {
                return ErrorHelper.ErrorResult("ValidFrom", "Weekend from this date already exists.");
            }

            WeekendDetail data= new()
            {
                EmpId = input.EmpId,
                BranchId = input.BranchId,
                ValidFrom = validFrom,
                Sunday = input.Sunday,
                Monday = input.Monday,
                Tuesday = input.Tuesday,
                Wednesday = input.Wednesday,
                Thursday = input.Thursday,
                Friday = input.Friday,
                Saturday = input.Saturday
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: WeekendDetails/5
        [CustomAuthorize("update-weekend")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.WeekendDetails.FirstOrDefaultAsync(c => c.Id == id);

            DateOnly? validFrom = null;

            if (!string.IsNullOrEmpty(input.ValidFrom))
            {
                validFrom = DateOnlyHelper.ParseDateOrNow(input.ValidFrom);
            }

            if (await _context.WeekendDetails.AnyAsync(x => x.EmpId == input.EmpId && x.ValidFrom == validFrom))
            {
                return ErrorHelper.ErrorResult("ValidFrom", "Weekend from this date already exists.");
            }

            data.EmpId = input.EmpId;
            data.BranchId = input.BranchId;
            data.ValidFrom = validFrom;
            data.Sunday = input.Sunday;
            data.Monday = input.Monday;
            data.Tuesday = input.Tuesday;
            data.Wednesday = input.Wednesday;
            data.Thursday = input.Thursday;
            data.Friday = input.Friday;
            data.Saturday = input.Saturday;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: WeekendDetails/5
        [CustomAuthorize("delete-weekend")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.WeekendDetails.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }
            
            _context.WeekendDetails.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public short? BranchId { get; set; }
            public int? EmpId { get; set; }
            public string ValidFrom { get; set; }
            public bool Sunday { get; set; }
            public bool Monday { get; set; }
            public bool Tuesday { get; set; }
            public bool Wednesday { get; set; }
            public bool Thursday { get; set; }
            public bool Friday { get; set; }
            public bool Saturday { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.BranchId)
                    .IdMustExist(_context.Branches.AsQueryable())
                    .Unless(x => x.BranchId is null);

                RuleFor(x => x.EmpId)
                    .IdMustExist(_context.EmpDetails.AsQueryable())
                    .Unless(x => x.EmpId is null);

                RuleFor(x => x.ValidFrom)
                    .NotEmpty()
                    .MustBeDate();
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

                RuleFor(x => x.EmpId)
                    .IdMustExist(_context.EmpDetails.AsQueryable())
                    .Unless(x => x.EmpId is null);

                RuleFor(x => x.BranchId)
                    .IdMustExist(_context.Branches.AsQueryable())
                    .Unless(x => x.BranchId is null);

                RuleFor(x => x.ValidFrom)
                    .NotEmpty()
                    .MustBeDate();
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.WeekendDetails.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
