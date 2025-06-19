using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class BranchesController : Controller
    {
        private readonly DataContext _context;

        public BranchesController(DataContext context)
        {
            _context = context;
        }

        // GETALL: Branches
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Branches.ToListAsync();

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
