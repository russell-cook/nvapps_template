using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminApps.Models
{
    public class DeskManual
    {
        public int ID { get; set; }
        public int ApplicationUserID { get; set; }

        public virtual ApplicationUser DeskOwner { get; set; }
        public virtual ICollection<OfficeTask> Tasks { get; set; }
    }

    public class OfficeTask
    {
        public int ID { get; set; }
        public int DeskManualID { get; set; }
        public int TaskFrequencyID { get; set; }
        public string Name { get; set; }

        public virtual DeskManual DeskManual { get; set; }
        public virtual TaskFrequency TaskFrequency { get; set; }
        public virtual ICollection<TaskStep> Steps { get; set; }
        public virtual ApplicationUser TaskOwner { get; set; }
        public virtual ICollection<ApplicationUser> TaskBackups { get; set; }
    }

    public class TaskStep
    {
        public int ID { get; set; }
        public int DeskManualTaskID { get; set; }
        public string Name { get; set; }
        [AllowHtml]
        public string Instructions { get; set; }

        public virtual OfficeTask OfficeTask { get; set; }
    }

    public class TaskFrequency
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}