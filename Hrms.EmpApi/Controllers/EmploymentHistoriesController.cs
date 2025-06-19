using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Xml.Linq;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class EmploymentHistoriesController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public EmploymentHistoriesController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: EmploymentHistories
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);

            var data = await _context.EmploymentHistories
                .Where(x => x.EmpId == user.EmpId)
                .ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Organization,
                    x.FromDate,
                    x.ToDate,
                    x.Designation,
                    x.Location,
                    x.City
                }),
            });
        }

        // GET: EmploymentHistories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.EmploymentHistories
                .SingleOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                EmploymentHistory = new
                {
                    data.Id,
                    data.Organization,
                    data.FromDate,
                    data.ToDate,
                    data.Designation,
                    data.Location,
                    data.City
                }
            });
        }

        // Post: EmploymentHistories/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var user = await _userManager.GetUserAsync(User);

            DateOnly fromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);
            DateOnly toDate = DateOnlyHelper.ParseDateOrNow(input.ToDate);

            EmploymentHistory data = new()
            {
                EmpId = user.EmpId ?? 1,
                Organization = input.Organization,
                FromDate = fromDate,
                ToDate = toDate,
                Designation = input.Designation,
                Location = input.Location,
                City = input.City
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: EmploymentHistories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.EmploymentHistories.FirstOrDefaultAsync(c => c.Id == id);

            DateOnly fromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);
            DateOnly toDate = DateOnlyHelper.ParseDateOrNow(input.ToDate);

            data.Organization = input.Organization;
            data.FromDate = fromDate;
            data.ToDate = toDate;
            data.Designation = input.Designation;
            data.Location = input.Location;
            data.City = input.City;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: EmploymentHistories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.EmploymentHistories.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            _context.EmploymentHistories.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Organization { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string Designation { get; set; }
            public string Location { get; set; }
            public string City { get; set; }

        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            public AddInputModelValidator()
            {
                RuleFor(x => x.Organization)
                    .NotEmpty();

                RuleFor(x => x.FromDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.ToDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfter(x => x.FromDate, "From Date");

                RuleFor(x => x.Designation)
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

                RuleFor(x => x.Organization)
                    .NotEmpty();

                RuleFor(x => x.FromDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.ToDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfter(x => x.FromDate, "From Date");

                RuleFor(x => x.Designation)
                    .NotEmpty();
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.EmploymentHistories.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
