using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Enumerations
{
    public class EmpPersonType : Enumeration
    {
        public static EmpPersonType Onroll = new("onroll", "onroll");
        public static EmpPersonType Offroll = new("offroll", "offroll");

        public EmpPersonType(string id, string name)
            : base(id, name)
        {

        }
    }
}
