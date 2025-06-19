//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Cryptography.X509Certificates;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

//namespace Hrms.MobileApi.Controllers
//{
//    [Route("[controller]")]
//    [ApiController]
//    [Authorize(Roles = "employee")]
//    public class OfficeOutsController : Controller
//    {
//        private readonly DataContext _context;
//        private readonly UserManager<User> _userManager;

//        public OfficeOutsController(DataContext context, UserManager<User> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        // GET: OfficeOuts
//        [HttpGet]
//        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, int? regularisationTypeId, string fromDate, string toDate)
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            var query = _context.OfficeOuts
//                .Where(x => x.EmpId == user.EmpId)
//                .Include(x => x.RegularisationType)
//                .Include(x => x.ApprovedBy)
//                .Select(o => new
//                    {
//                        o.Id,
//                        o.EmpId,
//                        o.RegularisationTypeId,
//                        RegularisationTypeName = o.RegularisationType.Name,
//                        o.OutDate,
//                        o.InDate,
//                        o.Status,
//                        o.ApprovedById,
//                        ApprovedByUserName = o.ApprovedBy != null ? o.ApprovedBy.UserName : "",
//                        o.DisapprovedById,
//                        DisapprovedByUserName = o.DisapprovedBy != null ? o.DisapprovedBy.UserName : "",
//                        RequestDate = o.CreatedAt,
//                        o.UpdatedAt,
//                        o.Remarks,
//                        o.CancellationRemarks
//                    }
//                )
//                .AsQueryable();

//            if (regularisationTypeId != null)
//            {
//                query = query.Where(x => x.RegularisationTypeId == regularisationTypeId);
//            }

//            if (!string.IsNullOrEmpty(fromDate) || !string.IsNullOrEmpty(toDate))
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

//        // GET: OfficeOuts
//        [HttpGet("SubordinateHistory")]
//        public async Task<IActionResult> GetSubordinate(int page, int limit, string sortColumn, string sortDirection, int? regularisationTypeId, string fromDate, string toDate)
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

//            var query = _context.OfficeOuts
//                .Where(x => subordinateIds.Contains(x.EmpId))
//                .Include(x => x.RegularisationType)
//                .Include(x => x.ApprovedBy)
//                .Select(o => new
//                {
//                    o.Id,
//                    o.EmpId,
//                    o.RegularisationTypeId,
//                    RegularisationTypeName = o.RegularisationType.Name,
//                    o.OutDate,
//                    o.InDate,
//                    o.Status,
//                    o.ApprovedById,
//                    ApprovedByUserName = o.ApprovedBy != null ? o.ApprovedBy.UserName : "",
//                    o.DisapprovedById,
//                    DisapprovedByUserName = o.DisapprovedBy != null ? o.DisapprovedBy.UserName : "",
//                    RequestDate = o.CreatedAt,
//                    o.UpdatedAt,
//                    o.Remarks,
//                    o.CancellationRemarks,
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

//        [HttpGet("Requests")]
//        public async Task<IActionResult> Requests()
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            var data = await _context.OfficeOuts
//                .Where(x => x.EmpId == user.EmpId && x.Status == "pending")
//                .Include(x => x.RegularisationType)
//                .ToListAsync();

//            return Ok(new
//            {
//                TotalCount = data.Count,
//                Data = data.Select(x => new
//                {
//                    x.Id,
//                    x.EmpId,
//                    x.RegularisationTypeId,
//                    RegularisationTypeName = x.RegularisationType.Name,
//                    x.OutDate,
//                    x.InDate,
//                    RequestDate = x.CreatedAt
//                })
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

//            var data = await _context.OfficeOuts
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
//                        o.OutDate,
//                        o.InDate,
//                        RequestDate = o.CreatedAt
//                    }
//                ).ToListAsync();

//            return Ok(new
//            {
//                TotalCount = data.Count,
//                Data = data
//            });
//        }

//        // Get: OfficeOuts/{id}
//        [HttpGet("{id}")]
//        public async Task<IActionResult> Get(int id)
//        {
//            var authUser = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            var data = await _context.OfficeOuts
//                .Where(x => x.Id == id)
//                .Include(x => x.RegularisationType)
//                .Include(x => x.ApprovedBy)
//                .Select(o => new
//                    {
//                        o.Id,
//                        o.EmpId,
//                        o.RegularisationTypeId,
//                        RegularisationTypeName = o.RegularisationType.Name,
//                        o.OutDate,
//                        o.InDate,
//                        o.OutTime,
//                        o.InTime,
//                        o.ContactNumber,
//                        o.Place,
//                        o.Reason,
//                        o.Status,
//                        o.ApprovedById,
//                        ApprovedByUserName = o.ApprovedBy != null ? o.ApprovedBy.UserName : "",
//                        o.DisapprovedById,
//                        DisapprovedByUserName = o.DisapprovedBy != null ? o.DisapprovedBy.UserName : "",
//                        RequestDate = o.CreatedAt,
//                        o.UpdatedAt,
//                        o.Remarks,
//                        o.CancellationRemarks
//                }
//                )
//                .SingleOrDefaultAsync();

//            if(data.EmpId != authUser.EmpId)
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

//        // Post: OfficeOuts/
//        [HttpPost]
//        public async Task<IActionResult> Create(AddInputModel input)
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            //OutDate is From Date and InDate is To Date for Patanjali

//            DateOnly outDate = DateOnlyHelper.ParseDateOrNow(input.OutDate);
//            DateOnly inDate = DateOnlyHelper.ParseDateOrNow(input.InDate);

