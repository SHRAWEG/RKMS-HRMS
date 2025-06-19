using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Helpers
{
    public static class ValidationHelper
    {
        public static List<string> GetErrorFields(ValidationResult result)
        {
            return result.Errors
                .Select(x => x.PropertyName)
                .Distinct()
                .ToList();
        }
    }
}
