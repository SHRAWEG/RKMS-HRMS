//using Hrms.Common.Models;

//namespace Hrms.MobileApi.Controllers
//{
//    [Route("[controller]")]
//    [ApiController]
//    [Authorize(Roles = "employee")]
//    public class LeaveManagementsController : Controller
//    {
//        private readonly DataContext _context;
//        private readonly UserManager<User> _userManager;

//        public LeaveManagementsController(DataContext context, UserManager<User> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        // GET: LeaveManagements
//        [HttpGet("History")]
//        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string fromDate, string toDate)
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            var query = _context.LeaveApplicationHistories
//                .Include(x => x.Leave)
//                .Include(x => x.ApprovedBy)
//                .Include(x => x.ApprovedBy.Emp)
//                .Include(x => x.DisapprovedBy)
//                .Include(x => x.DisapprovedBy.Emp)
//                .Where(x => x.EmpId == user.EmpId)
//                .AsQueryable();

//            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
//            {
//                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);
//                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

//                query = query.Where(b => b.StartDate >= FromDate && b.EndDate <= ToDate);
//            }

//            Expression<Func<LeaveApplicationHistory, object>> field = sortColumn switch
//            {
//                "CreatedAt" => x => x.CreatedAt,
//                "LeaveId" => x => x.LeaveId,
//                "StartDate" => x => x.StartDate,
//                "EndDate" => x => x.EndDate,
//                "Status" => x => x.Status,
//                _ => x => x.Id
//            };

//            if (sortDirection == null)
//            {
//                query = query.OrderByDescending(p => p.Id);
//            }
//            else if (sortDirection == "asc")
//            {
//                query = query.OrderBy(field);
//            }
//            else
//            {
//                query = query.OrderByDescending(field);
//            }

//            var data = await PagedList<LeaveApplicationHistory>.CreateAsync(query.AsNoTracking(), page, limit);

//            return Ok(new
//            {
//                Data = data.Select(x => new
//                {
//                    x.Id,
//                    x.EmpId,
//                    x.LeaveId,
//                    LeaveName = x.Leave.Name,
//                    x.CreatedAt,
//                    x.UpdatedAt,
//                    x.StartDate,
//                    x.EndDate,
//                    x.Status,
//                    x.ApprovedById,
//                    ApprovedByUserName = x.ApprovedBy?.UserName,
//                    ApprovedByName = x.ApprovedBy?.Emp?.FirstName + (x.ApprovedBy?.Emp?.MiddleName + " ") ?? "" + x.ApprovedBy?.Emp?.LastName,
//                    x.DisapprovedById,
//                    DisapprovedByUserName = x.DisapprovedBy?.UserName,
//                    DisapprovedByName = x.DisapprovedBy?.Emp?.FirstName + (x.DisapprovedBy?.Emp?.MiddleName + " ") ?? "" + x.DisapprovedBy?.Emp?.LastName
//                }),
//                data.TotalCount,
//                data.TotalPages
//            });
//        }

//        // GET: LeaveManagements
//        [HttpGet("SubordinateHistory")]
//        public async Task<IActionResult> GetSubordinateHistory(int page, int limit, string sortColumn, string sortDirection, string fromDate, string toDate)
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

//            var query = _context.LeaveApplicationHistories
//                .Where(x => subordinateIds.Contains(x.EmpId))
//                .Include(x => x.Leave)
//                .Include(x => x.ApprovedBy)
//                .Include(x => x.ApprovedBy.Emp)
//                .Include(x => x.DisapprovedBy)
//                .Include(x => x.DisapprovedBy.Emp)
//                .AsQueryable();

//            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
//            {
//                DateOnly FromDate = DateOnlyHelper.ParseDateOrNow(fromDate);
//                DateOnly ToDate = DateOnlyHelper.ParseDateOrNow(toDate);

//                query = query.Where(b => b.StartDate >= FromDate && b.EndDate <= ToDate);
//            }

