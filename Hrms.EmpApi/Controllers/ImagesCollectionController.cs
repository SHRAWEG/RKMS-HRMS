using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Hrms.EmpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class ImagesCollectionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly string _baseFolder;
        public ImagesCollectionController(DataContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _baseFolder = Path.Combine(Directory.GetCurrentDirectory(), appSettings.Value.GalleryFolderPath);
        }
        [HttpGet("{folderId}/All")]
        public async Task<IActionResult> GetImagesInFolder(Guid folderId)
        {
            var folder = await _context.ImagesFolders.FindAsync(folderId);
            if (folder == null)
            {
                return NotFound(new { message = "Folder not found." });
            }

            var images = await _context.ImageCollection
                .Where(i => i.FolderId == folderId)
                .Select(image => new ImageDto
                {
                    Id = image.Id,
                    Name = image.FileName,
                    Path = image.FilePath,
                    UploadedAt = image.CreatedAt,
                    FolderId = image.FolderId
                })
                .ToListAsync();

            return Ok(images);
        }

        [HttpGet()]
        public async Task<IActionResult> GetImages(int page, int limit, string sortColumn, string sortDirection, string name, Guid folderId)
        {

            var query = _context.ImageCollection.Where(f => f.FolderId == folderId).AsQueryable();


            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.FileName!.ToLower().Contains(name.ToLower()));
            }

            Expression<Func<ImageCollection, object>> field = sortColumn switch
            {
                "Name" => x => x.FileName,
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

            var data = await PagedList<ImageCollection>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        [HttpGet("ListImageUrls/{folderId}")]
        public async Task<IActionResult> ListImageUrls(Guid folderId)
        {
            var folder = await _context.ImagesFolders
                .Include(f => f.ImagesCollection)
                .FirstOrDefaultAsync(f => f.Id == folderId);

            if (folder == null)
            {
                return NotFound("Folder not found.");
            }

            var validExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

            var imageUrls = folder.ImagesCollection
                                 .Where(i => validExtensions.Contains(Path.GetExtension(i.FileName).ToLowerInvariant()))
                                 .Select(i => Url.Action("DownloadImage", new { folderId = folderId, id = i.Id }))
                                 .ToList();

            return Ok(imageUrls);
        }

        [HttpGet("DownloadImage/{folderId}/{id}")]
        public async Task<IActionResult> DownloadImage(Guid folderId, Guid id)
        {
            var folder = await _context.ImagesFolders
                .Include(f => f.ImagesCollection)
                .FirstOrDefaultAsync(f => f.Id == folderId);

            if (folder == null)
            {
                return NotFound("Folder not found.");
            }

            var image = folder.ImagesCollection.FirstOrDefault(i => i.Id == id);

            if (image == null)
            {
                return NotFound("Image not found in the folder.");
            }

            string folderPath = Path.Combine(_baseFolder, folder.Name);
            string imagePath = Path.Combine(folderPath, image.FileName);

            var validExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(imagePath).ToLowerInvariant();

            if (!validExtensions.Contains(fileExtension) || !System.IO.File.Exists(imagePath))
            {
                return NotFound("Image not found on the server.");
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(imagePath);
            var contentType = GetContentType(imagePath);

            return File(bytes, contentType, Path.GetFileName(imagePath));
        }

        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
                    {
                        {".jpg", "image/jpeg"},
                        {".jpeg", "image/jpeg"},
                        {".png", "image/png"},
                        {".gif", "image/gif"},
                        {".bmp", "image/bmp"}
                    };
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.TryGetValue(ext, out var contentType) ? contentType : "application/octet-stream";
        }


    }


   




    public class ImageDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime UploadedAt { get; set; }
        public Guid FolderId { get; set; }
    }
}
