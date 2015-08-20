using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminApps.Models.ProjectManagement
{
    public class ITProject : Project
    {
        public string Goals { get; set; }
    }

    public class ITProjectVM
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public string Status { get; set; }

        public string Goals { get; set; }
    }

}