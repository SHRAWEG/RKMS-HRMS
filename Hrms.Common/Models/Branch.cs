using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    [Table("BRANCH")]
    public partial class Branch
    {
        [Key]
        [Column("BRANCH_ID")]
        public short Id { get; set; }
        
        [Column("BRANCH_NAME", TypeName = "varchar(50)")]
        public string? Name { get; set; }
        public City? City { get; set; }

        [Column("CITY_ID")]
        public int? CityId { get; set; }

        public State? State { get; set; }

        [Column("STATE_ID")]
        public int? StateId { get; set; }

        [Column("ISOUTBRANCH")]
        public byte? IsOutBranch { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string Address { get; set; }
        
        [Column(TypeName = "varchar(150)")]
        public string Contact { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
