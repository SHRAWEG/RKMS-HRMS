using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]

    public class RegularisationTypesController : Controller
    {
        private readonly DataContext _context;

        public RegularisationTypesController(DataContext context)
        {
            _context = context;
        }

        // GET: RegularisationTypes
        [HttpGet("All")]
        public async Task<IActionResult> Get()
        {
            var data = await _context.RegularisationTypes.ToListAsync();

            return Ok(new
            {
                Data = data,
            });
        }
    }
}
