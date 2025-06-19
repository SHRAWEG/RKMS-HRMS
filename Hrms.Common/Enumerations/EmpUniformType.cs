using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Enumerations
{
    public class EmpUniformType : Enumeration
    {
        public static EmpUniformType Shirt = new("shirt", "Shirt");
        public static EmpUniformType Jacket = new("jacket", "Jacket");

        public EmpUniformType(string id, string name)
            : base(id, name)
        {

        }
    }
}
