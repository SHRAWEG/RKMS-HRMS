using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("ROLE_PERMISSIONS")]
    public class RolePermission
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public Role Role { get; set; }

        [Column("ROLE_ID")]
        public int? RoleId { get; set; }

        public Permission Permission { get; set; }

        [Column("PERMISSION_ID")]
        public int? PermissionId { get; set; }
    }
}
