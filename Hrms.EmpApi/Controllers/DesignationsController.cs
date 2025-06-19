using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]
    public class DesignationsController : Controller
    {
        private readonly DataContext _context;

        public DesignationsController(DataContext context)
        {
            _context = context;
        }

        // GET: Designations/all
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var query = _context.Designations
                .AsQueryable();


            var data = await query.ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code
                }),
            });
        }
    }
}