//            Expression<Func<LeaveApplicationHistory, object>> field = sortColumn switch
//            {
//                "CreatedAt" => x => x.CreatedAt,
//                "LeaveId" => x => x.LeaveId,
//                "StartDate" => x => x.StartDate,
//                "EndDate" => x => x.EndDate,
//                "Status" => x => x.Status,
//                _ => x => x.Id
//            };

//            if (sortDirection == null)
//            {
//                query = query.OrderByDescending(p => p.Id);
//            }
//            else if (sortDirection == "asc")
//            {
//                query = query.OrderBy(field);
//            }
//            else
//            {
//                query = query.OrderByDescending(field);
//            }

//            var data = await PagedList<LeaveApplicationHistory>.CreateAsync(query.AsNoTracking(), page, limit);

//            return Ok(new
//            {
//                Data = data.Select(x => new
//                {
//                    x.Id,
//                    x.EmpId,
//                    x.LeaveId,
//                    LeaveName = x.Leave.Name,
//                    x.CreatedAt,
//                    x.UpdatedAt,
//                    x.StartDate,
//                    x.EndDate,
//                    x.Status,
//                    x.ApprovedById,
//                    ApprovedByUserName = x.ApprovedBy?.UserName,
//                    ApprovedByName = x.ApprovedBy?.Emp?.FirstName + (x.ApprovedBy?.Emp?.MiddleName + " ") ?? "" + x.ApprovedBy?.Emp?.LastName,
//                    x.DisapprovedById,
//                    DisapprovedByUserName = x.DisapprovedBy?.UserName,
//                    DisapprovedByName = x.DisapprovedBy?.Emp?.FirstName + (x.DisapprovedBy?.Emp?.MiddleName + " ") ?? "" + x.DisapprovedBy?.Emp?.LastName
//                }),
//                data.TotalCount,
//                data.TotalPages
//            });
//        }

//        [HttpGet("Requests")]
//        public async Task<IActionResult> Requests()
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            var data = await _context.LeaveApplicationHistories
//                .Where(x => x.EmpId == user.EmpId && x.Status == "pending")
//                .Include(x => x.Leave)
//                .ToListAsync();

//            return Ok(new
//            {
//                TotalCount = data.Count,
//                Data = data.Select(x => new
//                {
//                    x.Id,
//                    x.EmpId,
//                    x.LeaveId,
//                    LeaveName = x.Leave.Name,
//                    x.CreatedAt,
//                    x.UpdatedAt,
//                    x.StartDate,
//                    x.EndDate,
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

//            var data = await _context.LeaveApplicationHistories
//                .Where(x => subordinateIds.Contains(x.EmpId) && x.Status == "pending")
//                .Include(x => x.Leave)
//                .Include(x => x.Emp)
//                .ToListAsync();

//            return Ok(new
//            {
//                TotalCount = data.Count,
//                Data = data.Select(x => new
//                {
//                    x.Id,
//                    x.EmpId,
//                    EmpCode = x.Emp.CardId,
//                    EmpName = FullName(x.Emp.FirstName, x.Emp.MiddleName, x.Emp.LastName),
//                    x.LeaveId,
//                    LeaveName = x.Leave.Name,
//                    x.CreatedAt,
//                    x.UpdatedAt,
//                    x.StartDate,
//                    x.EndDate,
//                })
//            });
//        }

//        // Get Leave Summary for filling Leave Application
//        [HttpGet("LeaveSummary")]
//        public async Task<IActionResult> GetLeaveSummary()
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            var settings = await _context.Settings.SingleOrDefaultAsync();

