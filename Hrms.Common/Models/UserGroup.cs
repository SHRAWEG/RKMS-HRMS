using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    [Table("User_Group")]
    public class UserGroup
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int GroupId { get; set; }
        public UGroup UGroup { get; set; }
    }
}
