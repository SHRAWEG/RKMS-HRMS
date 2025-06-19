using Npgsql.Replication.PgOutput.Messages;
using NPOI.OpenXmlFormats.Dml.Diagram;

namespace Hrms.Common.Helpers
{
    public class DbHelper
    {
        private readonly DataContext _context;

        public DbHelper(DataContext context)
        {
            _context = context;
        }

        public bool EmpRegistered(int empId)
        {
            return _context.EmpLogs.Any(x => x.EmployeeId == empId);
        }

        public async Task<bool> EmpRegisteredAsync(int empId)
        {
            return await _context.EmpLogs.AnyAsync(x => x.EmployeeId == empId);
        }

        public bool EmpIsActive(int empId)
        {
            var empLog = _context.EmpLogs.FirstOrDefaultAsync(x => x.EmployeeId == empId);

            return _context.EmpTransactions.Any(x => x.Id == empLog.Id && x.StatusId == 1);
        }

        //public async decimal RecurringSh(List<EmpSalaryHead> empSalaryHeads, EmpSalaryHead empSalaryHead, decimal monthlySalary)
        //{
        //    decimal amount = 0;

        //    foreach (var data in empSalaryHead.EmpSalaryHeadDetails)
        //    {
        //        if (data.IsPercentageOfMonthlySalary)
        //        {
        //            amount += monthlySalary * (data.Percent ?? 0) / 100;

        //            data.Amount = 
        //        }
        //        else
        //        {
        //            if (data.ReferenceEmpSh.ShDataType == "PERCENT")
        //            {
        //                var recurringEmpSalaryHead = empSalaryHeads.Where(x => x.EmpShId == data.ReferenceEmpShId).FirstOrDefault();

        //                amount += RecurringSh(empSalaryHeads, recurringEmpSalaryHead, monthlySalary) * (data.Percent ?? 0) / 100;
        //            } else
        //            {
        //                amount += (data.ReferenceEmpSh.Amount ?? 0) * (data.Percent ?? 0) / 100;
        //            }
        //        }
        //    }

        //    return amount;
        //}

        public bool CompareAnxCircularDependency(List<SalaryAnnexureHead> salaryAnnexureHeads, int SalaryAnnexureHeadId, int referenceSahId)
        {
            var referenceSah = salaryAnnexureHeads.FirstOrDefault(x => x.SalaryAnnexureHeadId == referenceSahId);

            if (referenceSah == null)
                return false; // Reference not found, no circular dependency

            if (referenceSah.ShDataType == "PERCENT")
            {
                foreach (var sahDetail in referenceSah.SalaryAnnexureHeadDetails)
                {
                    if (sahDetail.IsPercentageOfMonthlySalary)
                    {
                        continue;
                    }

                    if (sahDetail.ReferenceSalaryAnnexureHeadId == SalaryAnnexureHeadId)
                    {
                        return true; // Circular dependency found, return immediately
                    }
                    else
                    {
                        // Recursively check for circular dependency in referenced items
                        if (CompareAnxCircularDependency(salaryAnnexureHeads, SalaryAnnexureHeadId, sahDetail.ReferenceSalaryAnnexureHeadId ?? 0))
                        {
                            return true; // If any recursive call finds a circular dependency, propagate the result
                        }
                    }
                }
            }

            return false; // No circular dependency found
        }

        public bool CompareCircularDependency(List<EmpSalaryHead> empSalaryHeads, int EmpShId, int referenceEmpShId)
        {
            var referenceEmpSh = empSalaryHeads.FirstOrDefault(x => x.EmpShId == referenceEmpShId);

            if (referenceEmpSh == null)
                return false; // Reference not found, no circular dependency

            if (referenceEmpSh.ShDataType == "PERCENT")
            {
                foreach (var empShDetail in referenceEmpSh.EmpSalaryHeadDetails)
                {
                    if (empShDetail.IsPercentageOfMonthlySalary)
                    {
                        continue;
                    }

                    if (empShDetail.ReferenceEmpShId == EmpShId)
                    {
                        return true; // Circular dependency found, return immediately
                    }
                    else
                    {
                        // Recursively check for circular dependency in referenced items
                        if (CompareCircularDependency(empSalaryHeads, EmpShId, empShDetail.ReferenceEmpShId ?? 0))
                        {
                            return true; // If any recursive call finds a circular dependency, propagate the result
                        }
                    }
                }
            }

            return false; // No circular dependency found
        }

        public async Task RecurringUpdate(List<EmpSalaryHead> empSalaryHeads, List<EmpSalaryHeadDetail> empSalaryHeadDetails, decimal monthlySalary)
        {
            foreach (var empSh in empSalaryHeads)
            {
                decimal totalAmount = 0;

                foreach (var empShDetail in empSh.EmpSalaryHeadDetails)
                {
                    if (empShDetail.IsPercentageOfMonthlySalary)
                    {
                        var amount = monthlySalary * (empShDetail.Percent ?? 0) / 100;
                        empShDetail.Amount = amount;
                        totalAmount += amount;
                    }
                    else
                    {
                        var amount = (empShDetail.ReferenceEmpSh?.Amount ?? 0) * (empShDetail.Percent ?? 0) / 100;
                        empShDetail.Amount = amount;
                        totalAmount += amount;
                    }
                }

                empSh.Amount = totalAmount;

                await _context.SaveChangesAsync();

                var empShIds = empSalaryHeadDetails.Where(x => x.ReferenceEmpShId == empSh.EmpShId).Select(x => x.EmpShId).ToList();

                var updateEmpShs = await _context.EmpSalaryHeads
                    .Include(x => x.EmpSalaryHeadDetails)
                    .ThenInclude(x => x.ReferenceEmpSh)
                    .Where(x => empShIds.Contains(x.EmpShId)).ToListAsync();

                if (updateEmpShs.Count > 0)
                {
                    await RecurringUpdate(updateEmpShs, empSalaryHeadDetails, monthlySalary);
                }
            }
        }
    }
}
