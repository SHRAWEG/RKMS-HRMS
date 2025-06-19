using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace Hrms.EmpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly string _baseFolder;
        public FolderController(DataContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _baseFolder = Path.Combine(Directory.GetCurrentDirectory(), appSettings.Value.GalleryFolderPath);
        }

        [HttpGet]
        public async Task<IActionResult> GetFolder(int page, int limit, string sortColumn, string sortDirection, string name)
        {
            var query = _context.ImagesFolders.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            Expression<Func<ImagesFolder, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                _ => x => x.Id
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

            var data = await PagedList<ImagesFolder>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetFolders()
        {
            var folders = await _context.ImagesFolders
                .Select(folder => new FolderDto
                {
                    Id = folder.Id,
                    Name = folder.Name,
                    IsDownloadable = folder.IsDownloadable,
                    CreatedAt = folder.CreatedAt,
                    CreatedBy = folder.CreatedBy
                })
                .ToListAsync();

            return Ok(folders);
        }


        [HttpGet("Downloadzip/{Id}")]

        public async Task<IActionResult> DownloadFolderAsZip(Guid Id)
        {

            var folder = await _context.ImagesFolders
                .Include(f => f.ImagesCollection)
                .FirstOrDefaultAsync(f => f.Id == Id);

            if (folder == null)
            {
                return NotFound("Folder not found.");
            }

            if (folder.IsDownloadable == false)
            {
                return BadRequest(new { message = "Folder is not Downloadable." });
            }

            string folderPath = Path.Combine(_baseFolder, folder.Name);

            if (!Directory.Exists(folderPath))
            {
                return NotFound("Folder not found on the server.");
            }

            var memoryStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in folder.ImagesCollection)
                {
                    string filePath = Path.Combine(folderPath, file.FileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        zipArchive.CreateEntryFromFile(filePath, file.FileName);
                    }
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream, "application/zip", $"{folder.Name}.zip");
        }
    }

    public class FolderDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsDownloadable { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}
