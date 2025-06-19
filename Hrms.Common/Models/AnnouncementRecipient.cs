using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    [Table("Announcement_Recipient")]
    public class AnnouncementRecipient
    {
        public int Id { get; set; }

        public int AnnouncementId { get; set; }
        public Announcement Announcement { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int? GroupId { get; set; }
        public UGroup UGroup { get; set; }

        public ICollection<User> Users { get; set; }
        public ICollection<UGroup> UGroups { get; set; }
        public DateTime? ReadOn { get; set; }
    }
}