//            var leaves = await (from leave in _context.Leaves
//                                join leaveLedger in _context.LeaveLedgers.Where(x => x.EmpId == user.EmpId && x.LeaveYearId == settings.LeaveYearId)
//                                .GroupBy(x => x.LeaveId)
//                                .Select(x => new
//                                {
//                                    LeaveId = x.Key,
//                                    TotalGiven = x.Sum(x => x.Given),
//                                    TotalTaken = x.Sum(x => x.Taken),
//                                }) on leave.Id equals leaveLedger.LeaveId into leaveLedgers
//                                from leaveLedger in leaveLedgers.DefaultIfEmpty()
//                                select new
//                                {
//                                    LeaveId = leave.Id,
//                                    LeaveName = leave.Name,
//                                    LeaveAbbreviation = leave.Abbreviation,
//                                    Credited = leaveLedger != null ? leaveLedger.TotalGiven ?? 0 : 0,
//                                    Availed = leaveLedger != null ? leaveLedger.TotalTaken ?? 0 : 0,
//                                    Balance = leaveLedger != null ? (leaveLedger.TotalGiven - leaveLedger.TotalTaken) ?? 0 : 0,
//                                })
//                        .ToListAsync();

//            //var carryForwardedLeaves = await _context.LeaveLedgers
//            //    .Where(x => x.EmpId == user.EmpId && x.LeaveYearId != settings.LeaveYearId)
//            //    .GroupBy(x => x.LeaveId)
//            //    .Select(x => new
//            //    {
//            //        LeaveId = x.Key,
//            //        Total = x.Sum(x => x.Given) - x.Sum(x => x.Taken),
//            //    })
//            //    .ToListAsync();

//            var pendingLeaves = await _context.LeaveApplicationHistories
//                .Where(x => x.EmpId == user.EmpId && x.LeaveYearId == settings.LeaveYearId && x.Status == "pending")
//                .GroupBy(x => x.LeaveId)
//                .Select(x => new
//                {
//                    LeaveId = x.Key,
//                    TotalPending = x.Sum(y => y.TotalDays)
//                })
//                .ToListAsync();

//            List<LeaveSummary> data = new();

//            foreach (var leave in leaves)
//            {
//                var totalPending = pendingLeaves.Where(x => x.LeaveId == leave.LeaveId).Select(x => x.TotalPending).SingleOrDefault();
//                //var totalCarryForwarded = carryForwardedLeaves.Where(x => x.LeaveId == leave.LeaveId).FirstOrDefault();

//                data.Add(new LeaveSummary
//                {
//                    LeaveId = leave.LeaveId,
//                    LeaveName = leave.LeaveName,
//                    LeaveAbbreviation = leave.LeaveAbbreviation,
//                    CarryForwarded = 0,
//                    Credited = leave.Credited,
//                    Availed = leave.Availed,
//                    Pending = totalPending ?? 0,
//                    Balance = leave.Balance - (totalPending ?? 0)
//                });
//            }

//            return Ok(new
//            {
//                Data = data
//            });
//        }

//        //Leave Application
//        [HttpPost("LeaveApplication")]
//        public async Task<IActionResult> LeaveApplication(LeaveApplicationModel input)
//        {
//            DateOnly startDate = DateOnlyHelper.ParseDateOrNow(input.StartDate);
//            DateOnly endDate = DateOnlyHelper.ParseDateOrNow(input.EndDate);

//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            if (await _context.LeaveLedgers
//                .Where(x => x.EmpId == user.EmpId && x.IsRegular == 0 && x.Leave_Date >= startDate && x.Leave_Date <= endDate)
//                .AnyAsync())
//            {
//                return ErrorHelper.ErrorResult("StartDate", "User has already taken leave somewhere in the given date range.");
//            }

//            if (await _context.LeaveApplicationHistories
//                .Where(x => x.EmpId == user.EmpId && x.Status == "pending" && x.StartDate <= endDate && x.EndDate >= startDate)
//                .AnyAsync())
//            {
//                return ErrorHelper.ErrorResult("StartDate", "User has already pending leave somewhere in the given date range.");
//            };

//            var settings = await _context.Settings.SingleOrDefaultAsync();

//            var leaveLedgers = await _context.LeaveLedgers.Where(x => x.EmpId == user.EmpId && x.LeaveId == input.LeaveId && x.LeaveYearId == settings.LeaveYearId).ToListAsync();

