using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    public class User : IdentityUser<int>
    {
        public ICollection<UserRole> UserRoles { get; set; }

        public ICollection<UserGroup> UserGroups { get; set; }

       
        public bool IsActive { get; set; }

        public EmpDetail? Emp { get; set; }

        public int? EmpId { get; set; }

        public bool InOutTmChange { get; set; }

        public bool RefreshAttendance { get; set; }

        public bool ExtDayChange { get; set; }

        public bool BatchUpdate { get; set; }

        public bool UserAlert { get; set; }

        public bool SelfAttendance { get; set; }

        public bool MonthlyBulkAttendance { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
