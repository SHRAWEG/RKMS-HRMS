using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hrms.AdminApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewController : ControllerBase
    {
        #region
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<InterviewController> _logger;
        private readonly HttpClient _httpClient;
        private readonly string smtpServer = "smtp.office365.com";
        private readonly int smtpPort = 587;
        private readonly string smtpUsername = "hrmshelpdesk@bharuwasolutions.com";
        private readonly string smtpPassword = "Ruf02214";
        public InterviewController(DataContext context, IConfiguration config, ILogger<InterviewController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        #endregion

        #region
        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleInterview([FromBody] InterviewRequest request)
        {
            if (!ModelState.IsValid || request == null || string.IsNullOrEmpty(request.CandidateEmail))
                return BadRequest("Invalid interview details.");

            try
            {
                string stageName = await GetStageNameById(request.StageId);
                string subject = $"{stageName} Interview - {request.Subject}";
                var meetingLink = await CreateTeamsMeeting(request);

                var emailTasks = new List<Task>
        {
            SendEmail(request.CandidateEmail, request.Subject, meetingLink, request.StartTime, request.EndTime, "Candidate")
        };

                var interview = new Interview
                {
                    CandidateId = request.CandidateId,
                    CreatedById = request.CreatedBy > 0 ? request.CreatedBy : null,
                    CandidateEmail = request.CandidateEmail,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Subject = request.Subject,
                    MeetingLink = meetingLink,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Interviews.Add(interview);
                await _context.SaveChangesAsync();

              
                var attendanceIdsList = request.AttendanceIds?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id, out int parsedId) ? parsedId : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList() ?? new List<int>();

                if (attendanceIdsList.Any())
                {
                    var attendeeEmails = await _context.EmpDetails
                        .Where(e => attendanceIdsList.Contains(e.Id))
                        .Select(e => e.Email)
                        .ToListAsync();

                    emailTasks.AddRange(attendeeEmails.Select(email =>
                        SendEmail(email, request.Subject, meetingLink, request.StartTime, request.EndTime, "Attendee")));
                }

                await Task.WhenAll(emailTasks);
                return Ok(new { message = "Interview scheduled successfully.", interviewId = interview.Id, InterviewLink = interview.MeetingLink });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling interview.");
                return StatusCode(500, "Internal Server Error.");
            }
        }


        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateInterview(int id, [FromBody] InterviewRequest request)
        {
            if (request == null || !ModelState.IsValid)
                return BadRequest("Invalid interview details.");

            try
            {
                var interview = await _context.Interviews.FindAsync(id);
                if (interview == null)
                    return NotFound("Interview not found.");

                string stageName = await GetStageNameById(request.StageId);
                interview.Subject = $"{stageName} Interview - {request.Subject}";
                interview.CandidateId = request.CandidateId;
                interview.CreatedById = request.CreatedBy;
                interview.CandidateEmail = request.CandidateEmail;
                interview.StartTime = request.StartTime;
                interview.EndTime = request.EndTime;
                interview.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(interview.MeetingLink) &&
                    (interview.StartTime != request.StartTime || interview.EndTime != request.EndTime))
                {
                    interview.MeetingLink = await RescheduleTeamsMeeting(request);
                }

                var existingAttendance = await _context.interviewAttendeces
                    .Where(a => a.InterviewId == id)
                    .ToListAsync();
                _context.interviewAttendeces.RemoveRange(existingAttendance);

                var attendanceIdsList = request.AttendanceIds?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id, out int parsedId) ? parsedId : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList() ?? new List<int>();

                if (attendanceIdsList.Any())
                {
                    var newAttendanceRecords = attendanceIdsList.Select(attendeeId => new InterviewAttendece
                    {
                        attendeesId = attendeeId,
                        InterviewId = id
                    }).ToList();

                    _context.interviewAttendeces.AddRange(newAttendanceRecords);
                }

                await _context.SaveChangesAsync();

                var emailTasks = new List<Task>
        {
            SendEmail(request.CandidateEmail, interview.Subject, interview.MeetingLink, request.StartTime, request.EndTime, "Candidate")
        };

                if (attendanceIdsList.Any())
                {
                    var attendeeEmails = await _context.EmpDetails
                        .Where(e => attendanceIdsList.Contains(e.Id))
                        .Select(e => e.Email)
                        .ToListAsync();

                    emailTasks.AddRange(attendeeEmails.Select(email =>
                        SendEmail(email, interview.Subject, interview.MeetingLink, request.StartTime, request.EndTime, "Attendee")));
                }

                await Task.WhenAll(emailTasks);

                return Ok(new { message = "Interview updated successfully.", interviewId = interview.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating interview.");
                return StatusCode(500, "Internal Server Error.");
            }
        }

        private async Task<string> RescheduleTeamsMeeting(InterviewRequest request)
        {
            try
            {
                var accessToken = await GetAccessToken();

                // Extract first valid attendee email from AttendanceIds
                var attendeeIds = request.AttendanceIds?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id, out int parsedId) ? parsedId : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList() ?? new List<int>();

                var firstAttendeeEmail = await _context.EmpDetails
                    .Where(e => attendeeIds.Contains(e.Id))
                    .Select(e => e.Email)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(firstAttendeeEmail))
                    throw new Exception("No valid attendee found for the meeting.");

                // Use first attendee email instead of `me`
                string graphApiUrl = $"https://graph.microsoft.com/v1.0/users/{firstAttendeeEmail}/events";

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var attendeeEmails = await _context.EmpDetails
                    .Where(e => attendeeIds.Contains(e.Id))
                    .Select(e => new { emailAddress = new { address = e.Email }, type = "required" })
                    .ToListAsync();

                if (!attendeeEmails.Any())
                    throw new Exception("No valid attendees found for the meeting.");

                var jsonContent = new
                {
                    subject = request.Subject,
                    start = new { dateTime = request.StartTime.ToString("o"), timeZone = "UTC" },
                    end = new { dateTime = request.EndTime.ToString("o"), timeZone = "UTC" },
                    attendees = new[]
                    {
                new { emailAddress = new { address = request.CandidateEmail }, type = "required" }
            }.Concat(attendeeEmails).ToArray(),
                    isOnlineMeeting = true,
                    onlineMeetingProvider = "teamsForBusiness"
                };

                var content = new StringContent(JsonConvert.SerializeObject(jsonContent), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(graphApiUrl, content);

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Teams API Response: {responseBody}");

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Error rescheduling Teams meeting: {response.StatusCode} - {responseBody}");

                var jsonResponse = JsonConvert.DeserializeObject<JObject>(responseBody);
                return jsonResponse?["onlineMeeting"]?["joinUrl"]?.ToString() ?? throw new Exception("Join URL not found in response.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Teams meeting rescheduling failed: {ex.Message}");
                return null;
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetInterviews()
        {
            var interviews = await _context.Interviews
                .Select(i => new
                {
                    i.Id,
                    i.CandidateId,
                    AttendanceIds = _context.interviewAttendeces
                        .Where(a => a.InterviewId == i.Id)
                        .Select(a => a.attendeesId)
                        .ToList(),
                    i.CreatedById,
                    i.CandidateEmail,
                    i.InterviewerEmail,
                    i.StartTime,
                    i.EndTime,
                    i.Subject,
                    i.MeetingLink,
                    i.CreatedAt,
                    i.UpdatedAt
                })
                .ToListAsync();

            return Ok(interviews);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInterview(int id)
        {
            var interview = await _context.Interviews
                .Where(i => i.Id == id)
                .Select(i => new
                {
                    i.Id,
                    i.CandidateId,
                    AttendanceIds = _context.interviewAttendeces
                        .Where(a => a.InterviewId == i.Id)
                        .Select(a => a.attendeesId)
                        .ToList(),
                    i.CreatedById,
                    i.CandidateEmail,
                    i.InterviewerEmail,
                    i.StartTime,
                    i.EndTime,
                    i.Subject,
                    i.MeetingLink,
                    i.CreatedAt,
                    i.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (interview == null)
                return NotFound("Interview not found.");

            return Ok(interview);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInterview(int id)
        {
            var interview = await _context.Interviews.FindAsync(id);
            if (interview == null)
                return NotFound("Interview not found.");

            _context.Interviews.Remove(interview);
            await _context.SaveChangesAsync();

            return Ok("Interview deleted successfully.");
        }

        private async Task<string> CreateTeamsMeeting(InterviewRequest request)
        {
            try
            {
                var accessToken = await GetAccessToken();

           
                var attendeeIds = request.AttendanceIds?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id, out int parsedId) ? parsedId : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList() ?? new List<int>();

                var firstAttendeeEmail = await _context.EmpDetails
                    .Where(e => attendeeIds.Contains(e.Id))
                    .Select(e => e.Email)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(firstAttendeeEmail))
                    throw new Exception("No valid attendee found for the meeting.");

               
                string graphApiUrl = $"https://graph.microsoft.com/v1.0/users/{firstAttendeeEmail}/events";

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var jsonContent = new
                {
                    subject = request.Subject,
                    start = new { dateTime = request.StartTime.ToString("o"), timeZone = "UTC" },
                    end = new { dateTime = request.EndTime.ToString("o"), timeZone = "UTC" },
                    attendees = new[]
                    {
                new { emailAddress = new { address = request.CandidateEmail }, type = "required" },
                new { emailAddress = new { address = firstAttendeeEmail }, type = "required" }
            },
                    isOnlineMeeting = true,
                    onlineMeetingProvider = "teamsForBusiness"
                };

                var content = new StringContent(JsonConvert.SerializeObject(jsonContent), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(graphApiUrl, content);

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Teams API Response: {responseBody}");

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Error creating Teams meeting: {response.StatusCode} - {responseBody}");

                var jsonResponse = JsonConvert.DeserializeObject<JObject>(responseBody);
                return jsonResponse?["onlineMeeting"]?["joinUrl"]?.ToString() ?? throw new Exception("Join URL not found in response.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Teams meeting creation failed: {ex.Message}");
                return null;
            }
        }


        private async Task<string> GetAccessToken()
        {
            var clientId = "9cf30c08-1071-4316-ac78-9a43cdad8b56";
            var clientSecret = "Nee8Q~erjg9.MYE.sIl8R65hjoc5fwVI2MijkcJL";
            var tenantId = "d918974d-4413-4e37-8b2d-2472c222fdbc";
            var tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var requestData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "scope", "https://graph.microsoft.com/.default" },
                { "grant_type", "client_credentials" }
            };

            var requestContent = new FormUrlEncodedContent(requestData);
            var response = await _httpClient.PostAsync(tokenUrl, requestContent);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JObject>(responseContent)["access_token"]?.ToString();
        }

        private async Task SendEmail(string recipientEmail, string subject, string meetingLink, DateTime startTime, DateTime endTime, string recipientType)
        {
            var emailBody = GenerateEmailBody(recipientType, startTime, endTime, meetingLink);

            using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUsername),
                    Subject = subject,
                    Body = emailBody,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(recipientEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

        private string GenerateEmailBody(string recipientType, DateTime startTime, DateTime endTime, string meetingLink)
        {
            return $@"
               Dear {recipientType},
             
               You have an interview scheduled as follows:
             
               Start Time: {startTime}
               End Time: {endTime}
             
               Join the interview via Microsoft Teams using this link:
               {meetingLink}
             
               Best regards,
               Your Company
            ";
        }

        private async Task<string> GetStageNameById(int stageId)
        {
            var stage = await _context.HiringStages.FindAsync(stageId);
            return stage?.Name ?? "Unknown Stage";
        }

        public class InterviewRequest
        {
            public int? CandidateId { get; set; }
            public string? AttendanceIds { get; set; }
            public int? CreatedBy { get; set; }
            public string? CandidateEmail { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string? Subject { get; set; }
            public int StageId { get; set; }
        }
        #endregion
    }
}
