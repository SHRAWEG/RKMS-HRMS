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
    public class FamiliesController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private static readonly IEnumerable<string> _relationshipTypes = new List<string>()
            {
                "Mother",
                "Father",
                "Spouse",
                "Child"
            };
        private static readonly IEnumerable<string> _genders = new List<string>()
            {
                "Male",
                "Female"
            };

        public FamiliesController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Families
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);

            var data = await _context.Families
                .Where(x => x.EmpId == user.EmpId)
                .ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.RelationshipType,
                    x.Name,
                    x.Gender,
                    x.DateOfBirth,
                    x.IsWorking,
                    x.PlaceOfBirth
                }),
            });
        }

        // GET: Families/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Families
                .SingleOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                Family = new
                {
                    data.Id,
                    data.RelationshipType,
                    data.Name,
                    data.Gender,
                    data.DateOfBirth,
                    data.IsWorking,
                    data.PlaceOfBirth
                }
            });
        }

        // Post: Families/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var user = await _userManager.GetUserAsync(User);

            DateOnly dateOfBirth = DateOnlyHelper.ParseDateOrNow(input.DateOfBirth);

            Family data = new()
            {
                EmpId = user.EmpId,
                RelationshipType = input.RelationshipType,
                Name = input.Name,
                Gender = input.Gender,
                DateOfBirth = dateOfBirth,
                IsWorking = input.IsWorking,
                PlaceOfBirth = input.PlaceOfBirth
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: Families/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Families.FirstOrDefaultAsync(c => c.Id == id);

            DateOnly dateOfBirth = DateOnlyHelper.ParseDateOrNow(input.DateOfBirth);

            data.RelationshipType = input.RelationshipType;
            data.Name = input.Name;
            data.Gender = input.Gender;
            data.DateOfBirth = dateOfBirth;
            data.IsWorking = input.IsWorking;
            data.PlaceOfBirth = input.PlaceOfBirth;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Families/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Families.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            _context.Families.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string RelationshipType { get; set; }
            public string Name { get; set; }
            public string Gender { get; set; }
            public string DateOfBirth { get; set; }
            public bool IsDependent{ get; set; }
            public bool IsWorking { get; set; }
            public string? PlaceOfBirth { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.RelationshipType)
                    .NotEmpty()
                    .MustBeIn(_relationshipTypes);

                RuleFor(x => x.Name)
                    .NotEmpty();

                RuleFor(x => x.Gender)
                    .NotEmpty()
                    .MustBeIn(_genders);

                RuleFor(x => x.DateOfBirth)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateBeforeNow();

                RuleFor(x => x.IsDependent)
                    .NotNull();

                RuleFor(x => x.IsWorking)
                    .NotNull();
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

                RuleFor(x => x.RelationshipType)
                    .NotEmpty()
                    .MustBeIn(_relationshipTypes);

                RuleFor(x => x.Name)
                    .NotEmpty();

                RuleFor(x => x.Gender)
                    .NotEmpty()
                    .MustBeIn(_genders);

                RuleFor(x => x.DateOfBirth)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateBeforeNow();

                RuleFor(x => x.IsDependent)
                    .NotNull();

                RuleFor(x => x.IsWorking)
                    .NotNull();
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Families.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
