using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public class ImagesFolder
    {
        [Key]
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public Boolean IsDownloadable { get; set; }
        public string? CreatedBy { get; set; }
        
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<ImageCollection>? ImagesCollection { get; set; }
    }
}
