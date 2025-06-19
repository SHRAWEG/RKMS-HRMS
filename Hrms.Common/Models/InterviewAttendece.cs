using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Models
{
    public class InterviewAttendece
    {
        public int Id { get; set; } 

        public int? attendeesId { get; set; }  
        public EmpDetail? attendees { get; set; } 

        public int? InterviewId { get; set; }
        public Interview? Interview { get; set; }      
        
        

    }
}
