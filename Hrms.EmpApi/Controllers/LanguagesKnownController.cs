using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Xml.Linq;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class LanguagesKnownController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public LanguagesKnownController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: LanguagesKnown
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);

            var data = await _context.LanguagesKnown
                .Where(x => x.EmpId == user.EmpId)
                .ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Language,
                    x.CanRead,
                    x.CanWrite,
                    x.CanSpeak
                }),
            });
        }

        // GET: LanguagesKnown/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.LanguagesKnown
                .SingleOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                LanguageKnown = new
                {
                    data.Id,
                    data.Language,
                    data.CanRead,
                    data.CanWrite,
                    data.CanSpeak
                }
            });
        }

        // Post: LanguagesKnown/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var user = await _userManager.GetUserAsync(User);

            LanguageKnown data = new()
            {
                EmpId = user.EmpId,
                Language = input.Language,
                CanRead = input.CanRead,
                CanWrite = input.CanWrite,
                CanSpeak = input.CanSpeak
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: LanguagesKnown/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.LanguagesKnown.FirstOrDefaultAsync(c => c.Id == id);

            data.Language = input.Language;
            data.CanRead = input.CanRead;
            data.CanWrite = input.CanWrite;
            data.CanSpeak = input.CanSpeak;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: LanguagesKnown/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.LanguagesKnown.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            _context.LanguagesKnown.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Language { get; set; }
            public bool CanRead{ get; set; }
            public bool CanWrite { get; set; }
            public bool CanSpeak { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            public AddInputModelValidator()
            {
                RuleFor(x => x.Language)
                    .NotEmpty();
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

                RuleFor(x => x.Language)
                    .NotEmpty();
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.LanguagesKnown.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
