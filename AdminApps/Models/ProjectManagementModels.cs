using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace AdminApps.Models
{
    public class Project
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public virtual ProjectStatus ProjectStatus { get; set; }
        public virtual ICollection<GanttTask> GanttTasks { get; set; }
        public virtual ICollection<GanttLink> GanttLinks { get; set; }
    }



    public enum ProjectStatus
    {
        Deferred,
        Completed,
        InProgress,
        Ongoing,
        Discontinued,
        DeferredInProgress
    }

    static class ProjectStatusExtender
    {
        public static String AsText(this ProjectStatus status)
        {
            switch(status)
            {
                case ProjectStatus.Deferred: return "Deferred";
                case ProjectStatus.Completed: return "Completed";
                case ProjectStatus.InProgress: return "In-Progress";
                case ProjectStatus.Ongoing: return "Ongoing";
                case ProjectStatus.Discontinued: return "Discontinued";
                case ProjectStatus.DeferredInProgress: return "Deferred (In-Progress)";
                default: return "";
            }
        }
    }


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

    public class UserProject : Project
    {
        [ForeignKey("CreatedByUser")]
        public string ApplicationUserID { get; set; }

        // navigation properties
        [InverseProperty("CreatedUserProjects")]
        public virtual ApplicationUser CreatedByUser { get; set; }
    }

    public class BudgetProject : Project
    {

    }

    public class GanttTask
    {
        public int GanttTaskId { get; set; }
        public int ProjectID { get; set; }
        [MaxLength(255)]
        public string Text { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
        public decimal Progress { get; set; }
        public int SortOrder { get; set; }
        public string Type { get; set; }
        public int? ParentId { get; set; }
    }

    public class GanttLink
    {
        public int GanttLinkId { get; set; }
        public int ProjectID { get; set; }
        [MaxLength(1)]
        public string Type { get; set; }
        public int SourceTaskId { get; set; }
        public int TargetTaskId { get; set; }
    }

    public class GanttRequest
    {
        public GanttMode Mode { get; set; }
        public GanttAction Action { get; set; }

        public GanttTask UpdatedTask { get; set; }
        public GanttLink UpdatedLink { get; set; }
        public long SourceId { get; set; }

        /// <summary>
        /// Create new GanttData object and populate it
        /// </summary>
        /// <param name="form">Form collection</param>
        /// <returns>New GanttData</returns>
        public static List<GanttRequest> Parse(FormCollection form, string ganttMode)
        {
            // save current culture and change it to InvariantCulture for data parsing
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var dataActions = new List<GanttRequest>();
            var prefixes = form["ids"].Split(',');

            foreach (var prefix in prefixes)
            {
                var request = new GanttRequest();

                // lambda expression for form data parsing
                Func<string, string> parse = x => form[String.Format("{0}_{1}", prefix, x)];

                request.Mode = (GanttMode)Enum.Parse(typeof(GanttMode), ganttMode, true);
                request.Action = (GanttAction)Enum.Parse(typeof(GanttAction), parse("!nativeeditor_status"), true);
                request.SourceId = Int64.Parse(parse("id"));

                // parse gantt task
                if (request.Action != GanttAction.Deleted && request.Mode == GanttMode.Tasks)
                {
                    request.UpdatedTask = new GanttTask()
                    {
                        GanttTaskId = (request.Action == GanttAction.Updated) ? (int)request.SourceId : 0,
                        //ProjectID = (int)request.UpdatedTask.ProjectID,
                        Text = parse("text"),
                        StartDate = DateTime.Parse(parse("start_date")),
                        Duration = Int32.Parse(parse("duration")),
                        Progress = (request.Action == GanttAction.Inserted) ? 0 : Decimal.Parse(parse("progress")),
                        ParentId = (parse("parent") != "0") ? Int32.Parse(parse("parent")) : (int?)null,
                        SortOrder = (parse("order") != null) ? Int32.Parse(parse("order")) : 0,
                        Type = parse("type")
                    };
                    //request.UpdatedTask = new GanttTask();
                    //request.UpdatedTask.GanttTaskId = (request.Action == GanttAction.Updated) ? (int)request.SourceId : 0;
                    ////ProjectID = (int)request.UpdatedTask.ProjectID,
                    //request.UpdatedTask.Text = parse("text");
                    //request.UpdatedTask.StartDate = DateTime.Parse(parse("start_date"));
                    //request.UpdatedTask.Duration = Int32.Parse(parse("duration"));
                    //request.UpdatedTask.Progress = (request.Action == GanttAction.Inserted) ? 0 : Decimal.Parse(parse("progress"));
                    //request.UpdatedTask.ParentId = (parse("parent") != "0") ? Int32.Parse(parse("parent")) : (int?)null;
                    //request.UpdatedTask.SortOrder = (parse("order") != null) ? Int32.Parse(parse("order")) : 0;
                    //request.UpdatedTask.Type = parse("type");
                }
                // parse gantt link
                else if (request.Action != GanttAction.Deleted && request.Mode == GanttMode.Links)
                {
                    request.UpdatedLink = new GanttLink()
                    {
                        GanttLinkId = (request.Action == GanttAction.Updated) ? (int)request.SourceId : 0,
                        //ProjectID = (int)request.UpdatedLink.ProjectID,
                        SourceTaskId = Int32.Parse(parse("source")),
                        TargetTaskId = Int32.Parse(parse("target")),
                        Type = parse("type")
                    };
                }

                dataActions.Add(request);
            }

            // return current culture back
            Thread.CurrentThread.CurrentCulture = currentCulture;

            return dataActions;
        }
    }

    /// <summary>
    /// Gantt modes
    /// </summary>
    public enum GanttMode
    {
        Tasks,
        Links
    }

    /// <summary>
    /// Gantt actions
    /// </summary>
    public enum GanttAction
    {
        Inserted,
        Updated,
        Deleted,
        Error
    }

}