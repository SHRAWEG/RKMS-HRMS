using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class LeavesController : Controller
    {
        private readonly DataContext _context;

        private readonly byte zero = 0;
        private readonly byte one = 1;

        public LeavesController(DataContext context)
        {
            _context = context;
        }

        // GET: Leaves
        [CustomAuthorize("list-leave")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, 
            string name, int? days, byte? type, short? leaveMax, byte? isPaidLeave, 
            decimal? payQuantity, string abbreviation, byte? useLimit, byte? leaveEarn )
        {
            var query = _context.Leaves.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name.Contains(name));
            }

            if (days != null)
            {
                query = query.Where(b => b.Days == days);
            }

            if (type != null)
            {
                query = query.Where(b => b.Type == type);
            }

            if (leaveMax != null)
            {
                query = query.Where(b => b.LeaveMax == leaveMax);
            }

            if (isPaidLeave != null)
            {
                query = query.Where(b => b.IsPaidLeave == isPaidLeave);
            }

            if (payQuantity != null)
            {
                query = query.Where(b => b.PayQuantity == payQuantity);
            }

            if (!string.IsNullOrEmpty(abbreviation))
            {
                query = query.Where(b => b.Abbreviation.Contains(abbreviation));
            }

            if (useLimit != null)
            {
                query = query.Where(b => b.UseLimit == useLimit);
            }

            if (leaveEarn != null)
            {
                query = query.Where(b => b.LeaveEarnType == leaveEarn);
            }

            Expression<Func<Leave, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "Days" => x => x.Days,
                "Type" => x => x.Type,
                "LeaveMax" => x => x.LeaveMax,
                "IsPaidLeave" => x => x.IsPaidLeave,
                "PayQuantity" => x => x.PayQuantity,
                "Abbreviation" => x => x.Abbreviation,
                "UseLimit" => x => x.UseLimit,
                "LeaveEarnType" => x => x.LeaveEarnType,
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

            var data = await PagedList<Leave>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: Leaves/all
        [CustomAuthorize("search-leave")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Leaves.ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Days,
                    x.LeaveMax,
                    x.IsHalfLeave
                })
            });
        }

        // GET: Leaves/5
        [CustomAuthorize("view-leave")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(short id)
        {
            var data = await _context.Leaves
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                Leave = data
            });
        }

        // Post: Leaves/Create
        [CustomAuthorize("write-leave")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            Leave data = new()
            {
                Name = input.Name,
                Days = input.Days,
                Type = input.Type,
                LeaveMax = input.Type == 1 ? input.LeaveMax : 0, // Type == 1 is Accumulative, 2 is Expire Yearly
                IsPaidLeave = input.IsPaidLeave,
                PayQuantity = input.IsPaidLeave == 1 ? input.PayQuantity : 0,  // IsPaidLeave == 1 is PaidLeave, 0 is UnPaid Leave, PayQuantity should be value between 0 to 1 (Including 0.1, 0.2 so on...)
                Abbreviation = input.Abbreviation,
                UseLimit = (byte)(input.UseLimit ?? 0),
                LeaveEarnType = input.IsPaidLeave == 1 ? input.LeaveEarnType : zero, //  LeaveEarnType 0 is not earned leave, 1 is monthly basis, 2 is days basis.
                LeaveEarnDays = input.LeaveEarnType == 2 ? input.LeaveEarnDays : null,
                LeaveEarnQuantity = input.LeaveEarnType != 0 ? input.LeaveEarnQuantity : null,
                IsHalfLeave = input.IsHalfLeave
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: Leaves/5
        [CustomAuthorize("update-leave")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(short id, UpdateInputModel input)
        {
            var data = await _context.Leaves.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.Days = input.Days;
            data.Type = input.Type;
            data.LeaveMax = input.Type == 1 ? input.LeaveMax : 0;
            data.IsPaidLeave = input.IsPaidLeave;
            data.PayQuantity = input.IsPaidLeave == 1 ? input.PayQuantity : 0;
            data.Abbreviation = input.Abbreviation;
            data.UseLimit = (byte)(input.UseLimit ?? 0);
            data.LeaveEarnType = input.IsPaidLeave == 1 ? input.LeaveEarnType : zero;
            data.LeaveEarnDays = input.LeaveEarnType == 2 ? input.LeaveEarnDays : null;
            data.LeaveEarnQuantity = input.LeaveEarnType != 0 ? input.LeaveEarnQuantity : null;
            data.IsHalfLeave = input.IsHalfLeave;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Leaves/5
        [CustomAuthorize("delete-leave")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(short id)
        {
            var data = await _context.Leaves.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.EmpLeaves.AnyAsync(x => x.LeaveId == id) || await _context.LeaveLedgers.AnyAsync(x => x.LeaveId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Leave is already in use.");
            }

            _context.Leaves.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
            public int Days { get; set; }
            public byte Type { get; set; }
            public short? LeaveMax { get; set; }
            public byte IsPaidLeave { get; set; }
            public decimal PayQuantity { get; set; }
            public string Abbreviation { get; set; }
            public byte? UseLimit { get; set; }
            public byte LeaveEarnType { get; set; }
            public double? LeaveEarnDays { get; set; }
            public decimal? LeaveEarnQuantity { get; set; }
            public bool IsHalfLeave { get; set; }
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
                    .MustBeUnique(_context.Leaves.AsQueryable(), "Name");

                Transform(x => x.Abbreviation, v => v?.Trim())
                    .MustBeUnique(_context.Leaves.AsQueryable(), "Abbreviation");

                RuleFor(x => x.Days)
                    .NotEmpty();

                RuleFor(x => x.PayQuantity)
                    .NotEmpty()
                    .Unless(value => value.IsPaidLeave == 0)
                    .Must(value => value >= 0 && value <= 1);

                RuleFor(x => x.Type)
                    .NotEmpty()
                    .MustBeValues(new List<byte> { 1, 2 });

                RuleFor(x => x.IsPaidLeave)
                    .MustBeValues(new List<byte> { 0, 1 });

                RuleFor(x => x.LeaveEarnType)
                    .MustBeValues(new List<byte> { 0, 1, 2});

                RuleFor(x => x.LeaveEarnDays)
                    .NotEmpty()
                    .GreaterThan(0)
                    .Unless(value => value.LeaveEarnType != 2);

                RuleFor(x => x.LeaveEarnQuantity)
                    .NotEmpty()
                    .GreaterThan(0)
                    .Unless(value => value.LeaveEarnType == 0);

                RuleFor(x => x.LeaveMax)
                    .NotEmpty()
                    .Unless(value => value.Type != 1);
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
                    .MustBeUnique(_context.Leaves.Where(x => x.Id != short.Parse(_id)).AsQueryable(), "Name");

                Transform(x => x.Abbreviation, v => v?.Trim())
                    .MustBeUnique(_context.Leaves.Where(x => x.Id != short.Parse(_id)).AsQueryable(), "Abbreviation");

                RuleFor(x => x.Days)
                    .NotEmpty();

                RuleFor(x => x.PayQuantity)
                    .NotEmpty()
                    .Must(value => value >= 0 && value <= 1)
                    .Unless(value => value.IsPaidLeave == 0);

                RuleFor(x => x.Type)
                    .NotEmpty()
                    .MustBeValues(new List<byte> { 1, 2 });

                RuleFor(x => x.IsPaidLeave)
                    .MustBeValues(new List<byte> { 0, 1 });

                RuleFor(x => x.LeaveEarnType)
                    .MustBeValues(new List<byte> { 0, 1, 2 });

                RuleFor(x => x.LeaveEarnDays)
                    .NotEmpty()
                    .GreaterThan(0)
                    .Unless(value => value.LeaveEarnType != 2);

                RuleFor(x => x.LeaveEarnQuantity)
                    .NotEmpty()
                    .GreaterThan(0)
                    .Unless(value => value.LeaveEarnType == 0);

                RuleFor(x => x.LeaveMax)
                    .NotEmpty()
                    .Unless(value => value.Type != 1);
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Leaves.Find(short.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
