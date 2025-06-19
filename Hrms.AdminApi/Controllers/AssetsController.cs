using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [CustomAuthorize("asset-management")]

    public class AssetsController : Controller
    {
        private readonly DataContext _context;

        public AssetsController(DataContext context)
        {
            _context = context;
        }

        // GET: Assets
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, int? empId, int? assetTypeId, string givenDate, string returnDate)
        {
            var query = _context.Assets
                .Include(x => x.Emp)
                .Include(x => x.AssetType)
                .AsQueryable();

            if (empId is not null)
            {
                query = query.Where(b => b.EmpId == empId);
            }

            if (assetTypeId is not null)
            {
                query = query.Where(b => b.AssetTypeId == assetTypeId);
            }

            if (!string.IsNullOrEmpty(givenDate))
            {
                DateOnly date = DateOnlyHelper.ParseDateOrNow(givenDate);
                
                query = query.Where(x => x.GivenDate == date);
            }

            if (!string.IsNullOrEmpty(returnDate))
            {
                DateOnly date = DateOnlyHelper.ParseDateOrNow(returnDate);

                query = query.Where(x => x.ReturnDate == date);
            }

            Expression<Func<Asset, object>> field = sortColumn switch
            {
                "EmpId" => x => x.EmpId,
                "AssetTypeId" => x => x.AssetTypeId,
                "GivenDate" => x => x.GivenDate,
                "ReturnDate" => x => x.ReturnDate,
                "AssetDetails" => x => x.AssetDetails,
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

            var data = await PagedList<Asset>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    EmpId = x.EmpId,
                    EmpCode = x.Emp.CardId,
                    EmpName = x.Emp.FirstName + " " + ((x.Emp.MiddleName + " ") ?? "") + x.Emp.LastName,
                    AssetTypeId = x.AssetTypeId,
                    AssetTypeName = x.AssetType.Name,
                    GivenDate = x.GivenDate,
                    ReturnDate = x.ReturnDate,
                    AssetDetails = x.AssetDetails,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: Assets/5

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Assets
                .Include(x => x.Emp)
                .Include(x => x.AssetType)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                Asset = new
                {
                    EmpId = data.EmpId,
                    EmpCode = data.Emp.CardId,
                    EmpName = data.Emp.FirstName + " " + (data.Emp.MiddleName + " ") ?? "" + data.Emp.LastName,
                    AssetTypeId = data.AssetTypeId,
                    AssetTypeName = data.AssetType.Name,
                    GivenDate = data.GivenDate,
                    ReturnDate = data.ReturnDate,
                    AssetDetails = data.AssetDetails,
                    CreatedAt = data.CreatedAt,
                    UpdatedAt = data.UpdatedAt
                }
            });
        }

        // Post: Assets/Create
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            DateOnly givenDate = DateOnlyHelper.ParseDateOrNow(input.GivenDate);
            DateOnly? returnDate = null;

            if (!string.IsNullOrEmpty(input.ReturnDate))
            {
                returnDate = DateOnlyHelper.ParseDateOrNow(input.ReturnDate);
            }

            Asset data= new()
            {
                EmpId = input.EmpId,
                AssetTypeId = input.AssetTypeId,
                GivenDate = givenDate,
                ReturnDate = returnDate,
                AssetDetails = input.AssetDetails,
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: Assets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Assets.FirstOrDefaultAsync(c => c.Id == id);

            DateOnly givenDate = DateOnlyHelper.ParseDateOrNow(input.GivenDate);
            DateOnly? returnDate = null;

            if (!string.IsNullOrEmpty(input.ReturnDate))
            {
                returnDate = DateOnlyHelper.ParseDateOrNow(input.ReturnDate);
            }

            data.EmpId = input.EmpId;
            data.AssetTypeId = input.AssetTypeId;
            data.GivenDate = givenDate;
            data.ReturnDate = returnDate;
            data.AssetDetails = input.AssetDetails;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Assets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Assets.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }
            
            _context.Assets.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public int EmpId { get; set; }
            public int AssetTypeId { get; set; }
            public string GivenDate { get; set; }
            public string ReturnDate { get; set; }
            public string AssetDetails { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.AssetTypeId)
                    .NotEmpty()
                    .IdMustExist(_context.AssetTypes.AsQueryable());

                RuleFor(x => x.GivenDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.ReturnDate)
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.GivenDate, "Given Date")
                    .Unless(x => string.IsNullOrEmpty(x.ReturnDate));
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

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.AssetTypeId)
                    .NotEmpty()
                    .IdMustExist(_context.AssetTypes.AsQueryable());

                RuleFor(x => x.GivenDate)
                    .NotEmpty()
                    .MustBeDate();

                RuleFor(x => x.ReturnDate)
                    .MustBeDate()
                    .MustBeDateAfterOrEqual(x => x.GivenDate, "Given Date")
                    .Unless(x => string.IsNullOrEmpty(x.ReturnDate));
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Assets.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
