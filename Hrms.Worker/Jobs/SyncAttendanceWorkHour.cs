using Hrms.Common.Models;
using NPOI.OpenXmlFormats.Dml.Diagram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Worker.Jobs
{
    public class SyncAttendanceWorkHour : IJob
    {
        private readonly DataContext _context;

        public SyncAttendanceWorkHour(DataContext context)
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            do
            {
                var attendances = await _context.Attendances.Where(x => x.WorkHourId == null).Take(1000).ToListAsync();

                foreach(var attendance in attendances)
                {
                    short? WorkHourId;
                    var roster = await _context.Rosters.Where(x => x.Date == attendance.TransactionDate && x.EmpId == attendance.EmpId).FirstOrDefaultAsync();
                    WorkHourId = roster?.WorkHourId;

                    if (roster is null)
                    {
                        var defaultWorkHour = await _context.DefaultWorkHours
                        .Where(x => x.EmpId == attendance.EmpId || x.EmpId == null && x.DayId == ((short)attendance.TransactionDate.DayOfWeek + 1))
                        .OrderBy(x => x.EmpId)
                        .FirstOrDefaultAsync();

                        WorkHourId = defaultWorkHour?.WorkHourId;
                    }

                    attendance.WorkHourId = WorkHourId;
                }

                await _context.SaveChangesAsync();
                
            } while(await _context.Attendances.AnyAsync(x => x.WorkHourId == null));
        }
    }
}
