using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Hrms.AdminApi.Controllers.LoanController;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly DataContext _context;

        public LoanController(DataContext context)
        {
            _context = context;
        }

        //Loan Application -? Create, Update, Get All, Pagination, Delete
        [HttpPost("CreateLoanApplication")]
        public async Task<IActionResult> CreateLoanApplication([FromBody] AddInputModel loanApplicationDto)
        {
            if (loanApplicationDto == null)
            {
                return BadRequest("Invalid data.");
            }

            var loanApplication = new LoanApplication
            {
                EmployeeId = loanApplicationDto.EmployeeId,
                LoanType = loanApplicationDto.LoanType,
                LoanAmount = loanApplicationDto.LoanAmount,
                RepaymentPeriod = loanApplicationDto.RepaymentPeriod,
                InterestRate = loanApplicationDto.InterestRate,
                CreatedAt = DateTime.UtcNow
               
            };

            _context.LoanApplications.Add(loanApplication);
            await _context.SaveChangesAsync();

           
            if (loanApplicationDto.Documents != null && loanApplicationDto.Documents.Any())
            {
                foreach (var documentName in loanApplicationDto.Documents)
                {
                    var loanDocument = new LoanDocument
                    {
                        FileName = documentName,
                        FileExtension = System.IO.Path.GetExtension(documentName), 
                        FileDescription = "Loan Documents", 
                        Remarks = "N/A", 
                        LoanLoanId = loanApplication.LoanId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.LoanDocuments.Add(loanDocument);
                }

                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(CreateLoanApplication), new { id = loanApplication.LoanId }, loanApplication);
        }
        [HttpPut("UpdateLoanApplication/{loanId}")]
        public async Task<IActionResult> UpdateLoanApplication(int loanId, [FromBody] UpdateInputModel loanApplicationDto)
        {
            if (loanApplicationDto == null || loanId != loanApplicationDto.LoanId)
            {
                return BadRequest("Invalid data or mismatched LoanId.");
            }

          
            var loanApplication = await _context.LoanApplications
                                                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loanApplication == null)
            {
                return NotFound($"Loan Application with ID {loanId} not found.");
            }

            
            loanApplication.EmployeeId = loanApplicationDto.EmployeeId ?? loanApplication.EmployeeId;
            loanApplication.LoanType = loanApplicationDto.LoanType ?? loanApplication.LoanType;
            loanApplication.LoanAmount = loanApplicationDto.LoanAmount ?? loanApplication.LoanAmount;
            loanApplication.RepaymentPeriod = loanApplicationDto.RepaymentPeriod ?? loanApplication.RepaymentPeriod;
            loanApplication.InterestRate = loanApplicationDto.InterestRate ?? loanApplication.InterestRate;
            loanApplication.UpdatedAt = DateTime.UtcNow;

          
            if (loanApplicationDto.Documents != null && loanApplicationDto.Documents.Any())
            {
                
                var existingDocuments = await _context.LoanDocuments
                                                      .Where(d => d.LoanLoanId == loanId)
                                                      .ToListAsync();
                _context.LoanDocuments.RemoveRange(existingDocuments);

                
                foreach (var documentName in loanApplicationDto.Documents)
                {
                    var loanDocument = new LoanDocument
                    {
                        FileName = documentName,
                        FileExtension = System.IO.Path.GetExtension(documentName),
                        FileDescription = "Loan Documents", 
                        Remarks = "N/A", 
                        LoanLoanId = loanApplication.LoanId, 
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.LoanDocuments.Add(loanDocument);
                }
            }
            await _context.SaveChangesAsync();
            return Ok(loanApplication);
        }
        [HttpGet("GetAllLoanApplications")]
        public async Task<IActionResult> GetAllLoanApplications()
        {
            var loanApplications = await _context.LoanApplications.Include(x=>x.Employee)   
                                                 .OrderBy(l => l.CreatedAt) 
                                                 .ToListAsync();
            var loanDocuments = await _context.LoanDocuments.ToListAsync();
            var loanApplicationDtos = loanApplications.Select(la => new LoanApplicationDto
            {
                LoanId = la.LoanId,
                EmployeeId = la.EmployeeId,
                EmployeeFirstName = la.Employee?.FirstName ,
                EmployeeMiddleName = la.Employee?.MiddleName,
                EmployeeLastName = la.Employee?.LastName,       
                LoanType = la.LoanType,
                LoanAmount = la.LoanAmount,
                RepaymentPeriod = la.RepaymentPeriod,
                InterestRate = la.InterestRate,
                CreatedAt = la.CreatedAt,
                UpdatedAt = la.UpdatedAt,
                Documents = loanDocuments
                             .Where(ld => ld.LoanLoanId == la.LoanId) 
                             .Select(ld => new LoanDocumentDto
                             {
                                 Id = ld.Id,
                                 FileName = ld.FileName,
                                 FileExtension = ld.FileExtension,
                                 FileDescription = ld.FileDescription
                             })
                             .ToList()
            }).ToList();

           
            return Ok(loanApplicationDtos);
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllLoanApplication(int page = 1,int pageSize = 10,decimal? minLoanAmount = null,decimal? maxLoanAmount = null)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }
            var loanApplicationsQuery = _context.LoanApplications.AsQueryable();
            if (minLoanAmount.HasValue)
            {
                loanApplicationsQuery = loanApplicationsQuery.Where(la => la.LoanAmount >= minLoanAmount.Value);
            }

            if (maxLoanAmount.HasValue)
            {
                loanApplicationsQuery = loanApplicationsQuery.Where(la => la.LoanAmount <= maxLoanAmount.Value);
            }
            var totalCount = await loanApplicationsQuery.CountAsync();
            var loanApplications = await loanApplicationsQuery
                                                 .OrderBy(l => l.CreatedAt) 
                                                 .Skip((page - 1) * pageSize) 
                                                 .Take(pageSize) 
                                                 .ToListAsync();
            var loanDocuments = await _context.LoanDocuments.ToListAsync();
            var loanApplicationDtos = loanApplications.Select(la => new LoanApplicationDto
            {
                LoanId = la.LoanId,
                EmployeeId = la.EmployeeId,
                LoanType = la.LoanType,
                EmployeeFirstName = la.Employee?.FirstName,
                EmployeeMiddleName = la.Employee?.MiddleName,   
                EmployeeLastName = la.Employee?.LastName,       
                LoanAmount = la.LoanAmount,
                RepaymentPeriod = la.RepaymentPeriod,
                InterestRate = la.InterestRate,
                CreatedAt = la.CreatedAt,
                UpdatedAt = la.UpdatedAt,
                Documents = loanDocuments
                             .Where(ld => ld.LoanLoanId == la.LoanId) 
                             .Select(ld => new LoanDocumentDto
                             {
                                 Id = ld.Id,
                                 FileName = ld.FileName,
                                 FileExtension = ld.FileExtension,
                                 FileDescription = ld.FileDescription
                             })
                             .ToList()
            }).ToList();

            
            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                LoanApplications = loanApplicationDtos
            };

            return Ok(response);
        }
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.LoanApplications.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.LoanApplications.AnyAsync(x => x.LoanId == id))
            {
                return ErrorHelper.ErrorResult("Id", "Loan Application is already in use.");
            }

            _context.LoanApplications.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }


        //Loan Approved -?  Create, Update, Get All, Pagination, Delete
        [HttpPost("LoanApplicationStatus")]
        public async Task<IActionResult> CreateLoanApplicationStatus([FromBody] AddLoanStatusModel input)
        {
            if (input == null)
            {
                return BadRequest("Invalid data.");
            }
            var loanApplicationExists = await _context.LoanApplications
                                                      .AnyAsync(la => la.LoanId == input.LoanId);
            if (!loanApplicationExists)
            {
                return NotFound($"LoanApplication with ID {input.LoanId} not found.");
            }
            var loanStatus = new LoanStatus
            {
                LoanApplicationLoanId = input.LoanId,
                LoanApplicatonStatus = input.LoanApplicationStatus, 
                LoanStatusAmount = input.LoanStatusAmount,
                CreatedAt = DateTime.UtcNow
            };
            _context.LoanStatus.Add(loanStatus);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Loan Status created successfully", loanStatusId = loanStatus.Id });
        }
        [HttpPut("LoanApplicationStatus/{id}")]
        public async Task<IActionResult> UpdateLoanApplicationStatus(int id, [FromBody] UpdateLoanStatusModel input)
        {
            if (input == null)
            {
                return BadRequest("Invalid data.");
            }
            var loanStatus = await _context.LoanStatus
                                           .FirstOrDefaultAsync(ls => ls.Id == id);

            if (loanStatus == null)
            {
                return NotFound($"LoanStatus with ID {id} not found.");
            }
            var loanApplicationExists = await _context.LoanApplications
                                                      .AnyAsync(la => la.LoanId == input.LoanId);

            if (!loanApplicationExists)
            {
                return NotFound($"LoanApplication with ID {input.LoanId} not found.");
            }
            loanStatus.LoanApplicationLoanId = input.LoanId;
            loanStatus.LoanApplicatonStatus = input.LoanApplicationStatus;
            loanStatus.LoanStatusAmount = input.LoanStatusAmount;
            loanStatus.CreatedAt = DateTime.UtcNow; 
            await _context.SaveChangesAsync();
            return Ok(new { message = "Loan Status updated successfully", loanStatusId = loanStatus.Id });
        }
        [HttpGet("LoanApplicationStatus/GetAll")]
        public async Task<IActionResult> GetAllLoanApplicationStatus()
        {
            var loanStatuses = await _context.LoanStatus.OrderBy(ls => ls.CreatedAt).ToListAsync();
            return Ok(loanStatuses);
        }
        [HttpGet("LoanApplicationStatus/Paginated")]
        public async Task<IActionResult> GetLoanApplicationStatusPaginated(int startIndex = 0, int pageSize = 10)
        {
            if (startIndex < 0 || pageSize <= 0)
            {
                return BadRequest("Invalid pagination parameters.");
            }
            var loanStatuses = await _context.LoanStatus.OrderBy(ls => ls.CreatedAt).Skip(startIndex).Take(pageSize).ToListAsync();
            var totalRecords = await _context.LoanStatus.CountAsync();

            var paginatedResult = new
            {
                TotalRecords = totalRecords,
                LoanStatuses = loanStatuses
            };

            return Ok(paginatedResult);
        }
        [HttpDelete("DeleteLoanStatus/{id}")]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            var data = await _context.LoanStatus.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.LoanStatus.AnyAsync(x => x.Id == id))
            {
                return ErrorHelper.ErrorResult("Id", "Loan Status is already in use.");
            }

            _context.LoanStatus.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }



        //LoanDisbursement -? Create, Update, Get All
        [HttpPost("LoanDisbursement")]
        public async Task<IActionResult> CreateLoanDisbursement([FromBody] AddLoanDisbursementModel input)
        {
            if (input == null)
            {
                return BadRequest("Invalid data.");
            }
            var loanApplicationExists = await _context.LoanApplications
                                                      .AnyAsync(la => la.LoanId == input.LoanId);
            if (!loanApplicationExists)
            {
                return NotFound($"LoanDisbursement with ID {input.LoanId} not found.");
            }
            var loanDisbursement = new LoanDisbursement
            {
                LoanApplicationLoanId = input.LoanId,
                LoanDisbursementAmount = input.DisbursementAmount,
                DisbursementDate = input.DisbursementDate,
                CreatedAt = DateTime.UtcNow
            };
            _context.LoanDisbursements.Add(loanDisbursement);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Loan Disbursement created successfully", loanDisbursementId = loanDisbursement.Id });
        }
        [HttpPut("LoanDisbursement/{id}")]
        public async Task<IActionResult> UpdateLoanDisbursement(int id, [FromBody] UpdateLoanDisbursementModel input)
        {
            if (input == null)
            {
                return BadRequest("Invalid data.");
            }
            var loanStatus = await _context.LoanDisbursements
                                           .FirstOrDefaultAsync(ls => ls.Id == id);

            if (loanStatus == null)
            {
                return NotFound($"LoanDisbursement with ID {id} not found.");
            }
            var loanApplicationExists = await _context.LoanApplications
                                                      .AnyAsync(la => la.LoanId == input.LoanId);

            if (!loanApplicationExists)
            {
                return NotFound($"LoanApplication with ID {input.LoanId} not found.");
            }
            loanStatus.LoanApplicationLoanId = input.LoanId;
            loanStatus.LoanDisbursementAmount = input.DisbursementAmount;
            loanStatus.DisbursementDate = input.DisbursementDate;
            loanStatus.CreatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Loan Disbursement updated successfully", loanStatusId = loanStatus.Id });
        }
        [HttpGet("LoanDisbursement/GetAll")]
        public async Task<IActionResult> GetAllLoanDisbursement()
        {
            var loanStatuses = await _context.LoanDisbursements.OrderBy(ls => ls.CreatedAt).ToListAsync();
            return Ok(loanStatuses);
        }




        //LoanDisbursement -? Create, Update, Get , Get By EmployeeId
        [HttpPost("LoanRepayment")]
        public async Task<IActionResult> CreateLoanRepayment([FromBody] AddLoanRepaymentModel input)
        {
            if (input == null)
            {
                return BadRequest("Invalid data.");
            }
            var loanApplicationExists = await _context.LoanApplications
                                                      .AnyAsync(la => la.LoanId == input.LoanId);
            if (!loanApplicationExists)
            {
                return NotFound($"Loan Application with ID {input.LoanId} not found.");
            }
            var loanRepayment = new LoanRepayment
            {
                LoanApplicationLoanId = input.LoanId,
                PaymentAmount = input.PaymentAmount,
                PaymentDate = input.PaymentDate,
                CreatedAt = DateTime.UtcNow
            };
            _context.LoanRepayments.Add(loanRepayment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Loan Repayment created successfully", loanRepaymentId = loanRepayment.Id });
        }
        [HttpPut("LoanRepayment/{id}")]
        public async Task<IActionResult> UpdateLoanRepayment(int id, [FromBody] UpdateLoanRepaymentModel input)
        {
            if (input == null)
            {
                return BadRequest("Invalid data.");
            }
            var loanRepayment = await _context.LoanRepayments.FirstOrDefaultAsync(lr => lr.Id == id);
            if (loanRepayment == null)
            {
                return NotFound($"Loan Repayment with ID {id} not found.");
            }
            var loanApplicationExists = await _context.LoanApplications
                                                      .AnyAsync(la => la.LoanId == input.LoanId);
            if (!loanApplicationExists)
            {
                return NotFound($"Loan Application with ID {input.LoanId} not found.");
            }

            loanRepayment.LoanApplicationLoanId = input.LoanId;
            loanRepayment.PaymentAmount = input.PaymentAmount;
            loanRepayment.PaymentDate = input.PaymentDate;
            loanRepayment.UpdatedAt = DateTime.UtcNow; 
            await _context.SaveChangesAsync();

            return Ok(new { message = "Loan Repayment updated successfully", loanRepaymentId = loanRepayment.Id });
        }
        [HttpGet("getbyempid/{employeeId}")]
        public async Task<IActionResult> GetLoanApplicationsByEmployeeId(int employeeId)
        {
         
            var loanApplications = await _context.LoanApplications
                                                 .Where(la => la.EmployeeId == employeeId)
                                                 .Include(x => x.Employee)
                                                 .OrderBy(l => l.CreatedAt)
                                                 .ToListAsync();
            var LoanStatus = await _context.LoanStatus.ToListAsync();           
            var loanDocuments = await _context.LoanDocuments.ToListAsync();
            var loanApplicationDtos = loanApplications.Select(la => new LoanApplicationDto
            {
                LoanId = la.LoanId,
                EmployeeId = la.EmployeeId,
                EmployeeFirstName = la.Employee?.FirstName,
                EmployeeMiddleName = la.Employee?.MiddleName,
                EmployeeLastName = la.Employee?.LastName,
                LoanType = la.LoanType,
                LoanAmount = la.LoanAmount,
                RepaymentPeriod = la.RepaymentPeriod,
                InterestRate = la.InterestRate,
                CreatedAt = la.CreatedAt,
                UpdatedAt = la.UpdatedAt,
                Documents = loanDocuments
                             .Where(ld => ld.LoanLoanId == la.LoanId)
                             .Select(ld => new LoanDocumentDto
                             {
                                 Id = ld.Id,
                                 FileName = ld.FileName,
                                 FileExtension = ld.FileExtension,
                                 FileDescription = ld.FileDescription
                             })
                             .ToList()
            }).ToList();
            if (!loanApplicationDtos.Any())
            {
                return NotFound($"No loan applications found for employee with ID {employeeId}.");
            }

            return Ok(loanApplicationDtos);
        }


        //EMI Calculation
        [HttpGet("calculateEMI")]
        public IActionResult CalculateEMI([FromQuery] decimal loanAmount, [FromQuery] decimal interestRate, [FromQuery] int repaymentPeriod)
        {
            if (loanAmount <= 0 || interestRate <= 0 || repaymentPeriod <= 0)
                return BadRequest("Invalid input parameters.");

            var emiResult = CalculateLoanEMI(loanAmount, interestRate, repaymentPeriod);

            return Ok(emiResult);
        }
        private LoanEMI CalculateLoanEMI(decimal loanAmount, decimal interestRate, int repaymentPeriod)
        {
            decimal monthlyInterestRate = interestRate / 12 / 100;
            decimal emiAmount = (loanAmount * monthlyInterestRate * (decimal)Math.Pow((double)(1 + monthlyInterestRate), repaymentPeriod)) /
                                ((decimal)Math.Pow((double)(1 + monthlyInterestRate), repaymentPeriod) - 1);
            decimal totalRepaymentAmount = emiAmount * repaymentPeriod;
            decimal interestAmount = totalRepaymentAmount - loanAmount;

            return new LoanEMI
            {
                EmiAmount = Math.Round(emiAmount, 2),
                TotalRepaymentAmount = Math.Round(totalRepaymentAmount, 2),
                InterestAmount = Math.Round(interestAmount, 2)
            };
        }



        //Loan Repayment
        public class BaseLoanRepaymentModel
        {
            public int? LoanId { get; set; }
            public decimal? PaymentAmount { get; set; }
            public DateTime? PaymentDate { get; set; }
        }
        public class AddLoanRepaymentModel : BaseLoanRepaymentModel { }
        public class UpdateLoanRepaymentModel : BaseLoanRepaymentModel { }
        public class LoanEMI
        {
            public decimal EmiAmount { get; set; }
            public decimal TotalRepaymentAmount { get; set; }
            public decimal InterestAmount { get; set; }
        }
    




        //Loan Disbursement 
        public class BaseLoanDisbursementModel
        {
            public int? LoanId { get; set; }
            public decimal? DisbursementAmount { get; set; }
            public DateTime? DisbursementDate { get; set; }
        }
        public class AddLoanDisbursementModel : BaseLoanDisbursementModel { }
        public class UpdateLoanDisbursementModel : BaseLoanDisbursementModel { }



        //Loan Application Status 
        public class BaseLoanStatusModel
        {
            public int? LoanId { get; set; }    
            public string? LoanApplicationStatus { get; set; }      
            public decimal? LoanStatusAmount { get; set; }        
        }
        public class AddLoanStatusModel : BaseLoanStatusModel { }
        public class UpdateLoanStatusModel : BaseLoanStatusModel { }


        //Loan Application 
        public class LoanApplicationDto
        {
          public string? EmployeeFirstName { get; set; }
            public string? EmployeeMiddleName { get; set; }
            public string? EmployeeLastName { get; set; }
            public int LoanId { get; set; }
            public int? EmployeeId { get; set; }
            public string? LoanType { get; set; }
            public decimal? LoanAmount { get; set; }
            public decimal? RepaymentPeriod { get; set; }
            public decimal? InterestRate { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }

            public List<LoanDocumentDto> Documents { get; set; } = new List<LoanDocumentDto>();
        }
        public class LoanDocumentDto
        {
            public int Id { get; set; }
            public string FileName { get; set; }
            public string FileExtension { get; set; }
            public string FileDescription { get; set; }
        }
        public class BaseInputModel
        {
            public int? EmployeeId { get; set; }
            public string? LoanType { get; set; }
            public decimal? LoanAmount { get; set; }
            public decimal? RepaymentPeriod { get; set; }
            public decimal? InterestRate { get; set; }
            public List<string> Documents { get; set; } = new List<string>(); 
        }
        public class BaseUpdateInputModel
        {
            public int LoanId { get; set; }
            public int? EmployeeId { get; set; }
            public string? LoanType { get; set; }
            public decimal? LoanAmount { get; set; }
            public decimal? RepaymentPeriod { get; set; }
            public decimal? InterestRate { get; set; }
            public List<string> Documents { get; set; } = new List<string>(); 
        }
        public class AddInputModel : BaseInputModel { }
        public class UpdateInputModel : BaseUpdateInputModel { }
    }
}
