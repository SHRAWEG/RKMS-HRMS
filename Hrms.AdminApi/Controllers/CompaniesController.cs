using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.OpenXmlFormats.Dml.Diagram;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class CompaniesController : Controller
    {
        private readonly DataContext _context;

        public CompaniesController(DataContext context)
        {
            _context = context;
        }

        // GET: Companies
        [CustomAuthorize("list-company")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name, string code)
        {
            var query = _context.Companies.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrEmpty(code))
            {
                query = query.Where(b => b.Code!.ToLower().Contains(code.ToLower()));
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                query = query.Where(x => companyIds.Contains(x.Id));
            }

            Expression<Func<Company, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "Code" => x => x.Code,
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

            var data = await PagedList<Company>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // GETALL: Companies
        [CustomAuthorize("search-company")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Companies.ToListAsync();

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                data = data.Where(x => companyIds.Contains(x.Id)).ToList();
            }

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code
                })
            });
        }

        // GET: Companies/5
        [CustomAuthorize("view-company")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Companies
                .Include(x => x.LeaveYearCompanies)
                .ThenInclude(x => x.LeaveYear)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Contains(data.Id))
                {
                    return Forbid();
                }
            }

            return Ok( new
            {
                Company = new
                {
                    Id = data.Id,
                    Name = data.Name,
                    Code = data.Code,
                    LeaveYearId = data.LeaveYearCompanies?.FirstOrDefault()?.LeaveYearId,
                    LeaveYearName = data.LeaveYearCompanies?.FirstOrDefault()?.LeaveYear?.Year,
                }
            });
        }

        // Post: Companies/Create
        [CustomAuthorize("write-company")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    Company data = new()
                    {
                        Name = input.Name,
                        Code = input.Code,
                    };

                    _context.Add(data);
                    await _context.SaveChangesAsync();

                    _context.Add(new UserCompany
                    {
                        UserId = User.GetUserId(),
                        CompanyId = data.Id
                    });

                    if (input.LeaveYearId is not null)
                    {
                        _context.Add(new LeaveYearCompany
                        {
                            CompanyId = data.Id,
                            LeaveYearId = input.LeaveYearId ?? 0
                        });
                    }

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                } catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(ex.Message);
                }
            }
            
            return Ok();
        }

        // PUT: Companies/5
        [CustomAuthorize("update-company")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);

            if (data is null)
            {
                return ErrorHelper.ErrorResult("Id", "Company is not assigned to the user.");
            }

            var leaveYear = await _context.LeaveYearCompanies.Where(x => x.CompanyId == id).FirstOrDefaultAsync();

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Contains(data.Id))
                {
                    return Forbid();
                }
            }

            data.Name = input.Name;
            data.Code = input.Code;
            data.UpdatedAt = DateTime.UtcNow;

            if (input.LeaveYearId is not null && leaveYear?.LeaveYearId != input.LeaveYearId)
            {
                if (await _context.LeaveLedgers.AnyAsync(x => x.CompanyId == data.Id && !x.IsClosed))
                {
                    return ErrorHelper.ErrorResult("Id", "The previous leave year is already in use. Please initiate the closing and try again.");
                }

                if (leaveYear is not null)
                {
                    leaveYear.LeaveYearId = input.LeaveYearId ?? 0;
                } else
                {
                    _context.Add(new LeaveYearCompany
                    {
                        CompanyId = data.Id,
                        LeaveYearId = input.LeaveYearId ?? 0
                    });
                }
            } else
            {
                if (leaveYear is not null)
                {
                    if (await _context.LeaveLedgers.AnyAsync(x => x.CompanyId == data.Id && !x.IsClosed))
                    {
                        return ErrorHelper.ErrorResult("Id", "The previous leave year is already in use. Please initiate the closing and try again.");
                    }

                    _context.Remove(leaveYear);
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Companies/5
        [CustomAuthorize("delete-company")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Companies.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Contains(data.Id))
                {
                    return Forbid();
                }
            }

            if (await _context.EmpTransactions.AnyAsync(x => x.CompanyId == id)
                || await _context.LeaveYearCompanies.AnyAsync(x => x.CompanyId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Company is already in use.");
            }

            _context.Companies.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public int? LeaveYearId { get; set; }
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
                    .MustBeUnique(_context.Companies.AsQueryable(), "Name");

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.Companies.AsQueryable(), "Code");

                RuleFor(x => x.LeaveYearId)
                    .IdMustExist(_context.LeaveYears.AsQueryable())
                    .Unless(x => x.LeaveYearId is null);
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
                    .MustBeUnique(_context.Companies.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name");

                Transform(x => x.Code, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.Companies.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Code");

                RuleFor(x => x.LeaveYearId)
                    .IdMustExist(_context.LeaveYears.AsQueryable())
                    .Unless(x => x.LeaveYearId is null);
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Companies.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
