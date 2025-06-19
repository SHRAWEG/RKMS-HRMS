//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace Hrms.MobileApi.Controllers
//{
//    [Route("[controller]")]
//    [ApiController]
//    [Authorize(Roles = "employee")]
//    public class ForcedAttendancesController : Controller
//    {
//        private readonly DataContext _context;
//        private readonly UserManager<User> _userManager;

//        public ForcedAttendancesController(DataContext context, UserManager<User> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        // GET: GetMisPunchDetials
//        [HttpGet]
//        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, int? regularisationTypeId, string fromDate, string toDate)
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            var query = _context.ForcedAttendances
//                .Where(x => x.EmpId == user.EmpId)
//                .Include(x => x.RegularisationType)
//                .Include(x => x.ApprovedBy)
//                .Select(o => new
//                    {
//                        o.Id,
//                        o.EmpId,
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

//            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
//            {
//                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);
//                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

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

//        // GET: GetMisPunchDetials
//        [HttpGet("SubordinateHistory")]
//        public async Task<IActionResult> GetSubordinateHistory(int page, int limit, string sortColumn, string sortDirection, int? regularisationTypeId, string fromDate, string toDate)
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            var subordinateIds = await _context.EmpLogs
//                .Join(_context.EmpTransactions.Where(x => x.RmEmpId == user.EmpId || x.HodEmpId == user.EmpId),
//                    el => el.Id,
//                    et => et.Id,
//                    (el, et) => new
//                    {
//                        PId = el.Id,
//                        EmpId = el.EmployeeId,
//                    }
//                )
//                .Select(x => x.EmpId)
//                .ToListAsync();

//            var query = _context.ForcedAttendances
//                .Where(x => subordinateIds.Contains(x.EmpId))
//                .Include(x => x.RegularisationType)
//                .Include(x => x.ApprovedBy)
//                .Select(o => new
//                {
//                    o.Id,
//                    o.EmpId,
//                    o.RegularisationTypeId,
//                    RegularisationTypeName = o.RegularisationType.Name,
//                    o.Date,
//                    o.Status,
//                    o.ApprovedById,
//                    ApprovedByUserName = o.ApprovedBy != null ? o.ApprovedBy.UserName : "",
//                    RequestDate = o.TransactionDate
//                }
//                )
//                .AsQueryable();

//            if (regularisationTypeId != null)
//            {
//                query = query.Where(x => x.RegularisationTypeId == regularisationTypeId);
//            }

//            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
//            {
//                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);
//                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

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

//        [HttpGet("SubordinateRequests")]
//        public async Task<IActionResult> SubordinateRequests()
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            var subordinateIds = await _context.EmpLogs
//                .Join(_context.EmpTransactions.Where(x => x.RmEmpId == user.EmpId || x.HodEmpId == user.EmpId),
//                    el => el.Id,
//                    et => et.Id,
//                    (el, et) => new
//                    {
//                        PId = el.Id,
//                        EmpId = el.EmployeeId,
//                    }
//                )
//                .Select(x => x.EmpId)
//                .ToListAsync();

//            var data = await _context.ForcedAttendances
//                .Where(x => subordinateIds.Contains(x.EmpId) && x.Status == "pending")
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
//                    }
//                ).ToListAsync();

//            return Ok(new
//            {
//                TotalCount = data.Count,
//                Data = data
//            });
//        }

//        // Post: ForcedAttendance/SignIn
//        [HttpPost("SignIn")]
//        public async Task<IActionResult> SignIn(AddInputModel input)
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            //OutDate is From Date and InDate is To Date for Patanjali

//            DateOnly date = DateOnlyHelper.ParseDateOrNow(input.Date);

//            if (await _context.ForcedAttendances.AnyAsync(x => x.Date == date && x.EmpId == user.EmpId) ||
//                await _context.OfficeOuts.AnyAsync(x => date >= x.OutDate && date <= x.InDate && x.EmpId == user.EmpId))
//            {
//                return ErrorHelper.ErrorResult("Date", "Regularisation in this date already exists.");
//            }

