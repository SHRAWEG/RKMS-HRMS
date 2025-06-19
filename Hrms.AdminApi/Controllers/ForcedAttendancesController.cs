//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace Hrms.AdminApi.Controllers
//{
//    [Route("[controller]")]
//    [ApiController]
//        [Authorize(Roles = "super-admin, admin")]
//    public class ForcedAttendancesController : Controller
//    {
//        private readonly DataContext _context;

//        public ForcedAttendancesController(DataContext context)
//        {
//            _context = context;
//        }

//        // GET: GetMisPunchDetials
//        [HttpGet]
//        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, int? regularisationTypeId, int? empId, string fromDate, string toDate)
//        {
//            var query = _context.ForcedAttendances
//                .Include(x => x.RegularisationType)
//                .Join(_context.EmpDetails,
//                    o => o.EmpId,
//                    ed => ed.Id,
//                    (o, ed) => new
//                    {
//                        o.Id,
//                        o.EmpId,
//                        EmpCode = ed.CardId,
//                        Name = FullName(ed.FirstName!, ed.MiddleName!, ed.LastName!),
//                        o.RegularisationTypeId,
//                        RegularisationTypeName = o.RegularisationType.Name,
//                        o.Date,
//                        o.Status,
//                        o.ApprovedById,
//                        ApprovedByUserName = o.ApprovedBy != null ? o.ApprovedBy.UserName : "",
//                        RequestDate = o.TransactionDate
//                    }
//                )
//                .AsQueryable();

//            if (regularisationTypeId != null)
//            {
//                query = query.Where(x => x.RegularisationTypeId == regularisationTypeId);
//            }

//            if (empId != null)
//            {
//                query = query.Where(x => x.EmpId == empId);
//            }

//            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
//            {
//                DateOnly FromDate = DateOnly.ParseExact(fromDate, "yyyy-MM-dd");
//                DateOnly ToDate = DateOnly.ParseExact(toDate, "yyyy-MM-dd");

//                query = query.Where(b => b.Date >= FromDate && b.Date <= ToDate);
//            }

//            if (sortDirection == null)
//            {
//                query = query.OrderByDescending(p => p.Id);
//            }
//            else if (sortDirection == "asc")
//            {
//                switch (sortColumn)
//                {
//                    case "EmpCode":
//                        query.OrderBy(x => x.EmpCode);
//                        break;

//                    case "RegularisationTypeName":
//                        query.OrderBy(x => x.RegularisationTypeId);
//                        break;

//                    case "RequestDate":
//                        query.OrderBy(x => x.RequestDate);
//                        break;

//                    case "Date":
//                        query.OrderBy(x => x.Date);
//                        break;

//                    default:
//                        query.OrderBy(x => x.Id);
//                        break;
//                }
//            }
//            else
//            {
//                switch (sortColumn)
//                {
//                    case "EmpCode":
//                        query.OrderByDescending(x => x.EmpCode);
//                        break;

//                    case "RegularisationTypeName":
//                        query.OrderByDescending(x => x.RegularisationTypeId);
//                        break;

//                    case "RequestDate":
//                        query.OrderByDescending(x => x.RequestDate);
//                        break;

//                    case "Date":
//                        query.OrderByDescending(x => x.Date);
//                        break;

//                    default:
//                        query.OrderByDescending(x => x.Id);
//                        break;
//                }
//            }

//            var TotalCount = await query.CountAsync();
//            var TotalPages = (int)Math.Ceiling(TotalCount / (double)page);
//            var data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

//            return Ok(new
//            {
//                Data = data,
//                TotalCount,
//                TotalPages
//            });
//        }

//        // Post: ForcedAttendance/SignIn
//        [HttpPost("SignIn")]
//        public async Task<IActionResult> SignIn(AddInputModel input)
//        {
//            //OutDate is From Date and InDate is To Date for Patanjali

//            DateOnly date = DateOnly.ParseExact(input.Date, "yyyy-MM-dd");

//            if (await _context.ForcedAttendances.AnyAsync(x => x.Date == date && x.EmpId == input.EmpId) || 
//                await _context.OfficeOuts.AnyAsync(x => date >= x.OutDate && date <= x.InDate && x.EmpId == input.EmpId))
//            {
//                return ErrorHelper.ErrorResult("Date", "Regularisation in this date already exists.");
//            }

//            var attendance = await _context.Attendances.Where(x => x.TransactionDateOut == date && x.EmpId == input.EmpId).FirstOrDefaultAsync();

//            if (attendance is null)
//            {
//                return ErrorHelper.ErrorResult("Date", "User has not Punched Out in the given date.");
//            }

//            if (attendance.InTime != null)
//            {
//                return ErrorHelper.ErrorResult("Date", "User has already Punched In in the given date.");
//            }

//            ForcedAttendance data= new()
//            {
//                RegularisationTypeId = input.RegularisationTypeId,
//                EmpId = input.EmpId,
//                Date = date,
//                Time = input.Time,
//                ContactNumber = input.ContactNumber,
//                Remarks = input.Remarks,
//                ApprovedById = User.GetUserId(),
//                TransactionUser = User.GetUsername(),

