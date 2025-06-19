//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace Hrms.AdminApi.Controllers
//{
//    [Route("[controller]")]
//    [ApiController]
//    [Authorize(Roles = "super-admin, admin")]

//    public class OfficeOutsController : Controller
//    {
//        private readonly DataContext _context;

//        public OfficeOutsController(DataContext context)
//        {
//            _context = context;
//        }

//        // GET: OfficeOuts
//        [HttpGet]
//        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, int? regularisationTypeId, int? empId, string fromDate, string toDate)
//        {
//            var query = _context.OfficeOuts
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
//                        o.OutDate,
//                        o.InDate,
//                        o.Status,
//                        o.ApprovedById,
//                        ApprovedByUserName = o.ApprovedBy != null ? o.ApprovedBy.UserName : "",
//                        RequestDate = o.CreatedAt
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
//                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);
//                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

//                query = query.Where(b => b.OutDate >= FromDate && b.InDate <= ToDate);
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

//                    case "OutDate":
//                        query.OrderBy(x => x.OutDate);
//                        break;

//                    case "InDate":
//                        query.OrderBy(x => x.InDate);
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

//                    case "OutDate":
//                        query.OrderByDescending(x => x.OutDate);
//                        break;

//                    case "InDate":
//                        query.OrderByDescending(x => x.InDate);
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

//        // Post: OfficeOuts/Create
//        [HttpPost]
//        public async Task<IActionResult> Create(AddInputModel input)
//        {
//            //OutDate is From Date and InDate is To Date for Patanjali

//            DateOnly outDate = DateOnlyHelper.ParseDateOrNow(input.OutDate);
//            DateOnly inDate = DateOnlyHelper.ParseDateOrNow(input.InDate);

//            if (await _context.ForcedAttendances.AnyAsync(x => x.Date >= outDate && x.Date <= inDate && x.EmpId == input.EmpId) || 
//                await _context.OfficeOuts.AnyAsync(x => x.OutDate <= inDate && x.InDate >= outDate && x.EmpId == input.EmpId))
//            {
//                return ErrorHelper.ErrorResult("OutDate", "Regularisation in this date already exists.");
//            }

//            if (await _context.Attendances.AnyAsync(x => 
//                            (x.TransactionDate >= outDate || x.TransactionDateOut >= outDate) &&
//                            (x.TransactionDate <= inDate || x.TransactionDateOut <= inDate) &&
//                            x.EmpId == input.EmpId)
//                )
//            {
//                return ErrorHelper.ErrorResult("OutDate", "User already has attendance somewhere between the given dates.");
//            }

//            OfficeOut data= new()
//            {
//                EmpId = input.EmpId,
//                RegularisationTypeId = input.RegularisationTypeId,
//                OutDate = outDate,
//                InDate = inDate,
//                OutTime = input.OutTime,
//                InTime = input.InTime,
//                ContactNumber = input.ContactNumber,
//                Place = input.Place,
//                Reason = input.Reason,
//                Status = "approved",
//                ApprovedById = User.GetUserId()
//            };

//            List<Attendance> officeOutAttendances = new();

//            do
//            {
//                officeOutAttendances.Add(new Attendance
//                {
//                    RegularisationTypeId = input.RegularisationTypeId,
//                    EmpId = input.EmpId,
//                    TransactionDate = outDate,
//                    TransactionDateOut = outDate,
//                    InTime = input.OutTime,
//                    OutTime = input.InTime,
                        
//                    //Defaults
//                    AttendanceStatus = 0,
//                    FlagIn = false,
//                    FlagOut = false,
//                    AttendanceType = 0,
//                    CheckInMode = 'N',
//                    AttendanceId = 0,
//                    SignOutTimeStamp = 0,
//                    SignInTimeStamp = 0,
//                });

//                outDate = outDate.AddDays(1);

//            } while (outDate <= inDate);


//            _context.Add(data);
//            _context.AddRange(officeOutAttendances);
//            await _context.SaveChangesAsync();
            
//            return Ok();
//        }

//        //// PUT: OfficeOuts/5
//        //[HttpPut("{id}")]
//        //public async Task<IActionResult> Edit(int id, UpdateInputModel input)
//        //{
//        //    var data = await _context.OfficeOuts.FirstOrDefaultAsync(c => c.Id == id);

//        //    data.Name = input.Name;

//        //    await _context.SaveChangesAsync();

//        //    return Ok();
//        //}

//        //// DELETE: OfficeOuts/5
//        //[HttpDelete("{id}")]
//        //public async Task<IActionResult> Delete(int id)
//        //{
//        //    var data = await _context.OfficeOuts.FindAsync(id);

//        //    if (data == null)
//        //    {
//        //        return ErrorHelper.ErrorResult("Id", "Id is invalid.");
//        //    }

//        //    _context.OfficeOuts.Remove(data);
//        //    await _context.SaveChangesAsync();

//        //    return Ok();
//        //}

//        private static string FullName(string firstName, string middleName, string lastName)
//        {
//            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
//        }

//        public class BaseInputModel
//        {
//            public int EmpId { get; set; }
//            public string OutDate { get; set; }
//            public string OutTime { get; set; }
//            public string InDate { get; set; }
//            public string InTime { get; set; }
//            public string ContactNumber { get; set; }
//            public string Place { get; set; }
//            public string Reason { get; set; }
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

//                RuleFor(x => x.OutDate)
//                    .NotEmpty()
//                    .MustBeDate();

//                RuleFor(x => x.InDate)
//                    .NotEmpty()
//                    .MustBeDate()
//                    .MustBeDateAfterOrEqual(x => x.OutDate, "Start Date");

//                RuleFor(x => x.InTime)
//                    .NotEmpty()
//                    .MustBeTime();

//                RuleFor(x => x.OutTime)
//                    .NotEmpty()
//                    .MustBeTime();

//                RuleFor(x => x.Reason)
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

//                RuleFor(x => x.OutDate)
//                    .NotEmpty()
//                    .MustBeDate();

//                RuleFor(x => x.InDate)
//                    .NotEmpty()
//                    .MustBeDate()
//                    .MustBeDateAfterOrEqual(x => x.OutDate, "Start Date");

//                RuleFor(x => x.InTime)
//                    .NotEmpty()
//                    .MustBeTime();

//                RuleFor(x => x.OutTime)
//                    .NotEmpty()
//                    .MustBeTime();

//                RuleFor(x => x.Reason)
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
//                if (_context.OfficeOuts.Find(int.Parse(_id)) == null)
//                {
//                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
//                    return false;
//                }

//                return true;
//            }
//        }
//    }
//}