//            var attendance = await _context.Attendances.Where(x => x.TransactionDateOut == date && x.EmpId == user.EmpId).FirstOrDefaultAsync();

//            if (attendance is null)
//            {
//                return ErrorHelper.ErrorResult("Date", "You have not Punched Out in the given date.");
//            }

//            if (attendance.InTime != null)
//            {
//                return ErrorHelper.ErrorResult("Date", "You have already Punched In in the given date.");
//            }

//            ForcedAttendance data= new()
//            {
//                RegularisationTypeId = input.RegularisationTypeId,
//                EmpId = user.EmpId ?? 0,
//                Date = date,
//                Time = input.Time,
//                ContactNumber = input.ContactNumber,
//                Remarks = input.Remarks,

//                //Defaults
//                TransactionUser = "",
//                Status = "pending",
//                ModeNM = "Sign In",
//                Mode = 0,
//                Flag = 0,
//                TimeVal = 0,
//            };

//            _context.Add(data);
//            await _context.SaveChangesAsync();
            
//            return Ok();
//        }


//        // Post: ForcedAttendance/SignOut
//        [HttpPost("SignOut")]
//        public async Task<IActionResult> SignOut(AddInputModel input)
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            //OutDate is From Date and InDate is To Date for Patanjali

//            DateOnly date = DateOnlyHelper.ParseDateOrNow(input.Date);

//            if (await _context.ForcedAttendances.AnyAsync(x => x.Date == date && x.EmpId == user.EmpId) ||
//                await _context.OfficeOuts.AnyAsync(x => date >= x.OutDate && date <= x.InDate && x.EmpId == user.EmpId))
//            {
//                return ErrorHelper.ErrorResult("Date", "Regularisation in this date already exists.");
//            }

//            var attendance = await _context.Attendances.Where(x => x.TransactionDateOut == date && x.EmpId == user.EmpId).FirstOrDefaultAsync();

//            if (attendance is null)
//            {
//                return ErrorHelper.ErrorResult("Date", "You have not Punched Out in the given date.");
//            }

//            if (attendance.InTime != null)
//            {
//                return ErrorHelper.ErrorResult("Date", "You have already Punched In in the given date.");
//            }

//            ForcedAttendance data = new()
//            {
//                RegularisationTypeId = input.RegularisationTypeId,
//                EmpId = user.EmpId ?? 0,
//                Date = date,
//                Time = input.Time,
//                ContactNumber = input.ContactNumber,
//                Remarks = input.Remarks,

//                //Defaults
//                TransactionUser = "",
//                Status = "pending",
//                ModeNM = "Sign Out",
//                Mode = 0,
//                Flag = 0,
//                TimeVal = 0,

//            };

//            _context.Add(data);
//            await _context.SaveChangesAsync();

//            return Ok();
//        }

//        // Get: FocedAttendances/{id}
//        [HttpGet("{id}")]
//        public async Task<IActionResult> Get(int id)
//        {
//            var authUser = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            var data = await _context.ForcedAttendances
//                .Where(x => x.Id == id)
//                .Include(x => x.RegularisationType)
//                .Include(x => x.ApprovedBy)
//                .Select(o => new
//                {
//                    o.Id,
//                    o.EmpId,
//                    o.RegularisationTypeId,
//                    RegularisationTypeName = o.RegularisationType.Name,
//                    o.Date,
//                    o.Time,
//                    o.ContactNumber,
//                    o.Remarks,
//                    o.Status,
//                    o.ApprovedById, 
//                    ApprovedByUserName = o.ApprovedBy != null ? o.ApprovedBy.UserName : "",
//                }
//                )
//                .SingleOrDefaultAsync();

//            if (data.EmpId != authUser.EmpId)
//            {
//                return Unauthorized("Invalid User");
//            }

