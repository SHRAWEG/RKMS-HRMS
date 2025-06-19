using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public class DepartmentForLevel
    {
     
      
        public int Id { get; set; }

      
        public string Name { get; set; }

        public ICollection<HierarchyLevel> HierarchyLevels { get; set; } 

        public DateTime? CreateAt { get; set; } = DateTime.UtcNow;   
        public DateTime? UpdateAt { get; set; } = DateTime.UtcNow;
    }
}