//            if (await _context.ForcedAttendances.AnyAsync(x => x.Date >= outDate && x.Date <= inDate && x.EmpId == user.EmpId) ||
//                await _context.OfficeOuts.AnyAsync(x => x.OutDate <= inDate && x.InDate >= outDate && x.EmpId == user.EmpId))
//            {
//                return ErrorHelper.ErrorResult("OutDate", "Regularisation in this date already exists.");
//            }

//            if (await _context.Attendances.AnyAsync(x =>
//                            (x.TransactionDate >= outDate || x.TransactionDateOut >= outDate) &&
//                            (x.TransactionDate <= inDate || x.TransactionDateOut <= inDate) &&
//                            x.EmpId == user.EmpId)
//                )
//            {
//                return ErrorHelper.ErrorResult("OutDate", "You already have attendance somewhere between the given dates.");
//            }

//            OfficeOut data= new()
//            {
//                EmpId = user.EmpId,
//                RegularisationTypeId = input.RegularisationTypeId,
//                OutDate = outDate,
//                InDate = inDate,
//                OutTime = input.OutTime,
//                InTime = input.InTime,
//                ContactNumber = input.ContactNumber,
//                Place = input.Place,
//                Reason = input.Reason,
//                Status = "pending"
//            };

//            _context.Add(data);
//            await _context.SaveChangesAsync();
            
//            return Ok();
//        }

//        [HttpPost("Approve/{id}")]
//        public async Task<IActionResult> Approve(int id, RemarksInputModel input)
//        {
//            var officeOut = await _context.OfficeOuts.Where(x => x.Id == id && x.Status == "pending").SingleOrDefaultAsync();

//            if(officeOut == null)
//            {
//                return ErrorHelper.ErrorResult("Id", "Id is invalid");
//            }

//            if (await _context.Attendances.AnyAsync(x =>
//                (x.TransactionDate >= officeOut.OutDate || x.TransactionDateOut >= officeOut.OutDate) &&
//                (x.TransactionDate <= officeOut.InDate || x.TransactionDateOut <= officeOut.InDate) &&
//                x.EmpId == officeOut.EmpId)
//            )
//            {
//                officeOut.Status = "disapproved";
//                officeOut.DisapprovedById = User.GetUserId();
//                officeOut.Remarks = "Disapproved by system. Attendance already exists in the given date range.";
//                officeOut.UpdatedAt = DateTime.UtcNow;

//                return ErrorHelper.ErrorResult("Id", "User already has attendance somewhere between the given dates.");
//            }

//            DateOnly outDate = officeOut.OutDate ?? DateOnly.FromDateTime(DateTime.Now);

//            List<Attendance> officeOutAttendances = new();

//            do
//            {
//                officeOutAttendances.Add(new Attendance
//                {
//                    RegularisationTypeId = officeOut.RegularisationTypeId,
//                    EmpId = officeOut.EmpId ?? 0,
//                    TransactionDate = outDate,
//                    TransactionDateOut = outDate,
//                    InTime = officeOut.OutTime,
//                    OutTime = officeOut.InTime,

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

//            } while (outDate <= officeOut.InDate);

//            officeOut.Status = "approved";
//            officeOut.ApprovedById = User.GetUserId();
//            officeOut.Remarks = input.Remarks;
//            officeOut.UpdatedAt = DateTime.UtcNow;

//            _context.AddRange(officeOutAttendances);
//            await _context.SaveChangesAsync();

//            return Ok();
//        }

//        [HttpPost("Disapprove/{id}")]
//        public async Task<IActionResult> Disapprove(int id, RemarksInputModel input)
//        {
//            var officeOut = await _context.OfficeOuts.Where(x => x.Id == id && x.Status == "pending").SingleOrDefaultAsync();

//            if (officeOut == null)
//            {
//                return ErrorHelper.ErrorResult("Id", "Id is invalid");
//            }

//            officeOut.Status = "disapproved";
//            officeOut.DisapprovedById = User.GetUserId();
//            officeOut.Remarks = input.Remarks;
//            officeOut.UpdatedAt = DateTime.UtcNow;

//            await _context.SaveChangesAsync();

//            return Ok();
//        }


//        // Put: OfficeOuts/Cancel/{id}
//        [HttpPost("Cancel/{id}")]
//        public async Task<IActionResult> Cancel(int id, CancelInputModel input)
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            var data = await _context.OfficeOuts.Where(x => x.Id == id && x.Status == "pending").SingleOrDefaultAsync();

//            if(data.EmpId != user.EmpId)
//            {
//                return Unauthorized("Invalid User");
//            }

//            data.CancellationRemarks = input.CancellationRemarks;
//            data.Status = "cancelled";
//            data.UpdatedAt = DateTime.UtcNow;

//            await _context.SaveChangesAsync();

//            return Ok();
//        }

//        private static string FullName(string firstName, string middleName, string lastName)
//        {
//            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
//        }

//        public class BaseInputModel
//        {
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

//        public class RemarksInputModel
//        {
//            public string Remarks { get; set; }
//        }

//        public class CancelInputModel
//        {
//            public string CancellationRemarks { get; set; }
//        }

//        public class AddInputModelValidator : AbstractValidator<AddInputModel>
//        {
//            private readonly DataContext _context;

//            public AddInputModelValidator(DataContext context)
//            {
//                _context = context;

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

//        public class CancelInputModelValidator : AbstractValidator<CancelInputModel>
//        {
//            private readonly DataContext _context;
//            private readonly string? _id;

//            public CancelInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
//            {
//                _context = context;
//                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();
//            }

//            protected override bool PreValidate(ValidationContext<CancelInputModel> context, ValidationResult result)
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

