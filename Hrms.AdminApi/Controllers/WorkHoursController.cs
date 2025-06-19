using NPOI.OpenXmlFormats.Wordprocessing;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class WorkHoursController : Controller
    {
        private readonly DataContext _context;

        public WorkHoursController(DataContext context)
        {
            _context = context;
        }

        // GET: WorkHours
        [CustomAuthorize("list-shift")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection,
            string name, string startTime, string endTime, string halfDayStartTime, string halfDayEndTime, int? flexiDuration, int? lateInGraceTime)
        {
            var query = _context.WorkHours.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrEmpty(startTime))
            {
                query = query.Where(b => b.StartTime!.ToLower().Contains(startTime.ToLower()));
            }

            if (!string.IsNullOrEmpty(endTime))
            {
                query = query.Where(b => b.EndTime!.ToLower().Contains(endTime.ToLower()));
            }

            if (!string.IsNullOrEmpty(halfDayStartTime))
            {
                query = query.Where(b => b.HalfDayStartTime!.ToLower().Contains(halfDayStartTime.ToLower()));
            }

            if (!string.IsNullOrEmpty(halfDayEndTime))
            {
                query = query.Where(b => b.HalfDayEndTime!.ToLower().Contains(halfDayEndTime.ToLower()));
            }

            if (flexiDuration is not null)
            {
                query = query.Where(b => b.FlexiDuration! == flexiDuration);
            }

            if (lateInGraceTime is not null)
            {
                query = query.Where(b => b.LateInGraceTime! == lateInGraceTime);
            }

            Expression<Func<WorkHour, object>> field = sortColumn switch
            {
                "Name" => x => x.Name!,
                "StartTime" => x => x.StartTime!,
                "EndTime" => x => x.EndTime!,
                "HalfDayStartTime" => x => x.HalfDayStartTime!,
                "HalfDayEndTime" => x => x.HalfDayEndTime!,
                "FlexiDuration" => x => x.FlexiDuration!,
                "LateInGraceTime" => x => x.LateInGraceTime!,
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

            var data = await PagedList<WorkHour>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.StartTime,
                    x.EndTime,
                    x.HalfDayStartTime,
                    x.HalfDayEndTime,
                    x.FlexiDuration,
                    x.LateInGraceTime
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // GETALL: WorkHours
        [CustomAuthorize("search-shift")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.WorkHours.ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name,
                })
            });
        }

        // GET: WorkHours/5
        [CustomAuthorize("view-shift")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(short id)
        {
            var data = await _context.WorkHours
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                WorkHour = data
            });
        }

        // Post: WorkHours/Create
        [CustomAuthorize("write-shift")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            bool isNightShift = false;

            if (TimeOnly.Parse(input.StartTime) > TimeOnly.Parse(input.EndTime))
            {
                isNightShift = true;
            }

            WorkHour data;

            if (input.IsFlexible)
            {
                if (input.IsNightShift)
                {
                    data = new()
                    {
                        IsFlexible = input.IsFlexible,
                        Name = input.Name,
                        StartTime = input.StartTime,
                        EndTime = input.EndTime,
                        MinHalfDayTime = input.MinHalfDayTime,
                        MinDutyTime = input.MinDutyTime,
                        IsNightShift = true,
                        NightStartTime = input.NightStartTime,
                        NightEndTime = input.NightEndTime,

                        //TimeTrax
                        TotalHour = "N/A",
                        WorkType = 0,
                        WorkDayCount = 0,
                        InStartGrace = 0,
                        InEndGrace = 0,
                        OutStartGrace = 0,
                        OutEndGrace = 0,
                        STMVAL = 0,
                        ETMVAL = 0,
                        LockIn = 0,
                        LockOut = 0,
                        LockLunch = 0
                    };
                } else
                {
                    data = new()
                    {
                        IsFlexible = input.IsFlexible,
                        Name = input.Name,
                        StartTime = input.StartTime,
                        EndTime = input.EndTime,
                        MinHalfDayTime = input.MinHalfDayTime,
                        MinDutyTime = input.MinDutyTime,
                        IsNightShift = false,

                        //TimeTrax
                        TotalHour = "N/A",
                        WorkType = 0,
                        WorkDayCount = 0,
                        InStartGrace = 0,
                        InEndGrace = 0,
                        OutStartGrace = 0,
                        OutEndGrace = 0,
                        STMVAL = 0,
                        ETMVAL = 0,
                        LockIn = 0,
                        LockOut = 0,
                        LockLunch = 0
                    };
                }
                
            }
            else
            {
                data = new()
                {
                    IsFlexible = input.IsFlexible,
                    Name = input.Name,
                    StartTime = input.StartTime,
                    EndTime = input.EndTime,
                    HalfDayStartTime = input.HalfDayStartTime,
                    HalfDayEndTime = input.HalfDayEndTime,
                    FlexiDuration = input.FlexiDuration,
                    LateInGraceTime = input.LateInGraceTime,
                    IsEarlyGoingButNoOt = input.IsEarlyGoingButNoOt,
                    IsNightShift = isNightShift,

                    //TimeTrax
                    TotalHour = "N/A",
                    WorkType = 0,
                    WorkDayCount = 0,
                    InStartGrace = 0,
                    InEndGrace = 0,
                    OutStartGrace = 0,
                    OutEndGrace = 0,
                    STMVAL = 0,
                    ETMVAL = 0,
                    LockIn = 0,
                    LockOut = 0,
                    LockLunch = 0
                };
            }

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [CustomAuthorize("make-default-shift")]
        [HttpPost("MakeDefault/{id}")]
        public async Task<IActionResult> MakeDefault(short id)
        {
            if (!await _context.WorkHours.AnyAsync(x => x.Id == id))
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            var defaultWorkHours = await _context.DefaultWorkHours.Where(x => x.EmpId == 0).ToListAsync();

            if (defaultWorkHours.Any(x => x.WorkHourId == id)) return Ok();

            //if(defaultWorkHours != null)
            //{
            //    foreach(var defaultWorkHour in defaultWorkHours)
            //    {
            //        defaultWorkHour.WorkHourId = id;
            //        defaultWorkHour.UpdatedAt = DateTime.UtcNow;
            //    }
            //} else
            //{
            List<DefaultWorkHour> data = new();

            if (defaultWorkHours.Count > 0)
            {
                for (short i = 1; i < 8; i++)
                {
                    var defaultWorkHour = defaultWorkHours.Where(x => x.DayId == i).SingleOrDefault();

                    defaultWorkHour.WorkHourId = id;
                    defaultWorkHour.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                for (short i = 1; i < 8; i++)
                {
                    data.Add(new DefaultWorkHour
                    {
                        DayId = i,
                        WorkHourId = id,
                    });
                }

                _context.AddRange(data);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: WorkHours/5
        [CustomAuthorize("update-shift")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(short id, UpdateInputModel input)
        {
            var data = await _context.WorkHours.FirstOrDefaultAsync(c => c.Id == id);

            if (input.IsFlexible)
            {
                if (input.IsNightShift)
                {
                    data.IsFlexible = input.IsFlexible;
                    data.IsNightShift = input.IsNightShift;
                    data.NightStartTime = input.NightStartTime;
                    data.NightEndTime = input.NightEndTime;
                    data.Name = input.Name;
                    data.StartTime = input.StartTime;
                    data.EndTime = input.EndTime;
                    data.MinHalfDayTime = input.MinHalfDayTime;
                    data.MinDutyTime = input.MinDutyTime;
                    data.HalfDayStartTime = null;
                    data.HalfDayEndTime = null;
                    data.FlexiDuration = null;
                    data.LateInGraceTime = null;
                    data.IsEarlyGoingButNoOt = null;
                    data.UpdatedAt = DateTime.UtcNow;
                } else
                {
                    data.IsFlexible = input.IsFlexible;
                    data.IsNightShift = false;
                    data.NightStartTime = null;
                    data.NightEndTime = null;
                    data.Name = input.Name;
                    data.StartTime = input.StartTime;
                    data.EndTime = input.EndTime;
                    data.MinHalfDayTime = input.MinHalfDayTime;
                    data.MinDutyTime = input.MinDutyTime;
                    data.HalfDayStartTime = null;
                    data.HalfDayEndTime = null;
                    data.FlexiDuration = null;
                    data.LateInGraceTime = null;
                    data.IsEarlyGoingButNoOt = null;
                    data.UpdatedAt = DateTime.UtcNow;
                }
            } else
            {
                bool isNightShift = false;

                if (TimeOnly.Parse(input.StartTime) > TimeOnly.Parse(input.EndTime))
                {
                    isNightShift = true;
                }

                data.IsFlexible = input.IsFlexible;
                data.Name = input.Name;
                data.StartTime = input.StartTime;
                data.EndTime = input.EndTime;
                data.HalfDayStartTime = input.HalfDayStartTime;
                data.HalfDayEndTime = input.HalfDayEndTime;
                data.FlexiDuration = input.FlexiDuration;
                data.LateInGraceTime = input.LateInGraceTime;
                data.MinHalfDayTime = null;
                data.MinDutyTime = null;
                data.IsEarlyGoingButNoOt = input.IsEarlyGoingButNoOt;
                data.IsNightShift = isNightShift;
                data.NightStartTime = null;
                data.NightEndTime = null;
                data.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: WorkHours/5
        [CustomAuthorize("delete-shift")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(short id)
        {
            var data = await _context.WorkHours.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.DefaultWorkHours.AnyAsync(x => x.WorkHourId == id) || await _context.Rosters.AnyAsync(x => x.WorkHourId == id))
            {
                return ErrorHelper.ErrorResult("Id", "WorkHour is already in use.");
            }

            if (await _context.Attendances.AnyAsync(x => x.WorkHourId == id))
            {
                return ErrorHelper.ErrorResult("Id", "WorkHour is already in use.");
            }

            _context.WorkHours.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public bool IsFlexible { get; set; }
            public string Name { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string HalfDayStartTime { get; set; }
            public string HalfDayEndTime { get; set; }
            public int? FlexiDuration { get; set; }
            public int? LateInGraceTime { get; set; }
            public int? MinHalfDayTime { get; set; }
            public int? MinDutyTime { get; set; }
            public bool IsEarlyGoingButNoOt { get; set; }
            public bool IsNightShift { get; set; }
            public string NightStartTime { get; set; }
            public string NightEndTime { get; set; }
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
                    .NotEmpty()
                    .MustBeUnique(_context.WorkHours.AsQueryable(), "Name");

                RuleFor(x => x.StartTime)
                    .NotEmpty()
                    .MustBeTime();

                RuleFor(x => x.EndTime)
                    .NotEmpty()
                    .MustBeTime();

                RuleFor(x => x.HalfDayEndTime)
                    .NotEmpty()
                    .MustBeTime()
                    .Unless(x => x.IsFlexible);

                RuleFor(x => x.HalfDayStartTime)
                    .NotEmpty()
                    .MustBeTime()
                    .Unless(x => x.IsFlexible);

                RuleFor(x => x.MinHalfDayTime)
                    .NotEmpty()
                    .Unless(x => !x.IsFlexible);

                RuleFor(x => x.MinDutyTime)
                    .NotEmpty()
                    .Unless(x => !x.IsFlexible);

                RuleFor(x => x.NightStartTime)
                    .NotEmpty()
                    .MustBeTime()
                    .Unless(x => !x.IsNightShift);

                RuleFor(x => x.NightEndTime)
                    .NotEmpty()
                    .MustBeTime()
                    .Unless(x => !x.IsNightShift);
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
                    .NotEmpty()
                    .MustBeUnique(_context.WorkHours.Where(x => x.Id != short.Parse(_id)).AsQueryable(), "Name");

                RuleFor(x => x.StartTime)
                    .NotEmpty()
                    .MustBeTime();

                RuleFor(x => x.EndTime)
                    .NotEmpty()
                    .MustBeTime();

                RuleFor(x => x.HalfDayEndTime)
                    .NotEmpty()
                    .MustBeTime()
                    .Unless(x => x.IsFlexible);

                RuleFor(x => x.HalfDayStartTime)
                    .NotEmpty()
                    .MustBeTime()
                    .Unless(x => x.IsFlexible);

                RuleFor(x => x.MinHalfDayTime)
                    .NotEmpty()
                    .Unless(x => !x.IsFlexible);

                RuleFor(x => x.MinDutyTime)
                    .NotEmpty()
                    .Unless(x => !x.IsFlexible);

                RuleFor(x => x.NightStartTime)
                    .NotEmpty()
                    .MustBeTime()
                    .Unless(x => !x.IsNightShift);

                RuleFor(x => x.NightEndTime)
                    .NotEmpty()
                    .MustBeTime()
                    .Unless(x => !x.IsNightShift);
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.WorkHours.Find(short.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
