using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.MobileApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class LeavesController : Controller
    {
        private readonly DataContext _context;

        public LeavesController(DataContext context)
        {
            _context = context;
        }

        // GET: Leaves/all
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Leaves.ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Days,
                    x.LeaveMax,
                    x.IsHalfLeave
                })
            });
        }
    }
}
