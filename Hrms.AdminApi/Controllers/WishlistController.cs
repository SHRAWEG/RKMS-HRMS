using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Hrms.AdminApi.Controllers
{

    //string Wish_Type, DateTime Wish_date, string Wish_Title, string Wish_Template


    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "super-admin, admin")]
    public class WishlistController : Controller
    {
        private readonly DataContext _context;
        public WishlistController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection)
        {
            var query = _context.Wishlist
                .AsQueryable();

            


            Expression<Func<Wishlist, object>> field = sortColumn switch
            {
                "Wish_Type" => x => x.Wish_Type!,
                "Wish_Date" => x => x.Wish_Date!,
                "Wist_Title" => x => x.Wish_Title!,
                "Wish_Template" => x => x.Wish_Template!,
                _ => x => x.Id
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Id);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<Wishlist>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Wish_Type = x.Wish_Type,
                    Wish_Date = x.Wish_Date,
                    Wish_Title = x.Wish_Title,
                    Wish_Template = x.Wish_Template
                }),
                data.TotalCount,
                data.TotalPages
            });
        }



        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Wishlist.ToListAsync();
                

            return Ok(new
            {
                Wishlist = data
            });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Wishlist.FirstOrDefaultAsync(a => a.Id == id);
                

            return Ok(new
            {
                Wishlist = data
            });
        }


        [HttpGet("Wishtype")]
        public async Task<IActionResult> GetByType(string typeofwish)
        {
            var data = await _context.Wishlist.FirstOrDefaultAsync(c => c.Wish_Type == typeofwish);
            
            return Ok(new
            {
                Wishlist = data
            });
        }


        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {


            Wishlist data = new()
            {
                Wish_Title = input.Wish_Title,
                Wish_Date =   input.Wish_Date,
                Wish_Template = input.Wish_Template,
                Wish_Type = input.Wish_Type,

            };

            _context.Add(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Wishlist.FirstOrDefaultAsync(c => c.Id == id);

            data.Wish_Type = input.Wish_Type;
            data.Wish_Date = input.Wish_Date;
            data.Wish_Title = input.Wish_Title;
            data.Wish_Template = input.Wish_Template;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Wishlist.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            _context.Wishlist.Remove(data);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }



    public class AddInputModel
    {

        public string Wish_Type { get; set; }
        public DateTime? Wish_Date { get; set; } 
        public string Wish_Title { get; set; }
        public string Wish_Template { get; set; }
    }

    public class UpdateInputModel : AddInputModel
    {
    }

    public class AddInputModelValidator : AbstractValidator<AddInputModel>
    {
        public AddInputModelValidator()
        {
            Transform(x => x.Wish_Title, v => v?.Trim())
            .NotEmpty();
            Transform(x => x.Wish_Type, v => v?.Trim())
             .NotEmpty();
            Transform(x => x.Wish_Template, v => v?.Trim())
             .NotEmpty();
        }
    }

    public class UpdateInputModelValidator : AbstractValidator<UpdateInputModel>
    {
        private readonly DataContext _context;
        private readonly string? _id;

        public UpdateInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

            Transform(x => x.Wish_Title, v => v?.Trim())
            .NotEmpty();
            Transform(x => x.Wish_Type, v => v?.Trim())
            .NotEmpty();
            Transform(x => x.Wish_Template, v => v?.Trim())
             .NotEmpty();

        }

        protected override bool PreValidate(ValidationContext<UpdateInputModel> context, FluentValidation.Results.ValidationResult result)
        {
            if (_context.Wishlist.Find(int.Parse(_id)) == null)
            {
                result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                return false;
            }

            return true;
        }
    }
}
