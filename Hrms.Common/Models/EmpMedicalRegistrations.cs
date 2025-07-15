using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Hrms.Common.Models
    {
        [Table("EMP_MEDICAL_REGISTRATION")]
        public class EmpMedicalRegistration
        {
            [Key]
            [Column("ID")]
            public int Id { get; set; }

            public EmpDetail? Emp { get; set; }

            [Column("EMP_ID")]
            public int? EmpId { get; set; }

            [Column("REGISTRATION_NUMBER", TypeName = "varchar(255)")]
            public string RegistrationNumber { get; set; }

            [Column("START_DATE")]
            public DateOnly StartDate { get; set; }

            [Column("END_DATE")]
            public DateOnly EndDate { get; set; }
            public EmpDocument? Document { get; set; }

            [Column("DOCUMENT_ID")]
            public int? DocumentId { get; set; }

            [Column("CREATED_AT")]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            [Column("UPDATED_AT")]
            public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        }
    }

