using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    [Table("HIRING_STAGES")]
    public class HiringStage
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME", TypeName ="varchar(255)")]
        public string Name { get; set; }

        [Column("STEP")]
        public int Step { get; set; }

        [Column("IS_FIXED")]
        public bool IsFixed { get; set; } = false;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
