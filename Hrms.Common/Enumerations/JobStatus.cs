using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Enumerations
{
    public class JobStatus : Enumeration
    {
        public static JobStatus Active = new("active", "Active");
        public static JobStatus InActive = new("in-active", "InActive");
        public static JobStatus Closed = new("closed", "Closed");

        public JobStatus(string id, string name)
            : base(id, name)
        {

        }
    }
}