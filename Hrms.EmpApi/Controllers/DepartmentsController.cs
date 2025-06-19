using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class DepartmentsController : Controller
    {
        private readonly DataContext _context;

        public DepartmentsController(DataContext context)
        {
            _context = context;
        }

        // GET: Departments/all
        [HttpGet("All")]
        public async Task<IActionResult> GetAll(string parentName)
        {
            var query = _context.Departments
                .AsQueryable();

            if (!string.IsNullOrEmpty(parentName))
            {
                query = query.Where(x => x.Parent == parentName);
            }

            var data = await query.Where(x => x.FLDType == "D").ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Parent = x.Parent,
                    Name = x.Name,
                    Code = x.Code,
                    Level = x.Level
                })
            });
        }
    }
}
