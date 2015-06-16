using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdminApps.Models
{
    public class Notice
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime? MeetingDate { get; set; }
        [Required]
        [DataType(DataType.Time)]
        //[Range(typeof(TimeSpan), "06:00:00", "22:00:00", ErrorMessage = "Meeting time must be between 6:00 AM and 10:00 PM")]
        public DateTime? MeetingTime { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? MeetingDateTime { get; set; }
    }
}