//            var pendingLeaves = await _context.LeaveApplicationHistories
//                .Where(x => x.EmpId == user.EmpId && x.LeaveId == input.LeaveId && x.LeaveYearId == settings.LeaveYearId && x.Status == "pending")
//                .ToListAsync();

//            var branchId = await _context.EmpLogs
//                .Where(x => x.EmployeeId == user.EmpId)
//                .Join(_context.EmpTransactions,
//                    el => el.Id,
//                    et => et.Id,
//                    (el, et) => et.BranchId
//                ).SingleOrDefaultAsync();

//            var weekendDetail = await _context.WeekendDetails
//                .Where(x => x.EmpId == user.EmpId || x.BranchId == branchId || x.EmpId == null && x.ValidFrom < startDate)
//                .OrderBy(x => x.EmpId)
//                .OrderBy(x => x.BranchId)
//                .OrderByDescending(x => x.ValidFrom)
//                .FirstOrDefaultAsync();

//            var holidays = await _context.Holidays
//                .Where(x => x.Date >= startDate && x.Date <= endDate)
//                .ToListAsync();

//            decimal? balance = leaveLedgers.Sum(x => x.Given) - leaveLedgers.Sum(x => x.Taken) - pendingLeaves.Sum(x => x.TotalDays);

//            decimal totalDays = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days + 1;

//            List<int> weekends = new();

//            if (weekendDetail?.Sunday == true)
//            {
//                weekends.Add(1);
//            }

//            if (weekendDetail?.Monday == true)
//            {
//                weekends.Add(2);
//            }

//            if (weekendDetail?.Tuesday == true)
//            {
//                weekends.Add(3);
//            }

//            if (weekendDetail?.Wednesday == true)
//            {
//                weekends.Add(4);
//            }

//            if (weekendDetail?.Thursday == true)
//            {
//                weekends.Add(5);
//            }

//            if (weekendDetail?.Friday == true)
//            {
//                weekends.Add(6);
//            }

//            if (weekendDetail?.Saturday == true)
//            {
//                weekends.Add(7);
//            }

//            if (startDate == endDate)
//            {
//                if (weekends.Contains((int)startDate.DayOfWeek + 1))
//                {
//                    startDate = startDate.AddDays(1);
//                    return ErrorHelper.ErrorResult("StartDate", "Cannot apply leave on weekend.");
//                }

//                if (holidays.Any(x => x.Date == startDate))
//                {
//                    startDate = startDate.AddDays(1);
//                    return ErrorHelper.ErrorResult("StartDate", "Cannot apply leave on holiday.");
//                }
//            }

//            LeaveApplicationHistory history = new()
//            {
//                EmpId = user.EmpId ?? 0,
//                LeaveId = input.LeaveId,
//                StartDate = startDate,
//                EndDate = endDate,
//                Address = input.Address,
//                ContactNumber = input.ContactNumber,
//                Reason = input.Remarks,
//                Status = "pending",
//                HLeaveType = input.HLeaveType,
//                LeaveYearId = settings.LeaveYearId
//            };

//            do
//            {
//                if (weekends.Contains((int)startDate.DayOfWeek + 1))
//                {
//                    totalDays--;
//                    startDate = startDate.AddDays(1);
//                    continue;
//                }

//                if (holidays.Any(x => x.Date == startDate))
//                {
//                    totalDays--;
//                }

//                startDate = startDate.AddDays(1);

//            } while (startDate <= endDate);

//            totalDays /= (decimal)(input.HLeaveType == 0 ? 1 : 2);

//            if (balance < totalDays)
//            {
//                return ErrorHelper.ErrorResult("StartDate", "Not enough balance on selected leave.");
//            };

//            history.TotalDays = totalDays;

//            _context.Add(history);
//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                Message = "Successfull"
//            });
//        }

//        [HttpPost("ApproveLeave/{id}")]
//        public async Task<IActionResult> ApproveLeave(int id)
//        {
//            var leaveHistory = await _context.LeaveApplicationHistories.Where(x => x.Id == id && x.Status == "pending").SingleOrDefaultAsync();

