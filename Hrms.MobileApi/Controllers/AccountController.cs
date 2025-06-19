using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hrms.MobileApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly DataContext _context;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService, DataContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
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

        var result = await _signInManager
            .CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (!result.Succeeded) return Unauthorized("Invalid Password");

        var emp = await _context.EmpLogs.Where(x => x.EmployeeId == user.EmpId)
                        .Join(_context.EmpTransactions,
                            el => el.Id,
                            et => et.Id,
                            (el, et) => new
                            {
                                et.RmEmpId,
                                RmEmpName = FullName(et.RmEmp.FirstName, et.RmEmp.MiddleName, et.RmEmp.LastName),
                                RmEmpCode = et.RmEmp.CardId,
                                et.HodEmpId,
                                HodEmpName = FullName(et.HodEmp.FirstName, et.HodEmp.MiddleName, et.HodEmp.LastName),
                                HodEmpCode = et.HodEmp.CardId,
                            }
                        ).FirstOrDefaultAsync();

        return Ok(new
        {
            user.EmpId,
            Username = user.UserName,
            user.IsActive,
            emp.RmEmpId,
            emp.RmEmpCode,
            emp.RmEmpName,
            emp.HodEmpId,
            emp.HodEmpCode,
            emp.HodEmpName,
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

    private static string FullName(string firstName, string middleName, string lastName)
    {
        return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
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