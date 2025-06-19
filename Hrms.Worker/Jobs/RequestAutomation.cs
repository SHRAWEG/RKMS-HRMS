using Hrms.Common.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Worker.Jobs
{
    public class RequestAutomation : IJob
    {
        private readonly DataContext _context;

        public RequestAutomation(DataContext context)
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();

            if (settings is null)
            {
                return;
            }

            DateOnly date = DateOnly.FromDateTime(DateTime.Now);

            if (date.Day != 1)
            {
                return;
            }

            var pendingLeaves = await _context.LeaveApplicationHistories.Where(x => x.Status == "pending" && x.StartDate < date).ToListAsync();

            var pendingRegularisations = await _context.Regularisations.Where(x => x.Status == "pending" && x.FromDate < date).ToListAsync();

            foreach(var leave in pendingLeaves)
            {
                leave.Status = "disapproved";
                leave.DisapprovedByUserId = 1;
                leave.Remarks = "Auto disapproved by system.";
                leave.UpdatedAt = DateTime.UtcNow;
            }

            foreach (var regularisation in pendingRegularisations)
            {
                regularisation.Status = "disapproved";
                regularisation.DisapprovedByUserId = 1;
                regularisation.Remarks = "Auto disapproved by system.";
                regularisation.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return;
        }
    }
}
