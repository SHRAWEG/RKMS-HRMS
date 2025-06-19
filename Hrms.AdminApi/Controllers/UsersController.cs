using Microsoft.AspNetCore.Identity;
using NPOI.POIFS.FileSystem;

namespace Hrms.AdminApi.Controllers;

[ApiController]
[Route("[controller]")]

public class UsersController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public UsersController(UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService, DataContext context)
    {
        _context = context;
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [CustomAuthorize("list-user")]
    [HttpGet()]
    public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, int? companyId, string username)
    {
        var roles = await _context.Roles
            .Where(x => x.Name != "employee" & x.Name != "super-admin")
            .Select(x => x.Id)
            .ToListAsync();

        var query = _userManager.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Where(x => x.UserRoles.Any(x => roles.Contains(x.RoleId)))
            .AsQueryable();

        if (companyId is not null)
        {
            var userIds = await _context.UserCompanies.Where(x => x.CompanyId == companyId).Select(x => x.UserId).ToListAsync();

            query = query.Where(x => userIds.Contains(x.Id));
        }

        if (!string.IsNullOrEmpty(username))
        {
            query = query.Where(x => x.UserName ==  username);
        }

        Expression<Func<User, object>> field = sortColumn switch
        {
            "UserName" => x => x.UserName,
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

        var data = await PagedList<User>.CreateAsync(query.AsNoTracking(), page, limit);

        return Ok(new
        {
            Data = data.Select(x => new
            {
                Id = x.Id,
                UserName = x.UserName,
                IsActive = x.IsActive,
                RoleName = x.UserRoles.Select(x => x.Role.Name).FirstOrDefault()
            }),
            data.TotalCount,
            data.TotalPages
        });
    }

    [CustomAuthorize("list-user")]
    [HttpGet("Emp")]
    public async Task<IActionResult> GetEmp(int page, int limit, string sortColumn, string sortDirection, string username)
    {
        var role = await _context.Roles.Where(x => x.Name == "employee").Select(x => x.Id).FirstOrDefaultAsync();

        var query = _userManager.Users
            .Include(x => x.UserRoles)
            .Where(x => x.UserRoles.Any(x => x.RoleId == role)).AsQueryable();

        if (!string.IsNullOrEmpty(username))
        {
            query = query.Where(x => x.UserName == username);
        }

        Expression<Func<User, object>> field = sortColumn switch
        {
            "UserName" => x => x.UserName,
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

        var data = await PagedList<User>.CreateAsync(query.AsNoTracking(), page, limit);

        return Ok(new
        {
            Data = data.Select(x => new
            {
                Id = x.Id,
                UserName = x.UserName,
                IsActive = x.IsActive
            }),
            data.TotalCount,
            data.TotalPages
        });
    }

    [CustomAuthorize("view-user")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmp(int id)
    {
        var data = await _userManager.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Include(x => x.Emp)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (data is null)
        {
            return ErrorHelper.ErrorResult("Id", "Id is inavlid.");
        }

        var permissions = await _context.UserPermissions
            .Where(x => x.UserId == id)
            .Select(x => new
            {
                Id = x.Permission.Id,
                Name = x.Permission.Name,
                DisplayName = x.Permission.DisplayName
            })
            .ToListAsync();

        var companies = await _context.UserCompanies
            .Include(x => x.Company)
            .Where(X => X.UserId == id)
            .Select(x => new
            {
                Id = x.CompanyId,
                Name = x.Company.Name
            })
            .ToListAsync();

        return Ok(new
        {
            User = new
            {
                Id = data.Id,
                UserName = data.UserName,
                RoleId = data.UserRoles.Select(x => x.RoleId).FirstOrDefault(),
                RoleName = data.UserRoles.Select(x => x.Role.Name).FirstOrDefault(),
                IsActive = data.IsActive,
                EmpId = data.EmpId,
                EmpName = Helper.FullName(data.Emp?.FirstName, data.Emp?.MiddleName, data.Emp?.LastName),
                EmpCode = data.Emp?.CardId,
                Companies = companies,
                Permissions = permissions
            }
        });
    }

    [CustomAuthorize("write-user")]
    [HttpPost()]
    public async Task<ActionResult> Register(AddInputModel input)
    {
        User user = new()
        {
            UserName = input.Username.ToLower(),
            IsActive = true
        };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var result = await _userManager.CreateAsync(user, input.Password);

            var role = await _context.Roles.FindAsync(input.RoleId);

            if (role.Name == "super-admin" || role.Name == "employee")
            {
                return ErrorHelper.ErrorResult("RoleId", "Cannot create super admin or employee.");
            }

            var assignRole = await _userManager.AddToRolesAsync(user, new[] { role.Name });

            if (!result.Succeeded) return BadRequest(result.Errors);
            if (!assignRole.Succeeded) return BadRequest(assignRole.Errors);

            List<UserPermission> userPermission = new();
            List<UserCompany> userCompanies = new();

            foreach (var permissionId in input.PermissionIds)
            {
                userPermission.Add(new UserPermission
                {
                    UserId = user.Id,
                    PermissionId = permissionId
                });
            };

            foreach (var companyId in input.CompanyIds)
            {
                userCompanies.Add(new UserCompany
                {
                    UserId = user.Id,
                    CompanyId = companyId
                });
            }

            _context.AddRange(userPermission);
            _context.AddRange(userCompanies);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok();
        } catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return ErrorHelper.ErrorResult("Id", ex.Message);
        }
    }

    [CustomAuthorize("update-user")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateInputModel input)
    {
        var user = await _context.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user.UserRoles.Any(x => x.Role.Name == "super-admin" || x.Role.Name == "employee"))
        {
            return ErrorHelper.ErrorResult("Id", "Cannot edit super admin or employee.");
        }

        user.UserName = input.Username;

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (!string.IsNullOrEmpty(input.Password))
            {
                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, input.Password);
            }

            if (input.RoleId != user.UserRoles.Select(x => x.RoleId).FirstOrDefault())
            {
                user.UserRoles.Clear();

                var role = await _roleManager.FindByIdAsync(input.RoleId.ToString());

                var assignRole = await _userManager.AddToRolesAsync(user, new[] { role.Name });
            }

            List<UserPermission> userPermission = new();
            List<UserCompany> userCompanies = new();

            if (input.PermissionIds is not null)
            {
                var existingPermission = await _context.UserPermissions.Where(x => x.UserId == user.Id).ToListAsync();

                var removePermissions = existingPermission.Where(x => !input.PermissionIds.Contains(x.PermissionId)).ToList();

                foreach (var permissionId in input.PermissionIds)
                {
                    if (existingPermission.Any(x => x.PermissionId == permissionId)) continue;

                    userPermission.Add(new UserPermission
                    {
                        UserId = user.Id,
                        PermissionId = permissionId
                    });
                }

                _context.RemoveRange(removePermissions);
            }

            if (input.CompanyIds.Any())
            {
                var existingCompanies = await _context.UserCompanies.Where(x => x.UserId == user.Id).ToListAsync();

                var removeCompanies = existingCompanies.Where(x => !input.CompanyIds.Contains(x.CompanyId)).ToList();

                foreach (var companyId in input.CompanyIds)
                {
                    if (existingCompanies.Any(x => x.CompanyId == companyId)) continue;

                    userCompanies.Add(new UserCompany
                    {
                        UserId = user.Id,
                        CompanyId = companyId
                    });
                }

                _context.RemoveRange(removeCompanies);
            }

            _context.AddRange(userPermission);
            _context.AddRange(userCompanies);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok();
        } catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return ErrorHelper.ErrorResult("Id", ex.Message);
        }
    }

    public class BaseModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
        public int RoleId { get; set; }
        public List<int> PermissionIds { get; set; }
        public List<int> CompanyIds { get; set; }
    }

    public class AddInputModel : BaseModel { }

    public class UpdateInputModel : BaseModel { }

    public class AddInputModelValidator : AbstractValidator<AddInputModel>
    {
        private readonly DataContext _context;

        public AddInputModelValidator(DataContext context)
        {
            _context = context;

            Transform(x => x.Username, v => v?.Trim())
                .NotEmpty()
                .MustBeUnique(_context.Users.AsQueryable(), "UserName");

            RuleFor(x => x.Password)
                .NotEmpty();

            RuleFor(x => x.PasswordConfirmation)
                .NotEmpty()
                .Equal(x => x.Password)
                .WithMessage("'{PropertyName}' does not match.");

            RuleFor(x => x.RoleId)
                .NotEmpty()
                .IdMustExist(_context.Roles.AsQueryable());

            RuleFor(x => x.PermissionIds)
                .NotEmpty();

            RuleForEach(x => x.PermissionIds)
                .IdMustExist(_context.Permissions.AsQueryable());

            RuleFor(x => x.CompanyIds)
                .NotEmpty();

            RuleForEach(x => x.CompanyIds)
                .IdMustExist(_context.Companies.AsQueryable());
        }
    }

    public class UpdateInputModelValidator : AbstractValidator<UpdateInputModel>
    {
        private readonly string? _id;
        private readonly DataContext _context;

        public UpdateInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

            Transform(x => x.Username, v => v?.Trim())
                .NotEmpty()
                .MustBeUnique(_context.Users.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "UserName");

            RuleFor(x => x.PasswordConfirmation)
                .NotEmpty()
                .Equal(x => x.Password)
                .WithMessage("'{PropertyName}' does not match.")
                .Unless(x => string.IsNullOrEmpty(x.Password));

            RuleFor(x => x.RoleId)
                .NotEmpty()
                .IdMustExist(_context.Roles.AsQueryable());

            RuleForEach(x => x.PermissionIds)
                .IdMustExist(_context.Permissions.AsQueryable());

            RuleForEach(x => x.CompanyIds)
                .IdMustExist(_context.Companies.AsQueryable());
        }

        protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
        {
            if (_context.Users.Find(int.Parse(_id)) == null)
            {
                result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                return false;
            }

            return true;
        }
    }
}