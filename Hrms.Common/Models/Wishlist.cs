using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public class Wishlist
    {
        [Key]
        [Column("Wish_ID")]
        public int Id { get; set; }
        public string Wish_Type { get; set; }

        [Column("Wish_Date")]
        public DateTime? Wish_Date { get; set; }

        [Column("Wish_Title", TypeName = "varchar(30)")]
        public string Wish_Title { get; set; }

        [Column("Wish_Template", TypeName = "TEXT")]
        public string Wish_Template { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        
    }
}
