using Hrms.Common.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Spreadsheet;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [Authorize(Roles = "super-admin, admin")]
    [ApiController]
    public class ImagesCollectionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly string _baseFolder;
        private readonly string _foldername;
        public ImagesCollectionController(DataContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _baseFolder = Path.Combine(Directory.GetCurrentDirectory(), appSettings.Value.GalleryFolderPath);
            _foldername = appSettings.Value.NameofFolder;

        }



        [HttpGet("{folderId}/All")]
        [Authorize]
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

        [HttpPost]
        public async Task<IActionResult> UploadImages(Guid folderId, [FromForm] List<IFormFile> files)
        {
            const long maxFileSize = 200 * 1024 * 1024; // 200MB in bytes
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "No files uploaded." });
            }

            var folder = await _context.ImagesFolders.FindAsync(folderId);
            if (folder == null)
            {
                return BadRequest(new { message = "Folder does not exist." });
            }

            var folderPath = Path.Combine(_baseFolder, folder.Name);
            if (!Directory.Exists(folderPath))
            {
                return BadRequest(new { message = "Folder path does not exist on the server." });
            }

            foreach (var file in files)
            {
                if (file.Length > maxFileSize)
                {
                    return BadRequest(new { message = $"File '{file.FileName}' exceeds the maximum allowed size of 200MB." });
                }

                var filePath = Path.Combine(folderPath, file.FileName);


                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                //Url = $"{Request.Scheme}:/{Request.Host}/{_foldername}/{img.Folder.Name}/{img.FileName}"
                var image = new ImageCollection
                {
                    Id = Guid.NewGuid(),
                    FolderId = folder.Id,
                    FileName = file.FileName,
                    FilePath = $"{Request.Scheme}://{Request.Host}/{_foldername}/{folder.Name}/{file.FileName}",
                    CreatedAt = DateTime.UtcNow,
                    UploadedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                _context.ImageCollection.Add(image);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Files uploaded successfully." });
        }



        [HttpPut("{folderId}")]
        [Authorize]
        public async Task<IActionResult> RenameImage([FromBody] RenameImageModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewImageName))
            {
                return BadRequest(new { message = "New image name cannot be empty." });
            }

            var image = await _context.ImageCollection.FirstOrDefaultAsync(i => i.Id == model.ImageId);
            if (image == null)
            {
                return BadRequest(new { message = "Image does not exist." });
            }

            var folder = await _context.ImagesFolders.FirstOrDefaultAsync(f => f.Id == image.FolderId);
            if (folder == null)
            {
                return BadRequest(new { message = "Associated folder does not exist." });
            }

            var currentImagePath = Path.Combine(_baseFolder, folder.Name, image.FileName);
            var newImagePath = Path.Combine(_baseFolder, folder.Name, model.NewImageName);

            if (System.IO.File.Exists(newImagePath))
            {
                return BadRequest(new { message = "An image with the new name already exists." });
            }

            try
            {
                System.IO.File.Move(currentImagePath, newImagePath);

                image.FileName = model.NewImageName;
                image.FilePath = newImagePath;
                _context.ImageCollection.Update(image);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Image renamed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while renaming the image: {ex.Message}" });
            }
        }





        [HttpDelete("{folderid}/{ImageId}")]
        public async Task<IActionResult> DeleteImage(Guid folderid, Guid ImageId)
        {
            var image = await _context.ImageCollection.FirstOrDefaultAsync(f => f.Id == ImageId);
            if (image == null)
            {
                return NotFound(new { message = "Image not found." });
            }

            var foldername = await _context.ImagesFolders.FirstOrDefaultAsync(f => f.Id == folderid);

            var imagePath = Path.Combine(_baseFolder, foldername.Name, image.FileName);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _context.ImageCollection.Remove(image);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Images deleted successfully." });
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







    public class RenameImageModel
    {
        public Guid ImageId { get; set; }
        public string? NewImageName { get; set; }
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