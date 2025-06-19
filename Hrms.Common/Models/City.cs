using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("CITY")]
    public class City
    {
        [Key]
        [Column("CITY_ID")]
        public int Id { get; set; }

        public State State { get; set; }

        [Column("STATE_ID")]
        public int StateId { get; set; }

        [Column("CITY_NAME", TypeName = "varchar(255)")]
        public string Name { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
