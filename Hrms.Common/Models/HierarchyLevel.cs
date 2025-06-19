using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public class HierarchyLevel
    {
       
        public int Id { get; set; }

        public int Level { get; set; }

      
        public string LevelName { get; set; }

        public int? ParentId { get; set; }

        public int? DepartmentId { get; set; } 

        [ForeignKey("ParentId")]
        public HierarchyLevel Parent { get; set; } 

        [ForeignKey("DepartmentId")]
        public DepartmentForLevel Department { get; set; } 

        public ICollection<HierarchyLevel> ChildLevels { get; set; }

        public DateTime? CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; } = DateTime.UtcNow;
    }
}