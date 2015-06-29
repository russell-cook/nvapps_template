using AdminApps.DAL;
using AdminApps.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using Omu.ValueInjecter;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace AdminApps.Controllers
{
    public class ProjectsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Projects
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult IT()
        {
            var itProjects = db.ITProjects
                .Include(p => p.ProjectStatus)
                .OrderBy(p => p.CreatedAt)
                .ToList();
            return View(itProjects);
        }

        public ActionResult IT_Details(int id)
        {
            var project = db.ITProjects.Find(id);
            ITProjectVM viewModel = new ITProjectVM();
            viewModel.InjectFrom(project);
            viewModel.Status = project.ProjectStatus.AsText();
            return View(viewModel);
        }



        [HttpGet]
        public JsonResult GetGanttData(int id)
        {
            var jsonData = new
            {
                // create tasks array
                data = (
                    from t in db.GanttTasks.AsEnumerable()
                    where t.ProjectID == id
                    select new
                    {
                        id = t.GanttTaskId,
                        //proj_id = t.ProjectID,
                        text = t.Text,
                        start_date = t.StartDate.ToString("u"),
                        duration = t.Duration,
                        order = t.SortOrder,
                        progress = t.Progress,
                        open = true,
                        parent = t.ParentId,
                        type = (t.Type != null) ? t.Type : String.Empty
                    }
                ).ToArray(),
                // create links array
                links = (
                    from l in db.GanttLinks.AsEnumerable()
                    where l.ProjectID == id
                    select new
                    {
                        id = l.GanttLinkId,
                        //proj_id = l.ProjectID,
                        source = l.SourceTaskId,
                        target = l.TargetTaskId,
                        type = l.Type
                    }
                ).ToArray()
            };
            return new JsonResult { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ContentResult Save(int id, FormCollection form)
        {
            var dataActions = GanttRequest.Parse(form, Request.QueryString["gantt_mode"]);
            try
            {
                foreach (var ganttData in dataActions)
                {
                    switch (ganttData.Mode)
                    {
                        case GanttMode.Tasks:
                            UpdateTasks(id, ganttData);
                            break;
                        case GanttMode.Links:
                            UpdateLinks(id, ganttData);
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
        private void UpdateTasks(int projectID, GanttRequest ganttData)
        {
            switch (ganttData.Action)
            {
                case GanttAction.Inserted:
                    // set ProjectID
                    ganttData.UpdatedTask.ProjectID = projectID;
                    // add new gantt task entity
                    db.GanttTasks.Add(ganttData.UpdatedTask);
                    break;
                case GanttAction.Deleted:
                    // remove gantt tasks
                    db.GanttTasks.Remove(db.GanttTasks.Find(ganttData.SourceId));
                    break;
                case GanttAction.Updated:
                    // set ProjectID
                    ganttData.UpdatedTask.ProjectID = projectID;
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
        private void UpdateLinks(int projectID, GanttRequest ganttData)
        {
            switch (ganttData.Action)
            {
                case GanttAction.Inserted:
                    // set ProjectID
                    ganttData.UpdatedLink.ProjectID = projectID;
                    // add new gantt link
                    db.GanttLinks.Add(ganttData.UpdatedLink);
                    break;
                case GanttAction.Deleted:
                    // remove gantt link
                    db.GanttLinks.Remove(db.GanttLinks.Find(ganttData.SourceId));
                    break;
                case GanttAction.Updated:
                    // set ProjectID
                    ganttData.UpdatedLink.ProjectID = projectID;
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
                actions.Add(action);
            }

            var data = new XDocument(new XElement("data", actions));
            data.Declaration = new XDeclaration("1.0", "utf-8", "true");
            return Content(data.ToString(), "text/xml");
        }

    }
}