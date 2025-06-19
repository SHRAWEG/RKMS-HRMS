using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("EMP_DEVICE_CODE")]
    public class EmpDeviceCode
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        public EmpDetail Emp { get; set; }

        [Column("EMP_ID")]
        public int EmpId { get; set; }
 
        [Column("DEVICE_CODE", TypeName ="varchar(255)")]
        public string DeviceCode { get; set; }

        public DeviceSetting? Device { get; set; }

        [Column("DEVICE_ID")]
        public int? DeviceId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("LAST_FETCHED_AT")]
        public DateTime? LastFetchedAt { get; set; } 
    }
}
