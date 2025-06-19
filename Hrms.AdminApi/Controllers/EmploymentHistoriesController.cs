using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Xml.Linq;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [CustomAuthorize("write-employee", "update-employee")]

    public class EmploymentHistoriesController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public EmploymentHistoriesController(DataContext context, UserManager<User> userManager, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
        }

        // GET: EmploymentHistories
        [HttpGet("All/{empId}")]
        public async Task<IActionResult> GetAll(int empId)
        {
            var data = await _context.EmploymentHistories
                .Where(x => x.EmpId == empId)
                .ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Organization,
                    x.FromDate,
                    x.ToDate,
                    x.Designation,
                    x.Location,
                    x.City,
                    x.DocumentId
                }),
            });
        }

        // GET: EmploymentHistories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.EmploymentHistories
                .SingleOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                EmploymentHistory = new
                {
                    data.Id,
                    data.Organization,
                    data.FromDate,
                    data.ToDate,
                    data.Designation,
                    data.Location,
                    data.City,
                    data.DocumentId
                }
            });
        }

        // Post: EmploymentHistories/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] AddInputModel input)
        {
            DateOnly fromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);
            DateOnly toDate = DateOnlyHelper.ParseDateOrNow(input.ToDate);

            EmpDocument document = new();

            if (input.File != null)
            {
                string filename = input.File.FileName;

                document = new()
                {
                    FileName = filename,
                    FileDescription = input.Organization,
                    FileExtension = Path.GetExtension(filename),
                    Remarks = input.Organization,
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

            EmploymentHistory data = new()
            {
                EmpId = input.EmpId,
                Organization = input.Organization,
                FromDate = fromDate,
                ToDate = toDate,
                Designation = input.Designation,
                Location = input.Location,
                City = input.City,
                DocumentId = input.File != null ? document.Id : null,
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: EmploymentHistories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] UpdateInputModel input)
        {
            var data = await _context.EmploymentHistories
                .Include(x => x.Document)
                .FirstOrDefaultAsync(c => c.Id == id);

            DateOnly fromDate = DateOnlyHelper.ParseDateOrNow(input.FromDate);
            DateOnly toDate = DateOnlyHelper.ParseDateOrNow(input.ToDate);

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
                        FileDescription = input.Organization,
                        FileExtension = Path.GetExtension(filename),
                        Remarks = input.Organization,
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

            data.Organization = input.Organization;
            data.FromDate = fromDate;
            data.ToDate = toDate;
            data.Designation = input.Designation;
            data.Location = input.Location;
            data.City = input.City;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: EmploymentHistories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.EmploymentHistories.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            _context.EmploymentHistories.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Organization { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string Designation { get; set; }
            public string Location { get; set; }
            public string City { get; set; }
            public IFormFile File { get; set; }

        }

        public class AddInputModel : BaseInputModel 
        { 
            public int EmpId { get; set; }
        }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            public static DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.Organization)
                    .NotEmpty();

                RuleFor(x => x.FromDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.ToDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.FromDate, "Start Date");

                RuleFor(x => x.Designation)
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

                RuleFor(x => x.Organization)
                    .NotEmpty();

                RuleFor(x => x.FromDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.ToDate)
                    .NotEmpty()
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.FromDate, "Start Date");

                RuleFor(x => x.Designation)
                    .NotEmpty();
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.EmploymentHistories.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
