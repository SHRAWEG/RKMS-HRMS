using System.Net;
using System.Net;
using System.Text;

namespace Hrms.EmpApi.Controllers
{
    [Authorize(Roles = "employee")]
    [Route("[Controller]")]
    [Controller]
    public class DocumentsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public DocumentsController(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string filename)
        {
            var query = _context.Documents
                .Include(x => x.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filename))
            {
                query = query.Where(x => x.Filename.ToLower().Contains(filename.ToLower()));
            }

            Expression<Func<Document, object>> field = sortColumn switch
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

            var data = await PagedList<Document>.CreateAsync(query.AsNoTracking(), page, limit);

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
            var result = await _context.Documents.FindAsync(id);

            if (result == null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id");
            };

            var filePath = Path.Combine(_config["FilePath"], result.FilePath);

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(bytes, "application/pdf", Path.GetFileName(filePath));
        }
    }
}
