using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    [Table("Announcement_Documents")]
    public class AnnouncementDocuments
    {
        [Key]
        public int Id { get; set; }
        public string DocumentType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int AnnouncementId { get; set; }
        public Announcement Announcement { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UploadedBy { get; set; }

    }
}
