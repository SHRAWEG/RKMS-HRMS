using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [CustomAuthorize("write-employee", "update-employee")]

    public class EducationsController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public EducationsController(DataContext context, UserManager<User> userManager, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
        }

        // GET: Educations
        [HttpGet("All/{empId}")]
        public async Task<IActionResult> GetAll(int empId)
        {
            var data = await _context.Educations
                .Where(x => x.EmpId == empId)
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
                    CountryName = x.Country?.Name,
                    x.DocumentId
                }),
            });
        }

        // GET: Educations/Single/5
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
                    CountryName = data.Country?.Name,
                    data.DocumentId
                }
            });
        }

        // Post: Educations/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] AddInputModel input)
        {
            DateOnly startDate = DateOnlyHelper.ParseDateOrNow(input.StartDate);
            DateOnly endDate = DateOnlyHelper.ParseDateOrNow(input.EndDate);

            EmpDocument document = new();

            if (input.File != null)
            {
                string filename = input.File.FileName;

                document = new()
                {
                    FileName = filename,
                    FileDescription = input.CertificateName,
                    FileExtension = Path.GetExtension(filename),
                    Remarks = input.CertificateName,
                };

                _context.Add(document);
                await _context.SaveChangesAsync();

                string directoryPath = Path.Combine(Folder.EmpDocuments, document.Id.ToString());

                string filePath = Path.Combine(directoryPath, filename);

                string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                Directory.CreateDirectory(fullDirectoryPath);

                using (var stream = System.IO.File.Create(fullFilePath))
                {
                    await input.File.CopyToAsync(stream);
                };
            }

            Education data = new()
            {
                EmpId = input.EmpId,
                EducationLevelId = input.EducationLevelId,
                CertificateName = input.CertificateName,
                StartDate = startDate,
                EndDate = endDate,
                Subject = input.Subject,
                Institute = input.Institute,
                FinalGrade = input.FinalGrade,
                University = input.University,
                CountryId = input.CountryId,
                DocumentId = input.File != null ? document.Id : null,

                //default
                Flag = 0
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: Educations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] UpdateInputModel input)
        {
            var data = await _context.Educations
                .Include(x => x.Document)
                .FirstOrDefaultAsync(c => c.Id == id);

            DateOnly startDate = DateOnlyHelper.ParseDateOrNow(input.StartDate);
            DateOnly endDate = DateOnlyHelper.ParseDateOrNow(input.EndDate);

            EmpDocument document = new();

            if (input.File != null)
            {
                string filename = input.File.FileName;

                if (data.DocumentId != null)
                {
                    string directoryPath = Path.Combine(Folder.EmpDocuments, data.DocumentId.ToString());

                    string filePath = Path.Combine(directoryPath, data.Document?.FileName);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    System.IO.File.Delete(fullFilePath);

                    string newFilePath = Path.Combine(directoryPath, filename);

                    string newFullFilePath = Path.Combine(_config["FilePath"], newFilePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(newFullFilePath))
                    {
                        await input.File.CopyToAsync(stream);
                    };

                    data.Document.FileName = filename;
                    data.Document.FileExtension = Path.GetExtension(filename);
                }
                else
                {
                    document = new()
                    {
                        FileName = filename,
                        FileDescription = input.CertificateName,
                        FileExtension = Path.GetExtension(filename),
                        Remarks = input.CertificateName,
                    };

                    _context.Add(document);
                    await _context.SaveChangesAsync();

                    string directoryPath = Path.Combine(Folder.EmpDocuments, document.Id.ToString());

                    string filePath = Path.Combine(directoryPath, filename);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(fullFilePath))
                    {
                        await input.File.CopyToAsync(stream);
                    };
                }
            }

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
            public IFormFile File { get; set; }

        }

        public class AddInputModel : BaseInputModel 
        { 
            public int EmpId { get; set; }
        }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

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
                    .MustBeDateAfterOrEqual(x => x.StartDate, "Start Date");

                RuleFor(x => x.Subject)
                    .NotEmpty();

                RuleFor(x => x.Institute)
                    .NotEmpty();

                RuleFor(x => x.CountryId)
                    .IdMustExist(_context.Countries.AsQueryable())
                    .Unless(x => x.CountryId is null or 0);
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
                    .MustBeDateAfterOrEqual(x => x.StartDate, "Start Date");

                RuleFor(x => x.Subject)
                    .NotEmpty();

                RuleFor(x => x.Institute)
                    .NotEmpty();

                RuleFor(x => x.CountryId)
                    .IdMustExist(_context.Countries.AsQueryable())
                    .Unless(x => x.CountryId is null or 0);
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
