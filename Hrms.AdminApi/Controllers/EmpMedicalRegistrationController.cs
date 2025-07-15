using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.Results;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [CustomAuthorize("write-employee", "update-employee")]
    public class MedicalRegistrationsController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public MedicalRegistrationsController(DataContext context, UserManager<User> userManager, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
        }

        // GET: MedicalRegistrations/All/EmpId
        [HttpGet("All/{empId}")]
        public async Task<IActionResult> GetAll(int empId)
        {
            var data = await _context.empMedicalRegistrations
                .Where(x => x.EmpId == empId)
                .ToListAsync();


            var docIds = data
                .Where(x => x.DocumentId.HasValue)
                .Select(x => x.DocumentId ?? 0) 
                .ToList();

            var documents = await _context.EmpDocuments
                .Where(d => docIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id);

            var downloadBase = GetDownloadBaseUrl();

            string? GetFileUrl(int? docId, string? fileName)
            {
                if (!docId.HasValue || string.IsNullOrWhiteSpace(fileName))
                    return null;
                try
                {
                    return Uri.UnescapeDataString($"{downloadBase}download/{docId}/{fileName}");
                }
                catch
                {
                    return null;
                }
            }

            var result = data.Select(x => new
            {
                x.Id,
                x.EmpId,
                x.RegistrationNumber,
                x.StartDate,
                x.EndDate,
                x.DocumentId,
                FileName = x.DocumentId.HasValue && documents.ContainsKey(x.DocumentId.Value)
                    ? documents[x.DocumentId.Value].FileName
                    : null,
                MedicalRegistrationFilePath = x.DocumentId.HasValue && documents.ContainsKey(x.DocumentId.Value)
                    ? GetFileUrl(x.DocumentId.Value, documents[x.DocumentId.Value].FileName)
                    : null
            });

            return Ok(new { Data = result });
        }


        // GET: MedicalRegistrations/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.empMedicalRegistrations
                .SingleOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                MedicalRegistration = new
                {
                    data.Id,
                    data.EmpId,
                    data.RegistrationNumber,
                    data.StartDate,
                    data.EndDate,
                    data.DocumentId
                }
            });
        }

        // POST: MedicalRegistrations
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
                    FileDescription = "Medical Registration",
                    FileExtension = Path.GetExtension(filename),
                    Remarks = input.RegistrationNumber
                };

                _context.Add(document);
                await _context.SaveChangesAsync();

                string directoryPath = Path.Combine(Folder.EmpDocuments, document.Id.ToString());
                string filePath = Path.Combine(directoryPath, filename);
                string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);
                string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                Directory.CreateDirectory(fullDirectoryPath);

                using var stream = System.IO.File.Create(fullFilePath);
                await input.File.CopyToAsync(stream);
            }

            EmpMedicalRegistration data = new()
            {
                EmpId = input.EmpId,
                RegistrationNumber = input.RegistrationNumber,
                StartDate = startDate,
                EndDate = endDate,
                DocumentId = input.File != null ? document.Id : null
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: MedicalRegistrations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] UpdateInputModel input)
        {
            var data = await _context.empMedicalRegistrations
                .Include(x => x.Emp)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            DateOnly startDate = DateOnlyHelper.ParseDateOrNow(input.StartDate);
            DateOnly endDate = DateOnlyHelper.ParseDateOrNow(input.EndDate);

            if (input.File != null)
            {
                string filename = input.File.FileName;

                if (data.DocumentId != null)
                {
                    var oldDoc = await _context.EmpDocuments.FindAsync(data.DocumentId);
                    string directoryPath = Path.Combine(Folder.EmpDocuments, oldDoc.Id.ToString());
                    string fullFilePath = Path.Combine(_config["FilePath"], directoryPath, oldDoc.FileName);
                    System.IO.File.Delete(fullFilePath);

                    string newFullFilePath = Path.Combine(_config["FilePath"], directoryPath, filename);
                    Directory.CreateDirectory(Path.Combine(_config["FilePath"], directoryPath));

                    using var stream = System.IO.File.Create(newFullFilePath);
                    await input.File.CopyToAsync(stream);

                    oldDoc.FileName = filename;
                    oldDoc.FileExtension = Path.GetExtension(filename);
                }
                else
                {
                    EmpDocument document = new()
                    {
                        FileName = filename,
                        FileDescription = "Medical Registration",
                        FileExtension = Path.GetExtension(filename),
                        Remarks = input.RegistrationNumber
                    };

                    _context.Add(document);
                    await _context.SaveChangesAsync();

                    string directoryPath = Path.Combine(Folder.EmpDocuments, document.Id.ToString());
                    string filePath = Path.Combine(directoryPath, filename);
                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using var stream = System.IO.File.Create(Path.Combine(fullDirectoryPath, filename));
                    await input.File.CopyToAsync(stream);

                    data.DocumentId = document.Id;
                }
            }

            data.RegistrationNumber = input.RegistrationNumber;
            data.StartDate = startDate;
            data.EndDate = endDate;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: MedicalRegistrations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.empMedicalRegistrations.FindAsync(id);

            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            _context.empMedicalRegistrations.Remove(data);
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
        // ------------ INPUT MODELS ------------
        public class BaseInputModel
        {
            public string RegistrationNumber { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public IFormFile? File { get; set; }
        }

        public class AddInputModel : BaseInputModel
        {
            public int EmpId { get; set; }
        }

        public class UpdateInputModel : BaseInputModel { }
    }
}
