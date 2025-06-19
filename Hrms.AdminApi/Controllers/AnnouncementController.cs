using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.HPSF;
using System.Xml.Linq;
using static Hrms.AdminApi.Controllers.AttendancesController;

namespace Hrms.AdminApi.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class AnnouncementsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly string _baseFolder;


        public AnnouncementsController(DataContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _baseFolder = Path.Combine(Directory.GetCurrentDirectory(), appSettings.Value.GalleryFolderPath);
        }


        [HttpGet]
        public async Task<ActionResult> GetAnnouncements()
        {
            var announcements = await _context.Announcements
             .Include(a => a.AnnouncementDocuments)
             .Include(a => a.AnnouncementRecipients)
                 .ThenInclude(ar => ar.UGroup)
             .Include(a => a.AnnouncementRecipients)
                 .ThenInclude(ar => ar.User)
             .ToListAsync();

            var results = announcements.Select(announcement => new
            {
                Announcement = new
                {
                    AnnouncementId = announcement.Id,
                    AnnouncementCategoryId = announcement.AnnouncementCategoryId,
                    Subject = announcement.Subject,
                    Description = announcement.Description,
                    CreatedAt = announcement.CreatedAt,
                    UpdatedAt = announcement.UpdatedAt,
                    AnnouncementDocuments = announcement.AnnouncementDocuments.Select(d => new
                    {
                        DocumentID = d.Id,
                        DocumentType = d.DocumentType,
                        FileName = d.FileName,
                        UploadedBy = d.UploadedBy
                    }),
                    AnnouncementRecipients = announcement.AnnouncementRecipients.Select(r => new
                    {
                        r.UserId,
                        UserName = r.User?.NormalizedUserName,
                        Group = r.UGroup?.Name
                    })
                }
            }).ToList();

            var jsonresult = JsonConvert.SerializeObject(results, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None
            });

            return Ok(jsonresult);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetAnnouncement(int id)
        {

            var announcement = await _context.Announcements
                 .Include(a => a.AnnouncementDocuments)
                 .Include(a => a.AnnouncementRecipients)
                .ThenInclude(a => a.UGroup)
                .Include(a => a.AnnouncementRecipients)
                .ThenInclude(u => u.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
            {
                return NotFound();
            }


            var result = new
            {
                Announcement = new
                {
                    AnnouncementId = announcement.Id,
                    AnnouncementCategoryId = announcement.AnnouncementCategoryId,
                    Subject = announcement?.Subject,
                    Description = announcement?.Description,
                    CreatedAt = announcement?.CreatedAt,
                    UpdatedAt = announcement?.UpdatedAt,
                    AnnouncementDocuments = announcement?.AnnouncementDocuments?.Select(d => new
                    {
                        DocumentID = d.Id,
                        DocumentType = d.DocumentType,
                        FileName = d.FileName,
                        UploadedBy = d.UploadedBy,

                    }),
                    AnnouncementRecipients = announcement?.AnnouncementRecipients?.Select(r => new
                    {
                        r.UserId,
                        UserName = r.User?.NormalizedUserName,
                        Group = r.UGroup?.Name
                    })
                }
            };

            var jsonresult = JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None
            });

            return Ok(jsonresult);

        }


        [HttpPost]
        public async Task<IActionResult> CreateAnnouncement([FromForm] AnnouncementDto announcementDto)
        {
            var invalidDocuments = announcementDto.Documents.Where(doc => !IsValidDocumentType(doc)).ToList();
            if (invalidDocuments.Any())
            {
                return BadRequest(new { message = "Invalid document type. Only jpeg, png, pdf, excel, and word documents are allowed." });
            }

            var announcement = new Announcement
            {
                AnnouncementCategoryId = announcementDto.AnnouncementCategoryId,
                Subject = announcementDto.Subject,
                Description = announcementDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            var announcementDocuments = new List<AnnouncementDocuments>();
            foreach (var document in announcementDto.Documents)
            {
                var documentPath = SaveDocument(document);
                announcementDocuments.Add(new AnnouncementDocuments
                {
                    DocumentType = document.ContentType,
                    FileName = document.FileName,
                    FilePath = documentPath,
                    AnnouncementId = announcement.Id,
                    CreatedAt = DateTime.UtcNow,
                    UploadedBy = User.FindFirstValue(ClaimTypes.Name)
                });
            }

            _context.AnnouncementDocuments.AddRange(announcementDocuments);
            await _context.SaveChangesAsync();

            var recipients = new List<AnnouncementRecipient>();

            if (announcementDto.UserIds != null)
            {
                foreach (var userId in announcementDto.UserIds)
                {
                    recipients.Add(new AnnouncementRecipient { AnnouncementId = announcement.Id, UserId = userId });
                }
            }

            if (announcementDto.GroupIds != null)
            {
                foreach (var groupId in announcementDto.GroupIds)
                {
                    var groupUsers = await _context.UserGroups
                        .Where(ug => ug.GroupId == groupId)
                        .Select(ug => ug.UserId)
                        .ToListAsync();
                    foreach (var userId in groupUsers)
                    {
                        if (!recipients.Any(r => r.UserId == userId)) // Avoid duplicates
                        {
                            recipients.Add(new AnnouncementRecipient { AnnouncementId = announcement.Id, UserId = userId });
                        }
                    }
                }
            }

            _context.AnnouncementRecipients.AddRange(recipients);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Announcement created and sent", id = announcement.Id });
        }

        [HttpPost("{announcementId}/read")]
        public async Task<IActionResult> RecordReadTime(int announcementId, [FromBody] int userId)
        {
            var recipient = await _context.AnnouncementRecipients
                .FirstOrDefaultAsync(ar => ar.AnnouncementId == announcementId && ar.UserId == userId);

            if (recipient == null)
            {
                return NotFound();
            }

            recipient.ReadOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Read time recorded" });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> EditAnnouncement(int id, [FromForm] AnnouncementDto announcementDto)
        {
            var announcement = await _context.Announcements
                .Include(a => a.AnnouncementDocuments)
                .Include(a => a.AnnouncementRecipients)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
            {
                return NotFound();
            }

            var invalidDocuments = announcementDto.Documents.Where(doc => !IsValidDocumentType(doc)).ToList();
            if (invalidDocuments.Any())
            {
                return BadRequest(new { message = "Invalid document type. Only jpeg, png, pdf, excel, and word documents are allowed." });
            }


            announcement.AnnouncementCategoryId = announcementDto.AnnouncementCategoryId;
            announcement.Subject = announcementDto.Subject;
            announcement.Description = announcementDto.Description;
            announcement.UpdatedAt = DateTime.UtcNow;


            _context.AnnouncementDocuments.RemoveRange(announcement.AnnouncementDocuments);
            var announcementDocuments = new List<AnnouncementDocuments>();
            foreach (var document in announcementDto.Documents)
            {
                var documentPath = SaveDocument(document);
                announcementDocuments.Add(new AnnouncementDocuments
                {
                    DocumentType = document.ContentType,
                    FileName = document.FileName,
                    FilePath = documentPath,
                    AnnouncementId = announcement.Id,
                    CreatedAt = DateTime.UtcNow,
                    UploadedBy = User.FindFirstValue(ClaimTypes.Name)
                });
            }

            _context.AnnouncementDocuments.AddRange(announcementDocuments);


            _context.AnnouncementRecipients.RemoveRange(announcement.AnnouncementRecipients);
            var recipients = new List<AnnouncementRecipient>();

            if (announcementDto.UserIds != null)
            {
                foreach (var userId in announcementDto.UserIds)
                {
                    recipients.Add(new AnnouncementRecipient { AnnouncementId = announcement.Id, UserId = userId });
                }
            }

            if (announcementDto.GroupIds != null)
            {
                foreach (var groupId in announcementDto.GroupIds)
                {
                    var groupUsers = await _context.UserGroups
                        .Where(ug => ug.GroupId == groupId)
                        .Select(ug => ug.UserId)
                        .ToListAsync();
                    foreach (var userId in groupUsers)
                    {
                        if (!recipients.Any(r => r.UserId == userId)) // in case of duplicates
                        {
                            recipients.Add(new AnnouncementRecipient { AnnouncementId = announcement.Id, UserId = userId });
                        }
                    }
                }
            }

            _context.AnnouncementRecipients.AddRange(recipients);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Announcement updated", id = announcement.Id });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.AnnouncementDocuments)
                .Include(a => a.AnnouncementRecipients)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
            {
                return NotFound();
            }

            // Delete documents from storage
            foreach (var document in announcement.AnnouncementDocuments)
            {
                DeleteDocument(document.FilePath);
            }

            // Remove related entities from the context
            _context.AnnouncementDocuments.RemoveRange(announcement.AnnouncementDocuments);
            _context.AnnouncementRecipients.RemoveRange(announcement.AnnouncementRecipients);
            _context.Announcements.Remove(announcement);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Announcement deleted" });
        }


        private bool AnnouncementExists(int id)
        {
            return _context.Announcements.Any(e => e.Id == id);
        }


        private string SaveDocument(IFormFile document)
        {

            var uploadsFolder = Path.Combine(_baseFolder, "AnnouncementDocument");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, Guid.NewGuid() + Path.GetExtension(document.FileName));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                document.CopyTo(stream);
            }

            return filePath;
        }

        private void DeleteDocument(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        private bool IsValidDocumentType(IFormFile document)
        {
            var allowedExtensions = new List<string> { ".jpeg", ".jpg", ".png", ".pdf", ".xls", ".xlsx", ".doc", ".docx" };
            var extension = Path.GetExtension(document.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }
    }






    public class AnnouncementDto
    {
        public int AnnouncementCategoryId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public List<IFormFile> Documents { get; set; }
        public List<int> UserIds { get; set; }
        public List<int> GroupIds { get; set; }
    }

}