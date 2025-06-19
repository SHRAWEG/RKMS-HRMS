using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class StatesController : Controller
    {
        private readonly DataContext _context;

        public StatesController(DataContext context)
        {
            _context = context;
        }

        // GETALL: States
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.States.ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code
                })
            });
        }
    }
}
