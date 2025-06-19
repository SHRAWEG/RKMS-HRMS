using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class EducationsController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public EducationsController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Educations
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);

            var data = await _context.Educations
                .Where(x => x.EmpId == user.EmpId)
                .Include(x => x.EducationLevel)
                .Include(x => x.Country)
                .ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.EducationLevelId,
                    EducationLevelName = x.EducationLevel?.Name,
                    x.CertificateName,
                    x.StartDate,
                    x.EndDate,
                    x.Subject,
                    x.Institute,
                    x.FinalGrade,
                    x.University,
                    x.CountryId,
                    CountryName = x.Country.Name
                }),
            });
        }

        // GET: Educations/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Educations
                .Include(x => x.EducationLevel)
                .Include(x => x.Country)
                .SingleOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                Education = new
                {
                    data.Id,
                    data.EducationLevelId,
                    EducationLevelName = data.EducationLevel?.Name,
                    data.CertificateName,
                    data.StartDate,
                    data.EndDate,
                    data.Subject,
                    data.Institute,
                    data.FinalGrade,
                    data.University,
                    data.CountryId,
                    CountryName = data.Country.Name
                }
            });
        }

        // Post: Educations/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var user = await _userManager.GetUserAsync(User);

            DateOnly startDate = DateOnlyHelper.ParseDateOrNow(input.StartDate);
            DateOnly endDate = DateOnlyHelper.ParseDateOrNow(input.EndDate);

            Education data = new()
            {
                EmpId = user.EmpId ?? 1,
                EducationLevelId = input.EducationLevelId,
                CertificateName = input.CertificateName,
                StartDate = startDate,
                EndDate = endDate,
                Subject = input.Subject,
                Institute = input.Institute,
                FinalGrade = input.FinalGrade,
                University = input.University,
                CountryId = input.CountryId,

                //default
                Flag = 0
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: Educations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Educations.FirstOrDefaultAsync(c => c.Id == id);

            DateOnly startDate = DateOnlyHelper.ParseDateOrNow(input.StartDate);
            DateOnly endDate = DateOnlyHelper.ParseDateOrNow(input.EndDate);

            data.EducationLevelId = input.EducationLevelId;
            data.CertificateName = input.CertificateName;
            data.StartDate = startDate;
            data.EndDate = endDate;
            data.Subject = input.Subject;
            data.Institute = input.Institute;
            data.FinalGrade = input.FinalGrade;
            data.University = input.University;
            data.CountryId = input.CountryId;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Educations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Educations.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            _context.Educations.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public short EducationLevelId { get; set; }
            public string CertificateName { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string Subject { get; set; }
            public string Institute { get; set; }
            public string FinalGrade { get; set; }
            public string University { get; set; }
            public int? CountryId { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.EducationLevelId)
                    .NotEmpty()
                    .IdMustExist(_context.EducationLevels.AsQueryable());

                RuleFor(x => x.CertificateName)
                    .NotEmpty();

                RuleFor(x => x.StartDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.EndDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfter(x => x.StartDate, "Start Date");

                RuleFor(x => x.Subject)
                    .NotEmpty();

                RuleFor(x => x.Institute)
                    .NotEmpty();

                RuleFor(x => x.CountryId)
                    .IdMustExist(_context.Countries.AsQueryable())
                    .Unless(x => x.CountryId is null);
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

                RuleFor(x => x.EducationLevelId)
                     .NotEmpty()
                     .IdMustExist(_context.EducationLevels.AsQueryable());

                RuleFor(x => x.CertificateName)
                    .NotEmpty();

                RuleFor(x => x.StartDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.EndDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfter(x => x.StartDate, "Start Date");

                RuleFor(x => x.Subject)
                    .NotEmpty();

                RuleFor(x => x.Institute)
                    .NotEmpty();

                RuleFor(x => x.CountryId)
                    .IdMustExist(_context.Countries.AsQueryable())
                    .Unless(x => x.CountryId is null);
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Educations.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
