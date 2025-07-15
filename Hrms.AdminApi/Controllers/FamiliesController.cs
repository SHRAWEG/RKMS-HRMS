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

    public class FamiliesController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
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


        public FamiliesController(DataContext context, UserManager<User> userManager, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
        }


        // GET: Families
        [HttpGet("All/{empId}")]
        public async Task<IActionResult> GetAll(int empId)
        {
            var data = await _context.Families
                .Where(x => x.EmpId == empId)
                .ToListAsync();

            var docIds = data
                .Where(x => x.DocumentId.HasValue)
                .Select(x => x.DocumentId!.Value)
                .ToList();

            var documents = await _context.EmpDocuments
                .Where(d => docIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id);

            var downloadBase = GetDownloadBaseUrl();

            string? GetFileUrl(int docId, string? fileName)
            {
                if (string.IsNullOrWhiteSpace(fileName)) return null;
                return Uri.UnescapeDataString($"{downloadBase}download/{docId}/{fileName}");
            }

            var result = data.Select(x => new
            {
                x.Id,
                x.RelationshipType,
                x.Name,
                x.Gender,
                x.DateOfBirth,
                x.IsWorking,
                x.PlaceOfBirth,
                x.IsNominee,
                x.ContactNumber,
                x.PercentageOfShare,
                x.DocumentId,
                FileName = x.DocumentId.HasValue && documents.ContainsKey(x.DocumentId.Value)
                    ? documents[x.DocumentId.Value].FileName
                    : null,
                NomineeDocumentPath = x.DocumentId.HasValue && documents.ContainsKey(x.DocumentId.Value)
                    ? GetFileUrl(x.DocumentId.Value, documents[x.DocumentId.Value].FileName)
                    : null
            });

            return Ok(new { Data = result });
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
                    data.PlaceOfBirth,
                    data.IsNominee,
                    data.ContactNumber,
                    data.PercentageOfShare,
                    data.DocumentId

                }
            });
        }

        // Post: Families/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] AddInputModel input)
        {
            DateOnly dateOfBirth = DateOnlyHelper.ParseDateOrNow(input.DateOfBirth);
            int? documentId = null;

            if (input.NomineeDocument != null)
            {
                var filename = input.NomineeDocument.FileName;
                var document = new EmpDocument
                {
                    FileName = filename,
                    FileDescription = "Nominee Document",
                    FileExtension = Path.GetExtension(filename),
                    Remarks = input.Name
                };

                _context.EmpDocuments.Add(document);
                await _context.SaveChangesAsync();

                string directoryPath = Path.Combine(Folder.EmpDocuments, document.Id.ToString());
                string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);
                string fullFilePath = Path.Combine(fullDirectoryPath, filename);

                Directory.CreateDirectory(fullDirectoryPath);
                using var stream = System.IO.File.Create(fullFilePath);
                await input.NomineeDocument.CopyToAsync(stream);

                documentId = document.Id;
            }

            Family data = new()
            {
                EmpId = input.EmpId,
                RelationshipType = input.RelationshipType,
                Name = input.Name,
                Gender = input.Gender,
                DateOfBirth = dateOfBirth,
                IsWorking = input.IsWorking,
                PlaceOfBirth = input.PlaceOfBirth,
                IsNominee = input.IsNominee,
                ContactNumber = input.ContactNumber,
                PercentageOfShare = input.PercentageOfShare,
                DocumentId = documentId
            };

            _context.Families.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }


        // PUT: Families/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] UpdateInputModel input)
        {
            var data = await _context.Families.FirstOrDefaultAsync(c => c.Id == id);
            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            DateOnly dateOfBirth = DateOnlyHelper.ParseDateOrNow(input.DateOfBirth);

            // Update basic fields
            data.RelationshipType = input.RelationshipType;
            data.Name = input.Name;
            data.Gender = input.Gender;
            data.DateOfBirth = dateOfBirth;
            data.IsWorking = input.IsWorking;
            data.PlaceOfBirth = input.PlaceOfBirth;
            data.IsNominee = input.IsNominee;
            data.ContactNumber = input.ContactNumber;
            data.PercentageOfShare = input.PercentageOfShare;
            data.UpdatedAt = DateTime.UtcNow;

            // Replace document if new file is uploaded
            if (input.NomineeDocument != null)
            {
                // Delete old file if exists
                if (data.DocumentId.HasValue)
                {
                    var existingDoc = await _context.EmpDocuments.FindAsync(data.DocumentId.Value);
                    if (existingDoc != null)
                    {
                        string existingPath = Path.Combine(_config["FilePath"], Folder.EmpDocuments, existingDoc.Id.ToString(), existingDoc.FileName);
                        if (System.IO.File.Exists(existingPath))
                            System.IO.File.Delete(existingPath);

                        _context.EmpDocuments.Remove(existingDoc);
                        await _context.SaveChangesAsync();
                    }
                }

                // Save new document
                var newDoc = new EmpDocument
                {
                    FileName = input.NomineeDocument.FileName,
                    FileDescription = "Nominee Document",
                    FileExtension = Path.GetExtension(input.NomineeDocument.FileName),
                    Remarks = input.Name
                };

                _context.EmpDocuments.Add(newDoc);
                await _context.SaveChangesAsync();

                string dir = Path.Combine(_config["FilePath"], Folder.EmpDocuments, newDoc.Id.ToString());
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, newDoc.FileName);
                using var stream = System.IO.File.Create(path);
                await input.NomineeDocument.CopyToAsync(stream);

                data.DocumentId = newDoc.Id;
            }

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
        private string GetDownloadBaseUrl()
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
            bool isUat = true; // or inject from config if needed

            if (isUat)
            {
                if (baseUrl.Contains("7129"))
                {
                    baseUrl = baseUrl.Replace("7129", "6002");
                    baseUrl = baseUrl.Replace("::", ":");
                }
                else if (baseUrl.Contains("59.179.16.123"))
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
                }
                else
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host.Value}:6002";
                }
            }

            return $"{baseUrl}/download/";
        }
        public class BaseInputModel
        {
            public string RelationshipType { get; set; }
            public string Name { get; set; }
            public string Gender { get; set; }
            public string DateOfBirth { get; set; }
            public bool IsWorking { get; set; }
            public string? PlaceOfBirth { get; set; }
            public bool IsNominee { get; set; } = false;
            public string? ContactNumber { get; set; }
            public decimal? PercentageOfShare { get; set; }
        }

        public class AddInputModel : BaseInputModel
        {
            public int EmpId { get; set; }
            [FromForm]
            public IFormFile? NomineeDocument { get; set; }
        }

        
        public class UpdateInputModel : BaseInputModel
        {
            [FromForm]
            public IFormFile? NomineeDocument { get; set; } 
        }
        [FromForm]
        public IFormFile? NomineeDocument { get; set; }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

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
