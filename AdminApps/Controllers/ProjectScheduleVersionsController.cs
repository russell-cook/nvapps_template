using AdminApps.DAL;
using AdminApps.Models;
using AdminApps.Models.ProjectManagement;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace AdminApps.Controllers
{
    public class ProjectScheduleVersionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        public JsonResult GetGanttData(int id)
        {
            var jsonData = new
            {
                // create tasks array
                data = (
                    from t in db.GanttTasks.AsEnumerable()
                    where t.ProjectScheduleVersionID == id
                    orderby t.SortOrder
                    select new
                    {
                        id = t.GanttTaskId,
                        text = t.Text,
                        start_date = t.StartDate.ToString("u"),
                        duration = t.Duration,
                        order = t.SortOrder,
                        progress = t.Progress,
                        open = t.Open,
                        parent = t.ParentId,
                        type = (t.Type != null) ? t.Type : String.Empty,
                        planned_start = (t.PlannedStartDate == null ? String.Empty : t.PlannedStartDate.Value.ToString("u")),
                        planned_end = (t.PlannedEndDate == null ? String.Empty : t.PlannedEndDate.Value.ToString("u")),
                        reorder = false // reorder flag is used only on client side during syncSortOrder, to prevent updates from being fired when tasks are opened/closed during syncSortOrder operation
                    }
                ).ToArray(),
                // create links array
                links = (
                    from l in db.GanttLinks.AsEnumerable()
                    where l.ProjectScheduleVersionID == id
                    select new
                    {
                        id = l.GanttLinkId,
                        source = l.SourceTaskId,
                        target = l.TargetTaskId,
                        type = l.Type
                    }
                ).ToArray()
            };
            return new JsonResult { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ContentResult Save(int id, int projectId, FormCollection form)
        {
            var dataActions = GanttRequest.Parse(form, Request.QueryString["gantt_mode"]);
            try
            {
                foreach (var ganttData in dataActions)
                {
                    switch (ganttData.Mode)
                    {
                        case GanttMode.Tasks:
                            UpdateTasks(id, projectId, ganttData);
                            break;
                        case GanttMode.Links:
                            UpdateLinks(id, projectId, ganttData);
                            break;
                    }
                }
                db.SaveChanges();
            }
            catch
            {
                // return error to client if something went wrong
                dataActions.ForEach(g => { g.Action = GanttAction.Error; });
            }
            return GanttRespose(dataActions);
        }

        /// <summary>
        /// Update gantt tasks
        /// </summary>
        /// <param name="ganttData">GanttData object</param>
        private void UpdateTasks(int projectScheduleVersionID, int projectID, GanttRequest ganttData)
        {
            switch (ganttData.Action)
            {
                case GanttAction.Inserted:
                    // set ProjectID and ProjectScheduleVersionID
                    ganttData.UpdatedTask.ProjectID = projectID;
                    ganttData.UpdatedTask.ProjectScheduleVersionID = projectScheduleVersionID;
                    // add new gantt task entity
                    db.GanttTasks.Add(ganttData.UpdatedTask);
                    break;
                case GanttAction.Deleted:
                    // remove gantt tasks
                    db.GanttTasks.Remove(db.GanttTasks.Find(ganttData.SourceId));
                    break;
                case GanttAction.Order:
                case GanttAction.Updated:
                    // set ProjectID and ProjectScheduleVersionID
                    ganttData.UpdatedTask.ProjectID = projectID;
                    ganttData.UpdatedTask.ProjectScheduleVersionID = projectScheduleVersionID;
                    // update gantt task
                    db.Entry(db.GanttTasks.Find(ganttData.UpdatedTask.GanttTaskId)).CurrentValues.SetValues(ganttData.UpdatedTask);
                    break;
                default:
                    ganttData.Action = GanttAction.Error;
                    break;
            }
        }

        /// <summary>
        /// Update gantt links
        /// </summary>
        /// <param name="ganttData">GanttData object</param>
        private void UpdateLinks(int projectScheduleVersionID, int projectID, GanttRequest ganttData)
        {
            switch (ganttData.Action)
            {
                case GanttAction.Inserted:
                    // set ProjectID and ProjectScheduleVersionID
                    ganttData.UpdatedLink.ProjectID = projectID;
                    ganttData.UpdatedLink.ProjectScheduleVersionID = projectScheduleVersionID;
                    // add new gantt link
                    db.GanttLinks.Add(ganttData.UpdatedLink);
                    break;
                case GanttAction.Deleted:
                    // remove gantt link
                    db.GanttLinks.Remove(db.GanttLinks.Find(ganttData.SourceId));
                    break;
                case GanttAction.Updated:
                    // set ProjectID and ProjectScheduleVersionID
                    ganttData.UpdatedLink.ProjectID = projectID;
                    ganttData.UpdatedLink.ProjectScheduleVersionID = projectScheduleVersionID;
                    // update gantt link
                    db.Entry(db.GanttLinks.Find(ganttData.UpdatedLink.GanttLinkId)).CurrentValues.SetValues(ganttData.UpdatedLink);
                    break;
                default:
                    ganttData.Action = GanttAction.Error;
                    break;
            }
        }

        /// <summary>
        /// Create XML response for gantt
        /// </summary>
        /// <param name="ganttData">Gantt data</param>
        /// <returns>XML response</returns>
        private ContentResult GanttRespose(List<GanttRequest> ganttDataCollection)
        {
            var actions = new List<XElement>();
            foreach (var ganttData in ganttDataCollection)
            {
                var action = new XElement("action");
                action.SetAttributeValue("type", ganttData.Action.ToString().ToLower());
                action.SetAttributeValue("sid", ganttData.SourceId);
                if (ganttData.Action != GanttAction.Deleted)
                {
                    action.SetAttributeValue("tid", (ganttData.Mode == GanttMode.Tasks) ? ganttData.UpdatedTask.GanttTaskId : ganttData.UpdatedLink.GanttLinkId);
                }
                action.SetAttributeValue("mode", (ganttData.Mode == GanttMode.Tasks) ? "tasks" : "links"); // set 'mode' flag for use by onAfterUpdate javascript event listener
                actions.Add(action);
            }

            var data = new XDocument(new XElement("data", actions));
            data.Declaration = new XDeclaration("1.0", "utf-8", "true");
            return Content(data.ToString(), "text/xml");
        }

    }
}