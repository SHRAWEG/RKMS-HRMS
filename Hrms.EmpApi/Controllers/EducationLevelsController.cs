using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class EducationLevelsController : Controller
    {
        private readonly DataContext _context;

        public EducationLevelsController(DataContext context)
        {
            _context = context;
        }

        // GETALL: EducationLevels
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.EducationLevels.ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name
                })
            });
        }
    }
}
