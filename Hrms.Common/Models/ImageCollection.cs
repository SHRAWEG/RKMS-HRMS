using System.ComponentModel.DataAnnotations.Schema;

namespace Hrms.Common.Models
{
    public class ImageCollection
    {
        public Guid Id { get; set; }
        public Guid FolderId { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? UploadedBy { get; set; }
        public ImagesFolder Folder { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    }
}