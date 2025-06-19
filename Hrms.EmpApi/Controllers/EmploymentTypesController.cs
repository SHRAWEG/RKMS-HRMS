using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]

    public class EmploymentTypesController : Controller
    {
        private readonly DataContext _context;

        public EmploymentTypesController(DataContext context)
        {
            _context = context;
        }

        // GETALL: EmploymentTypes
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.EmploymentTypes.ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }
    }
}
