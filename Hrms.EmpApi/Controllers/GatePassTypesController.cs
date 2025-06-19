using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class GatePassTypesController : Controller
    {
        private readonly DataContext _context;

        public GatePassTypesController(DataContext context)
        {
            _context = context;
        }

        // GETALL: GatePassTypes
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.GatePassTypes.ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }
    }
}
