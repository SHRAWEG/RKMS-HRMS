using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("USER_PERMISSIONS")]
    public class UserPermission
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public User User { get; set; }

        [Column("USER_ID")]
        public int UserId { get; set; }

        public Permission Permission { get; set; }

        [Column("PERMISSION_ID")]
        public int PermissionId { get; set; }
    }
}
