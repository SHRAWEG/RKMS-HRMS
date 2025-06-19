using NPOI.OpenXmlFormats.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    [Table("CANDIDATES")]
    public class Candidate
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("FIRST_NAME", TypeName ="varchar(255)")]
        public string FirstName { get; set; }

        [Column("MIDDLE_NAME", TypeName ="varchar(255)")]
        public string? MiddleName { get; set; }

        [Column("LAST_NAME", TypeName ="varchar(255)")]
        public string? LastName { get; set; }

        [Column("EMAIL", TypeName ="varchar(255)")]
        public string Email { get; set; }

        [Column("PHONE", TypeName ="varchar(20)")]
        public string ContactNumber { get; set; }

        [Column("SKILLS")]
        public List<string>? Skills { get; set; }

        public Job? Job { get; set; }

        [Column("JOB_ID")]
        public int? JobId { get; set; }

        [Column("COVER_LETTER")]
        public string? CoverLetter { get; set; }

        [Column("OVERALL_RATING")]
        public int? OverallRating { get; set; }

        [Column("REMARKS")]
        public string? Remarks { get; set; }

        public HiringStage? Stage { get; set; }

        [Column("STAGE_ID")]
        public int? StageId { get; set; }

        public User? CreatedByUser { get; set; }

        [Column("CREATED_BY_USER_ID")]
        public int? CreatedByUserId { get; set; }

        public User? EvaluatedByUser { get; set; }

        [Column("EVALUATED_BY_USER_ID")]
        public int? EvaluatedByUserId { get; set; }

        public User? HiredByUser { get; set; }

        [Column("HIRED_BY_USER_ID")]
        public int? HiredByUserId { get; set; }

        [Column("CV_FILE_NAME")]
        public string? CvFileName { get; set; }

        [Column("CV_PATH", TypeName ="varchar(255)")]
        public string? CvPath { get; set; }

        [Column("IMAGE_FILE_NAME")]
        public string? ImageFileName { get; set; }

        [Column("IMAGE_PATH", TypeName = "varchar(255)")]
        public string? ImagePath { get; set; }

        public decimal? CurrentCTC { get; set; }            
        public decimal? ExpectedCTC { get; set; }           
        public string? NoticePeriod { get; set; }           

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<CandidateSource> CandidateSources { get; set; } = new List<CandidateSource>();
    }
}
