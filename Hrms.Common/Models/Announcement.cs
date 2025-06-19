using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        public AnnouncementCategory Category { get; set; }
        public int AnnouncementCategoryId { get; set; }
        public string Subject { get; set; }
        public string Description {  get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AnnouncementDocuments> AnnouncementDocuments { get; set; }

        public ICollection<AnnouncementRecipient> AnnouncementRecipients { get; set; }

    }

}
