using AdminApps.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdminApps.ViewModels
{
    public class ProjectViewModel
    {
        public ProjectViewModel()
        {
            this.FullScreen = false;
            this.ReadOnly = true;
            this.AbandonedSchedule = false;
            this.ProjectScheduleVersion = new ProjectScheduleVersion();
        }

        public int ID { get; set; }
        public int ProjectStatusID { get; set; }
        [Display(Name = "Project Name")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Date Modified")]
        public DateTime ModifiedAt { get; set; }

        // navigation properties
        public virtual ProjectStatus ProjectStatus { get; set; }
        public virtual ProjectScheduleVersion ProjectScheduleVersion { get; set; }
        //public virtual ICollection<GanttTask> GanttTasks { get; set; }
        //public virtual ICollection<GanttLink> GanttLinks { get; set; }

        // view logic properties
        public bool FullScreen { get; set; }
        public bool ReadOnly { get; set; }
        public bool AbandonedSchedule { get; set; }

    }

    public class UserProjectViewModel : ProjectViewModel
    {
        public string ApplicationUserID { get; set; }

        public virtual ApplicationUser CreatedByUser { get; set; }

        public string TruncName
        {
            get
            {
                if (this.Name.Length > 25)
                {
                    return this.Name.Substring(0, 25) + "...";
                }
                else
                {
                    return this.Name;
                }
            }
        }


        public string TruncComments
        {
            get
            {
                if (this.Comments.Length > 50)
                {
                    return this.Comments.Substring(0, 50) + "...";
                }
                else
                {
                    return this.Comments;
                }
            }
        }

        public string TruncDateCreated
        {
            get
            {
                return this.CreatedAt.ToShortDateString();
            }
        }

    }

    public class CreateUserProjectViewModel : UserProjectViewModel
    {
        [Display(Name = "Project Start Date")]
        [DataType(DataType.Date)]
        public DateTime ProjectStartDate { get; set; }
    }
}