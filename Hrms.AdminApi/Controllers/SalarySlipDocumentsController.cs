using DocumentFormat.OpenXml.Packaging;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Hrms.AdminApi.Controllers
{
    [Route("[Controller]")]
    [ApiController]

    public class SalarySlipDocumentsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public SalarySlipDocumentsController(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string filename, int? empId, int? year, int? month)
        {
            var query = _context.SalarySlipDocuments
                .Include(x => x.User)
                .Include(x => x.Emp)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filename))
            {
                query = query.Where(x => x.Filename.ToLower().Contains(filename.ToLower()));
            }

            if (empId is not null)
            {
                query = query.Where(x => x.EmpId == empId);
            }

            if (year is not null)
            {
                query = query.Where(x => x.Year == year);
            }

            if (month is not null)
            {
                query = query.Where(x => x.Month == month);
            }

            Expression<Func<SalarySlipDocument, object>> field = sortColumn switch
            {
                "Filename" => x => x.Filename,
                "EmpId" => x => x.EmpId,
                "Year" => x => x.Year,
                "Month" => x => x.Month,
                "UploadedAt" => x => x.UploadedAt,
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

            var data = await PagedList<SalarySlipDocument>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data.Select(x => new
                {
                    Id = x.Id,
                    Filename = x.Filename,
                    UploadedAt = x.UploadedAt,
                    UserId = x.UserId,
                    UserName = x.User.UserName,
                    EmpId = x.EmpId,
                    EmpName = Helper.FullName(x.Emp.FirstName, x.Emp.MiddleName, x.Emp.LastName),
                    Year = x.Year,
                    Month = x.Month
                }),
                data.TotalCount,
                data.TotalPages
            });
        }
        [HttpGet("Download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var result = await _context.SalarySlipDocuments.FindAsync(id);

            if (result == null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id");
            };

            var filePath = Path.Combine(_config["FilePath"], result.FilePath);

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(bytes, "application/pdf", Path.GetFileName(filePath));
        }

        [AllowAnonymous]
        [HttpGet("Download")]
        public async Task<IActionResult> Downloadvia(string Userid,int Year,int Month)
        {
            var user = await _context.Users.FirstOrDefaultAsync(a => a.UserName.ToLower().Contains(Userid.ToLower()));

            if (user == null)
            {
                return ErrorHelper.ErrorResult("Id", "Invalid Id");
            };
            var result = await _context.SalarySlipDocuments.FirstOrDefaultAsync(a => a.Filename.ToLower().Contains(user.UserName.ToLower()) && a.Year == Year && a.Month == Month);

            var filePath = Path.Combine(_config["FilePath"], result.FilePath);

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(bytes, "application/pdf", Path.GetFileName(filePath));
        }
        [HttpPost("Import")]
        public async Task<IActionResult> Upload([FromForm] ImportInputModel input)
        {
            if (await _context.SalarySlipDocuments.AnyAsync(x => x.EmpId == input.EmpId && x.Month == input.Month && x.Year == input.Year)) 
            {
                return ErrorHelper.ErrorResult("Month", "Salary slip for this month already exists.");
            }

            string filename = input.File.FileName;

            if (Path.GetExtension(filename).ToLower() != ".pdf")
            {
                return ErrorHelper.ErrorResult("File", filename + " file does not have a valid file type");
            }



            SalarySlipDocument import = new()
            {
                Filename = filename,
                FilePath = "status running",
                UserId = User.GetUserId(),
                EmpId = input.EmpId,
                Year = input.Year,
                Month = input.Month
            };

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                _context.Add(import);
                await _context.SaveChangesAsync();

                string directoryPath = Path.Combine(Folder.SalarySlipDocuments, import.Id.ToString());

                string filePath = Path.Combine(directoryPath, filename);

                string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                Directory.CreateDirectory(fullDirectoryPath);

                using (var stream = System.IO.File.Create(fullFilePath))
                {
                    await input.File.CopyToAsync(stream);
                }

                import.FilePath = filePath;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok();

            } catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return ErrorHelper.ErrorResult("Id", ex.Message);
            }            
        }
        [HttpPost("Import/Multiple")]
        //[RequestSizeLimit(200 * 1024 * 1024)]
        //[RequestFormLimits(MultipartBodyLengthLimit = 200 * 1024 * 1024)]
        public async Task<IActionResult> UploadMultiple([FromForm] MultipleImportInputModel input)
        {
            List<string> filenames = input.Files.Select(x => Path.GetFileNameWithoutExtension(x.FileName)).ToList();

            var emps = await _context.EmpDetails.Where(x => filenames.Contains(x.CardId)).ToListAsync();

            if (emps.Count != filenames.Count)
            {
                List<string> invalidNames = new();

                foreach(IFormFile file in input.Files) 
                { 
                    if (emps.Any(x => x.CardId != file.FileName))
                    {
                        invalidNames.Add(file.FileName);
                    }
                }

                return ErrorHelper.ErrorResult("Files", "Invalid EmpCodes: " + string.Join(", ", invalidNames));
            }

            List<int> empIds = emps.Select(x => x.Id).ToList();

            if (await _context.SalarySlipDocuments.AnyAsync(x => x.Year == input.Year && x.Month == input.Month && empIds.Contains(x.EmpId)))
            {
                return ErrorHelper.ErrorResult("Files", "Salary slips in the given date already exist.");
            }

            if (input.Files.Any(x => Path.GetExtension(x.FileName).ToLower() != ".pdf"))
            {
                return ErrorHelper.ErrorResult("File", "Invalid file type");
            }

            var transactionOptions = new TransactionOptions 
            { 
                IsolationLevel = IsolationLevel.ReadCommitted, 
                Timeout = TimeSpan.FromMinutes(2) 
            };

            using var scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                foreach (IFormFile file in input.Files)
                {
                    var emp = emps.Where(x => x.CardId == Path.GetFileNameWithoutExtension(file.FileName)).FirstOrDefault();

                    SalarySlipDocument import = new()
                    {
                        Filename = file.FileName,
                        FilePath = "status running",
                        UserId = User.GetUserId(),
                        EmpId = emp.Id,
                        Year = input.Year,
                        Month = input.Month
                    };

                    _context.Add(import);
                    await _context.SaveChangesAsync();

                    string directoryPath = Path.Combine(Folder.SalarySlipDocuments, import.Id.ToString());

                    string filePath = Path.Combine(directoryPath, file.FileName);

                    string fullDirectoryPath = Path.Combine(_config["FilePath"], directoryPath);

                    string fullFilePath = Path.Combine(_config["FilePath"], filePath);

                    Directory.CreateDirectory(fullDirectoryPath);

                    using (var stream = System.IO.File.Create(fullFilePath))
                    {
                        await file.CopyToAsync(stream);
                    }

                    import.FilePath = filePath;

                    await _context.SaveChangesAsync();
                }

                scope.Complete();
                return Ok();
            }
            catch (Exception ex)
            {
                return ErrorHelper.ErrorResult("Id", ex.Message);
            }
        }

        public class ImportInputModel
        {
            public IFormFile File { get; set; }
            public int EmpId { get; set; }
            public int Year { get; set; }
            public int Month { get; set; }
        }
        public class MultipleImportInputModel
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public List<IFormFile> Files { get; set; }
        }
        public class ImportInputModelValidator : AbstractValidator<ImportInputModel>
        {
            private readonly DataContext _context;

            public ImportInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.File)
                    .NotEmpty();

                RuleFor(x => x.EmpId)
                    .NotEmpty()
                    .IdMustExist(_context.EmpDetails.AsQueryable());

                RuleFor(x => x.Year)
                    .NotEmpty()
                    .GreaterThan(0);

                RuleFor(x => x.Month)
                    .NotEmpty()
                    .GreaterThan(0)
                    .LessThan(13);
            }
        }
        public class MultipleImportInputModelValidator : AbstractValidator<MultipleImportInputModel>
        {
            private readonly DataContext _context;

            public MultipleImportInputModelValidator(DataContext context)
            {
                _context = context;

                RuleFor(x => x.Files)
                    .NotEmpty();

                RuleFor(x => x.Year)
                    .NotEmpty()
                    .GreaterThan(0);

                RuleFor(x => x.Month)
                    .NotEmpty()
                    .GreaterThan(0)
                    .LessThan(13);
            }
        }
    }
}
