using System.Net;
using System.Text;

namespace Hrms.AdminApi.Controllers
{
    [Route("[Controller]")]
    [Controller]

    public class PdfServiceDocumentsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public PdfServiceDocumentsController(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string filename)
        {
            var query = _context.PdfServiceDocuments
                .Include(x => x.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filename))
            {
                query = query.Where(x => x.Filename.ToLower().Contains(filename.ToLower()));
            }

            Expression<Func<PdfServiceDocument, object>> field = sortColumn switch
            {
                "Filename" => x => x.Filename,
                "UploadedAt" => x => x.UploadedAt,
                _ => x => x.UploadedAt
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Id);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<PdfServiceDocument>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Filename,
                    x.UploadedAt,
                    x.UserId,
                    x.User.UserName,
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        [HttpGet("Download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var result = await _context.PdfServiceDocuments.FindAsync(id);

            if (result == null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id");
            };

            var filePath = Path.Combine(_config["FilePath"], result.FilePath);

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(bytes, "application/pdf", Path.GetFileName(filePath));
        }

        [HttpPost("Import")]
        public async Task<IActionResult> Upload([FromForm] ImportInputModel input)
        {
            List<string> fileNames = new();
            List<string> Errors = new();

            foreach (IFormFile file in input.Files)
            {
                fileNames.Add(file.FileName);
            }

            var uploadedFiles = await _context.PdfServiceDocuments
                .Where(x => fileNames.Any(y => y == x.Filename))
                .ToListAsync();

            foreach (IFormFile file in input.Files)
            {
                string filename = file.FileName;

                if (uploadedFiles.Any(x => x.Filename == filename))
                {
                    continue;
                }

                if (Path.GetExtension(filename).ToLower() != ".pdf")
                {
                    return ErrorHelper.ErrorResult("File", filename + " file does not have a valid file type");
                }

                Document import = new()
                {
                    Filename = filename,
                    FilePath = "status running",
                    UserId = User.GetUserId()
                };

                _context.Add(import);
                await _context.SaveChangesAsync();

                string directoryPath = Path.Combine(Folder.PdfServiceDocuments, import.Id.ToString());

                string filePath = Path.Combine(directoryPath, filename);

                string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                Directory.CreateDirectory(fullDirectoryPath);

                using (var stream = System.IO.File.Create(fullFilePath))
                {
                    await file.CopyToAsync(stream);
                }

                import.FilePath = filePath;

                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Errors
            });
        }

        public class ImportInputModel
        {
            public List<IFormFile> Files { get; set; }
        }
    }
}
