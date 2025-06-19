using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Enumerations
{
    public class TransactionMode : Enumeration
    {
        public static TransactionMode Join = new("JOIN", "Join");
        public static TransactionMode Promotion = new("PROMOTION", "Promotion");
        public static TransactionMode Transfer = new("TRANSFER", "Transfer");
        public static TransactionMode Terminate = new("TERMINATE", "Terminate");

        public TransactionMode(string id, string name)
            : base(id, name)
        {

        }
    }
}
