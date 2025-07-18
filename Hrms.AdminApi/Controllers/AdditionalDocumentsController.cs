using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [CustomAuthorize("write-employee", "update-employee")]
    public class AdditionalDocumentsController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public AdditionalDocumentsController(DataContext context, UserManager<User> userManager, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
        }

        [HttpGet("All/{empId}")]
        public async Task<IActionResult> GetAll(int empId)
        {
            var data = await _context.EmpAdditionalDocuments
                .Where(x => x.EmpId == empId)
                .Include(x => x.Document)
                .Include(x => x.DocumentType) // <-- Make sure this navigation exists
                .ToListAsync();

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
                x.DocumentTypeId,
                DocumentTypeName = x.DocumentType?.Name, 
                x.DocumentNumber,
                x.DocumentId,
                FileName = x.Document?.FileName,
                FilePath = GetFileUrl(x.DocumentId, x.Document?.FileName)
            });

            return Ok(new { Data = result });
        }


        // POST: AdditionalDocuments
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] AddInputModel input)
        {
            EmpDocument document = new();

            if (input.File != null)
            {
                string filename = input.File.FileName;

                document = new()
                {
                    FileName = filename,
                    FileDescription = "Additional Document",
                    FileExtension = Path.GetExtension(filename),
                    Remarks = input.DocumentNumber
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

            EmpAdditionalDocument data = new()
            {
                EmpId = input.EmpId,
                DocumentTypeId = input.DocumentTypeId,
                DocumentNumber = input.DocumentNumber,
                DocumentId = input.File != null ? document.Id : null
            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: AdditionalDocuments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] UpdateInputModel input)
        {
            var data = await _context.EmpAdditionalDocuments
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

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
                        FileDescription = "Additional Document",
                        FileExtension = Path.GetExtension(filename),
                        Remarks = input.DocumentNumber
                    };

                    _context.Add(document);
                    await _context.SaveChangesAsync();

                    string directoryPath = Path.Combine(Folder.EmpDocuments, document.Id.ToString());
                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using var stream = System.IO.File.Create(Path.Combine(fullDirectoryPath, filename));
                    await input.File.CopyToAsync(stream);

                    data.DocumentId = document.Id;
                }
            }

            data.DocumentTypeId = input.DocumentTypeId;
            data.DocumentNumber = input.DocumentNumber;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: AdditionalDocuments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.EmpAdditionalDocuments.FindAsync(id);

            if (data == null)
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");

            _context.EmpAdditionalDocuments.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private string GetDownloadBaseUrl()
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
            bool isUat = true;

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
            public int DocumentTypeId { get; set; }
            public string? DocumentNumber { get; set; }
            public IFormFile? File { get; set; }
        }

        public class AddInputModel : BaseInputModel
        {
            public int EmpId { get; set; }
        }

        public class UpdateInputModel : BaseInputModel { }
    }
}
