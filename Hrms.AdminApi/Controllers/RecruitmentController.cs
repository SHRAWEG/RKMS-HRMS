using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Net.Http;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RecruitmentController : ControllerBase
    {
        private readonly DataContext _context;

        public RecruitmentController(DataContext context)
        {
            _context = context;
            
        }
        [HttpGet("ManPowerReqByDepartment")]
        public async Task<IActionResult> ManPowerReqByDepartment()
        {
            try
            {
                var data = await _context.ManpowerRequisitions
                    .Where(m => m.DepartmentId != null)
                    .GroupBy(m => m.DepartmentId)
                    .Join(
                        _context.Departments,
                        g => g.Key,
                        d => d.Id,
                        (g, d) => new
                        {
                            DepartmentId = d.Id,
                            DepartmentName = d.Name,
                            Count = g.Count()
                        })
                    .ToListAsync();

                if (data == null || !data.Any())
                    return NotFound(new { Message = "No manpower requisitions found." });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        [HttpGet("StageWiseVerification")]
        public async Task<IActionResult> StageWiseVerification()
        {
            try
            {
                var stageWiseCount = await _context.CandidateStages
                    .GroupBy(cs => cs.StageId)
                    .Select(g => new
                    {
                        StageId = g.Key,
                        StageName = _context.HiringStages
                            .Where(hs => hs.Id == g.Key)
                            .Select(hs => hs.Name)
                            .FirstOrDefault(),
                        CandidateCount = g.Count()
                    })
                    .ToListAsync();

                if (stageWiseCount == null || stageWiseCount.Count == 0)
                    return NotFound(new { Message = "No candidates found for any stage." });

                return Ok(stageWiseCount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        [HttpGet("JobWiseStageCount")]
        public async Task<IActionResult> JobWiseStageCount()
        {
            try
            {
                var jobWiseHiredCount = await _context.Candidates
                    .Where(c => c.StageId == 3 && c.JobId != null)
                    .GroupBy(c => c.JobId)
                    .Select(g => new
                    {
                        JobId = g.Key,
                        JobTitle = _context.Jobs
                            .Where(j => j.Id == g.Key)
                            .Select(j => j.Title)
                            .FirstOrDefault(),
                        HiredCandidatesCount = g.Count()
                    })
                    .ToListAsync();

                if (jobWiseHiredCount == null || jobWiseHiredCount.Count == 0)
                    return NotFound(new { Message = "No hired candidates found for any job." });

                return Ok(jobWiseHiredCount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        [HttpGet("GetCandidateSummary")]
        public async Task<IActionResult> GetCandidateSummary()
        {
            try
            {
               
                var totalCandidates = await _context.Candidates.CountAsync();

                var openJobsCount = await _context.Jobs.CountAsync(j => j.Status == "Active");
                var closedJobsCount = await _context.Jobs.CountAsync(j => j.Status == "Inactive");

               
                var hiredCount = await _context.CandidateStages
                    .Join(
                        _context.HiringStages.Where(hs => hs.Name == "Hired"),
                        cs => cs.StageId,
                        hs => hs.Id,
                        (cs, hs) => cs
                    )
                    .CountAsync();

              
                var rejectedCount = await _context.CandidateStages
                    .Join(
                        _context.HiringStages.Where(hs => hs.Name == "Rejected"),
                        cs => cs.StageId,
                        hs => hs.Id,
                        (cs, hs) => cs
                    )
                    .CountAsync();

             
                return Ok(new
                {
                    TotalCandidates = totalCandidates,
                    JobsOpen = openJobsCount,
                    JobsClosed = closedJobsCount,
                    HiredCandidates = hiredCount,
                    RejectedCandidates = rejectedCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal Server Error", Error = ex.Message });
            }
        }


    }
}