//            var user = await (from detail in _context.EmpDetails
//                              where detail.Id == data.EmpId
//                              join log in _context.EmpLogs on detail.Id equals log.EmployeeId into logs
//                              from log in logs.DefaultIfEmpty()
//                              join tran in _context.EmpTransactions on log.Id equals tran.Id into trans
//                              from tran in trans.DefaultIfEmpty()
//                              select new
//                              {
//                                  PId = log != null ? log.Id : 0,
//                                  EmpId = detail.Id,
//                                  EmpCode = detail.CardId,
//                                  Name = FullName(detail.FirstName, detail.MiddleName, detail.LastName),
//                                  BranchId = tran != null ? tran.BranchId : 0,
//                                  BranchName = tran != null ? (tran.Branch != null ? tran.Branch.Name : "") : "",
//                                  DepartmentId = tran != null ? tran.DepartmentId : 0,
//                                  DepartmentName = tran != null ? (tran.Department != null ? tran.Department.Name : "") : "",
//                                  DesignationId = tran != null ? tran.DesignationId : 0,
//                                  DesignationName = tran != null ? (tran.Designation != null ? tran.Designation.Name : "") : "",
//                                  detail.JoinDate,
//                              }
//                            ).SingleOrDefaultAsync();

//            return Ok(new
//            {
//                Data = data,
//                User = user,
//            });
//        }

//        [HttpPost("Approve/{id}")]
//        public async Task<IActionResult> Approve(int id)
//        {
//            var forcedAttendance = await _context.ForcedAttendances
//                .Include(x => x.RegularisationType)
//                .Where(x => x.Id == id && x.Status == "pending").SingleOrDefaultAsync();

//            if (forcedAttendance == null)
//            {
//                return ErrorHelper.ErrorResult("Id", "Id is invalid");
//            }

//            //OutDate is From Date and InDate is To Date for Patanjali

//            DateOnly date = forcedAttendance.Date;

//            var attendance = await _context.Attendances.Where(x => x.TransactionDate == date && x.EmpId == forcedAttendance.EmpId).SingleOrDefaultAsync();

//            if (attendance is null)
//            {
//                forcedAttendance.Status = "disapproved";
//                forcedAttendance.TransactionUser = "System";
//                forcedAttendance.TransactionDate = DateTime.UtcNow;

//                return ErrorHelper.ErrorResult("Id", "No attendance in this date.");
//            };

//            if (forcedAttendance.RegularisationType.Name == "In Punch Regularisation")
//            {
//                attendance.RegularisationTypeId = forcedAttendance.RegularisationTypeId;
//                attendance.InTime = forcedAttendance.Time;
//                attendance.TransactionDate = date;
//                attendance.InRemarks = forcedAttendance.Remarks;
//            } else
//            {
//                attendance.RegularisationTypeId = forcedAttendance.RegularisationTypeId;
//                attendance.OutTime = forcedAttendance.Time;
//                attendance.TransactionDateOut = date;
//                attendance.OutRemarks = forcedAttendance.Remarks;
//            }

//            forcedAttendance.Status = "approved";
//            forcedAttendance.ApprovedById = User.GetUserId();
//            forcedAttendance.TransactionUser = User.GetUsername();
//            forcedAttendance.TransactionDate = DateTime.UtcNow;
            
//            await _context.SaveChangesAsync();

//            return Ok();
//        }

//        [HttpPost("Disapprove/{id}")]
//        public async Task<IActionResult> Disapprove(int id)
//        {
//            var forcedAttendance = await _context.ForcedAttendances.Where(x => x.Id == id && x.Status == "pending").SingleOrDefaultAsync();

//            if (forcedAttendance == null)
//            {
//                return ErrorHelper.ErrorResult("Id", "Id is invalid");
//            }

//            forcedAttendance.Status = "disapproved";
//            forcedAttendance.DisapprovedById = User.GetUserId();
//            forcedAttendance.TransactionUser = User.GetUsername();
//            forcedAttendance.TransactionDate = DateTime.UtcNow;

//            await _context.SaveChangesAsync();

//            return Ok();
//        }

//        private static string FullName(string firstName, string middleName, string lastName)
//        {
//            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
//        }


//        public class BaseInputModel
//        {
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

