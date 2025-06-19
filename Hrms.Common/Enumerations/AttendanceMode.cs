using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Enumerations
{
    public class AttendanceMode : Enumeration
    {
        public static AttendanceMode Direction = new("direction", "Direction");
        public static AttendanceMode NoDirection = new("no-direction", "No Direction");

        public AttendanceMode(string id, string name)
            : base(id, name)
        {

        }
    }
}