//            if (leaveHistory == null)
//            {
//                return ErrorHelper.ErrorResult("Id", "Id is invalid");
//            }

//            var settings = await _context.Settings.SingleOrDefaultAsync();


//            var branchId = await _context.EmpLogs
//                .Where(x => x.EmployeeId == leaveHistory.EmpId)
//                .Join(_context.EmpTransactions,
//                    el => el.Id,
//                    et => et.Id,
//                    (el, et) => et.BranchId
//                ).SingleOrDefaultAsync();

//            var weekendDetail = await _context.WeekendDetails
//                .Where(x => x.EmpId == leaveHistory.EmpId || x.BranchId == branchId || x.EmpId == null && x.ValidFrom < leaveHistory.StartDate)
//                .OrderBy(x => x.EmpId)
//                .OrderBy(x => x.BranchId)
//                .OrderByDescending(x => x.ValidFrom)
//                .FirstOrDefaultAsync();

//            List<int> weekends = new();

//            if (weekendDetail.Sunday == true)
//            {
//                weekends.Add(1);
//            }

//            if (weekendDetail.Monday == true)
//            {
//                weekends.Add(2);
//            }

//            if (weekendDetail.Tuesday == true)
//            {
//                weekends.Add(3);
//            }

//            if (weekendDetail.Wednesday == true)
//            {
//                weekends.Add(4);
//            }

//            if (weekendDetail.Thursday == true)
//            {
//                weekends.Add(5);
//            }

//            if (weekendDetail.Friday == true)
//            {
//                weekends.Add(6);
//            }

//            if (weekendDetail.Saturday == true)
//            {
//                weekends.Add(7);
//            }

//            var holidays = await _context.Holidays
//                .Where(x => x.Date >= leaveHistory.StartDate && x.Date <= leaveHistory.EndDate)
//                .ToListAsync();

//            List<LeaveLedger> data = new();

//            DateOnly startDate = leaveHistory.StartDate;

//            do
//            {
//                if (weekends.Contains((int)startDate.DayOfWeek + 1))
//                {
//                    startDate = startDate.AddDays(1);
//                    continue;
//                }

//                if (holidays.Any(x => x.Date == startDate))
//                {
//                    startDate = startDate.AddDays(1);
//                    continue;
//                }

//                data.Add(new LeaveLedger
//                {
//                    EmpId = leaveHistory.EmpId,
//                    LeaveId = leaveHistory.LeaveId,
//                    Leave_Date = startDate,
//                    Remarks = leaveHistory.Reason,
//                    Address = leaveHistory.Address,
//                    ContactNumber = leaveHistory.ContactNumber,
//                    ApprovedById = User.GetUserId(),
//                    TransactionUser = User.GetUsername(),
//                    LeaveYearId = settings.LeaveYearId,
//                    HLeaveType = leaveHistory.HLeaveType,

//                    //Default
//                    Taken = (leaveHistory.HLeaveType == (byte)0 ? (decimal)1 : (decimal)0.5),
//                    IsRegular = 0,
//                    Adjusted = 0,
//                    NoHrs = 0,
//                });

//                startDate = startDate.AddDays(1);

//            } while (startDate <= leaveHistory.EndDate);

//            leaveHistory.Status = "approved";
//            leaveHistory.ApprovedById = User.GetUserId();
//            leaveHistory.UpdatedAt = DateTime.UtcNow;

//            _context.AddRange(data);
//            await _context.SaveChangesAsync();

//            return Ok();
//        }

//        [HttpPost("DisapproveLeave/{id}")]
//        public async Task<IActionResult> DisapproveLeave(int id)
//        {
//            var leaveHistory = await _context.LeaveApplicationHistories.Where(x => x.Id == id && x.Status == "pending").SingleOrDefaultAsync();

//            if (leaveHistory == null)
//            {
//                return ErrorHelper.ErrorResult("Id", "Id is invalid");
//            }

