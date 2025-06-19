using System.Net;
using System.Text;

namespace Hrms.AdminApi.Controllers
{
    [Route("[Controller]")]
    [Controller]

    public class EmpDocumentsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public EmpDocumentsController(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("All/{empId}")]
        public async Task<IActionResult> Get(int empId)
        {
            var data = await _context.EmpDocuments
                .ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.FileDescription,
                    x.FileName,
                    x.FileExtension,
                    x.CreatedAt,
                    x.UpdatedAt,
                }),
            });
        }

        [HttpGet("Download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var result = await _context.EmpDocuments.FindAsync(id);

            if (result == null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id");
            };

            var filePath = Path.Combine(_config["FilePath"], Folder.EmpDocuments, result.Id.ToString(), result.FileName);

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(bytes, "application/pdf", Path.GetFileName(filePath));
        }

        //[HttpPost("Import")]
        //public async Task<IActionResult> Upload([FromForm] ImportInputModel input)
        //{
        //    List<string> fileNames = new();
        //    List<string> Errors = new();

        //    foreach (IFormFile file in input.Files)
        //    {
        //        fileNames.Add(file.FileName);
        //    }

        //    var uploadedFiles = await _context.EmpDocuments
        //        .Where(x => fileNames.Any(y => y == x.Filename))
        //        .ToListAsync();

        //    foreach (IFormFile file in input.Files)
        //    {
        //        string filename = file.FileName;

        //        if (uploadedFiles.Any(x => x.Filename == filename))
        //        {
        //            continue;
        //        }

        //        if (Path.GetExtension(filename).ToLower() != ".pdf")
        //        {
        //            return ErrorHelper.ErrorResult("File", filename + " file does not have a valid file type");
        //        }

        //        Document import = new()
        //        {
        //            Filename = filename,
        //            FilePath = "status running",
        //            UserId = User.GetUserId()
        //        };

        //        _context.Add(import);
        //        await _context.SaveChangesAsync();

        //        string directoryPath = Path.Combine(Folder.EmpDocuments, import.Id.ToString());

        //        string filePath = Path.Combine(directoryPath, filename);

        //        string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

        //        string fullFilePath = Path.Combine(_config["FilePath"], filePath);

        //        Directory.CreateDirectory(fullDirectoryPath);

        //        using (var stream = System.IO.File.Create(fullFilePath))
        //        {
        //            await file.CopyToAsync(stream);    
        //        }

        //        import.FilePath = filePath;

        //        await _context.SaveChangesAsync();
        //    }

        //    return Ok(new
        //    {
        //        Errors
        //    });
        //}

        public class ImportInputModel
        {
            public List<IFormFile> Files { get; set; }
        }
    }
}
