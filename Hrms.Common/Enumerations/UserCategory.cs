using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Enumerations
{
    public class UserCategory : Enumeration
    {
        public static UserCategory Admin = new("admin", "Admin");
        public static UserCategory Operator = new("operator", "Operator");

        public UserCategory(string id, string name)
            : base(id, name)
        {

        }
    }
}