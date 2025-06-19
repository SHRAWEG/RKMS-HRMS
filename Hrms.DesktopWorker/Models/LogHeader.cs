using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.DesktopWorker.Models
{
    public class LogHeader
    {
        [Name("Log Date")]
        public DateTime LogDate { get; set; }

        [Name("Error Trace")]
        public string? ErrorTrace { get; set; }

        [Name("Error Message")]
        public string? ErrorMessage { get; set; }
    }
}
