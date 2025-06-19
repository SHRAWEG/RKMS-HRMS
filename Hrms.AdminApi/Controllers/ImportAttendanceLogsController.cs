using CsvHelper;
using CsvHelper.Configuration.Attributes;
using NpgsqlTypes;
using System.Globalization;
using System;
using System.Net;
using System.Text;
using Hrms.Common.Models;
using NPOI.HSSF.Record;

namespace Hrms.AdminApi.Controllers
{
    [Route("[Controller]")]
    [Controller]
    public class ImportAttendanceLogsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly List<string> _empIdPrefixes;

        public ImportAttendanceLogsController(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _empIdPrefixes = new List<string>()
            {
                "D100",
                "D200",
                "F",
                "G"
            };
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, 
            string filename, string attendanceFromDate, string attendanceToDate, string status)
        {
            var query = _context.ImportAttendanceLogs
                .Include(x => x.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filename))
            {
                query = query.Where(x => x.Filename.ToLower().Contains(filename.ToLower()));
            }

            if (!string.IsNullOrEmpty(attendanceFromDate) && !string.IsNullOrEmpty(attendanceToDate))
            {
                DateOnly fromDate = DateOnlyHelper.ParseDateOrNow(attendanceFromDate);
                DateOnly toDate = DateOnlyHelper.ParseDateOrNow(attendanceToDate);

                query = query.Where(x => x.AttendanceDate >= fromDate && x.AttendanceDate <= toDate);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(x => status == "Synced" ? x.IsSynced == true : x.IsSynced == false);
            }

