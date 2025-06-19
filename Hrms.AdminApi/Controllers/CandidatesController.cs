using Hrms.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class CandidatesController : Controller
    {
        private readonly DataContext _context;
        private readonly string _baseFolder;
        public CandidatesController(DataContext context)
        {
            _context = context;
        }

        // GET: Candidates
        [CustomAuthorize("list-candidate")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, int? jobId, int? stageId, string term)
        {
            var query = _context.Candidates
                .Include(x => x.Job)
                .Include(x => x.Stage)
                .Include(x => x.CreatedByUser)
                .Include(x => x.CandidateSources) 
                    .ThenInclude(cs => cs.Source) 
                .AsQueryable();

            if (jobId is not null && jobId is not 0)
            {
                query = query.Where(x => x.JobId == jobId);
            }

            if (stageId is not null && stageId is not 0)
            {
                query = query.Where(x => x.StageId == stageId);
            }

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(x => x.FirstName!.ToLower().Contains(term.ToLower())
                                     || x.MiddleName!.ToLower().Contains(term.ToLower())
                                     || x.LastName!.ToLower().Contains(term.ToLower())
                                     || x.Email!.ToLower().Contains(term.ToLower())
                                     || x.ContactNumber!.ToLower().Contains(term.ToLower())
                                     || x.Job.Title.ToLower().Contains(term.ToLower()));
            }

            Expression<Func<Candidate, object>> field = sortColumn switch
            {
                "Name" => x => x.FirstName,
                "Email" => x => x.Email,
                "Stage" => x => x.Stage!,
                "Job" => x => x.JobId!,
                "ContactNumber" => x => x.ContactNumber,
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

            var data = await PagedList<Candidate>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Name = Helper.FullName(x.FirstName, x.MiddleName, x.LastName),
                    Email = x.Email,
                    ContactNumber = x.ContactNumber,
                    JobId = x.JobId,
                    JobName = x.Job?.Title,
                    StageId = x.StageId,
                    StageName = x.Stage?.Name,
                    CreatedById = x.CreatedByUserId,
                    CreatedBy = x.CreatedByUser?.UserName,
                    ExpectedDate = x.Job?.EstimatedDate,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Sources = x.CandidateSources.Select(cs => new
                    {
                        CandidateSourceId = cs.Id,
                        Name = cs.Source.Name
                    })
                }),
                data.TotalCount,
                data.TotalPages
            });
        }


        // GETALL: Candidates
        [CustomAuthorize("search-candidate")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Candidates
                .Include(x => x.Job)
                .ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Name = Helper.FullName(x.FirstName, x.MiddleName, x.LastName),
                    JobId = x.JobId,
                    JobName = x.Job?.Title
                })
            });
        }

        // GET: Candidates/5
        [CustomAuthorize("view-candidate")]
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

        [CustomAuthorize("candidate-stage-history")]
        [HttpGet("CandidateStage/{id}")]
        public async Task<IActionResult> GetCandidateStages(int id)
        {
            if (!await _context.Candidates.AnyAsync(x => x.Id == id))
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id.");
            }

            var candidateStages = await _context.CandidateStages
                .Include(x => x.Candidate)
                .Include(x => x.Stage)
                .Include(x => x.ConcernedEmp)
                .Where(x => x.CandidateId == id)
                .OrderBy(x => x.Stage.Step)
                .ToListAsync();

            return Ok(new
            {
                Data = candidateStages.Select(x => new
                {
                    CandidateStageId = x.Id,
                    CandidateId = x.CandidateId,
                    CandidateName = Helper.FullName(x.Candidate.FirstName, x.Candidate.MiddleName, x.Candidate.LastName),
                    StageId = x.StageId,
                    StageName = x.Stage.Name,
                    Step = x.Stage.Step,
                    ConcernedEmpId = x.ConcernedEmpId,
                    ConcernedEmpName = Helper.FullName(x.ConcernedEmp?.FirstName, x.ConcernedEmp?.MiddleName, x.ConcernedEmp?.LastName),
                    ConcernedEmpCode = x.ConcernedEmp?.CardId,
                    ScheduledDate = x.ScheduledDate,
                    OverallRating = x.OverallRating,
                    Remarks = x.Remarks
                })
            });
        }

        //[HttpGet("CandidateStage/Detail/{candidateStageId}")]
        //public async Task<IActionResult> GetDetail(int candidateStageId)
        //{
        //    var data = await _context.CandidateStages
        //        .Include(x => x.Candidate)
        //        .Include(x => x.Stage)
        //        .Include(x => x.ConcernedEmp)
        //        .Where(x => x.Id == candidateStageId)
        //        .FirstOrDefaultAsync();

        //    if (data == null)
        //    {
        //        return ErrorHelper.ErrorResult("Id", "Id is invalid.");
        //    }

        //    return Ok(new
        //    {
        //        CandidateStage = new
        //        {
        //            Id = data.Id,
        //            CandidateId = data.CandidateId,
        //            CandidateName = Helper.FullName(data.Candidate.FirstName, data.Candidate.MiddleName, data.Candidate.LastName),
        //            StageId = data.StageId,
        //            StageName = data.Stage.Name,
        //            Step = data.Stage.Step,
        //            ConcernedEmpId = data.ConcernedEmpId,
        //            ConcernedEmpName = Helper.FullName(data.ConcernedEmp?.FirstName, data.ConcernedEmp?.MiddleName, data.ConcernedEmp?.LastName),
        //            ConcernedEmpCode = data.ConcernedEmp?.CardId,
        //            ScheduledDate = data.ScheduledDate,
        //            OverallRating = data.OverallRating,
        //            Remarks = data.Remarks
        //        }
        //    });
        //}

        // Post: Candidates/Create

        [CustomAuthorize("write-candidate")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var stage = await _context.HiringStages.Where(x => x.Name == "Applied").FirstOrDefaultAsync();

            Candidate data = new()
            {
                FirstName = input.FirstName,
                MiddleName = input.MiddleName,
                LastName = input.LastName,
                Email = input.Email.Trim().ToLower(),
                ContactNumber = input.ContactNumber,
                JobId = input.JobId,
                CoverLetter = input.CoverLetter,
                CurrentCTC = input.CurrentCTC,
                ExpectedCTC = input.ExpectedCTC,
                NoticePeriod = input.NoticePeriod,  
                CreatedByUserId = User.GetUserId(),
                StageId = stage.Id
            };
            CandidateDocument document = new();
            
            if (input.UploadResume != null)
            {
                string filename = input.UploadResume.FileName;
                document = new()
                {
                    FileName = filename,
                    FileDescription = "Upload Resume",
                    FileExtension = Path.GetExtension(filename),
                    Remarks = "Upload Resume",
                    CandidateId = data.Id
                };
                _context.Add(document);
                await _context.SaveChangesAsync();
                string directoryPath = Path.Combine(Folder.Documents, document.Id.ToString());
                string filePath = Path.Combine(directoryPath, filename);
                string fullDirectoryPath = Path.Combine(_baseFolder, directoryPath);
                string fullFilePath = Path.Combine(_baseFolder, filePath);
                Directory.CreateDirectory(fullDirectoryPath);
                using (var stream = System.IO.File.Create(fullFilePath))
                {
                    await input.UploadResume.CopyToAsync(stream);
                };
            }
            if (input.UploadCoverLetter != null)
            {
                string filename = input.UploadCoverLetter.FileName;
                document = new()
                {
                    FileName = filename,
                    FileDescription = "Upload Cover Letter",
                    FileExtension = Path.GetExtension(filename),
                    Remarks = "Upload Cover Letter",
                    CandidateId = data.Id
                };
                _context.Add(document);
                await _context.SaveChangesAsync();
                string directoryPath = Path.Combine(Folder.Documents, document.Id.ToString());
                string filePath = Path.Combine(directoryPath, filename);
                string fullDirectoryPath = Path.Combine(_baseFolder, directoryPath);
                string fullFilePath = Path.Combine(_baseFolder, filePath);
                Directory.CreateDirectory(fullDirectoryPath);
                using (var stream = System.IO.File.Create(fullFilePath))
                {
                    await input.UploadCoverLetter.CopyToAsync(stream);
                };
            }
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Add(data);
                await _context.SaveChangesAsync();

                var stages = await _context.HiringStages.ToListAsync();

                foreach (var s in stages)
                {
                    _context.Add(new CandidateStage
                    {
                        CandidateId = data.Id,
                        StageId = s.Id,
                    });
                }

                CandidateActivityLog activity = new()
                {
                    CandidateId = data.Id,
                    Activity = $"{User.GetUsername()} created the candidate."
                };

                if (input.SourceId is not null && input.SourceId is not 0)
                {
                    CandidateSource candidateSource = new()
                    {
                        CandidateId = data.Id,
                        SourceId = input.SourceId ?? 0
                    };

                    _context.Add(candidateSource);
                }

                _context.Add(activity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok();
            } catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return ErrorHelper.ErrorResult("Id", "Internal server error. Please contact administrator.");
            }
        }

        // PUT: Candidates/5
        [CustomAuthorize("update-candidate")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            var data = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == id);

            data.FirstName = input.FirstName;
            data.MiddleName = input.MiddleName;
            data.LastName = input.LastName;
            data.Email = input.Email.Trim().ToLower();
            data.ContactNumber = input.ContactNumber;
            data.JobId = input.JobId;
            data.CoverLetter = input.CoverLetter;
            data.UpdatedAt = DateTime.UtcNow;

            CandidateActivityLog activity = new()
            {
                CandidateId = data.Id,
                Activity = $"{User.GetUsername()} updated the candidate."
            };

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Candidates/5
        [CustomAuthorize("delete-candidate")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Candidates.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (data.Stage?.Name != "Applied")
            {
                return ErrorHelper.ErrorResult("Id", "Candidate is already in process.");
            }

            var candidateSources = await _context.CandidateSources.Where(x => x.CandidateId == id).ToListAsync();
            var candidateActivityLogs = await _context.CandidateActivityLogs.Where(x => x.CandidateId == id).ToListAsync();

            _context.RemoveRange(candidateSources);
            _context.RemoveRange(candidateActivityLogs);
            _context.Candidates.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: Candidates/AssignJob/1
        [CustomAuthorize("assign-candidate-job")]
        [HttpPost("AssignJob/{id}")]
        public async Task<IActionResult> AssignJob(int id, int JobId)
        {
            var data = await _context.Candidates.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (data is null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id");
            }

            var job = await _context.Jobs.Where(x => x.Id == JobId).FirstOrDefaultAsync();

            if (job is null) 
            {
                return ErrorHelper.ErrorResult("JobId", "Job does not exists.");
            }

            data.JobId = JobId;
            data.UpdatedAt = DateTime.UtcNow;

            _context.Add(new CandidateActivityLog
            {
                CandidateId = data.Id,
                Activity = $"{User.GetUsername()} assigned {job.Title} to the candidate."
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: Candidates/AssignJob/1
        [CustomAuthorize("add-candidate-source")]
        [HttpPost("AddSource/{id}")]
        public async Task<IActionResult> AddSource(int id, int SourceId)
        {
            var data = await _context.Candidates.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (data is null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id");
            }

            var source = await _context.Sources.Where(x => x.Id == SourceId).FirstOrDefaultAsync();

            if (source is null)
            {
                return ErrorHelper.ErrorResult("SourceId", "Source does not exists.");
            }

            if (await _context.CandidateSources.Where(x => x.CandidateId == id && x.SourceId == SourceId).AnyAsync()) 
            {
                return ErrorHelper.ErrorResult("SourceId", "Source already added to the candidate.");
            }

            _context.Add(new CandidateSource
            {
                CandidateId = id,
                SourceId = SourceId,
            });

            _context.Add(new CandidateActivityLog
            {
                CandidateId = data.Id,
                Activity = $"{User.GetUsername()} added source {source.Name} to the candidate."
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: Candidates/AssignJob/1
        [CustomAuthorize("add-candidate-skill")]
        [HttpPost("AddSkill/{id}")]
        public async Task<IActionResult> AddSkill(int id, string skill)
        {
            var data = await _context.Candidates.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (data is null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id");
            }

            if (string.IsNullOrEmpty(skill))
            {
                return ErrorHelper.ErrorResult("Id", "Skill must not be empty");
            }

            if (data.Skills is null)
            {
                data.Skills = new List<string> { skill.Trim() };
            } else
            {
                if (data.Skills.Contains(skill.Trim()))
                {
                    return ErrorHelper.ErrorResult("Id", "Skill already exists");
                }

                data.Skills.Add(skill.Trim());
            }

            _context.Add(new CandidateActivityLog
            {
                CandidateId = data.Id,
                Activity = $"{User.GetUsername()} added skill {skill.Trim()} to the candidate."
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Candidates/5
        [CustomAuthorize("remove-candidate-job")]
        [HttpDelete("RemoveJob/{id}")]
        public async Task<IActionResult> RemoveJob(int id)
        {
            var data = await _context.Candidates.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            _context.Add(new CandidateActivityLog
            {
                CandidateId = data.Id,
                Activity = $"{User.GetUsername()} removed job {data.Job.Title} from the candidate."
            });

            data.JobId = null;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Candidates/5
        [CustomAuthorize("add-candidate-source")]
        [HttpDelete("RemoveSource/{candidateSourceId}")]
        public async Task<IActionResult> RemoveSource(int candidateSourceId)
        {
            var data = await _context.CandidateSources.FindAsync(candidateSourceId);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            _context.Add(new CandidateActivityLog
            {
                CandidateId = data.CandidateId,
                Activity = $"{User.GetUsername()} removed source {data.Source.Name} from the candidate."
            });

            _context.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Candidates/5
        [CustomAuthorize("remove-candidate-skill")]
        [HttpDelete("RemoveSkill/{id}")]
        public async Task<IActionResult> RemoveSkill(int id, string skill)
        {
            var data = await _context.Candidates.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (data.Skills is null)
            {
                return ErrorHelper.ErrorResult("Id", "No skills added");
            }

            if (!data.Skills.Contains(skill.Trim()))
            {
                return ErrorHelper.ErrorResult("Id", "Skill does not exists.");
            }

            data.Skills.Remove(skill);

            _context.Add(new CandidateActivityLog
            {
                CandidateId = data.Id,
                Activity = $"{User.GetUsername()} removed skill {skill} from the candidate."
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        [CustomAuthorize("evaluate-candidate")]
        [HttpPut("Evaluate/{id}")]
        public async Task<IActionResult> Evaluate(int id, int OverallRating, string Remarks)
        {
            var data = await _context.Candidates.FindAsync(id);

            if (data is null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id.");
            }

            data.OverallRating = OverallRating;
            data.Remarks = Remarks;
            data.EvaluatedByUserId = User.GetUserId();

            _context.Add(new CandidateActivityLog
            {
                CandidateId = data.Id,
                Activity = $"{User.GetUsername()} evaluated the candidate."
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        [CustomAuthorize("update-candidate-stage")]
        [HttpGet("Stage/Detail")]
        public async Task<IActionResult> GetStageDetail(int candidateId, int stageId)
        {
            var candidateStage = await _context.CandidateStages
                .Where(x => x.CandidateId == candidateId && x.StageId == stageId)
                .Include(x => x.ConcernedEmp)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                Data = new
                {
                    CandidateId = candidateStage.CandidateId,
                    StageId = candidateStage.StageId,
                    ConcernedEmpId = candidateStage.ConcernedEmpId,
                    ConcernedEmpName = Helper.FullName(candidateStage.ConcernedEmp?.FirstName, candidateStage.ConcernedEmp?.MiddleName, candidateStage.ConcernedEmp?.LastName),
                    ConcernedEmpCode = candidateStage.ConcernedEmp?.CardId,
                    ScheduledDate = candidateStage.ScheduledDate,
                }
            });
        }

        [CustomAuthorize("update-candidate-stage")]
        [HttpPut("UpdateStage/{id}")]
        public async Task<IActionResult> UpdateStage(int id, UpdateStageModel input)
        {
            var candidate = await _context.Candidates
                .Include(x => x.Stage)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            var candidateStage = await _context.CandidateStages.Where(x => x.CandidateId == id && x.StageId == input.StageId).FirstOrDefaultAsync();

            DateOnly? ScheduledDate = null;

            if (!string.IsNullOrEmpty(input.ScheduledDate))
            {
                ScheduledDate = DateOnlyHelper.ParseDateOrNow(input.ScheduledDate);
            }

            if (candidateStage is not null)
            {
                candidateStage.ConcernedEmpId = input.ConcernedPersonEmpId;
                candidateStage.ScheduledDate = ScheduledDate;
            } else
            {
                _context.Add(new CandidateStage
                {
                    CandidateId = id,
                    StageId = input.StageId,
                    ConcernedEmpId = input.ConcernedPersonEmpId,
                    ScheduledDate = ScheduledDate
                });
            }

            _context.Add(new CandidateActivityLog
            {
                CandidateId = candidate.Id,
                Activity = $"Candidate cleared the stage: {candidate.Stage?.Name}"
            });

            candidate.StageId = input.StageId;
            candidate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string ContactNumber { get; set; }
            public int? JobId { get; set; }
            public int? SourceId { get; set; }
            public string CoverLetter { get; set; }
            public IFormFile? UploadResume { get; set; }
            public IFormFile? UploadCoverLetter { get; set; }
            public decimal? CurrentCTC { get; set; }     
            public decimal? ExpectedCTC { get; set; }
            public string? NoticePeriod { get; set; }        
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class UpdateStageModel
        {
            public int StageId { get; set; }
            public int? ConcernedPersonEmpId { get; set; }
            public string ScheduledDate { get; set; }
        }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.FirstName)
                    .NotEmpty();

                RuleFor(x => x.Email)
                    .NotEmpty()
                    .MustBeUnique(_context.Candidates.AsQueryable(), "Email");

                RuleFor(x => x.ContactNumber)
                    .NotEmpty()
                    .MustBeDigits(10)
                    .MustBeUnique(_context.Candidates.AsQueryable(), "ContactNumber");

                RuleFor(x => x.JobId)
                    .IdMustExist(_context.Jobs.AsQueryable())
                    .Unless(x => x.JobId is null);

                RuleFor(x => x.SourceId)
                    .IdMustExist(_context.Sources.AsQueryable())
                    .Unless(x => x.SourceId is null);
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

                RuleFor(x => x.FirstName)
                    .NotEmpty();

                RuleFor(x => x.Email)
                    .NotEmpty()
                    .MustBeUnique(_context.Candidates.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Email");

                RuleFor(x => x.ContactNumber)
                    .NotEmpty()
                    .MustBeDigits(10)
                    .MustBeUnique(_context.Candidates.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "ContactNumber");

                RuleFor(x => x.JobId)
                    .IdMustExist(_context.Jobs.AsQueryable())
                    .Unless(x => x.JobId is null);

                RuleFor(x => x.SourceId)
                    .IdMustExist(_context.Sources.AsQueryable())
                    .Unless(x => x.SourceId is null);
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.Candidates.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }

        public class UpdateStageModelValidator : AbstractValidator<UpdateStageModel>
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

                RuleFor(x => x.ConcernedPersonEmpId)
                    .IdMustExist(_context.EmpDetails.AsQueryable())
                    .Unless(x => x.ConcernedPersonEmpId is null);

                RuleFor(x => x.ScheduledDate)
                    .MustBeDate()
                    .MustBeDateAfterNow()
                    .Unless(x => string.IsNullOrEmpty(x.ScheduledDate));
            }

            protected override bool PreValidate(ValidationContext<UpdateStageModel> context, ValidationResult result)
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
