using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "super-admin")]

    public class LeaveYearsController : Controller
    {
        private readonly DataContext _context;

        public LeaveYearsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.LeaveYears.ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.LeaveYears.FindAsync(id);

            if (data is null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id");
            }

            return Ok(new
            {
                LeaveYear = data,
            });
        }

        // Post: LeaveYears/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            DateOnly startDate = DateOnlyHelper.ParseDateOrNow(input.StartDate);
            DateOnly endDate = DateOnlyHelper.ParseDateOrNow(input.EndDate);

            LeaveYear data= new()
            {
                Year = input.Year,
                StartDate = startDate,
                EndDate = endDate,
                ByDate = "AD"
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            List<LeaveYearMonths> monthSequenceData = new();

            for(int i = 1; i < 13; i++)
            {
                LeaveYearMonths leaveYearMonth = new()
                {
                    LeaveYearId = data.Id,
                    MonthSequence = i,
                    Month = startDate.Month,
                    Year = startDate.Year
                };

                monthSequenceData.Add(leaveYearMonth);

                startDate = startDate.AddMonths(1);
            }

            _context.AddRange(monthSequenceData);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: LeaveYears/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.LeaveYears.Where(x => x.Id == id).SingleOrDefaultAsync();

            _context.RemoveRange(_context.LeaveYearMonths.Where(x => x.LeaveYearId == id).ToList());
            await _context.SaveChangesAsync();

            DateOnly startDate = DateOnlyHelper.ParseDateOrNow(input.StartDate);
            DateOnly endDate = DateOnlyHelper.ParseDateOrNow(input.EndDate);

            data.Year = input.Year;
            data.StartDate = startDate;
            data.EndDate = endDate;

            await _context.SaveChangesAsync();

            List<LeaveYearMonths> monthSequenceData = new();

            for (int i = 1; i < 13; i++)
            {
                LeaveYearMonths leaveYearMonth = new()
                {
                    LeaveYearId = data.Id,
                    MonthSequence = i,
                    Month = startDate.Month,
                    Year = startDate.Year
                };

                monthSequenceData.Add(leaveYearMonth);

                startDate = startDate.AddMonths(1);
            }

            _context.AddRange(monthSequenceData);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: LeaveYears/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.LeaveYears.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }
            
            _context.RemoveRange(_context.LeaveYearMonths.Where(x => x.LeaveYearId == id).ToList());
            _context.LeaveYears.Remove(data);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("MakeDefault/{id}")]
        public async Task<IActionResult> MakeDefault(int id)
        {
            if (!await _context.LeaveYears.AnyAsync(x => x.Id == id))
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            var defaultLeaveYear = await _context.LeaveYearCompanies.Where(x => x.CompanyId == null).FirstOrDefaultAsync();

            if (defaultLeaveYear is null)
            {
                _context.Add(new LeaveYearCompany
                {
                    LeaveYearId = id,
                });
            }
            else
            {
                if (defaultLeaveYear.LeaveYearId == id) return Ok();

                if (await _context.LeaveLedgers.AnyAsync(x => x.LeaveYearId == id && !x.IsClosed))
                {
                    return ErrorHelper.ErrorResult("Id", "The previous leave year is already in use. Please initiate the closing and try again.");
                }

                defaultLeaveYear.LeaveYearId = id;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Year { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.Year)
                    .NotEmpty();

                RuleFor(x => x.StartDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.EndDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.StartDate, "Start Date");
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

                RuleFor(x => x.Year)
                    .NotEmpty();

                RuleFor(x => x.StartDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.EndDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.StartDate, "Start Date");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.LeaveYears.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
