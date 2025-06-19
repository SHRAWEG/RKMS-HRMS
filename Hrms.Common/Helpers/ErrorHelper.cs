using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Helpers
{
    public static class ErrorHelper
    {
        public static BadRequestObjectResult ErrorResult(string field, string message)
        {
            return new BadRequestObjectResult(Error(field, message));
        }

        public static Dictionary<string, string[]> Error(string field, string message)
        {
            return new Dictionary<string, string[]>()
            {
                { field, new string[] { message } }
            };
        }
    }
}
