using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Hrms.EmpApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "employee")]

    public class CandidatesController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public CandidatesController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Candidates
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, int? jobId, int? stageId, string term)
        {
            var user = await _userManager.GetUserAsync(User);

            var query = _context.CandidateStages
                .Include(x => x.Candidate)
                .Include(x => x.Stage)
                .Include(x => x.Candidate.Job)
                .Include(x => x.Candidate.CreatedByUser)
                .Where(x => x.ConcernedEmpId == user.EmpId)
                .AsQueryable();

            if (jobId is not null && jobId is not 0)
            {
                query = query.Where(x => x.Candidate.JobId == jobId);
            }

            if (stageId is not null && stageId is not 0)
            {
                query = query.Where(x => x.StageId == stageId);
            }

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(x => x.Candidate.FirstName!.ToLower().Contains(term.ToLower())
                                        || x.Candidate.MiddleName!.ToLower().Contains(term.ToLower())
                                        || x.Candidate.LastName!.ToLower().Contains(term.ToLower())
                                        || x.Candidate.Email!.ToLower().Contains(term.ToLower())
                                        || x.Candidate.ContactNumber!.ToLower().Contains(term.ToLower()));
            }

            Expression<Func<CandidateStage, object>> field = sortColumn switch
            {
                "Name" => x => x.Candidate.FirstName,
                "Email" => x => x.Candidate.Email,
                "Stage" => x => x.Candidate.StageId!,
                "ContactNumber" => x => x.Candidate.ContactNumber,
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

            var data = await PagedList<CandidateStage>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    CandidateStageId = x.Id,
                    CandidateId = x.CandidateId,
                    Name = Helper.FullName(x.Candidate.FirstName, x.Candidate.MiddleName, x.Candidate.LastName),
                    Email = x.Candidate.Email,
                    ContactNumber = x.Candidate.ContactNumber,
                    JobId = x.Candidate.JobId,
                    JobName = x.Candidate.Job?.Title,
                    StageId = x.StageId,
                    StageName = x.Stage?.Name,
                    CreatedById = x.Candidate.CreatedByUserId,
                    CreatedBy = x.Candidate.CreatedByUser?.UserName,
                    ExpectedDate = x.Candidate.Job?.EstimatedDate,
                    CreatedAt = x.Candidate.CreatedAt,
                    UpdatedAt = x.Candidate.UpdatedAt
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // GET: Candidates/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Candidates
                .Include(x => x.Job)
                .Include(x => x.Stage)
                .Include(x => x.CreatedByUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            var sources = await _context.CandidateSources.Where(x => x.CandidateId == data.Id)
                .Include(x => x.Candidate)
                .Include(x => x.Source)
                .ToListAsync();

            var activities = await _context.CandidateActivityLogs.Where(x => x.CandidateId == data.Id).ToListAsync();

            return Ok(new
            {
                Candidate = new
                {
                    Id = data.Id,
                    Name = Helper.FullName(data.FirstName, data.MiddleName, data.LastName),
                    FirstName = data.FirstName,
                    MiddleName = data.MiddleName,
                    LastName = data.LastName,
                    ContactNumber = data.ContactNumber,
                    Email = data.Email,
                    JobId = data.JobId,
                    JobTitle = data.Job?.Title,
                    StageId = data.StageId,
                    StageName = data.Stage?.Name,
                    Sources = sources.Select(x => new
                    {
                        CandidateSourceId = x.Id,
                        Name = x.Source.Name,
                    }),
                    Skills = data.Skills,
                    OverallRating = data.OverallRating,
                    EvaluationRemarks = data.Remarks,
                    Activities = activities.Select(x => new
                    {
                        Activity = x.Activity,
                        CreatedAt = x.CreatedAt
                    })
                }
            });
        }

        [HttpGet("Stage/Detail")]
        public async Task<IActionResult> GetStageDetail(int candidateId, int stageId)
        {
            var candidateStage = await _context.CandidateStages
                .Where(x => x.CandidateId == candidateId && x.StageId == stageId)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                Data = new
                {
                    CandidateId = candidateStage.CandidateId,
                    StageId = candidateStage.StageId,
                    OverallRating = candidateStage.OverallRating,
                    Remarks = candidateStage.Remarks
                }
            });
        }

        [HttpPut("Evaluate/{id}")]
        public async Task<IActionResult> UpdateStage(int id, EvaluateInputModel input)
        {
            var user = await _userManager.GetUserAsync(User);
                
            var candidate = await _context.Candidates
                .Include(x => x.Stage)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (candidate == null)
            {
                return ErrorHelper.ErrorResult("Id", "Candidate does not exist.");
            }

            var candidateStage = await _context.CandidateStages.Where(x => x.CandidateId == id && x.StageId == input.StageId && x.ConcernedEmpId == user.EmpId).FirstOrDefaultAsync();

            if (candidateStage is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid or you are not assigned person.");
            }

            candidateStage.OverallRating = input.OverallRating;
            candidateStage.Remarks = input.Remarks;
            candidateStage.UpdatedAt = DateTime.UtcNow;

            _context.Add(new CandidateActivityLog
            {
                CandidateId = candidate.Id,
                Activity = $"{Helper.FullName(user.Emp?.FirstName, user.Emp?.MiddleName, user.Emp?.LastName)} evaluated the candidate for the stage: {candidate.Stage?.Name}"
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        public class EvaluateInputModel
        {
            public int StageId { get; set; }
            public int OverallRating { get; set; }
            public string Remarks { get; set; }
        }
       
        public class UpdateStageModelValidator : AbstractValidator<EvaluateInputModel>
        {
            private readonly DataContext _context;
            private readonly string? _id;

            public UpdateStageModelValidator(DataContext context, IHttpContextAccessor contextAccessor)
            {
                _context = context;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

                RuleFor(x => x.StageId)
                    .NotEmpty()
                    .IdMustExist(_context.HiringStages.AsQueryable());

                RuleFor(x => x.OverallRating)
                    .NotEmpty()
                    .GreaterThan(0)
                    .LessThan(5);

                RuleFor(x => x.Remarks)
                    .NotEmpty();
            }

            protected override bool PreValidate(ValidationContext<EvaluateInputModel> context, ValidationResult result)
            {
                if (_context.Candidates.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
