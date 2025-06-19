using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Enumerations
{
    public class SalaryHeadCategoryType : Enumeration
    {
        public static SalaryHeadCategoryType Earning = new("EARNING", "Earning");
        public static SalaryHeadCategoryType Deduction = new("DEDUCTION", "Deduction");

        public SalaryHeadCategoryType(string id, string name)
            : base(id, name)
        {

        }
    }
}