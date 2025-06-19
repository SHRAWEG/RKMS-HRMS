using Hrms.Common.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

namespace Hrms.AdminApi.Controllers
{
    [Authorize(Roles = "super-admin, admin")]
    [Route("[controller]")]
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


        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] FolderCreationDetails FDetail)
        {
            if (string.IsNullOrWhiteSpace(FDetail.foldername))
            {
                return BadRequest(new { message = "Folder name cannot be empty." });
            }

            var folderPath = Path.Combine(_baseFolder, FDetail.foldername);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);

                var folder = new ImagesFolder
                {
                    Id = Guid.NewGuid(),
                    Name = FDetail.foldername,
                    IsDownloadable = FDetail.IsDownloadable,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.FindFirstValue(ClaimTypes.Name) // or User.Identity.Name if preferred
                };

                _context.ImagesFolders.Add(folder);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Folder created successfully.", folderId = folder.Id });
            }

            return BadRequest(new { message = "Folder already exists." });
        }

        [HttpPut]

        public async Task<IActionResult> RenameFolder([FromBody] RenameFolderModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewFolderName))
            {
                return BadRequest(new { message = "New folder name cannot be empty." });
            }

            var folder = await _context.ImagesFolders.FirstOrDefaultAsync(f => f.Id == model.FolderId);
            if (folder == null)
            {
                return BadRequest(new { message = "Folder does not exist." });
            }

            var currentFolderPath = Path.Combine(_baseFolder, folder.Name);
            var newFolderPath = Path.Combine(_baseFolder, model.NewFolderName);

            if (Directory.Exists(newFolderPath))
            {
                return BadRequest(new { message = "A folder with the new name already exists." });
            }

            try
            {
                Directory.Move(currentFolderPath, newFolderPath);

                folder.Name = model.NewFolderName;
                _context.ImagesFolders.Update(folder);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Folder renamed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while renaming the folder: {ex.Message}" });
            }
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteFolder(Guid Id)
        {
            var folder = await _context.ImagesFolders.Include(f => f.ImagesCollection).FirstOrDefaultAsync(f => f.Id == Id);
            if (folder == null)
            {
                return NotFound(new { message = "Folder not found." });
            }

            // Delete images from file system
            foreach (var image in folder.ImagesCollection)
            {
                var imagePath = Path.Combine(_baseFolder, folder.Name, image.FileName);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Delete folder from file system
            var folderPath = Path.Combine(_baseFolder, folder.Name);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }

            // Remove images from database
            _context.ImageCollection.RemoveRange(folder.ImagesCollection);

            // Remove folder from database
            _context.ImagesFolders.Remove(folder);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Folder and its images deleted successfully." });
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

    public class FolderCreationDetails
    {
        public string foldername { get; set; }
        public bool IsDownloadable { get; set; }

    }
    public class RenameFolderModel
    {
        public Guid FolderId { get; set; }
        public string? NewFolderName { get; set; }
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