using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]


    public class RostersController : Controller
    {
        private readonly DataContext _context;

        public RostersController(DataContext context)
        {
            _context = context;
        }

        // GET: Rosters
        [CustomAuthorize("check-roster")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, 
            int? empId, int? workHourId, string fromDate, string toDate)
        {
            DateOnly? FromDate;
            DateOnly? ToDate;

            var query = _context.Rosters
                .Include(x => x.Emp)
                .Include(x => x.WorkHour)
                .Include(x => x.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(fromDate))
            {
                FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);
                query = query.Where(x => x.Date >= FromDate);
            }

            if (!string.IsNullOrEmpty(fromDate))
            {
                ToDate = DateOnlyHelper.ParseDateOrNow(toDate);
                query = query.Where(x => x.Date <= ToDate);
            }

            if (empId is not null)
            {
                query = query.Where(b => b.EmpId == empId);
            }

            if (workHourId is not null)
            {
                query = query.Where(b => b.WorkHourId == workHourId);
            }

            Expression<Func<Roster, object>> field = sortColumn switch
            {
                "EmpName" => x => x.EmpId,
                "WorkHourName" => x => x.WorkHourId,
                "Date" => x => x.Date,
                "CreatedAt" => x => x.CreatedAt,
                "UpdatedAt" =>  x => x.UpdatedAt,
                _ => x => x.Date
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Date);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<Roster>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.EmpId,
                    EmpName = FullName(x.Emp.FirstName, x.Emp.MiddleName, x.Emp.LastName),
                    EmpCode = x.Emp.CardId,
                    x.WorkHourId,
                    WorkHourName = x.WorkHour.Name,
                    x.Date,
                    x.CreatedAt,
                    x.UpdatedAt,
                    x.UserId,
                    SetByUserName = x.User.UserName,
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // Post: Rosters/Create
        [CustomAuthorize("set-roster")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            DateOnly fromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);
            DateOnly toDate = DateOnlyHelper.ParseDateOrNow(input.ToDate);

            List<Roster> newRosters = new();

            do
            {
                foreach (var empId in input.EmpIds)
                {
                    if (newRosters.Any(x => x.EmpId == empId && x.Date == fromDate))
                    {
                        continue;
                    }

                    var attendance = await _context.Attendances.Where(x => x.EmpId == empId && x.TransactionDate == fromDate).FirstOrDefaultAsync();

                    if (attendance is not null)
                    {
                        attendance.WorkHourId = input.WorkHourId;
                    }

                    var roster = await _context.Rosters.Where(x => x.EmpId == empId && x.Date == fromDate).FirstOrDefaultAsync();

                    if (roster != null)
                    {
                        roster.WorkHourId = input.WorkHourId;
                        roster.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        newRosters.Add(new Roster
                        {
                            EmpId = empId,
                            Date = fromDate,
                            WorkHourId = input.WorkHourId,
                            UserId = User.GetUserId()
                        });
                    }
                }

                fromDate = fromDate.AddDays(1);
            } while (fromDate <= toDate);

            _context.AddRange(newRosters);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private static string FullName(string firstName, string middleName, string lastName)
        {
            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
        }

        public class BaseInputModel
        {
            public int[] EmpIds { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public short WorkHourId { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.FromDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.ToDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.FromDate, "From Date");

                RuleFor(x => x.WorkHourId)
                    .IdMustExist(_context.WorkHours.AsQueryable());
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

                RuleFor(x => x.FromDate)
                   .NotEmpty()
                   .MustBeDate();

                RuleFor(x => x.ToDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.FromDate, "From Date");

                RuleFor(x => x.WorkHourId)
                    .IdMustExist(_context.WorkHours.AsQueryable());
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Rosters.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
