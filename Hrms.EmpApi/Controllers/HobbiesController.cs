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
    public class HobbiesController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private static readonly IEnumerable<string> _hobbies = new List<string>()
        {
            "Sport",
            "Cultural",
            "Other"
        };

        public HobbiesController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Hobbies
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);

            var data = await _context.Hobbies
                .Where(x => x.EmpId == user.EmpId)
                .ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Type
                }),
            });
        }

        // GET: Hobbies/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Hobbies
                .SingleOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                Hobby = new
                {
                    data.Id,
                    data.Name,
                    data.Type,
                }
            });
        }

        // Post: Hobbies/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var user = await _userManager.GetUserAsync(User);

            Hobby data = new()
            {
                EmpId = user.EmpId,
                Name = input.Name,
                Type = input.Type
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: Hobbies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Hobbies.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.Type = input.Type;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Hobbies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Hobbies.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            _context.Hobbies.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
            public string Type{ get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            public AddInputModelValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty();

                RuleFor(x => x.Type)
                    .NotEmpty()
                    .MustBeIn(_hobbies);
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
                    .NotEmpty();

                RuleFor(x => x.Type)
                    .NotEmpty()
                    .MustBeIn(_hobbies);
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Hobbies.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
