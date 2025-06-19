using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Hrms.AdminApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RolesController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public RolesController(UserManager<User> userManager, RoleManager<Role> roleManager, SignInManager<User> signInManager, ITokenService tokenService, DataContext context)
    {
        _context = context;
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenService = tokenService;
        _roleManager = roleManager;
    }

    [CustomAuthorize("list-role")]
    [HttpGet()]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _context.Roles
            .Where(x => x.Name != "super-admin" && x.Name != "employee")
            .ToListAsync();

        return Ok(new
        {
            Data = roles
        });
    }

    [CustomAuthorize("view-role")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPermissions(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());

        if (role is null)
        {
            return ErrorHelper.ErrorResult("Id", "Id is invalid.");
        }

        var rolePermissions = await _context.RolePermissions
            .Include(x => x.Permission)
            .Where(x => x.RoleId == id)
            .ToListAsync();

        return Ok(new
        {
            Data = new
            {
                Name = role.Name,
                Permissions = rolePermissions
                                .Select(x => new
                                    {
                                        Id = x.PermissionId,
                                        Name = x.Permission.Name,
                                        DisplayName = x.Permission.DisplayName
                                    })
                                .ToList()
            }
        });
    }

    [CustomAuthorize("write-role")]
    [HttpPost()]
    public async Task<IActionResult> Create(AddInputModel input)
    {
        Role role = new() { Name = input.Name };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _roleManager.CreateAsync(role);

            List<RolePermission> rolePermissions = new();

            foreach (var permissionId in input.PermissionIds)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
            }

            _context.AddRange(rolePermissions);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok();
        } catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return ErrorHelper.ErrorResult("Id", ex.Message);
        }

        
    }

    [CustomAuthorize("update-role")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateInputModel input)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());

        if (role.Name == "super-admin" || role.Name == "employee")
        {
            return Forbid();
        }

        role.Name = input.Name;
        role.NormalizedName = _roleManager.NormalizeKey(input.Name);

        var rolePermission = await _context.RolePermissions.Where(x => x.RoleId == role.Id).ToListAsync();

        _context.RemoveRange(rolePermission);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.SaveChangesAsync();

            List<RolePermission> rolePermissions = new();

            foreach (var permissionId in input.PermissionIds)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = id,
                    PermissionId = permissionId
                });
            }

            _context.AddRange(rolePermissions);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok();
        } catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return ErrorHelper.ErrorResult("Id", ex.Message);
        }


    }

    [CustomAuthorize("delete-role")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());

        if (role.Name == "super-admin" || role.Name == "employee")
        {
            return Forbid();
        }

        if (role is null)
        {
            return ErrorHelper.ErrorResult("Id", "Id is invalid.");
        }
        
        if (await _context.UserRoles.AnyAsync(x => x.RoleId == role.Id))
        {
            return ErrorHelper.ErrorResult("Id", "Role is already in use.");
        }

        var rolePermission = await _context.RolePermissions.Where(x => x.RoleId == role.Id).ToListAsync();

        _context.RemoveRange(rolePermission);
        await _context.SaveChangesAsync();

        await _roleManager.DeleteAsync(role);

        return Ok();
    }

    public class BaseModel
    {
        public string Name { get; set; }
        public int[] PermissionIds { get; set; }
    }

    public class AddInputModel : BaseModel { }

    public class UpdateInputModel : BaseModel { }

    public class AddInputModelValidator : AbstractValidator<AddInputModel>
    {
        private DataContext _context;

        public AddInputModelValidator(DataContext context)
        {
            _context = context;

            RuleFor(x => x.Name)
                .NotEmpty()
                .MustBeUnique(_context.Roles.AsQueryable(), "Name");

            RuleFor(x => x.PermissionIds)
                .NotEmpty();

            RuleForEach(x => x.PermissionIds)
                .NotEmpty()
                .IdMustExist(_context.Permissions.AsQueryable());
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

            RuleFor(x => x.Name)
                .NotEmpty()
                .MustBeUnique(_context.Roles.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name");

            RuleFor(x => x.PermissionIds)
                .NotEmpty();

            RuleForEach(x => x.PermissionIds)
                .NotEmpty()
                .IdMustExist(_context.Permissions.AsQueryable());
        }

        protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
        {
            if (_context.Roles.Find(int.Parse(_id)) == null)
            {
                result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                return false;
            }

            return true;
        }
    }
}