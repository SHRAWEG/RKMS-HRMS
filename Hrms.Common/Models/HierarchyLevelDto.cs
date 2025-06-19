using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public class HierarchyLevelDto
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string LevelName { get; set; }
        public int? ParentId { get; set; }
        public string DepartmentName { get; set; } 
        public List<HierarchyLevelDto> ChildLevels { get; set; }
    }
}
