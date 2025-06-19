using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hrms.Common.Models;             // Adjust the namespace for your models
using Hrms.Common.Data;               // Adjust the namespace for your ApplicationDbContext
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LeaveEncashmentController : ControllerBase
    {
        private readonly DataContext _context;

        public LeaveEncashmentController(DataContext context)
        {
            _context = context;
        }

        // GET: api/LeaveEncashment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveEncashmentRequest>>> GetAllRequests()
        {
            var requests = await _context.leaveEncashmentRequests.ToListAsync();
            return Ok(requests);
        }

        // GET: api/LeaveEncashment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveEncashmentRequest>> GetRequestById(int id)
        {
            var request = await _context.leaveEncashmentRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            return Ok(request);
        }

        // POST: api/LeaveEncashment/apply
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyForEncashment([FromBody] LeaveEncashmentRequest request)
        {
            // Validate employee exists and has sufficient EL balance
            var employee = await _context.Leaves.FindAsync(request.EmployeeId);
            if (employee == null)
            {
                return BadRequest("Employee not found.");
            }

            if (employee.EL_Balance <= 0 || employee.EL_Balance < request.RequestedEL)
            {
                return BadRequest("Insufficient EL balance.");
            }

          
            employee.EL_Balance -= request.RequestedEL;

          
            request.Status = "Pending";
            request.CreatedDate = DateTime.UtcNow;

            _context.leaveEncashmentRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok("Request submitted successfully.");
        }

        // PUT: api/LeaveEncashment/hod-approve/5
        [HttpPut("hod-approve/{id}")]
        public async Task<IActionResult> HODApprove(int id, [FromBody] HODApprovalDto dto)
        {
            var request = await _context.leaveEncashmentRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound("Request not found.");
            }

            // Update request with HOD approval decision and remarks.
            request.HOD_Approval = dto.ApprovalStatus;
            request.Remarks = dto.Remarks;
            request.ModifiedDate = DateTime.UtcNow;

            if (dto.ApprovalStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                // Set status to pending HR approval if HOD approves
                request.Status = "Pending HR Approval";
            }
            else if (dto.ApprovalStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                request.Status = "Rejected";

                // Refund the EL balance back to the employee when the request is rejected
                var employee = await _context.Leaves.FindAsync(request.EmployeeId);
                if (employee != null)
                {
                    employee.EL_Balance += request.RequestedEL;
                }
            }
            else
            {
                return BadRequest("Invalid HOD approval status.");
            }

            await _context.SaveChangesAsync();
            return Ok("HOD approval updated successfully.");
        }

        // PUT: api/LeaveEncashment/hr-approve/5
        [HttpPut("hr-approve/{id}")]
        public async Task<IActionResult> HRApprove(int id, [FromBody] HRApprovalDto dto)
        {
            var request = await _context.leaveEncashmentRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound("Request not found.");
            }

            request.HR_Approval = dto.ApprovalStatus;
            request.Remarks = dto.Remarks;
            request.ModifiedDate = DateTime.UtcNow;

            if (dto.ApprovalStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                request.Status = "Approved";

                
                var history = new EncashmentHistory
                {
                    EmployeeId = request.EmployeeId,
                    EncashmentCount = request.RequestedEL,
                    Status = "Approved",
                    EncashmentDate = DateTime.UtcNow
                };
                _context.EncashmentHistories.Add(history);
            }
            else if (dto.ApprovalStatus.Equals("Hold", StringComparison.OrdinalIgnoreCase))
            {
                request.Status = "On Hold";
            }
            else if (dto.ApprovalStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                request.Status = "Rejected";

              
                var employee = await _context.Leaves.FindAsync(request.EmployeeId);
                if (employee != null)
                {
                    employee.EL_Balance += request.RequestedEL;
                }
            }
            else
            {
                return BadRequest("Invalid HR approval status.");
            }

            await _context.SaveChangesAsync();
            return Ok("HR approval updated successfully.");
        }

        // GET: api/LeaveEncashment/history/5
        [HttpGet("history/{employeeId}")]
        public async Task<ActionResult<IEnumerable<EncashmentHistory>>> GetEncashmentHistory(int employeeId)
        {
            var history = await _context.EncashmentHistories
                .Where(h => h.EmployeeId == employeeId)
                .AsNoTracking()
                .ToListAsync();
            return Ok(history);
        }

        
        public class HODApprovalDto
        {
            public string ApprovalStatus { get; set; } 
            public string Remarks { get; set; }
        }

        // DTO for HR Approval
        public class HRApprovalDto
        {
            public string ApprovalStatus { get; set; } 
            public string Remarks { get; set; }
        }
    }
}