            Expression<Func<ImportAttendanceLog, object>> field = sortColumn switch
            {
                "Filename" => x => x.Filename,
                "UploadedAt" => x => x.UploadedAt,
                "Status" => x => x.Status,
                _ => x => x.UploadedAt
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.UploadedAt);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<ImportAttendanceLog>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    x.Id,
                    x.Filename,
                    x.UploadedAt,
                    x.Status,
                    x.ErrorMessage,
                    x.ErrorTrace,
                    x.IsUploaded,
                    x.IsSynced,
                    x.UserId,
                    x.User.UserName,
                    x.AttendanceDate
                }),
                data.TotalCount,
                data.TotalPages
            });
        }

        [HttpPost("Import")]
        public async Task<IActionResult> Upload([FromForm] ImportInputModel input)
        {
            List<string> fileNames = new();
            List<string> Errors = new();

            foreach (IFormFile file in input.Files)
            {
                fileNames.Add(file.FileName);
            }

            var uploadedFiles = await _context.ImportAttendanceLogs
                .Where(x => fileNames.Any(y => y == x.Filename))
                .Where(x => x.IsUploaded == true)
                .ToListAsync();

            foreach (IFormFile file in input.Files)
            {
                string filename = file.FileName;

                if(uploadedFiles.Any(x => x.Filename == filename && x.IsSynced))
                {
                    continue;
                }

                if(Path.GetExtension(filename).ToLower() != ".txt")
                {
                    return ErrorHelper.ErrorResult("File", filename + " file does not have a valid file type");
                }

                if(uploadedFiles.Any(x => x.Filename == filename && !x.IsSynced))
                {
                    Errors.Add(filename + " file already uploaded. Please sync from the table");

                    continue;
                }

                ImportAttendanceLog import = new()
                {
                    Filename = filename,
                    Status = "running",
                    IsUploaded = false,
                    FilePath = "status running",
                    UserId = User.GetUserId()
                };

                _context.Add(import);
                await _context.SaveChangesAsync();

                string directoryPath = Path.Combine(Folder.AttendanceFiles, import.Id.ToString());

                string filePath = Path.Combine(directoryPath, filename);

                string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                Directory.CreateDirectory(fullDirectoryPath);

                using (var stream = System.IO.File.Create(fullFilePath))
                {
                    await file.CopyToAsync(stream);
                }

                filename = Path.GetFileNameWithoutExtension(filename);

                string textDate = filename
                    [^6..]
                    .Insert(4, "20")
                    .Insert(4, "-")
                    .Insert(2, "-");

                DateOnly date = new();

                try
                {
                    date = DateOnly.ParseExact(textDate, "dd-MM-yyyy");
                }
                catch (Exception ex)
                {
                    _context.Remove(import);
                    await _context.SaveChangesAsync();

                    Errors.Add(filename + "Invalid Date in Filename");

                    continue;
                }

                import.Status = "uploaded";
                import.IsUploaded = true;
                import.FilePath = filePath;
                import.AttendanceDate = date;

                await _context.SaveChangesAsync();

                string empIdPrefix = filename
                    .Remove(filename.Length - 6)
                    .Trim()
                    .Split()[0];

                if (!_empIdPrefixes.Contains(empIdPrefix))
                {
                    empIdPrefix = "";
                }

                List<Attendance> data = new();

                List<string> lines = new();
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    while (reader.Peek() >= 0)
                        lines.Add(reader.ReadLine());
                }

                if (lines.Count == 0) Errors.Add(filename + " file does not contain any data.");

                try
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        string[] splitted = lines[i].Trim().Split();

                        if (splitted.Length == 1)
                        {
                            continue;
                        }

                        var emp = await _context.EmpDetails.Where(x => x.CardId == empIdPrefix + "" + splitted[0]).SingleOrDefaultAsync();

                        Errors.Add(empIdPrefix + "" + splitted[0]);

                        if (emp == null)
                        {
                            continue;
                        }

                        var attendance = await _context.Attendances.Where(x => x.EmpId == emp.Id && x.TransactionDate == date).SingleOrDefaultAsync();

                        if (attendance != null)
                        {
                            TimeOnly attendanceTimeIn = TimeOnly.ParseExact(attendance.InTime ?? "23:59:00", "HH:mm:ss");
                            TimeOnly attendanceTimeOut = TimeOnly.ParseExact(attendance.OutTime ?? "00:00:00", "HH:mm:ss");
                            TimeOnly time = TimeOnly.ParseExact(splitted[1], "HH:mm:ss");

                            if (time < attendanceTimeIn)
                            {
                                attendance.InTime = splitted[1];
                                attendance.TransactionDate = date;
                                attendance.InMode = "fingerprint";
                            } else if (time > attendanceTimeOut)
                            {
                                attendance.OutTime = splitted[1];
                                attendance.TransactionDateOut = date;
                                attendance.OutMode = "fingerprint";
                            }
                        }

                        var syncedEmp = data.Where(x => x.EmpId == emp.Id).SingleOrDefault();

                        if (syncedEmp != null)
                        {
                            syncedEmp.TransactionDateOut = date;
                            syncedEmp.OutTime = splitted[1];
                            syncedEmp.OutMode = "fingerprint";
                        }
                        else
                        {
                            short? WorkHourId;

                            var roster = await _context.Rosters.Where(x => x.Date == date && x.EmpId == emp.Id).FirstOrDefaultAsync();

                            WorkHourId = roster?.WorkHourId;

                            if (roster is null)
                            {
                                var defaultWorkHour = await _context.DefaultWorkHours
                                    .Where(x => x.EmpId == emp.Id || x.EmpId == null && x.DayId == ((short)date.DayOfWeek + 1))
                                    .OrderByDescending(x => x.EmpId)
                                    .FirstOrDefaultAsync();

                                WorkHourId = defaultWorkHour?.WorkHourId;
                            }

                            if (WorkHourId is null)
                            {
                                continue;
                            }

                            data.Add(new Attendance
                            {
                                EmpId = emp.Id,
                                TransactionDate = date,
                                InTime = splitted[1],

                                //Defaults
                                InMode = "fingerprint",
                                AttendanceStatus = 0,
                                FlagIn = false,
                                FlagOut = false,
                                AttendanceType = 0,
                                CheckInMode = 'D',
                                AttendanceId = 0,
                                SignOutTimeStamp = 0,
                                SignInTimeStamp = 0,
                            });
                        }
                    }

                    _context.AddRange(data);

                    import.Status = "Sync Successful";
                    import.IsSynced = true;

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    import.Status = "Sync failed";
                    import.IsUploaded = false;
                    import.ErrorMessage = ex.Message;
                    import.ErrorTrace = ex.StackTrace;

                    System.IO.File.Delete(fullFilePath);

                    await _context.SaveChangesAsync();

                    Errors.Add(filename + "file could not be synced. Please Re-Import the file and try again.");
                }
            }

            return Ok(new
            {
                Errors
            });
        }

        [HttpPost("Import/Csv")]
        public async Task<IActionResult> UploadCsv([FromForm] ImportInputModel input)
        {
            List<string> fileNames = new();
            List<string> Errors = new();
            bool faceerror = false;

            foreach (IFormFile file in input.Files)
            {
                fileNames.Add(file.FileName);
            }

            foreach (IFormFile file in input.Files)
            {
                string filename = file.FileName;

                if (Path.GetExtension(filename).ToLower() != ".csv")
                {
                    return ErrorHelper.ErrorResult("File", filename + " file does not have a valid file type");
                }

                ImportAttendanceLog import = new()
                {
                    Filename = filename,
                    Status = "running",
                    IsUploaded = false,
                    FilePath = "status running",
                    UserId = User.GetUserId()
                };

                _context.Add(import);
                await _context.SaveChangesAsync();

                string directoryPath = Path.Combine(Folder.AttendanceFiles, import.Id.ToString());

                string filePath = Path.Combine(directoryPath, filename);

                string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                Directory.CreateDirectory(fullDirectoryPath);

                using (var stream = System.IO.File.Create(fullFilePath))
                {
                    await file.CopyToAsync(stream);
                };

                import.Status = "uploaded";
                import.IsUploaded = true;
                import.FilePath = filePath;
                import.AttendanceDate = DateOnly.FromDateTime(DateTime.UtcNow);

                await _context.SaveChangesAsync();

                List<Attendance> data = new();

                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<AttendanceHeader>().ToList();

                    if (!records.Any()) Errors.Add(filename + " file does not contain any data.");

                    try
                    {
                        foreach (var record in records)
                        {
                            var emp = await _context.EmpDetails.Where(x => x.CardId == record.EmpCode).SingleOrDefaultAsync();

                            if (emp == null)
                            {
                                Errors.Add(record.EmpCode + " do not exist");
                                continue;
                            } else
                            {
                                var statusId = await _context.EmpLogs.Where(x => x.EmployeeId == emp.Id)
                                                .Join(_context.EmpTransactions, el => el.Id, et => et.Id,
                                                (el, et) => et.StatusId)
                                                .FirstOrDefaultAsync();

                                if (statusId != 1)
                                {
                                    Errors.Add(record.EmpCode + " is not active.");
                                }
                            }

                            if (!DateOnly.TryParseExact(record.Date, "yyyy-MM-dd", out DateOnly date))
                            {
                                Errors.Add(record.EmpCode + "has invalid date format: " + record.Date);
                                faceerror = true;
                                continue;
                            }
                            
                             DateTime yesterdayDate = DateTime.Today.AddDays(-1);
                            if (Convert.ToDateTime(record.Date) >= yesterdayDate)
                            {
                                faceerror = true;
                                Errors.Add(record.EmpCode + "can only allowed to submit his attendance till yesterday, " +
                                    "Meaning today's attendance should not be on the list " + record.Date);
                                continue;
                            }

                            if (!TimeOnly.TryParseExact(record.InOutTime, "HH:mm:ss", out TimeOnly time))
                            {
                                faceerror = true;
                                Errors.Add(record.EmpCode + "has invalid time format: " + record.InOutTime);
                                continue;
                            }

                           

                            WorkHour? workHour;

                            var roster = await _context.Rosters
                                .Where(x => x.Date == date && x.EmpId == emp.Id)
                                .Include(x => x.WorkHour)
                                .FirstOrDefaultAsync();

                            workHour = roster?.WorkHour;

                            if (roster is null)
                            {
                                var defaultWorkHour = await _context.DefaultWorkHours
                                    .Where(x => (x.EmpId == emp.Id || x.EmpId == null) && x.DayId == ((short)date.DayOfWeek + 1))
                                    .OrderBy(x => x.EmpId)
                                    .Include(x => x.WorkHour)
                                    .FirstOrDefaultAsync();

                                workHour = defaultWorkHour?.WorkHour;
                            }

                            if (workHour is null)
                            {
                                Errors.Add(record.EmpCode + "does not have default shift on the date: " + record.Date);
                                continue;
                            }

                            if (workHour.IsFlexible && workHour.IsNightShift)
                            {
                                if (string.IsNullOrEmpty(workHour.NightStartTime) || string.IsNullOrEmpty(workHour.NightEndTime))
                                {
                                    Errors.Add(record.EmpCode + "does not have start time or end time in their shift on the date: " + record.Date);

                                    continue;
                                }

                                TimeOnly nightStartTime = TimeOnly.ParseExact(workHour.NightStartTime ?? "00:00:00", "HH:mm:ss");
                                TimeOnly nightEndtime = TimeOnly.ParseExact(workHour.NightEndTime ?? "00:00:00", "HH:mm:ss");

                                if (time > nightStartTime || time < nightEndtime)
                                {
                                    if (time > nightStartTime)
                                    {
                                        var existingNightAttendance = await _context.Attendances
                                            .Where(x => x.EmpId == emp.Id
                                                    && x.TransactionDate == date)
                                            .Include(x => x.WorkHour)
                                            .FirstOrDefaultAsync();

                                        if (existingNightAttendance is not null)
                                        {
                                            TimeOnly nightAttendanceInTime = TimeOnly.ParseExact(existingNightAttendance.InTime ?? "23:59:00", "HH:mm:ss");
                                            TimeOnly nightAttendanceOutTime = TimeOnly.ParseExact(existingNightAttendance.OutTime ?? "00:00:00", "HH:mm:ss");

                                            if (existingNightAttendance.WorkHour.IsFlexible && existingNightAttendance.WorkHour.IsNightShift)
                                            {
                                                if (time < nightAttendanceInTime)
                                                {
                                                    existingNightAttendance.InTime = time.ToString("HH:mm:ss");
                                                    existingNightAttendance.TransactionDate = date;
                                                    existingNightAttendance.InMode = "fingerprint";

                                                    continue;
                                                }
                                                else if (existingNightAttendance.TransactionDateOut == null ||
                                                         (existingNightAttendance.TransactionDateOut == existingNightAttendance.TransactionDate && time > nightAttendanceOutTime))
                                                {
                                                    existingNightAttendance.OutTime = time.ToString("HH:mm:ss");
                                                    existingNightAttendance.TransactionDateOut = date;
                                                    existingNightAttendance.OutMode = "fingerprint";

                                                    continue;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                        }

                                        var syncedNightEmp = data.Where(x => x.EmpId == emp.Id && x.TransactionDate == date).FirstOrDefault();

                                        if (syncedNightEmp is not null)
                                        {
                                            TimeOnly nightAttendanceInTime = TimeOnly.ParseExact(syncedNightEmp.InTime ?? "23:59:00", "HH:mm:ss");
                                            TimeOnly nightAttendanceOutTime = TimeOnly.ParseExact(syncedNightEmp.OutTime ?? "00:00:00", "HH:mm:ss");

                                            if (time < nightAttendanceInTime)
                                            {
                                                syncedNightEmp.InTime = time.ToString("HH:mm:ss");
                                                syncedNightEmp.TransactionDate = date;
                                                syncedNightEmp.InMode = "fingerprint";

                                                continue;
                                            }
                                            else if (syncedNightEmp.TransactionDateOut == null ||
                                                     (syncedNightEmp.TransactionDateOut == syncedNightEmp.TransactionDate && time > nightAttendanceOutTime))
                                            {
                                                syncedNightEmp.OutTime = time.ToString("HH:mm:ss");
                                                syncedNightEmp.TransactionDateOut = date;
                                                syncedNightEmp.OutMode = "fingerprint";

                                                continue;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }

                                        data.Add(new Attendance
                                        {
                                            WorkHourId = workHour.Id,
                                            EmpId = emp.Id,
                                            TransactionDate = date,
                                            InTime = time.ToString("HH:mm:ss"),

                                            //Default
                                            InMode = "fingerprint",
                                            AttendanceStatus = 0,
                                            FlagIn = false,
                                            FlagOut = false,
                                            AttendanceType = 0,
                                            CheckInMode = 'D',
                                            AttendanceId = 0,
                                            SignOutTimeStamp = 0,
                                            SignInTimeStamp = 0,
                                        });

                                        continue;
                                    }

                                    if (time < nightEndtime)
                                    {
                                        var existingNightAttendance = await _context.Attendances
                                            .Where(x => x.EmpId == emp.Id
                                                    && x.TransactionDate == date.AddDays(-1))
                                            .Include(x => x.WorkHour)
                                            .FirstOrDefaultAsync();

                                        if (existingNightAttendance is not null)
                                        {
                                            TimeOnly nightAttendanceInTime = TimeOnly.ParseExact(existingNightAttendance.InTime ?? "23:59:00", "HH:mm:ss");
                                            TimeOnly nightAttendanceOutTime = TimeOnly.ParseExact(existingNightAttendance.OutTime ?? "00:00:00", "HH:mm:ss");

                                            if (existingNightAttendance.WorkHour.IsFlexible && existingNightAttendance.WorkHour.IsNightShift)
                                            {
                                                if (existingNightAttendance.TransactionDateOut is null || existingNightAttendance.TransactionDateOut != date)
                                                {
                                                    existingNightAttendance.OutTime = time.ToString("HH:mm:ss");
                                                    existingNightAttendance.TransactionDateOut = date;
                                                    existingNightAttendance.OutMode = "fingerprint";

                                                    continue;
                                                }

                                                if (time > nightAttendanceOutTime)
                                                {
                                                    existingNightAttendance.OutTime = time.ToString("HH:mm:ss");
                                                    existingNightAttendance.TransactionDateOut = date;
                                                    existingNightAttendance.OutMode = "fingerprint";

                                                    continue;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                        }

                                        var syncedNightEmp = data.Where(x => x.EmpId == emp.Id && x.TransactionDate == date.AddDays(-1)).FirstOrDefault();

                                        if (syncedNightEmp is not null)
                                        {
                                            TimeOnly nightAttendanceInTime = TimeOnly.ParseExact(syncedNightEmp.InTime ?? "23:59:00", "HH:mm:ss");
                                            TimeOnly nightAttendanceOutTime = TimeOnly.ParseExact(syncedNightEmp.OutTime ?? "00:00:00", "HH:mm:ss");

                                            if (syncedNightEmp.TransactionDateOut is null || syncedNightEmp.TransactionDateOut != date)
                                            {
                                                syncedNightEmp.OutTime = time.ToString("HH:mm:ss");
                                                syncedNightEmp.TransactionDateOut = date;
                                                syncedNightEmp.OutMode = "fingerprint";

                                                continue;
                                            }

                                            if (time > nightAttendanceOutTime)
                                            {
                                                syncedNightEmp.OutTime = time.ToString("HH:mm:ss");
                                                syncedNightEmp.TransactionDateOut = date;
                                                syncedNightEmp.OutMode = "fingerprint";

                                                continue;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }

                                        data.Add(new Attendance
                                        {
                                            WorkHourId = workHour.Id,
                                            EmpId = emp.Id,
                                            TransactionDate = date.AddDays(-1),
                                            TransactionDateOut = date,
                                            OutTime = time.ToString("HH:mm:ss"),

                                            //Defaults
                                            OutMode = "fingerprint",
                                            AttendanceStatus = 0,
                                            FlagIn = false,
                                            FlagOut = false,
                                            AttendanceType = 0,
                                            CheckInMode = 'D',
                                            AttendanceId = 0,
                                            SignOutTimeStamp = 0,
                                            SignInTimeStamp = 0,
                                        });

                                        continue;
                                    }
                                }
                            }

                            var existingAttendance = await _context.Attendances
                                .Where(x => x.EmpId == emp.Id
                                        && (x.TransactionDate == date))
                                .Include(x => x.WorkHour)
                                .FirstOrDefaultAsync();

                            if (existingAttendance != null)
                            {
                                TimeOnly attendanceTimeIn = TimeOnly.ParseExact(existingAttendance.InTime ?? "23:59:00", "HH:mm:ss");
                                TimeOnly attendanceTimeOut = TimeOnly.ParseExact(existingAttendance.OutTime ?? "00:00:00", "HH:mm:ss");

                                if (time < attendanceTimeIn)
                                {
                                    existingAttendance.InTime = time.ToString("HH:mm:ss");
                                    existingAttendance.TransactionDate = date;
                                    existingAttendance.InMode = "fingerprint";

                                    continue;
                                }
                                else if (time > attendanceTimeOut)
                                {
                                    existingAttendance.OutTime = time.ToString("HH:mm:ss");
                                    existingAttendance.TransactionDateOut = date;
                                    existingAttendance.OutMode = "fingerprint";

                                    continue;
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            var syncedEmp = data.Where(x => x.EmpId == emp.Id && x.TransactionDate == date).FirstOrDefault();

                            if (syncedEmp != null)
                            {
                                TimeOnly attendanceTimeIn = TimeOnly.ParseExact(syncedEmp.InTime ?? "23:59:00", "HH:mm:ss");
                                TimeOnly attendanceTimeOut = TimeOnly.ParseExact(syncedEmp.OutTime ?? "00:00:00", "HH:mm:ss");


                                if (time < attendanceTimeIn)
                                {
                                    syncedEmp.InTime = time.ToString("HH:mm:ss");
                                    syncedEmp.TransactionDate = date;
                                    syncedEmp.InMode = "fingerprint";

                                    continue;
                                }
                                else if (time > attendanceTimeOut)
                                {
                                    syncedEmp.OutTime = time.ToString("HH:mm:ss");
                                    syncedEmp.TransactionDateOut = date;
                                    syncedEmp.OutMode = "fingerprint";

                                    continue;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                data.Add(new Attendance
                                {
                                    WorkHourId = workHour.Id,
                                    EmpId = emp.Id,
                                    TransactionDate = date,
                                    InTime = time.ToString("HH:mm:ss"),

                                    //Defaults
                                    InMode = "fingerprint",
                                    AttendanceStatus = 0,
                                    FlagIn = false,
                                    FlagOut = false,
                                    AttendanceType = 0,
                                    CheckInMode = 'D',
                                    AttendanceId = 0,
                                    SignOutTimeStamp = 0,
                                    SignInTimeStamp = 0,
                                });
                            }
                        }



                        //    var syncedEmp = data.Where(x => x.EmpId == emp.Id && x.TransactionDate == date).SingleOrDefault();

                        //    if (record.InOutPunchType == "P10")
                        //    {
                        //        if (syncedEmp != null)
                        //        {
                        //            if ()

                        //            syncedEmp.TransactionDate = date;
                        //            syncedEmp.InTime = record.InOutTime;
                        //            syncedEmp.InMode = "fingerprint";

                        //            continue;
                        //        }

                        //        data.Add(new Attendance
                        //        {
                        //            EmpId = emp.Id,
                        //            TransactionDate = date,
                        //            InTime = record.InOutTime,
                        //            WorkHourId = WorkHourId,

                        //            //Defaults
                        //            InMode = "fingerprint",
                        //            AttendanceStatus = 0,
                        //            FlagIn = false,
                        //            FlagOut = false,
                        //            AttendanceType = 0,
                        //            CheckInMode = 'D',
                        //            AttendanceId = 0,
                        //            SignOutTimeStamp = 0,
                        //            SignInTimeStamp = 0,
                        //        });
                        //    }
                        //    else
                        //    {
                        //        if (syncedEmp != null)
                        //        {
                        //            syncedEmp.TransactionDateOut = date;
                        //            syncedEmp.OutTime = record.InOutTime;
                        //            syncedEmp.OutMode = "fingerprint";

                        //            continue;
                        //        }

                        //        short? WorkHourId;

                        //        var roster = await _context.Rosters.Where(x => x.Date == date && x.EmpId == emp.Id).FirstOrDefaultAsync();

                        //        WorkHourId = roster?.WorkHourId;

                        //        if (roster is null)
                        //        {
                        //            var defaultWorkHour = await _context.DefaultWorkHours
                        //                .Where(x => (x.EmpId == emp.Id || x.EmpId == null) && x.DayId == ((short)date.DayOfWeek + 1))
                        //                .OrderBy(x => x.EmpId)
                        //                .FirstOrDefaultAsync();

                        //            WorkHourId = defaultWorkHour?.WorkHourId;
                        //        }

                        //        if (WorkHourId is null)
                        //        {
                        //            Errors.Add(record.EmpCode + "does not have default shift on the date: " + record.Date);

                        //            continue;
                        //        }

                        //        data.Add(new Attendance
                        //        {
                        //            EmpId = emp.Id,
                        //            TransactionDateOut = date,
                        //            OutTime = record.InOutTime,
                        //            WorkHourId = WorkHourId,

                        //            //Defaults
                        //            InMode = "fingerprint",
                        //            AttendanceStatus = 0,
                        //            FlagIn = false,
                        //            FlagOut = false,
                        //            AttendanceType = 0,
                        //            CheckInMode = 'D',
                        //            AttendanceId = 0,
                        //            SignOutTimeStamp = 0,
                        //            SignInTimeStamp = 0,
                        //        });
                        //    }
                        //}

                        _context.AddRange(data);

                        if (Errors.Count == 0)
                        {
                            import.Status = "Sync Successful";
                            import.IsSynced = true;

                            await _context.SaveChangesAsync();

                            return Ok();
                        } else
                        {
                            import.Status = "Sync failed";
                            import.IsUploaded = false;
                            System.IO.File.Delete(fullFilePath);

                            return BadRequest(new
                            {
                                Errors
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        import.Status = "Sync failed";
                        import.IsUploaded = false;
                        import.ErrorMessage = ex.Message;
                        import.ErrorTrace = ex.StackTrace;

                        System.IO.File.Delete(fullFilePath);

                        await _context.SaveChangesAsync();

                        Errors.Add(filename + "file could not be synced. Please Re-Import the file and try again.");

                        return BadRequest(new
                        {
                            Errors
                        });
                    }
                }
            };

            return Ok(new
            {
                Errors
            });
        }

        [HttpGet("DownloadFormat")]
        public async Task<IActionResult> DownloadFormat()
        {
            Type table = typeof(AttendanceHeader);

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

            return File(data, "text/csv", "Attendnace.csv");
        }

        public class AttendanceHeader
        {
            [Name("EMPCODE")]
            public string? EmpCode { get; set; }

            [Name("INOUTDATE")]
            public string? Date { get; set; }

            [Name("INOUTTIME")]
            public string? InOutTime { get; set; }
        }

        public class ImportInputModel
        {
            public List<IFormFile> Files { get; set; }
        }
    }
}
