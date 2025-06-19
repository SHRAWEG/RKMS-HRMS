using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("USER_COMPANIES")]
    public class UserCompany
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public User User { get; set; }

        [Column("USER_ID")]
        public int UserId { get; set; }

        public Company Company { get; set; }

        [Column("COMPANY_ID")]
        public int CompanyId { get; set; }
    }
}
