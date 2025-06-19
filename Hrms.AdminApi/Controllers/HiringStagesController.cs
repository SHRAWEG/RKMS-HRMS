using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class HiringStagesController : Controller
    {
        private readonly DataContext _context;

        public HiringStagesController(DataContext context)
        {
            _context = context;
        }

        // GET: HiringStages
        [CustomAuthorize("list-stage")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name)
        {
            var query = _context.HiringStages.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            Expression<Func<HiringStage, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                _ => x => x.Id
            };

            if (sortDirection == null)
            {
                query = query.OrderBy(p => p.Step);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<HiringStage>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }

        // GETALL: HiringStages
        [CustomAuthorize("search-stage")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.HiringStages
                .OrderBy(x => x.Step)
                .ToListAsync();

            return Ok(new
            {
                Data = data
            });
        }

        // GET: HiringStages/5
        [CustomAuthorize("view-stage")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.HiringStages
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                HiringStage = data
            });
        }

        // Post: HiringStages/Create
        [CustomAuthorize("write-stage")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var hiringStages = await _context.HiringStages.ToListAsync();

            int count = hiringStages.Count(x => !x.IsFixed) + 2;

            //if (!(input.Step > 1 && input.Step <= count))
            //{
            //    return ErrorHelper.ErrorResult("Id", "Stage should be between fixed stages.");
            //}

            var updateData = hiringStages.Where(x => x.Step >= count).ToList();

            foreach (var x in updateData)
            {
                x.Step++;
                x.UpdatedAt = DateTime.UtcNow;
            }

            _context.Add(new HiringStage
            {
                Name = input.Name,
                Step = count
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Seed")]
        public async Task<IActionResult> Seed()
        {
            if (await _context.HiringStages.AnyAsync())
            {
                return Ok();
            }

            var hiringStages = new List<HiringStage>
            {
                new() {
                    Name = "Applied",
                    Step = 1,
                    IsFixed = true
                },
                new() {
                    Name = "Offered",
                    Step = 2,
                    IsFixed = true
                },
                new()
                {
                    Name = "Hired",
                    Step = 3,
                    IsFixed = true
                },
                new()
                {
                    Name = "Rejected",
                    Step = 4,
                    IsFixed = true
                }
            };

            _context.AddRange(hiringStages);

            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: HiringStages/5
        [CustomAuthorize("update-stage")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.HiringStages.FirstOrDefaultAsync(c => c.Id == id);

            if (data.IsFixed)
            {
                return ErrorHelper.ErrorResult("Id", "Fixed hiring stage cannot be edited");
            }

            data.Name = input.Name;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [CustomAuthorize("update-stage-step")]
        [HttpPut("/UpdateStep/{id}")]
        public async Task<IActionResult> UpdateStep(int id, UpdateStepInputModel input)
        {
            var data = await _context.HiringStages.FindAsync(id);

            if (data.IsFixed)
            {
                return ErrorHelper.ErrorResult("Id", "Fixed hiring stage cannot be changed.");
            }

            int count = await _context.HiringStages.CountAsync(x => !x.IsFixed);

            if (input.Step < 1 || input.Step > count)
            {
                return ErrorHelper.ErrorResult("Id", "Stage should be between fixed stages.");
            }

            var hiringStages = await _context.HiringStages.Where(x => !x.IsFixed).ToListAsync();

            if (input.Step > data.Step)
            {
                var updateHiringStages = hiringStages.Where(x => x.Step <= input.Step);

                foreach( var hiringStage in updateHiringStages)
                {
                    hiringStage.Step--;
                    hiringStage.UpdatedAt = DateTime.UtcNow;
                }
            } else
            {
                var updateHiringStages = hiringStages.Where(x => x.Step >= input.Step);

                foreach( var hiringStage in updateHiringStages)
                {
                    hiringStage.Step++;
                    hiringStage.UpdatedAt = DateTime.UtcNow;
                }
            }

            data.Step = input.Step;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: HiringStages/5
        [CustomAuthorize("delete-stage")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.HiringStages.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (data.IsFixed)
            {
                return ErrorHelper.ErrorResult("Id", "Fixed hiring stage cannot be deleted.");
            }

            if (await _context.Candidates.AnyAsync(x => x.StageId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Asset Type is already in use.");
            }

            var updateData = await _context.HiringStages.Where(x => x.Step > data.Step).ToListAsync();

            foreach (var x in updateData)
            {
                x.Step--;
                x.UpdatedAt = DateTime.UtcNow;
            }

            _context.HiringStages.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
        }

        public class AddInputModel : BaseInputModel 
        { 
            //public int Step { get; set; }
        }

        public class UpdateInputModel : BaseInputModel { }

        public class UpdateStepInputModel
        {
            public int Step { get; set; }
        }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                Transform(x => x.Name, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.HiringStages.AsQueryable(), "Name");

                //RuleFor(x => x.Step)
                //    .NotEmpty();
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
                    .MustBeUnique(_context.HiringStages.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Name");
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.HiringStages.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }

        public class UpdateStepInputModelValidator : AbstractValidator<UpdateStepInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public UpdateStepInputModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

                RuleFor(x => x.Step)
                    .NotEmpty();
            }

            protected override bool PreValidate(ValidationContext<UpdateStepInputModel> context, ValidationResult result)
            {
                if (_context.HiringStages.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
