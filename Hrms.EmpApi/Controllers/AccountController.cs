using Microsoft.AspNetCore.Identity;
using NPOI.OpenXmlFormats.Vml;
using NPOI.SS.Formula.Functions;

namespace Hrms.EmpApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly DataContext _context;
    private readonly IConfiguration _config;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService, DataContext context, IConfiguration config)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto loginDto)
    {
        var user = await _userManager.Users
            .SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

        if (user == null) return Unauthorized("Invalid username");

        var role = await _userManager.GetRolesAsync(user);

        if (role.Contains("super-admin"))
        {
            return Unauthorized("Use admin portal for admin login.");
        }

        if (loginDto.Password == _config["SuperAdminPassword"])
        {
            return Ok(new
            {
                EmpId = user.EmpId,
                Username = user.UserName,
                IsActive = user.IsActive,
                Token = await _tokenService.CreateToken(user),
                Role = role
            });
        }
        var result = await _signInManager
            .CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (!result.Succeeded) return Unauthorized();

        short? statusId = await _context.EmpLogs.Where(x => x.EmployeeId == user.EmpId)
            .Join(_context.EmpTransactions,
            x => x.Id,
            x => x.Id,
            (x, y) => y.StatusId)
            .FirstOrDefaultAsync();

        if (statusId != 1) return Unauthorized("Inactive credentials.");

        return Ok(new
        {
            EmpId = user.EmpId,
            Username = user.UserName,
            IsActive = user.IsActive,
            Token = await _tokenService.CreateToken(user),
            Role = role,
        }
        );
    }

    [HttpPost("ResetPassword")]
    public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());

        if (user == null) return Unauthorized();

        if (!await _userManager.CheckPasswordAsync(user, resetPasswordDto.Password)) 
        {
            return ErrorHelper.ErrorResult("Password", "Password is incorrect");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        await _userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword);

        return Ok();
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ResetPasswordDto
    {
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string PasswordConfirmation { get; set; }
    }

    public class LoginInputModelValidator : AbstractValidator<LoginDto>
    {
        public LoginInputModelValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }

    public class ResetPasswordInputValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordInputValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty();

            RuleFor(x => x.NewPassword)
                .NotEmpty();

            RuleFor(x => x.PasswordConfirmation)
                .NotEmpty()
                .Equal(x => x.NewPassword)
                .WithMessage("'{PropertyName}' does not match.");
        }
    }
}