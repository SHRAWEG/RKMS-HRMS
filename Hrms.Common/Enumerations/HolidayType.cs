using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Enumerations
{
    public class HolidayType : Enumeration
    {
        public static HolidayType PublicHoliday = new("public-holiday", "Public Holiday");
        public static HolidayType NationalHoliday = new("national-holiday", "National Holiday");

        public HolidayType(string id, string name)
            : base(id, name)
        {

        }
    }
}
