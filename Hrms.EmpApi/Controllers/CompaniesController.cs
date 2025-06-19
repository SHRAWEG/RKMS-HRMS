using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class CompaniesController : Controller
    {
        private readonly DataContext _context;

        public CompaniesController(DataContext context)
        {
            _context = context;
        }

        // GETALL: Companies
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Companies.ToListAsync();

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
