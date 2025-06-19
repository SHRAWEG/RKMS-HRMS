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
    public class HolidaysController : Controller
    {
        private readonly DataContext _context;

        public HolidaysController(DataContext context)
        {
            _context = context;
        }

        // GET: Holidays
        [CustomAuthorize("list-holiday")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection,
            string holidayName, string dateFrom, string dateTo)
        {
            var query = _context.Holidays
                .AsQueryable();

            if (!string.IsNullOrEmpty(holidayName))
            {
                query = query.Where(b => b.Name.Contains(holidayName));
            }

            if (dateFrom != null)
            {
                DateOnly startDate = DateOnlyHelper.ParseDateOrNow(dateFrom);

                query = query.Where(p => p.Date >= startDate);
            }

            if (dateTo != null)
            {
                DateOnly endDate = DateOnlyHelper.ParseDateOrNow(dateTo);

                query = query.Where(b => b.Date  <= endDate);
            }

            Expression<Func<Holiday, object>> field = sortColumn switch
            {
                "Name" => x => x.Name!,
                "Date" => x => x.Date!,
                "Day" => x => x.Day!,
                "DayType" => x => x.DayType!,
                "Type" => x => x.Type!,
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

            var data = await PagedList<Holiday>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Day = x.Day,
                    Date = x.Date,
                    DayType = Enumeration.GetAll<HolidayDayType>().Where(y => y.Id == x.DayType).FirstOrDefault()?.Name,
                    Type = Enumeration.GetAll<HolidayType>().Where(y => y.Id == x.Type).FirstOrDefault()?.Name,
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: Holidays/5
        [CustomAuthorize("view-holiday")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Holidays
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                Holiday = new
                {
                    Id = data.Id,
                    Name = data.Name,
                    Day = data.Day,
                    Date = data.Date,
                    DayType = data.DayType,
                    DayTypeName = Enumeration.GetAll<HolidayDayType>().Where(y => y.Id == data.DayType).FirstOrDefault()?.Name,
                    Type = data.Type,                    
                    TypeName = Enumeration.GetAll<HolidayType>().Where(y => y.Id == data.Type).FirstOrDefault()?.Name,
                }
            });
        }

        [CustomAuthorize("search-holiday")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll(int id)
        {
            var data = await _context.Holidays
                .ToListAsync();

            return Ok(new
            {
                Holidays = data
            });
        }

        // Post: Holidays/Create
        [CustomAuthorize("write-holiday")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            /* TimeTrax
            
            var holidays = await _context.Holidays.ToListAsync();

            List<Holiday> newHolidays = new();

            DateOnly startDate = DateOnly.ParseExact(input.StartDate, "yyyy-MM-dd");
            DateOnly endDate = DateOnly.ParseExact(input.EndDate, "yyyy-MM-dd");

            do
            {
                newHolidays.Add(new Holiday
                {
                    Name = input.Name,
                    Date = startDate,
                    HolidayQuantity = 1
                });

                startDate = startDate.AddDays(1);

                if (holidays.Exists(x => x.Date.Equals(startDate)))
                {
                    return ErrorHelper.ErrorResult("StartDate", "Holiday already exists on some day between given date range");
                }

            } while (startDate <= endDate);

            _context.AddRange(newHolidays);

            */

            DateOnly date = DateOnlyHelper.ParseDateOrNow(input.Date);
            string day = date.ToString("dddd");

            if (await _context.Holidays.Where(x => x.Date == date).AnyAsync())
            {
                return ErrorHelper.ErrorResult("Date", "Holiday on this date already exists.");
            }

            Holiday data = new Holiday
            {
                Name = input.Name,
                Date = date,
                Day = day,
                DayType = input.DayType,
                Type = input.Type,
                Quantity = 1
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [CustomAuthorize("write-holiday")]
        [HttpPost("Bulk")]
        public async Task<IActionResult> CreateBulk(AddBulkInputModel bulkInput)
        {
            /* TimeTrax
            
            var holidays = await _context.Holidays.ToListAsync();

            List<Holiday> newHolidays = new();

            DateOnly startDate = DateOnly.ParseExact(input.StartDate, "yyyy-MM-dd");
            DateOnly endDate = DateOnly.ParseExact(input.EndDate, "yyyy-MM-dd");

            do
            {
                newHolidays.Add(new Holiday
                {
                    Name = input.Name,
                    Date = startDate,
                    HolidayQuantity = 1
                });

                startDate = startDate.AddDays(1);

                if (holidays.Exists(x => x.Date.Equals(startDate)))
                {
                    return ErrorHelper.ErrorResult("StartDate", "Holiday already exists on some day between given date range");
                }

            } while (startDate <= endDate);

            _context.AddRange(newHolidays);

            */

            List<Holiday> data = new();

            foreach(var input in bulkInput.Holidays)
            {
                DateOnly date = DateOnlyHelper.ParseDateOrNow(input.Date);
                string day = date.ToString("dddd");

                if (data.Where(x => x.Date == date).Any())
                {
                    continue;
                }

                if (await _context.Holidays.Where(x => x.Date == date).AnyAsync())
                {
                    return ErrorHelper.ErrorResult("Date", "Holiday on this date already exists.");
                }

                Holiday holiday = new Holiday
                {
                    Name = input.Name,
                    Date = date,
                    Day = day,
                    DayType = input.DayType,
                    Type = input.Type,
                    Quantity = 1
                };

                data.Add(holiday);
            }

            _context.AddRange(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: Holidays/5
        [CustomAuthorize("update-holiday")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            DateOnly date = DateOnlyHelper.ParseDateOrNow(input.Date);
            string day = date.ToString("dddd");

            var data = await _context.Holidays.FirstOrDefaultAsync(c => c.Id == id);

            if (await _context.Holidays.Where(x => x.Date == date && x.Id != id).AnyAsync())
            {
                return ErrorHelper.ErrorResult("Date", "Holiday on this date already exists.");
            }

            data.Name = input.Name;
            data.Date = date;
            data.Day = day;
            data.DayType = input.DayType;
            data.Type = input.Type;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Holidays/5
        [CustomAuthorize("delete-holiday")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Holidays.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.HolidayCalendars.AnyAsync(x => x.HolidayId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Holiday already exists in some calendars.");
            }
            
            _context.Holidays.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
            public string Date { get; set; }
            public string DayType { get; set; }
            public string Type { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddBulkInputModel
        {
            public AddInputModel[] Holidays { get; set; } 
        }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            public AddInputModelValidator()
            {
                Transform(x => x.Name, v => v?.Trim())
                    .NotEmpty();

                RuleFor(x => x.Date)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.DayType)
                    .NotEmpty()
                    .MustBeIn(Enumeration.GetAll<HolidayDayType>().Select(x => x.Id).ToList());

                RuleFor(x => x.Type)
                    .NotEmpty()
                    .MustBeIn(Enumeration.GetAll<HolidayType>().Select(x => x.Id).ToList());
            }
        }

        public class AddBulkInputModelValidator : AbstractValidator<AddBulkInputModel>
        {
            private readonly DataContext _context;

            public AddBulkInputModelValidator(DataContext context)
            {
                _context = context;

                RuleForEach(x => x.Holidays)
                    .NotEmpty()
                    .ChildRules(holidays =>
                    {

                        holidays.Transform(x => x.Name, v => v?.Trim())
                                    .NotEmpty();

                        holidays.RuleFor(x => x.Date)
                                    .NotEmpty()
                                    .MustBeDate();

                        holidays.RuleFor(x => x.DayType)
                                    .NotEmpty()
                                    .MustBeIn(Enumeration.GetAll<HolidayDayType>().Select(x => x.Id).ToList());

                        holidays.RuleFor(x => x.Type)
                                    .NotEmpty()
                                    .MustBeIn(Enumeration.GetAll<HolidayType>().Select(x => x.Id).ToList());
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

                Transform(x => x.Name, v => v?.Trim ())
                    .NotEmpty();

                RuleFor(x => x.Date)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.DayType)
                    .NotEmpty()
                    .MustBeIn(Enumeration.GetAll<HolidayDayType>().Select(x => x.Id).ToList());

                RuleFor(x => x.Type)
                    .NotEmpty()
                    .MustBeIn(Enumeration.GetAll<HolidayType>().Select(x => x.Id).ToList());
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Holidays.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
