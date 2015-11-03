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
        public int ProjectStatusID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public virtual ProjectStatus ProjectStatus { get; set; }
        public virtual ICollection<ProjectScheduleVersion> ProjectScheduleVersions { get; set; }
        public virtual ICollection<GanttTask> GanttTasks { get; set; }
        public virtual ICollection<GanttLink> GanttLinks { get; set; }
    }

    public class ProjectScheduleVersion
    {
        public int ID { get; set; }
        public int ProjectID { get; set; }
        public int VersionNum { get; set; }
        [Display(Name = "Revision Notes")]
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Revision Date")]
        public DateTime SavedAt { get; set; }

        public virtual Project Project { get; set; }
        public virtual ICollection<GanttTask> GanttTasks { get; set; }
        public virtual ICollection<GanttLink> GanttLinks { get; set; }

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


    }

    public class ProjectStatus
    {
        public int ID { get; set; }
        [Display(Name = "Status")]
        public string Description { get; set; }

        public virtual ICollection<Project> Projects { get; set; }
    }

    public class GanttTask
    {
        public int GanttTaskId { get; set; }
        public int ProjectID { get; set; }
        public int ProjectScheduleVersionID { get; set; }
        [MaxLength(255)]
        public string Text { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
        public decimal Progress { get; set; }
        public int SortOrder { get; set; }
        public string Type { get; set; }
        public int? ParentId { get; set; }
        public bool Open { get; set; }

        // custom properties for baseline date values
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }

        // navigation properties
        public virtual Project Project { get; set; }
        public virtual ProjectScheduleVersion ProjectScheduleVersion { get; set; }
    }

    public class GanttLink
    {
        public int GanttLinkId { get; set; }
        public int ProjectID { get; set; }
        public int ProjectScheduleVersionID { get; set; }
        [MaxLength(1)]
        public string Type { get; set; }
        public int SourceTaskId { get; set; }
        public int TargetTaskId { get; set; }

        // navigation properties
        public virtual Project Project { get; set; }
        public virtual ProjectScheduleVersion ProjectScheduleVersion { get; set; }
    }

    public class GanttRequest
    {
        public GanttMode Mode { get; set; }
        public GanttAction Action { get; set; }

        public GanttTask UpdatedTask { get; set; }
        public GanttLink UpdatedLink { get; set; }
        public long SourceId { get; set; }
        //public string ReOrderTarget { get; set; }

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
                    // try/catch added to prevent errors caused by null values in form collection.
                    var progressValue = new Decimal();
                    try { progressValue = Decimal.Parse(parse("progress")); }
                    catch { progressValue = 0; }

                    var plannedStartValue = new DateTime?();
                    try { plannedStartValue = DateTime.Parse(parse("planned_start")); }
                    catch { plannedStartValue = null; }

                    var plannedEndValue = new DateTime?();
                    try { plannedEndValue = DateTime.Parse(parse("planned_end")); }
                    catch { plannedEndValue = null; }

                    request.UpdatedTask = new GanttTask()
                    {
                        GanttTaskId = (request.Action == GanttAction.Updated || request.Action == GanttAction.Order) ? (int)request.SourceId : 0,
                        Text = parse("text"),
                        StartDate = DateTime.Parse(parse("start_date")),
                        Duration = Int32.Parse(parse("duration")),
                        // the dhtmlxGantt library does not pass a 'progress' value to the server when a new task is created.
                        // as a result the following line throws a null exception error when a newly-created task is modified before the gantt chart is reloaded from the db.
                        //Progress = Decimal.Parse(parse("progress")),
                        Progress = progressValue,
                        ParentId = (parse("parent") != "0") ? Int32.Parse(parse("parent")) : (int?)null,
                        SortOrder = (parse("order") != null) ? Int32.Parse(parse("order")) : 0,
                        Type = parse("type"),
                        PlannedStartDate = plannedStartValue,
                        PlannedEndDate = plannedEndValue,
                        Open = (parse("open") != null) ? bool.Parse(parse("open")) : false
                    };
                }
                // parse gantt link
                else if (request.Action != GanttAction.Deleted && request.Mode == GanttMode.Links)
                {
                    request.UpdatedLink = new GanttLink()
                    {
                        GanttLinkId = (request.Action == GanttAction.Updated) ? (int)request.SourceId : 0,
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
        Error,
        Order
    }

}