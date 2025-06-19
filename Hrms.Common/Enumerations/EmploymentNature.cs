using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Enumerations
{
    public class EmploymentNature : Enumeration
    {
        public static EmploymentNature NewHiring = new("new-hiring", "New Hiring");
        public static EmploymentNature Replacement = new("replacement", "Replacement");

        public EmploymentNature(string id, string name)
            : base(id, name)
        {

        }
    }
}
