using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class BloodGroupsController : Controller
    {
        private readonly DataContext _context;

        public BloodGroupsController(DataContext context)
        {
            _context = context;
        }

        // GETALL: Companies
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.BloodGroups.ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name,
                })
            });
        }
    }
}