//                //Defaults
//                Status = "approved",
//                ModeNM = "Sign In",
//                Mode = 0,
//                Flag = 0,
//                TimeVal = 0,

//            };

//            attendance.RegularisationTypeId = input.RegularisationTypeId;
//            attendance.InTime = input.Time;
//            attendance.TransactionDate = date;
//            attendance.InRemarks = input.Remarks;

//            await _context.SaveChangesAsync();
            
//            return Ok();
//        }


//        // Post: ForcedAttendance/SignOut
//        [HttpPost("SignOut")]
//        public async Task<IActionResult> SignOut(AddInputModel input)
//        {
//            //OutDate is From Date and InDate is To Date for Patanjali

//            DateOnly date = DateOnly.ParseExact(input.Date, "yyyy-MM-dd");

//            if (await _context.ForcedAttendances.AnyAsync(x => x.Date == date && x.EmpId == input.EmpId) ||
//                await _context.OfficeOuts.AnyAsync(x => date >= x.OutDate && date <= x.InDate && x.EmpId == input.EmpId))
//            {
//                return ErrorHelper.ErrorResult("Date", "Regularisation in this date already exists.");
//            }

//            var attendance = await _context.Attendances.Where(x => x.TransactionDate == date && x.EmpId == input.EmpId).FirstOrDefaultAsync();

//            if (attendance is null)
//            {
//                return ErrorHelper.ErrorResult("Date", "User has not Punched In in the given date.");
//            }

//            if (attendance.OutTime != null)
//            {
//                return ErrorHelper.ErrorResult("Date", "User has already Punched Out in the given date.");
//            }

//            ForcedAttendance data = new()
//            {
//                RegularisationTypeId = input.RegularisationTypeId,
//                EmpId = input.EmpId,
//                Date = date,
//                Time = input.Time,
//                ContactNumber = input.ContactNumber,
//                Remarks = input.Remarks,
//                ApprovedById = User.GetUserId(),
//                TransactionUser = User.GetUsername(),

//                //Defaults
//                Status = "approved",
//                ModeNM = "Sign Out",
//                Mode = 0,
//                Flag = 0,
//                TimeVal = 0,

//            };

//            attendance.RegularisationTypeId = input.RegularisationTypeId;
//            attendance.OutTime = input.Time;
//            attendance.TransactionDateOut = date;
//            attendance.OutRemarks = input.Remarks;
            
//            await _context.SaveChangesAsync();

//            return Ok();
//        }

//        private static string FullName(string firstName, string middleName, string lastName)
//        {
//            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
//        }


//        public class BaseInputModel
//        {
//            public int EmpId { get; set; }
//            public string Date { get; set; }
//            public string Time { get; set; }
//            public string ContactNumber { get; set; }
//            public string Remarks { get; set; }
//            public int RegularisationTypeId { get; set; }
//        }

//        public class AddInputModel : BaseInputModel { }

//        public class UpdateInputModel : BaseInputModel { }

//        public class AddInputModelValidator : AbstractValidator<AddInputModel>
//        {
//            private readonly DataContext _context;

//            public AddInputModelValidator(DataContext context)
//            {
//                _context = context;

//                RuleFor(x => x.EmpId)
//                    .NotEmpty()
//                    .IdMustExist(_context.EmpDetails.AsQueryable());

//                RuleFor(x => x.Date)
//                    .NotEmpty()
//                    .MustBeDate();

//                RuleFor(x => x.Time)
//                    .NotEmpty()
//                    .MustBeTime();

//                RuleFor(x => x.Remarks)
//                    .NotEmpty();

//                RuleFor(x => x.ContactNumber)
//                    .MustBeDigits(10)
//                    .Unless(x => string.IsNullOrEmpty(x.ContactNumber));

//                RuleFor(x => x.RegularisationTypeId)
//                    .NotEmpty()
//                    .IdMustExist(_context.RegularisationTypes.AsQueryable());
//            }
//        }

//        public class UpdateInputModelValidator : AbstractValidator<UpdateInputModel>
//        {
//            private readonly DataContext _context;
//            private readonly string? _id;

//            public UpdateInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
//            {
//                _context = context;
//                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

//                RuleFor(x => x.EmpId)
//                    .NotEmpty()
//                    .IdMustExist(_context.EmpDetails.AsQueryable());

//                RuleFor(x => x.Date)
//                    .NotEmpty()
//                    .MustBeDate();

//                RuleFor(x => x.Time)
//                    .NotEmpty()
//                    .MustBeTime();

//                RuleFor(x => x.Remarks)
//                    .NotEmpty();

//                RuleFor(x => x.ContactNumber)
//                    .MustBeDigits(10)
//                    .Unless(x => string.IsNullOrEmpty(x.ContactNumber));

//                RuleFor(x => x.RegularisationTypeId)
//                    .NotEmpty()
//                    .IdMustExist(_context.RegularisationTypes.AsQueryable());
//            }

//            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
//            {
//                if (_context.ForcedAttendances.Find(int.Parse(_id)) == null)
//                {
//                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
//                    return false;
//                }

//                return true;
//            }
//        }
//    }
//}

