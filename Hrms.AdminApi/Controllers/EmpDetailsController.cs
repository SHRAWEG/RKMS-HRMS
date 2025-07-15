 using FluentValidation;
using Hrms.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NPOI.HPSF;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class EmpDetailsController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private static readonly IEnumerable<string> _relationshipTypes = new List<string>()
        {
            "Mother",
            "Father",
            "Spouse",
            "Child"
        };
        private static readonly IEnumerable<string> _genders = new List<string>()
        {
            "Male",
            "Female"
        };
        private static readonly IEnumerable<string> _extenstions = new List<string>()
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".pdf",
        };

        public EmpDetailsController(DataContext context, IConfiguration config, UserManager<User> userManager)
        {
            _context = context;
            _config = config;
            _userManager = userManager;
        }

        // GET: EmpDetails
        [CustomAuthorize("list-employee")]
        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection,
            string empCode, string name, int? branchId, int? departmentId, short? designationId, int? gradeId)
        {
            var query = (from detail in _context.EmpDetails
                         join log in _context.EmpLogs on detail.Id equals log.EmployeeId into logs
                         from log in logs.DefaultIfEmpty()
                         join tran in _context.EmpTransactions on log.Id equals tran.Id into trans
                         from tran in trans.DefaultIfEmpty()
                         select new
                         {
                             PId = log != null ? log.Id : 0,
                             EmpId = detail.Id,
                             EmpCode = detail.CardId,
                             Title = detail.Title,
                             FirstName = detail.FirstName,
                             MiddleName = detail.MiddleName,
                             LastName = detail.LastName,
                             Name = FullName(detail.FirstName, detail.MiddleName, detail.LastName),
                             CompanyId = tran != null ? tran.CompanyId : 0,
                             CompanyName = tran != null ? (tran.Company != null ? tran.Company.Name : null) : null, 
                             BranchId = tran != null ? tran.BranchId : 0,
                             BranchName = tran != null ? (tran.Branch != null ? tran.Branch.Name : "") : "",
                             DepartmentId = tran != null ? tran.DepartmentId : 0,
                             DepartmentName = tran != null ? (tran.Department != null ? tran.Department.Name : "") : "",
                             DesignationId = tran != null ? tran.DesignationId : 0,
                             DesignationName = tran != null ? (tran.Designation != null ? tran.Designation.Name : "") : "",
                             GradeId = tran != null ? tran.GradeId : 0,
                             GradeName = tran != null ? (tran.Grade != null ? tran.Grade.Name : "") : "",
                             TransactionDate = log != null ? log.TransactionDate : null
                         }
                       ).AsQueryable();

            if (User.GetUserRole() != "super-admin")
            {
                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                query = query.Where(x => companyIds.Contains(x.CompanyId ?? 0) || x.CompanyId == null);
            }

            if (!string.IsNullOrEmpty(empCode))
            {
                query = query.Where(x => x.EmpCode!.ToLower().Contains(empCode.ToLower()));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => string.Concat(x.FirstName, x.MiddleName, x.LastName).ToLower().Contains(name.Replace(" ", string.Empty).ToLower()));
            }

            if (branchId != null)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (departmentId != null)
            {
                query = query.Where(x => x.DepartmentId == departmentId);
            }

            if (designationId != null)
            {
                query = query.Where(x => x.DesignationId == designationId);
            }

            if (gradeId != null)
            {
                query = query.Where(x => x.GradeId == gradeId);
            }

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.EmpId);
            }
            else if (sortDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderBy(x => x.EmpCode);
                        break;

                    case "Name":
                        query.OrderBy(x => x.FirstName);
                        break;

                    case "BranchName":
                        query.OrderBy(x => x.BranchId);
                        break;

                    case "DepartmentName":
                        query.OrderBy(x => x.DepartmentId);
                        break;

                    case "DesignationName":
                        query.OrderBy(x => x.DesignationId);
                        break;

                    case "GradeName":
                        query.OrderBy(x => x.GradeName);
                        break;

                    case "TransactionDate":
                        query.OrderBy(x => x.TransactionDate);
                        break;

                    default:
                        query.OrderBy(x => x.EmpId);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "EmpCode":
                        query.OrderByDescending(x => x.EmpCode);
                        break;

                    case "Name":
                        query.OrderByDescending(x => x.Name);
                        break;

                    case "BranchName":
                        query.OrderByDescending(x => x.BranchId);
                        break;

                    case "DepartmentName":
                        query.OrderByDescending(x => x.DepartmentId);
                        break;

                    case "DesignationName":
                        query.OrderByDescending(x => x.DesignationId);
                        break;

                    case "GradeName":
                        query.OrderByDescending(x => x.GradeName);
                        break;

                    case "TransactionDate":
                        query.OrderByDescending(x => x.TransactionDate);
                        break;

                    default:
                        query.OrderByDescending(x => x.EmpId);
                        break;
                }
            }

            var TotalCount = await query.CountAsync();
            var TotalPages = (int)Math.Ceiling(TotalCount / (double)page);
            var data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return Ok(new
            {
                Data = data,
                TotalCount,
                TotalPages
        });
        }

        [CustomAuthorize("view-employee")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.EmpDetails
                .Include(x => x.BirthCountry)
                .Include(x => x.BirthState)
                .Include(x => x.Religion)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (User.GetUserRole() != "super-admin")
            {
                var empLogs = await _context.EmpLogs.Where(x => x.EmployeeId == data.Id).FirstOrDefaultAsync();

                var empTransaction = await _context.EmpTransactions.Where(x => x.Id == empLogs!.Id).FirstOrDefaultAsync();

                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Any(x => x == empTransaction?.CompanyId))
                {
                    return Forbid();
                }
            }

            //var employmentHistoris = await _context.EmploymentHistories.Where(x => x.EmpId == id).ToListAsync();
            //var educations = await _context.Educations.Where(x => x.EmpId == id).ToListAsync();
            //var families = await _context.Families.Where(x => x.EmpId == id).ToListAsync();


            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }
            var downloadBase = GetDownloadBaseUrl();

            var profileDoc = await _context.EmpDocuments.FirstOrDefaultAsync(x => x.Id == data.ProfileDocumentId);
            var aadharDoc = await _context.EmpDocuments.FirstOrDefaultAsync(x => x.Id == data.AadharDocumentId);
            var panDoc = await _context.EmpDocuments.FirstOrDefaultAsync(x => x.Id == data.PanDocumentId);
            var dlDoc = await _context.EmpDocuments.FirstOrDefaultAsync(x => x.Id == data.DrivingLicenseDocumentId);
            var passportDoc = await _context.EmpDocuments.FirstOrDefaultAsync(x => x.Id == data.PassportDocumentId);


            string? GetFileUrl(int? docId, string? fileName)
            {
                if (!docId.HasValue || string.IsNullOrWhiteSpace(fileName))
                    return null;
                try
                {
                    return Uri.UnescapeDataString($"{downloadBase}download/{docId}/{fileName}");
                }
                catch
                {
                    return null; 
                }
            }


            return Ok(new
            {
                EmpDetail = new
                {
                    data.Id,
                    EmpCode = data.CardId,
                    data.Title,
                    data.FirstName,
                    data.MiddleName,
                    data.LastName,
                    data.Email,
                    data.ContactNumber,
                    data.Gender,
                    data.MaritalStatus,
                    data.BloodGroup,
                    data.Nationality,
                    data.BirthCountryId,
                    BirthCountryName = data.BirthCountry?.Name,
                    data.ReligionId,
                    ReligionName = data.Religion?.Name,
                    data.BirthStateId,
                    BirthStateName = data.BirthState?.Name,
                    data.BirthPlace,
                    data.DateOfBirth,
                    data.JoinDate,
                    data.MarriageDate,
                    data.RelevingDate,
                    data.AppointedDate,
                    data.PermanentAddress,
                    data.PermanentAddress2,
                    data.PermanentCity,
                    data.PermanentPincode,
                    data.PermanentState,
                    data.PermanentDistrict,
                    data.CorrespondanceAddress,
                    data.CorrespondanceAddress2,
                    data.CorrespondanceCity,
                    data.CorrespondancePincode,
                    data.CorrespondanceState,
                    data.CorrespondanceDistrict,
                    data.PassportNumber,
                    data.PassportDocumentId,
                    data.AadharNumber,
                    data.AadharDocumentId,
                    data.PanNumber,
                    data.PanDocumentId,
                    data.DrivingLicenseNumber,
                    data.DrivingLicenseDocumentId,
                    data.ProfileDocumentId,
                    //EmploymentHistories = employmentHistoris,
                    //Educations = educations,
                    //Families = families,
                    ProfileFilePath = GetFileUrl(data.ProfileDocumentId, profileDoc?.FileName),
                    AadharFilePath = GetFileUrl(data.AadharDocumentId, aadharDoc?.FileName),
                    PanFilePath = GetFileUrl(data.PanDocumentId, panDoc?.FileName),
                    DrivingLicenseFilePath = GetFileUrl(data.DrivingLicenseDocumentId, dlDoc?.FileName),
                    PassportFilePath = GetFileUrl(data.PassportDocumentId, passportDoc?.FileName),

                }
            });
        }

            // Post: EmpDetails/Create
            [CustomAuthorize("write-employee")]
            [HttpPost]
            public async Task<IActionResult> Create([FromForm] AddInputModel input)
            {
                DateOnly? dateOfBirth = null;
                DateOnly? joinDate = null;
                DateOnly? marriageDate = null;
                DateOnly? relevingDate = null;
                DateOnly? appointedDate = null;
                EmpDocument? passport = new();
                EmpDocument? aadhar = new();
                EmpDocument? pan = new();
                EmpDocument? drivingLicense = new();
                EmpDocument? profilefile = new();

                if (!string.IsNullOrEmpty(input.DateOfBirth))
                {
                    dateOfBirth = DateOnly.ParseExact(input.DateOfBirth, "yyyy-MM-dd");
                }

                if (!string.IsNullOrEmpty(input.JoinDate))
                {
                    joinDate = DateOnly.ParseExact(input.JoinDate, "yyyy-MM-dd");
                }

                if (!string.IsNullOrEmpty(input.MarriageDate))
                {
                    marriageDate = DateOnly.ParseExact(input.MarriageDate, "yyyy-MM-dd");
                }

                if (!string.IsNullOrEmpty(input.RelevingDate))
                {
                    relevingDate = DateOnly.ParseExact(input.RelevingDate, "yyyy-MM-dd");
                }

                if (!string.IsNullOrEmpty(input.AppointedDate))
                {
                    appointedDate = DateOnly.ParseExact(input.AppointedDate, "yyyy-MM-dd");
                }

                if (input.PassportFile != null)
                {
                    string filename = input.PassportFile.FileName;

                    passport = new()
                    {
                        FileName = filename,
                        FileDescription = "passport",
                        FileExtension = Path.GetExtension(filename),
                        Remarks = "passport",
                    };

                    _context.Add(passport);
                    await _context.SaveChangesAsync();

                    string directoryPath = Path.Combine(Folder.EmpDocuments, passport.Id.ToString());

                    string filePath = Path.Combine(directoryPath, filename);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(fullFilePath))
                    {
                        await input.PassportFile.CopyToAsync(stream);
                    };
                }

                if (input.AadharFile != null)
                {
                    string filename = input.AadharFile.FileName;

                    aadhar = new()
                    {
                        FileName = filename,
                        FileDescription = "aadhar",
                        FileExtension = Path.GetExtension(filename),
                        Remarks = "aadhar",
                    };

                    _context.Add(aadhar);
                    await _context.SaveChangesAsync();

                    string directoryPath = Path.Combine(Folder.EmpDocuments, aadhar.Id.ToString());

                    string filePath = Path.Combine(directoryPath, filename);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(fullFilePath))
                    {
                        await input.AadharFile.CopyToAsync(stream);
                    };
                }

                if (input.PanFile != null)
                {
                    string filename = input.PanFile.FileName;

                    pan = new()
                    {
                        FileName = filename,
                        FileDescription = "pan",
                        FileExtension = Path.GetExtension(filename),
                        Remarks = "pan",
                    };

                    _context.Add(pan);
                    await _context.SaveChangesAsync();

                    string directoryPath = Path.Combine(Folder.EmpDocuments, pan.Id.ToString());

                    string filePath = Path.Combine(directoryPath, filename);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(fullFilePath))
                    {
                        await input.PanFile.CopyToAsync(stream);
                    };
                }

                if (input.DrivingLicenseFile != null)
                {
                    string filename = input.DrivingLicenseFile.FileName;

                    drivingLicense = new()
                    {
                        FileName = filename,
                        FileDescription = "drivingLicense",
                        FileExtension = Path.GetExtension(filename),
                        Remarks = "drivingLicense",
                    };

                    _context.Add(drivingLicense);
                    await _context.SaveChangesAsync();

                    string directoryPath = Path.Combine(Folder.EmpDocuments, drivingLicense.Id.ToString());

                    string filePath = Path.Combine(directoryPath, filename);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(fullFilePath))
                    {
                        await input.DrivingLicenseFile.CopyToAsync(stream);
                    };
                }
                if (input.ProfileFile != null)
                {
                    string filename = input.ProfileFile.FileName;

                    profilefile = new()
                    {
                        FileName = filename,
                        FileDescription = "profilefile",
                        FileExtension = Path.GetExtension(filename),
                        Remarks = "profilefile",
                    };

                    _context.Add(profilefile);
                    await _context.SaveChangesAsync();

                    string directoryPath = Path.Combine(Folder.EmpDocuments, profilefile.Id.ToString());

                    string filePath = Path.Combine(directoryPath, filename);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(fullFilePath))
                    {
                        await input.ProfileFile.CopyToAsync(stream);
                    }
                    ;
                }

                EmpDetail data = new()
                {
                    CardId = input.EmpCode.Trim(),
                    Title = input.Title,
                    FirstName = input.FirstName,
                    MiddleName = input.MiddleName,
                    LastName = input.LastName,
                    Email = input.Email?.Trim().ToLower(),
                    ContactNumber = input.ContactNumber,
                    Gender = input.Gender,
                    MaritalStatus = input.MaritalStatus,
                    BloodGroup = input.BloodGroup ?? "",
                    Nationality = input.Nationality,
                    BirthCountryId = input.BirthCountryId,
                    ReligionId = input.ReligionId,
                    BirthStateId = input.BirthStateId,
                    BirthPlace = input.BirthPlace,
                    DateOfBirth = dateOfBirth,
                    JoinDate = joinDate,
                    MarriageDate = marriageDate,
                    RelevingDate = relevingDate,
                    AppointedDate = appointedDate,
                    PermanentAddress = input.PermanentAddress,
                    PermanentAddress2 = input.PermanentAddress2,
                    PermanentCity = input.PermanentCity,
                    PermanentPincode = input.PermanentPincode,
                    PermanentState = input.PermanentState,
                    PermanentDistrict = input.PermanentDistrict,
                    CorrespondanceAddress = input.CorrespondanceAddress,
                    CorrespondanceAddress2 = input.CorrespondanceAddress2,
                    CorrespondanceCity = input.CorrespondanceCity,
                    CorrespondancePincode = input.CorrespondancePincode,
                    CorrespondanceState = input.CorrespondanceState,
                    CorrespondanceDistrict = input.CorrespondanceDistrict,
                    PanNumber = input.PanNumber,
                    PassportNumber = input.PassportNumber,
                    AadharNumber = input.AadharNumber,
                    DrivingLicenseNumber = input.DrivingLicenseNumber ?? "",
                    PassportDocumentId = input.PassportFile != null ? passport.Id : null,
                    AadharDocumentId = input.AadharFile != null ? aadhar.Id : null,
                    PanDocumentId = input.PanFile != null ? pan.Id : null,
                    DrivingLicenseDocumentId = input.DrivingLicenseFile != null ? drivingLicense.Id : null,
                    ProfileDocumentId = input.ProfileFile != null ? profilefile.Id : null,

                    //DefaultValues
                    Appointed = 0,
                    EmergencyContactPerson = "N/A",
                    EmergencyContactNumber = "N/A",
                    FatherName = "N/A",
                    MotherName = "N/A",
                    GrandFatherName = "N/A",
                    TransactionUser = "N/A",
                };

                _context.Add(data);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Id = data.Id,
                });
            }

        [CustomAuthorize("register-employee")]
        [HttpPost("RegisterEmp/{id}")]
        public async Task<ActionResult> RegisterEmp(int id)
        {
            if (await _userManager.Users.Where(x => x.EmpId != 0).AnyAsync(x => x.EmpId == id)) return ErrorHelper.ErrorResult("Id", "Employee already has an account");

            var emp = await _context.EmpDetails.Where(x => x.Id == id).SingleOrDefaultAsync();

            if (emp == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            if (!await _context.EmpLogs.AnyAsync(x => x.EmployeeId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Employee is not registered yet.");
            }

            User user = new()
            {
                UserName = emp.CardId?.ToLower(),
                IsActive = true,
                EmpId = id
            };

            var result = await _userManager.CreateAsync(user, "admin");
            var role = await _userManager.AddToRolesAsync(user, new[] { AppRole.Employee });

            if (!result.Succeeded) return BadRequest(result.Errors);
            if (!role.Succeeded) return BadRequest(role.Errors);

            return Ok();
        }

        // PUT: EmpDetails/5
        [CustomAuthorize("update-employee")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] UpdateInputModel input)
        {
            var data = await _context.EmpDetails
                .Include(x => x.PanDocument)
                .Include(x => x.PassportDocument)
                .Include(x => x.AadharDocument)
                .Include(x => x.DrivingLicenseDocument)
                .Include(x => x.ProfileDocument)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (User.GetUserRole() != "super-admin")
            {
                var empLogs = await _context.EmpLogs.Where(x => x.EmployeeId == data.Id).FirstOrDefaultAsync();

                var empTransaction = await _context.EmpTransactions.Where(x => x.Id == empLogs!.Id).FirstOrDefaultAsync();

                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Any(x => x == empTransaction?.CompanyId))
                {
                    return Forbid();
                }
            }

            DateOnly? dateOfBirth = null;
            DateOnly? joinDate = null;
            DateOnly? marriageDate = null;
            DateOnly? relevingDate = null;
            DateOnly? appointedDate = null;
            EmpDocument? passport = new();
            EmpDocument? aadhar = new();
            EmpDocument? pan = new();
            EmpDocument? drivingLicense = new();
            EmpDocument? profilefile = new();

            if (!string.IsNullOrEmpty(input.DateOfBirth))
            {
                dateOfBirth = DateOnly.ParseExact(input.DateOfBirth, "yyyy-MM-dd");
            }

            if (!string.IsNullOrEmpty(input.JoinDate))
            {
                joinDate = DateOnly.ParseExact(input.JoinDate, "yyyy-MM-dd");
            }

            if (!string.IsNullOrEmpty(input.MarriageDate))
            {
                marriageDate = DateOnly.ParseExact(input.MarriageDate, "yyyy-MM-dd");
            }

            if (!string.IsNullOrEmpty(input.RelevingDate))
            {
                relevingDate = DateOnly.ParseExact(input.RelevingDate, "yyyy-MM-dd");
            }

            if (!string.IsNullOrEmpty(input.AppointedDate))
            {
                appointedDate = DateOnly.ParseExact(input.AppointedDate, "yyyy-MM-dd");
            }

            if (input.PassportFile != null)
            {
                string filename = input.PassportFile.FileName;

                if (data.PassportDocumentId != null)
                {
                    string directoryPath = Path.Combine(Folder.EmpDocuments, data.PassportDocumentId.ToString());

                    string filePath = Path.Combine(directoryPath, data.PassportDocument?.FileName);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    System.IO.File.Delete(fullFilePath);

                    string newFilePath = Path.Combine(directoryPath, filename);

                    string newFullFilePath = Path.Combine(_config["FilePath"], newFilePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(newFullFilePath))
                    {
                        await input.PassportFile.CopyToAsync(stream);
                    };

                    data.PassportDocument.FileName = filename;
                    data.PassportDocument.FileExtension = Path.GetExtension(filename);
                } else
                {
                    passport = new()
                    {
                        FileName = filename,
                        FileDescription = "passport",
                        FileExtension = Path.GetExtension(filename),
                        Remarks = "passport",
                    };

                    _context.Add(passport);
                    await _context.SaveChangesAsync();
                    data.PassportDocumentId = passport.Id;
                    string directoryPath = Path.Combine(Folder.EmpDocuments, passport.Id.ToString());

                    string filePath = Path.Combine(directoryPath, filename);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(fullFilePath))
                    {
                        await input.PassportFile.CopyToAsync(stream);
                    };
                }
            }

            if (input.AadharFile != null)
            {
                string filename = input.AadharFile.FileName;

                if (data.AadharDocumentId != null)
                {
                    string directoryPath = Path.Combine(Folder.EmpDocuments, data.AadharDocumentId.ToString());

                    string filePath = Path.Combine(directoryPath, data.AadharDocument?.FileName);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    System.IO.File.Delete(fullFilePath);

                    string newFilePath = Path.Combine(directoryPath, filename);

                    string newFullFilePath = Path.Combine(_config["FilePath"], newFilePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(newFullFilePath))
                    {
                        await input.AadharFile.CopyToAsync(stream);
                    };

                    data.AadharDocument.FileName = filename;
                    data.AadharDocument.FileExtension = Path.GetExtension(filename);
                }
                else
                {
                    aadhar = new()
                    {
                        FileName = filename,
                        FileDescription = "aadhar",
                        FileExtension = Path.GetExtension(filename),
                        Remarks = "aadhar",
                    };

                    _context.Add(aadhar);
                    await _context.SaveChangesAsync();
                    data.AadharDocumentId = aadhar.Id;
                    string directoryPath = Path.Combine(Folder.EmpDocuments, aadhar.Id.ToString());

                    string filePath = Path.Combine(directoryPath, filename);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(fullFilePath))
                    {
                        await input.AadharFile.CopyToAsync(stream);
                    };
                }
            }

            if (input.PanFile != null)
            {
                string filename = input.PanFile.FileName;

                if (data.PanDocumentId != null)
                {
                    string directoryPath = Path.Combine(Folder.EmpDocuments, data.PanDocumentId.ToString());

                    string filePath = Path.Combine(directoryPath, data.PanDocument?.FileName);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    System.IO.File.Delete(fullFilePath);

                    string newFilePath = Path.Combine(directoryPath, filename);

                    string newFullFilePath = Path.Combine(_config["FilePath"], newFilePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(newFullFilePath))
                    {
                        await input.PanFile.CopyToAsync(stream);
                    };

                    data.PanDocument.FileName = filename;
                    data.PanDocument.FileExtension = Path.GetExtension(filename);
                }
                else
                {
                    pan = new()
                    {
                        FileName = filename,
                        FileDescription = "pan",
                        FileExtension = Path.GetExtension(filename),
                        Remarks = "pan",
                    };

                    _context.Add(pan);
                    await _context.SaveChangesAsync();

                    data.PanDocumentId = pan.Id;

                    string directoryPath = Path.Combine(Folder.EmpDocuments, pan.Id.ToString());

                    string filePath = Path.Combine(directoryPath, filename);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(fullFilePath))
                    {
                        await input.PanFile.CopyToAsync(stream);
                    };
                }
            }

            if (input.DrivingLicenseFile != null)
            {
                string filename = input.DrivingLicenseFile.FileName;

                if (data.DrivingLicenseDocumentId != null)
                {
                    string directoryPath = Path.Combine(Folder.EmpDocuments, data.DrivingLicenseDocumentId.ToString());

                    string filePath = Path.Combine(directoryPath, data.DrivingLicenseDocument?.FileName);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    System.IO.File.Delete(fullFilePath);

                    string newFilePath = Path.Combine(directoryPath, filename);

                    string newFullFilePath = Path.Combine(_config["FilePath"], newFilePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(newFullFilePath))
                    {
                        await input.DrivingLicenseFile.CopyToAsync(stream);
                    };

                    data.DrivingLicenseDocument.FileName = filename;
                    data.DrivingLicenseDocument.FileExtension = Path.GetExtension(filename);
                }
                else
                {
                    drivingLicense = new()
                    {
                        FileName = filename,
                        FileDescription = "drivingLicense",
                        FileExtension = Path.GetExtension(filename),
                        Remarks = "drivingLicense",
                    };

                    _context.Add(drivingLicense);
                    await _context.SaveChangesAsync();

                    data.DrivingLicenseDocumentId = drivingLicense.Id;

                    string directoryPath = Path.Combine(Folder.EmpDocuments, drivingLicense.Id.ToString());

                    string filePath = Path.Combine(directoryPath, filename);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(fullFilePath))
                    {
                        await input.DrivingLicenseFile.CopyToAsync(stream);
                    };
                }
            }
            if (input.ProfileFile != null)
            {
                string filename = input.ProfileFile.FileName;

                if (data.ProfileDocumentId != null)
                {
                    string directoryPath = Path.Combine(Folder.EmpDocuments, data.ProfileDocumentId.ToString());

                    string filePath = Path.Combine(directoryPath, data.ProfileDocument?.FileName);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    System.IO.File.Delete(fullFilePath);

                    string newFilePath = Path.Combine(directoryPath, filename);

                    string newFullFilePath = Path.Combine(_config["FilePath"], newFilePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(newFullFilePath))
                    {
                        await input.ProfileFile.CopyToAsync(stream);
                    };

                    data.ProfileDocument.FileName = filename;
                    data.ProfileDocument.FileExtension = Path.GetExtension(filename);
                }
                else
                {
                    profilefile = new()
                    {
                        FileName = filename,
                        FileDescription = "ProfilePhoto",
                        FileExtension = Path.GetExtension(filename),
                        Remarks = "ProfilePhoto",
                    };

                    _context.Add(profilefile);
                    await _context.SaveChangesAsync();

                    string directoryPath = Path.Combine(Folder.EmpDocuments, profilefile.Id.ToString());

                    string filePath = Path.Combine(directoryPath, filename);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(fullFilePath))
                    {
                        await input.ProfileFile.CopyToAsync(stream);
                    }
                    input.ProfileDocumentId = profilefile.Id;
                }
            }

            data.CardId = input.EmpCode.Trim();
            data.Title = input.Title;
            data.FirstName = input.FirstName;
            data.MiddleName = input.MiddleName;
            data.LastName = input.LastName;
            data.Email = input.Email?.Trim().ToLower();
            data.ContactNumber = input.ContactNumber;
            data.Gender = input.Gender;
            data.MaritalStatus = input.MaritalStatus;
            data.BloodGroup = input.BloodGroup ?? "";
            data.Nationality = input.Nationality;
            data.BirthCountryId = input.BirthCountryId;
            data.ReligionId = input.ReligionId;
            data.BirthStateId = input.BirthStateId;
            data.BirthPlace = input.BirthPlace;
            data.DateOfBirth = dateOfBirth;
            data.JoinDate = joinDate;
            data.MarriageDate = marriageDate;
            data.RelevingDate = relevingDate;
            data.AppointedDate = appointedDate;
            data.PermanentAddress = input.PermanentAddress;
            data.PermanentAddress2 = input.PermanentAddress2;
            data.PermanentCity = input.PermanentCity;
            data.PermanentPincode = input.PermanentPincode;
            data.PermanentState = input.PermanentState;
            data.PermanentDistrict = input.PermanentDistrict;
            data.CorrespondanceAddress = input.CorrespondanceAddress;
            data.CorrespondanceAddress2 = input.CorrespondanceAddress2;
            data.CorrespondanceCity = input.CorrespondanceCity;
            data.CorrespondancePincode = input.CorrespondancePincode;
            data.CorrespondanceState = input.CorrespondanceState;
            data.CorrespondanceDistrict = input.CorrespondanceDistrict;
            data.PanNumber = input.PanNumber;
            data.PassportNumber = input.PassportNumber;
            data.AadharNumber = input.AadharNumber;
            data.DrivingLicenseNumber = input.DrivingLicenseNumber ?? "";
            data.ProfileDocumentId = input.ProfileDocumentId;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: EmpDetails/5
        [CustomAuthorize("delete-employee")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.EmpDetails.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (User.GetUserRole() != "super-admin")
            {
                var empLogs = await _context.EmpLogs.Where(x => x.EmployeeId == data.Id).FirstOrDefaultAsync();

                var empTransaction = await _context.EmpTransactions.Where(x => x.Id == empLogs!.Id).FirstOrDefaultAsync();

                var companyIds = await _context.UserCompanies.Where(x => x.UserId == User.GetUserId()).Select(x => x.CompanyId).ToListAsync();

                if (!companyIds.Any(x => x == empTransaction?.CompanyId))
                {
                    return Forbid();
                }
            }

            _context.RemoveRange(_context.EmpTransactions.Where(x => x.EmployeeId == id).ToList());
            _context.RemoveRange(_context.EmpLogs.Where(x => x.EmployeeId == id).ToList());
            _context.RemoveRange(_context.EmploymentHistories.Where(x => x.EmpId == id).ToList());
            _context.RemoveRange(_context.Educations.Where(x => x.EmpId == id).ToList());
            _context.RemoveRange(_context.Families.Where(x => x.EmpId == id).ToList());
            _context.RemoveRange(_context.DefaultWorkHours.Where(x => x.EmpId == id).ToList());

            await _context.SaveChangesAsync();

            _context.EmpDetails.Remove(data);



            return Ok();
        }

        private string GetDownloadBaseUrl()
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
            bool isUat = true; // or inject from config if needed

            if (isUat)
            {
                if (baseUrl.Contains("7129"))
                {
                    baseUrl = baseUrl.Replace("7129", "6002");
                    baseUrl = baseUrl.Replace("::", ":");
                }
                else if (baseUrl.Contains("59.179.16.123"))
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
                }
                else
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host.Value}:6002";
                }
            }

            return $"{baseUrl}/download/";
        }


        private static string FullName(string firstName, string middleName, string lastName)
        {
            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
        }

        public class BaseInputModel
        {
            public string EmpCode { get; set; }
            public string Title { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string ContactNumber { get; set; }
            public char Gender { get; set; }
            public char MaritalStatus { get; set; }
            public string BloodGroup { get; set; }
            public string Nationality { get; set; }
            public int? BirthCountryId { get; set; }
            public int? ReligionId { get; set; }
            public int? BirthStateId { get; set; }
            public string BirthPlace { get; set; }
            public string DateOfBirth { get; set; }
            public string JoinDate { get; set; }
            public string MarriageDate { get; set; }
            public string RelevingDate { get; set; }
            public string AppointedDate { get; set; }
            public string PermanentAddress { get; set; }
            public string PermanentAddress2 { get; set; }
            public string PermanentCity { get; set; }
            public string PermanentPincode { get; set; }
            public string PermanentState { get; set; }
            public string PermanentDistrict { get; set; }
            public string CorrespondanceAddress { get; set; }
            public string CorrespondanceAddress2 { get; set; }
            public string CorrespondanceCity { get; set; }
            public string CorrespondancePincode { get; set; }
            public string CorrespondanceState { get; set; }
            public string CorrespondanceDistrict { get; set; }
            public string PanNumber { get; set; }
            public string PassportNumber { get; set; }
            public string AadharNumber { get; set; }
            public string DrivingLicenseNumber { get; set; }
            public IFormFile PassportFile { get; set; }
            public IFormFile AadharFile { get; set; }
            public IFormFile PanFile { get; set; }
            public IFormFile DrivingLicenseFile { get; set; }
            public IFormFile ProfileFile { get; set; }
            public int? ProfileDocumentId { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            private readonly DataContext _context;

            public AddInputModelValidator(DataContext context)
            {
                _context = context;

                Transform(x => x.EmpCode, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.EmpDetails.AsQueryable(), "CardId")
                    .MustBeLength(10);

                RuleFor(x => x.Title)
                    .NotEmpty();

                RuleFor(x => x.FirstName)
                    .NotEmpty();

                Transform(x => x.Email, v => v?.Trim())
                    .EmailAddress()
                    .MustBeUnique(_context.EmpDetails.AsQueryable(), "Email")
                    .Unless(x => string.IsNullOrEmpty(x.Email));

                RuleFor(x => x.ContactNumber)
                    .NotEmpty()
                    .MustBeDigits(10)
                    .MustBeUnique(_context.EmpDetails.AsQueryable(), "ContactNumber");

                RuleFor(x => x.Gender)
                    .MustBeValues(new List<char> { 'M', 'F', 'O' });

                RuleFor(x => x.MaritalStatus)
                    .MustBeValues(new List<char> { 'M', 'S' });

                RuleFor(x => x.BirthCountryId)
                    .IdMustExist(_context.Countries.AsQueryable())
                    .Unless(x => x.BirthCountryId is null);

                RuleFor(x => x.BirthStateId)
                    .IdMustExist(_context.States.AsQueryable())
                    .Unless(x => x.BirthStateId is null);

                RuleFor(x => x.ReligionId)
                    .IdMustExist(_context.Religions.AsQueryable())
                    .Unless(x => x.ReligionId is null);

                RuleFor(x => x.DateOfBirth)
                    .MustBeDate()
                    .MustBeDateBeforeNow()
                    .Unless(x => string.IsNullOrEmpty(x.DateOfBirth));

                RuleFor(x => x.MarriageDate)
                    .Must((x, y) => x.MaritalStatus == 'M')
                    .WithMessage("Employee's marital status is single")
                    .MustBeDate()
                    .MustBeDateBeforeNow()
                    .Unless(x => string.IsNullOrEmpty(x.MarriageDate));

                RuleFor(x => x.JoinDate)
                    .MustBeDate()
                    .Unless(x => string.IsNullOrEmpty(x.JoinDate));

                RuleFor(x => x.AppointedDate)
                    .MustBeDate()
                    .MustBeDateBeforeOrEqual(x => x.JoinDate, "Join Date")
                    .Unless(x => string.IsNullOrEmpty(x.AppointedDate));

                RuleFor(x => x.RelevingDate)
                    .MustBeDate()
                    .Unless(x => string.IsNullOrEmpty(x.RelevingDate));

                RuleFor(x => x.CorrespondancePincode)
                    .MustBeDigits(6)
                    .Unless(x => string.IsNullOrEmpty(x.CorrespondancePincode));

                RuleFor(x => x.PermanentPincode)
                    .MustBeDigits(6)
                    .Unless(x => string.IsNullOrEmpty(x.PermanentPincode));

                RuleFor(x => x.ProfileFile)
                    .Must(x => _extenstions.Contains(Path.GetExtension(x.FileName)))
                    .WithMessage("Please upload only listed extensions: '.jpg', '.jpeg', '.png', '.pdf' ")
                    .Unless(x => x.ProfileFile is null);

                //RuleFor(x => x.PassportFile)
                //    .Must(x => _extenstions.Contains(Path.GetExtension(x.FileName)))
                //    .WithMessage("Please upload only listed extensions: '.jpg', '.jpeg', '.png', '.pdf' ")
                //    .Unless(x => x.PassportFile is null);

                //RuleFor(x => x.PanFile)
                //    .Must(x => _extenstions.Contains(Path.GetExtension(x.FileName)))
                //    .WithMessage("Please upload only listed extensions: '.jpg', '.jpeg', '.png', '.pdf' ")
                //    .Unless(x => x.PanFile is null);

                //RuleFor(x => x.AadharFile)
                //    .Must(x => _extenstions.Contains(Path.GetExtension(x.FileName)))
                //    .WithMessage("Please upload only listed extensions: '.jpg', '.jpeg', '.png', '.pdf' ")
                //    .Unless(x => x.AadharFile is null);

                //RuleFor(x => x.DrivingLicenseFile)
                //    .Must(x => _extenstions.Contains(Path.GetExtension(x.FileName)))
                //    .WithMessage("Please upload only listed extensions: '.jpg', '.jpeg', '.png', '.pdf' ")
                //    .Unless(x => x.DrivingLicenseFile is null);
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

                Transform(x => x.EmpCode, v => v?.Trim())
                    .NotEmpty()
                    .MustBeUnique(_context.EmpDetails.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "CardId")
                    .MustBeLength(10);

                RuleFor(x => x.Title)
                    .NotEmpty();

                RuleFor(x => x.FirstName)
                    .NotEmpty();

                Transform(x => x.Email, v => v?.Trim())
                    .EmailAddress()
                    .MustBeUnique(_context.EmpDetails.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "Email")
                    .Unless(x => string.IsNullOrEmpty(x.Email));

                RuleFor(x => x.ContactNumber)
                    .NotEmpty()
                    .MustBeDigits(10)
                    .MustBeUnique(_context.EmpDetails.Where(x => x.Id != int.Parse(_id)).AsQueryable(), "ContactNumber");

                RuleFor(x => x.Gender)
                    .MustBeValues(new List<char> { 'M', 'F', 'O' });

                RuleFor(x => x.MaritalStatus)
                    .MustBeValues(new List<char> { 'M', 'S' });

                RuleFor(x => x.BirthCountryId)
                    .IdMustExist(_context.Countries.AsQueryable())
                    .Unless(x => x.BirthCountryId is null);

                RuleFor(x => x.BirthStateId)
                    .IdMustExist(_context.States.AsQueryable())
                    .Unless(x => x.BirthStateId is null);

                RuleFor(x => x.ReligionId)
                    .IdMustExist(_context.Religions.AsQueryable())
                    .Unless(x => x.ReligionId is null);

                RuleFor(x => x.DateOfBirth)
                    .MustBeDate()
                    .MustBeDateBeforeNow()
                    .Unless(x => string.IsNullOrEmpty(x.DateOfBirth));

                RuleFor(x => x.MarriageDate)
                    .Must((x, y) => x.MaritalStatus == 'M')
                    .WithMessage("Employee's marital status is single")
                    .MustBeDate()
                    .MustBeDateBeforeNow()
                    .Unless(x => string.IsNullOrEmpty(x.MarriageDate));

                RuleFor(x => x.JoinDate)
                    .MustBeDate()
                    .Unless(x => string.IsNullOrEmpty(x.JoinDate));

                RuleFor(x => x.AppointedDate)
                    .MustBeDate()
                    .MustBeDateBeforeOrEqual(x => x.JoinDate, "Join Date")
                    .Unless(x => string.IsNullOrEmpty(x.AppointedDate));

                RuleFor(x => x.RelevingDate)
                    .MustBeDate()
                    .Unless(x => string.IsNullOrEmpty(x.RelevingDate));

                RuleFor(x => x.CorrespondancePincode)
                    .MustBeDigits(6)
                    .Unless(x => string.IsNullOrEmpty(x.CorrespondancePincode));

                RuleFor(x => x.PermanentPincode)
                    .MustBeDigits(6)
                    .Unless(x => string.IsNullOrEmpty(x.PermanentPincode));
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (_context.EmpDetails.Find(int.Parse(_id)) == null)
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
