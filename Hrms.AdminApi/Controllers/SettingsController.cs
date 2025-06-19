using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SettingsController : Controller
    {
        private readonly DataContext _context;

        public SettingsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Settings.FirstOrDefaultAsync();

            return Ok(new
            {
                Setting = data
            });
        }

        [Authorize(Roles = "super-admin")]
        // Post: Settings/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            if(await _context.Settings.AnyAsync())
            {
                return ErrorHelper.ErrorResult("Id", "Data already exists");
            }

            Setting data= new()
            {
                GrantLeaveType = input.GrantLeaveType,
                LeaveYearId = input.LeaveYearId,
                AttendanceReportInBs = input.AttendanceReportInBs,
                DailyAttendance = input.DailyAttendance,
                DailyAttendanceInBs = input.DailyAttendanceInBs,
                UniqueDeviceCode = input.UniqueDeviceCode
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        [Authorize(Roles = "super-admin")]
        // PUT: Settings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Settings.FirstOrDefaultAsync(c => c.Id == id);

            data.GrantLeaveType = input.GrantLeaveType;
            data.LeaveYearId = input.LeaveYearId;
            data.AttendanceReportInBs = input.AttendanceReportInBs;
            data.DailyAttendance = input.DailyAttendance;
            data.DailyAttendanceInBs = input.DailyAttendanceInBs;
            data.UniqueDeviceCode = input.UniqueDeviceCode;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "super-admin")]
        // DELETE: Settings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Settings.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }
            
            _context.Settings.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string GrantLeaveType { get; set; }
            public int LeaveYearId { get; set; }
            public bool AttendanceReportInBs { get; set; } = false;
            public bool DailyAttendance { get; set; } = false;
            public bool DailyAttendanceInBs { get; set; } = false;
            public bool UniqueDeviceCode { get; set; } = false;
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.GrantLeaveType)
                    .NotEmpty();

                RuleFor(x => x.LeaveYearId)
                    .NotEmpty()
                    .IdMustExist(_context.LeaveYears.AsQueryable())
                    .MustBeUnique(_context.Settings.AsQueryable(), "LeaveYearId");
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

                RuleFor(x => x.GrantLeaveType)
                    .NotEmpty();

                RuleFor(x => x.LeaveYearId)
                    .NotEmpty()
                    .IdMustExist(_context.LeaveYears.AsQueryable())
                    .MustBeUnique(_context.Settings.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "LeaveYearId");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Settings.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
