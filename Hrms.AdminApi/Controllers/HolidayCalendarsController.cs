using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.Formula.PTG;
using System.Security.Cryptography.X509Certificates;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HolidayCalendarsController : Controller
    {
        private readonly DataContext _context;

        public HolidayCalendarsController(DataContext context)
        {
            _context = context;
        }

        // GET: HolidayCalendars
        [CustomAuthorize("list-holiday-calendar")]
        [HttpGet]
        public async Task<IActionResult> Get(int? page, int? calendarId)
        {
            int Page = page ?? 1;

            var query = _context.Calendars
                .AsQueryable();

            if (calendarId is not null)
            {
                Page = 1;
                query = query.Where(x => x.Id == calendarId);
            }

            var calendar = await query.Skip(Page -1).FirstOrDefaultAsync();
            var count = await query.CountAsync();

            if (calendar is null)
            {
                return Ok(new
                {
                    Data = Array.Empty<int>(),
                    TotalCount = 0,
                    TotalPage = 0,
                });
            }

            var holidays = await _context.HolidayCalendars
                .Include(x => x.Holiday)
                .Where(x => x.CalendarId == calendar.Id)
                .Select(x => new
                {
                    HolidayId = x.HolidayId,
                    Name = x.Holiday.Name,
                    Date = x.Holiday.Date,
                    Day = x.Holiday.Day,
                    Type = x.Holiday.Type,
                    DayType = x.Holiday.DayType
                })
                .ToListAsync();


            return Ok(new
            {
                Data = new
                {
                    CalendarId = calendar.Id,
                    CalendarName = calendar.Name,
                    Holidays = holidays.Select(x => new
                    {
                        HolidayId = x.HolidayId,
                        Name = x.Name,
                        Date = x.Date,
                        Day = x.Day,
                        Type = Enumeration.GetAll<HolidayType>().Where(h => h.Id == x.Type).FirstOrDefault()?.Name,
                        DayType = Enumeration.GetAll<HolidayDayType>().Where(h => h.Id == x.DayType).FirstOrDefault()?.Name,
                    })
                },
                TotalCount = count,
                TotalPages = count
            });
        }

        [CustomAuthorize("list-holiday-calendar")]
        [HttpGet("{calendarId}")]
        public async Task<IActionResult> GetCalendar(int calendarId)
        {
            if (!await _context.Calendars.AnyAsync(x => x.Id == calendarId))
            {
                return ErrorHelper.ErrorResult("CalendarId", "Id does not exists.");
            }

            var data = await _context.HolidayCalendars.Where(x => x.CalendarId == calendarId).ToListAsync();

            return Ok(new
            {
                HolidayIds = data.Select(x => x.HolidayId)
            });
        }

        // Post: HolidayCalendars/Create
        [CustomAuthorize("add-holiday-to-calendar")]
        [HttpPost]
        public async Task<IActionResult> AddHoliday(AddInputModel input)
        {
            if (await _context.HolidayCalendars.Where(x => x.CalendarId == input.CalendarId && x.HolidayId == input.HolidayId).AnyAsync())
            {
                return ErrorHelper.ErrorResult("HolidayId", "Holiday already exists on this calendar.");
            }

            HolidayCalendar data = new()
            {
                CalendarId = input.CalendarId,
                HolidayId = input.HolidayId,
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // Post: HolidayCalendars/Create
        [CustomAuthorize("remove-holiday-from-calendar")]
        [HttpPost("RemoveHoliday")]
        public async Task<IActionResult> RemoveHoliday(AddInputModel input)
        {
            var holiday = await _context.HolidayCalendars.Where(x => x.CalendarId == input.CalendarId && x.HolidayId == input.HolidayId).FirstOrDefaultAsync();

            if (holiday is null)
            {
                return ErrorHelper.ErrorResult("HolidayId", "Holiday does not exist on this calendar.");
            }

            _context.Remove(holiday);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Bulk")]
        public async Task<IActionResult> CreateBulk(AddBulkInputModel bulkInput)
        {
            List<HolidayCalendar> data = new();

            foreach(var input in bulkInput.HolidayCalendars)
            {
                if (data.Where(x => x.CalendarId == input.CalendarId && x.HolidayId == input.HolidayId).Any())
                {
                    continue;
                }

                if (await _context.HolidayCalendars.Where(x => x.CalendarId == input.CalendarId && x.HolidayId == input.HolidayId).AnyAsync())
                {
                    continue;
                }

                HolidayCalendar holiday = new()
                {
                    CalendarId = input.CalendarId,
                    HolidayId = input.HolidayId
                };

                data.Add(holiday);
            }

            _context.AddRange(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: HolidayCalendars/5
        [CustomAuthorize("delete-holiday-calendar")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.HolidayCalendars.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.EmpCalendars.AnyAsync(x => x.CalendarId == data.CalendarId))
            {
                return ErrorHelper.ErrorResult("Id", "Calendar already assigned to some Employees.");
            }
            
            _context.HolidayCalendars.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public int CalendarId { get; set; }
            public int HolidayId { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddBulkInputModel
        {
            public AddInputModel[] HolidayCalendars { get; set; } 
        }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.CalendarId)
                    .NotEmpty()
                    .IdMustExist(_context.Calendars.AsQueryable());

                RuleFor(x => x.HolidayId)
                    .NotEmpty()
                    .IdMustExist(_context.Holidays.AsQueryable());
            }
        }

        public class AddBulkInputModelValidator : AbstractValidator<AddBulkInputModel>
        {
            private readonly DataContext _context;

            public AddBulkInputModelValidator(DataContext context)
            {
                _context = context;

                RuleForEach(x => x.HolidayCalendars)
                    .NotEmpty()
                    .ChildRules(holidays =>
                    {
                        holidays.RuleFor(x => x.CalendarId)
                            .NotEmpty()
                            .IdMustExist(_context.Calendars.AsQueryable());

                        holidays.RuleFor(x => x.HolidayId)
                            .NotEmpty()
                            .IdMustExist(_context.Holidays.AsQueryable());
                    });
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

                RuleFor(x => x.CalendarId)
                    .NotEmpty()
                    .IdMustExist(_context.Calendars.AsQueryable());

                RuleFor(x => x.HolidayId)
                    .NotEmpty()
                    .IdMustExist(_context.Holidays.AsQueryable());
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.HolidayCalendars.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
