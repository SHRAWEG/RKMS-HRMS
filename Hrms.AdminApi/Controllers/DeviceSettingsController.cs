using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class DeviceSettingsController : Controller
    {
        private readonly DataContext _context;

        public DeviceSettingsController(DataContext context)
        {
            _context = context;
        }

        // GET: DeviceSettings
        [HttpGet]
        [CustomAuthorize("list-device")]

        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, 
            string deviceModel, string deviceIp, string attendanceMode)
        {
            var query = _context.DeviceSettings
                .AsQueryable();

            if (!string.IsNullOrEmpty(deviceModel))
            {
                query = query.Where(b => b.DeviceModel!.ToLower().Contains(deviceModel.ToLower()));
            }

            if (!string.IsNullOrEmpty(deviceIp))
            {
                query = query.Where(b => b.DeviceIp!.ToLower().Contains(deviceIp.ToLower()));
            }

            if (!string.IsNullOrEmpty(attendanceMode))
            {
                query = query.Where(b => b.AttendanceMode!.ToLower().Contains(attendanceMode.ToLower()));
            }

            Expression<Func<DeviceSetting, object>> field = sortColumn switch
            {
                "DeviceModel" => x => x.DeviceModel,
                "DeviceIp" => x => x.DeviceIp,
                "ClearDeviceLog" => x => x.ClearDeviceLog,
                "AttendanceMode" => x => x.AttendanceMode,
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

            var data = await PagedList<DeviceSetting>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.DeviceModel,
                    x.DeviceIp,
                    x.PortNumber,
                    x.ClearDeviceLog,
                    x.AttendanceMode,
                    AttendanceModeName = Enumeration.GetAll<AttendanceMode>().Where(y => y.Id == x.AttendanceMode).FirstOrDefault()?.Name
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        // GETALL: DeviceSettings
        [CustomAuthorize("search-device")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.DeviceSettings
                .ToListAsync();

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.DeviceModel,
                    x.DeviceIp,
                    x.PortNumber,
                    x.ClearDeviceLog,
                    x.AttendanceMode,
                    AttendanceModeName = Enumeration.GetAll<AttendanceMode>().Where(y => y.Id == x.AttendanceMode).FirstOrDefault()?.Name
                })
            });
        }

        // GET: DeviceSettings/5
        [CustomAuthorize("view-device")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.DeviceSettings
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok( new
            {
                DeviceSetting = new
                {
                    data.Id,
                    data.DeviceModel,
                    data.DeviceIp,
                    data.PortNumber,
                    data.ClearDeviceLog,
                    data.AttendanceMode,
                    AttendanceModeName = Enumeration.GetAll<AttendanceMode>().Where(y => y.Id == data.AttendanceMode).FirstOrDefault()?.Name,
                    data.LastFetchedDate,
                    data.LastFetchedTime
                }
            });
        }

        // Post: DeviceSettings/Create
        [CustomAuthorize("write-device")]
        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            if (!IPAddress.TryParse(input.DeviceIp, out IPAddress address))
            {
                return ErrorHelper.ErrorResult("DeviceIp", "Invalid Device Ip.");
            }

            DeviceSetting data= new()
            {
                DeviceModel = input.DeviceModel,
                DeviceIp = input.DeviceIp,
                PortNumber = input.PortNumber ?? 4370,
                ClearDeviceLog = input.ClearDeviceLog,
                AttendanceMode = input.AttendanceMode
            };

            _context.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        // PUT: DeviceSettings/5
        [CustomAuthorize("update-device")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateInputModel input)
        {
            if (!IPAddress.TryParse(input.DeviceIp, out IPAddress address))
            {
                return ErrorHelper.ErrorResult("DeviceIp", "Invalid Device Ip.");
            }

            var data = await _context.DeviceSettings.FirstOrDefaultAsync(c => c.Id == id);

            data.DeviceModel = input.DeviceModel;
            data.DeviceIp = input.DeviceIp;
            data.PortNumber = input.PortNumber ?? 4370;
            data.ClearDeviceLog = input.ClearDeviceLog;
            data.AttendanceMode = input.AttendanceMode;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: DeviceSettings/5
        [CustomAuthorize("delete-device")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.DeviceSettings.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.EmpDeviceCodes.AnyAsync(x => x.DeviceId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Device is already in use.");
            }

            _context.DeviceSettings.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class BaseInputModel
        {
            public string DeviceModel { get; set; }
            public string DeviceIp { get; set; }
            public int? PortNumber { get; set; }
            public bool ClearDeviceLog { get; set; }
            public string AttendanceMode { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                Transform(x => x.DeviceModel, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.DeviceSettings.AsQueryable(), "DeviceModel");

                Transform(x => x.DeviceIp, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.DeviceSettings.AsQueryable(), "DeviceIp");

                RuleFor(x => x.AttendanceMode)
                    .NotEmpty()
                    .MustBeIn(Enumeration.GetAll<AttendanceMode>().Select(x => x.Id).ToList());
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

                Transform(x => x.DeviceModel, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.DeviceSettings.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "DeviceModel");

                Transform(x => x.DeviceIp, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.DeviceSettings.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "DeviceIp");

                RuleFor(x => x.AttendanceMode)
                    .NotEmpty()
                    .MustBeIn(Enumeration.GetAll<AttendanceMode>().Select(x => x.Id).ToList());
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.DeviceSettings.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
