namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "super-admin, admin")]
    public class SalaryHeadCategoriesController : Controller
    {
        private readonly DataContext _context;

        public SalaryHeadCategoriesController(DataContext context)
        {
            _context = context;
        }

        // GET: 
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name, string category)
        {
            var query = _context.SalaryHeadCategories.AsQueryable().Where(b => b.ShowCategory == true && b.FlgUse == true && b.FlgAssign == true);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b => b.Category!.ToLower().Contains(category.ToLower()));
            }

            Expression<Func<SalaryHeadCategory, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "Category" => x => x.Category!,
                _ => x => x.ShcId 
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.ShcId);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<SalaryHeadCategory>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GETALL: 
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.SalaryHeadCategories.AsQueryable().Where(b => b.ShowCategory).ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.ShcId,
                    x.Name,
                    x.Category
                })
            });
        }

        // GET: SalaryHeadCategories/5
        [HttpGet("{shcId}")]
        public async Task<IActionResult> Get(int shcId)
        {
            var data = await _context.SalaryHeadCategories.FirstOrDefaultAsync(x => x.ShcId == shcId);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                SalaryHeadCategory = data
            });
        }

        [HttpPut("{shcId}")]
        public async Task<IActionResult> Edit(int shcId, UpdateInputModel input)
        {
            var data = await _context.SalaryHeadCategories.FirstOrDefaultAsync(c => c.ShcId == shcId);

            data.Name = input.Name;

            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
        }

        public class UpdateInputModel : BaseInputModel { }
    }
}
