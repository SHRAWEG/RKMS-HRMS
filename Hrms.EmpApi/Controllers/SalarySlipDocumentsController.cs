using System.Net;
using System.Text;

namespace Hrms.EmpApi.Controllers
{
    [Authorize(Roles = "employee")]
    [Route("[Controller]")]
    [Controller]
    public class SalarySlipDocumentsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;

        public SalarySlipDocumentsController(DataContext context, IConfiguration config, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string filename, int? year, int? month)
        {
            var user = await _userManager.GetUserAsync(User);

            var query = _context.SalarySlipDocuments
                .Include(x => x.User)
                .Where(x => x.EmpId == user.EmpId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filename))
            {
                query = query.Where(x => x.Filename.ToLower().Contains(filename.ToLower()));
            }

            if (year is not null)
            {
                query = query.Where(x => x.Year == year);
            }

            if (month is not null)
            {
                query = query.Where(X => X.Month == month);
            }

            Expression<Func<SalarySlipDocument, object>> field = sortColumn switch
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
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<SalarySlipDocument>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Filename = x.Filename,
                    UploadedAt = x.UploadedAt,
                    UserId = x.UserId,
                    Username = x.User.UserName,
                    Year = x.Year,
                    Month = x.Month,
                }),
                data.TotalCount,
                data.TotalPages
            });
        }



        [AllowAnonymous]
        [HttpGet("Download")]
        public async Task<IActionResult> Downloadvia(int year, int month)
        {
            var user = await _userManager.GetUserAsync(User);
            //var user = await _context.Users.FirstOrDefaultAsync(a => a.UserName.ToLower().Contains(Userid.ToLower()));

            if (user == null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id");
            };
            
            var result = await _context.SalarySlipDocuments.FirstOrDefaultAsync(a => a.Filename.ToLower().Contains(user.UserName.ToLower()) && a.Year == year && a.Month == month);

            if (result == null)
            {
                return ErrorHelper.ErrorResult("Id", "No salary slip uploaded for this month.");
            }
            
            var filePath = Path.Combine(_config["FilePath"], result.FilePath);

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(bytes, "application/pdf", Path.GetFileName(filePath));
        }
        //[HttpPost("Import")]


        //[HttpGet("Download")]
        //public async Task<IActionResult> Downloadvia(int Year, int Month)
        //{
        //    var user = await _userManager.GetUserAsync(User);

        //    var result = await _context.SalarySlipDocuments.FirstOrDefaultAsync(a => a.EmpId == user.EmpId && a.Year == Year && a.Month == Month);

        //    if (result == null)
        //    {
        //        return ErrorHelper.ErrorResult("Id", "Invalid Id");
        //    };

        //    var filePath = Path.Combine(_config["FilePath"], result.FilePath);

        //    var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

        //    return File(bytes, "application/pdf", Path.GetFileName(filePath));
        //}

    }
}
