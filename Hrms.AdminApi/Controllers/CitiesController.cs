using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Hrms.AdminApi.Controllers.EmployeesController;
using System.Globalization;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CitiesController : Controller
    {
        private readonly DataContext _context;

        public CitiesController(DataContext context)
        {
            _context = context;
        }

        // GET: Cities
        [CustomAuthorize("list-city")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string cityName, int? stateId)
        {
            var query = _context.Cities
                .Include(x => x.State)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cityName))
            {
                query = query.Where(x => x.Name!.ToLower().Contains(cityName.ToLower()));
            }

            if (stateId != null)
            {
                query = query.Where(x => x.StateId == stateId);
            }

            Expression<Func<City, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "StateName" => x => x.State.Name,
                _ => x => x.Id,
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

            var data = await PagedList<City>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.StateId,
                    StateName = x.State.Name
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        [CustomAuthorize("search-city")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll(int? stateId)
        {
            var query = _context.Cities
                .Include(x => x.State)
                .AsQueryable();

            if (stateId != null)
            {
                query = query.Where(x => x.StateId == stateId);
            }

            var data = await query.ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    StateId = x.StateId,
                    StateName = x.State?.Name,
                    Name = x.Name,
                })
            });
        }

        // GET: Cities/5
        [CustomAuthorize("view-city")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Cities
                .Include(x => x.State)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                City = new
                {
                    data.Id,
                    data.Name,
                    data.StateId,
                    StateName = data.State.Name
                }
            });
        }

        // Post: Cities/Create
        [CustomAuthorize("write-city")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            City data= new()
            {
                Name = input.Name,
                StateId = input.StateId
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: Cities/5
        [CustomAuthorize("update-city")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Cities.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = input.Name;
            data.StateId = input.StateId;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Cities/5
        [CustomAuthorize("delete-city")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Cities.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.Branches.AnyAsync(x => x.CityId == id))
            {
                return ErrorHelper.ErrorResult("Id", "City is already in use.");
            }
            
            _context.Cities.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("DownloadFormat")]
        public async Task<IActionResult> UpdateEmpFormat()
        {
            Type table = typeof(CityHeader);

            byte[] data;

            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader(table);

                    csv.NextRecord();
                }

                data = stream.ToArray();
            }

            return File(data, "text/csv", "Cities.csv");
        }

        [HttpPost("Import")]
        public async Task<IActionResult> Import([FromForm] ImportInputModel input)
        {
            if (Path.GetExtension(input.File.FileName).ToLower() != ".csv")
            {
                return ErrorHelper.ErrorResult("File", "Invalid file type.");
            }

            List<Error> errors = new();

            using (var reader = new StreamReader(input.File.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<CityHeader>().ToList();

                if (!records.Any())
                {
                    return ErrorHelper.ErrorResult("File", "No records found.");
                }

                bool isError = false;
                
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    int i = 2;
                    List<string> errorData = new();

                    foreach(var record in records)
                    {
                        isError = false;

                        var state = await _context.States.FirstOrDefaultAsync(x => x.Code == record.StateCode);

                        if (state == null)
                        {
                            errorData.Add("Invalid State Code.");
                            isError = true;
                        }

                        if (await _context.Cities.AnyAsync(x => x.Name == record.Name))
                        {
                            errorData.Add("Name already exists.");
                            isError = true;
                        }

                        if (isError)
                        {
                            errors.Add(new Error
                            {
                                Record = i,
                                Errors = errorData
                            });

                            i++;

                            continue;
                        }

                        _context.Add(new City
                        {
                            StateId = state.Id,
                            Name = record.Name
                        });

                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return Ok();
                } catch(Exception ex)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(ex.Message);
                }
            }
        }

        public class CityHeader
        {
            [Name("State Code")]
            public string StateCode { get; set; }

            [Name("Name")]
            public string Name { get; set; }
        }

        public class ImportModel
        {
            public IFormFile File { get; set; }
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
            public int StateId { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                Transform(x => x.Name, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.Cities.AsQueryable(), "Name");

                RuleFor(x => x.StateId)
                    .NotEmpty()
                    .IdMustExist(_context.States.AsQueryable());
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

                Transform(x => x.Name, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.Cities.Where(x => x.StateId != int.Parse(_id)).AsQueryable(), "Name");

                RuleFor(x => x.StateId)
                    .NotEmpty()
                    .IdMustExist(_context.States.AsQueryable());
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Cities.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
