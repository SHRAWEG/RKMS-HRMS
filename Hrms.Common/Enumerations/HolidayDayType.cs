using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Enumerations
{
    public class HolidayDayType : Enumeration
    {
        public static HolidayDayType FullDay = new("full-day", "Full Day");
        public static HolidayDayType HalfDay = new("half-day", "Half Day");

        public HolidayDayType(string id, string name)
            : base(id, name)
        {

        }
    }
}
