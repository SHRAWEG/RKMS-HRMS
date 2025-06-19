using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class UniformTypesController : Controller
    {
        private readonly DataContext _context;

        public UniformTypesController(DataContext context)
        {
            _context = context;
        }

        // GETALL: UniformTypes
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.UniformTypes.ToListAsync();

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