//            var leaveLedgerTemps = await _context.LeaveLedgersTemp
//                .Where(x => x.EmpId == leaveHistory.EmpId && x.LeaveId == leaveHistory.LeaveId && x.Leave_Date >= leaveHistory.StartDate && x.Leave_Date <= leaveHistory.EndDate)
//                .ToListAsync();

//            leaveHistory.Status = "disapproved";
//            leaveHistory.DisapprovedById = User.GetUserId();
//            leaveHistory.UpdatedAt = DateTime.UtcNow;

//            _context.RemoveRange(leaveLedgerTemps);
//            await _context.SaveChangesAsync();

//            return Ok();
//        }

//        // Put: LeaveApplicationHistories/Cancel/{id}
//        [HttpPost("Cancel/{id}")]
//        public async Task<IActionResult> Cancel(int id, CancelInputModel input)
//        {
//            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

//            var data = await _context.LeaveApplicationHistories.Where(x => x.Id == id && x.Status == "pending").SingleOrDefaultAsync();

//            if (data.EmpId != user.EmpId)
//            {
//                return Unauthorized("Invalid User");
//            }

//            var leaveLedgerTemps = await _context.LeaveLedgersTemp
//                .Where(x => x.EmpId == data.EmpId && x.LeaveId == data.LeaveId && x.Leave_Date >= data.StartDate && x.Leave_Date <= data.EndDate)
//                .ToListAsync();

//            data.CancellationRemarks = input.CancellationRemarks;
//            data.Status = "cancelled";
//            data.UpdatedAt = DateTime.UtcNow;

//            _context.RemoveRange(leaveLedgerTemps);
//            await _context.SaveChangesAsync();

//            return Ok();
//        }

//        private static string FullName(string firstName, string middleName, string lastName)
//        {
//            return (firstName + " " + middleName + middleName ?? " " + lastName);
//        }

//        public class LeaveSummary
//        {
//            public short? LeaveId { get; set; }
//            public string? LeaveName { get; set; }
//            public string? LeaveAbbreviation { get; set; }
//            public decimal? CarryForwarded { get; set; }
//            public decimal? Credited { get; set; }
//            public decimal? Availed { get; set; }
//            public decimal? Pending { get; set; }
//            public decimal? Balance { get; set; }

//        }

//        public class LeaveApplicationModel
//        {
//            public short LeaveId { get; set; }
//            public string StartDate { get; set; }
//            public string EndDate { get; set; }
//            public string Remarks { get; set; }
//            public string Address { get; set; }
//            public string ContactNumber { get; set; }
//            public byte HLeaveType { get; set; }
//            public decimal TotalDays { get; set; }
//        }

//        public class CancelInputModel
//        {
//            public string CancellationRemarks { get; set; }
//        }

//        public class LeaveApplicationModelValidator : AbstractValidator<LeaveApplicationModel>
//        {
//            private readonly DataContext _context;

//            public LeaveApplicationModelValidator(DataContext context)
//            {
//                _context = context;

//                RuleFor(x => x.LeaveId)
//                    .NotEmpty()
//                    .IdMustExist(_context.Leaves.AsQueryable());

//                RuleFor(x => x.Remarks)
//                    .NotEmpty();

//                RuleFor(x => x.Address)
//                    .NotEmpty();

//                RuleFor(x => x.ContactNumber)
//                    .NotEmpty()
//                    .MustBeDigits(10);

//                RuleFor(x => x.HLeaveType)
//                    .LessThan((byte)4);

//                RuleFor(x => x.StartDate)
//                    .NotEmpty()
//                    .MustBeDate();

//                RuleFor(x => x.EndDate)
//                    .NotEmpty()
//                    .MustBeDate()
//                    .MustBeDateAfterOrEqual(x => x.StartDate, "StartDate");
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
//                if (_context.LeaveApplicationHistories.Find(int.Parse(_id)) == null)
//                {
//                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
//                    return false;
//                }

//                return true;
//            }
//        }
//    }
//}
