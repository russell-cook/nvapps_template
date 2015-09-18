using AdminApps.DAL;
using AdminApps.DAL.ProjectManagement;
using AdminApps.Models;
using AdminApps.ViewModels;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AdminApps.Controllers
{
    [Authorize(Roles = "UserProjects")]
    public class UserProjectsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        const string unauthorizedAccessMessage = "Unauthorized Access: A User Project can only be accessed by the user who created it.";

        // GET: UserProjects/Help
        public ActionResult Help()
        {
            return View();
        }

        // GET: UserProjects
        public async Task<ActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page, int pageSize = 10)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.BillSortParm = String.IsNullOrEmpty(sortOrder) ? "num_desc" : "";
            ViewBag.SummarySortParm = sortOrder == "Summary" ? "summary_desc" : "Summary";
            ViewBag.ReviewsSortParm = sortOrder == "Reviews" ? "reviews_desc" : "Reviews";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;


            ApplicationUser user = await ReturnCurrentUserAsync();
            UserProjectsRepository repo = new UserProjectsRepository();
            var userProjects = await repo.GetUserProjectList(user.Id);
            return View(userProjects);
        }

        // GET: UserProjects/Details/5
        public async Task<ActionResult> Details(int? id, int scheduleVersion = 1)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // load project from db, confirm that it's not null
            UserProjectsRepository repo = new UserProjectsRepository();
            UserProjectViewModel viewModel = await repo.GetUserProjectWithSchedule(id, scheduleVersion);

            if (viewModel == null)
            {
                return HttpNotFound();
            }

            // authorize user
            ApplicationUser user = await ReturnCurrentUserAsync();
            if (viewModel.ApplicationUserID == user.Id)
            {
                return View(viewModel);
            }

            Danger(unauthorizedAccessMessage, true);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> GetScheduleHistory(int id)
        {
            var archivedScheduleVersions = await db.ProjectScheduleVersions
                                                    .Where(v => v.ProjectID == id && v.VersionNum > 1)
                                                    .OrderBy(v => v.VersionNum)
                                                    .ToListAsync();

            if (archivedScheduleVersions == null)
            {
                return new HttpNotFoundResult();
            }

            return PartialView("_ArchivedScheduleVersions", archivedScheduleVersions);
        }

        public async Task<ActionResult> Create()
        {
            ApplicationUser user = await ReturnCurrentUserAsync();
            CreateUserProjectViewModel viewModel = new CreateUserProjectViewModel();
            viewModel.ApplicationUserID = user.Id;
            ViewBag.ProjectStatusID = new SelectList(await db.ProjectStatuses.ToListAsync(), "ID", "Description");
            return View(viewModel);
        }

        // POST: UserProjects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,ApplicationUserID,Name,Description,Comments,ProjectStartDate,ProjectStatusID")] CreateUserProjectViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                UserProjectsRepository repo = new UserProjectsRepository();
                int newProjectID = await repo.CreateNewProject(viewModel);

                Success("New Project Created Successfully", true);
                return RedirectToAction("Details", new { id = newProjectID });
            }

            return View(viewModel);
        }

        // GET: UserProjects/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // load project from db, confirm that it's not null
            UserProject userProject = await db.UserProjects.Where(p => p.ID == id).FirstOrDefaultAsync();
            if (userProject == null)
            {
                return HttpNotFound();
            }

            // authorize user
            ApplicationUser user = await ReturnCurrentUserAsync();
            if (userProject.ApplicationUserID == user.Id)
            {
                UserProjectViewModel viewModel = new UserProjectViewModel();
                viewModel.InjectFrom(userProject);
                ViewBag.ProjectStatusID = new SelectList(await db.ProjectStatuses.ToListAsync(), "ID", "Description", viewModel.ProjectStatusID);
                return View(viewModel);
            }
            else
            {
                Danger(unauthorizedAccessMessage, true);
                return RedirectToAction("Index");
            }
        }

        // POST: UserProjects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,ApplicationUserID,Name,Description,Comments,CreatedAt,ModifiedAt,ProjectStatusID")] UserProjectViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var userProject = db.UserProjects.Find(viewModel.ID);
                // authorize user
                ApplicationUser user = await ReturnCurrentUserAsync();
                if (userProject.ApplicationUserID == user.Id)
                {
                    userProject.InjectFrom(viewModel);
                    userProject.ModifiedAt = DateTime.Now;
                    db.Entry(userProject).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = viewModel.ID });
                }
                else
                {
                    Danger(unauthorizedAccessMessage, true);
                    return RedirectToAction("Index");
                }
            }
            ViewBag.ProjectStatusID = new SelectList(await db.ProjectStatuses.ToListAsync(), "ID", "Description", viewModel.ProjectStatusID);
            return View(viewModel);
        }

        // GET: UserProjects/EditScheduleFS/5
        public async Task<ActionResult> EditScheduleFS(int? id, bool deleteAbandonedSchedule = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // load project from db, confirm that it's not null
            UserProjectsRepository repo = new UserProjectsRepository();
            UserProjectViewModel viewModel = await repo.GetUserProject(id);
            if (viewModel == null)
            {
                return HttpNotFound();
            }

            // authorize user
            ApplicationUser user = await ReturnCurrentUserAsync();
            if (viewModel.ApplicationUserID == user.Id)
            {
                ProjectScheduleVersionsRepository scheduleRepo = new ProjectScheduleVersionsRepository();
                Tuple<ProjectScheduleVersion, bool> newSchedule = await scheduleRepo.CloneProjectScheduleVersionForEdit(viewModel.ID);
                if (deleteAbandonedSchedule)
                {
                    return RedirectToAction("RemoveProjectScheduleVersion", new { id = newSchedule.Item1.ID });
                }
                viewModel.ProjectScheduleVersion = newSchedule.Item1;
                viewModel.FullScreen = true;
                viewModel.ReadOnly = false;
                viewModel.AbandonedSchedule = newSchedule.Item2;
                return View(viewModel);
            }
            else
            {
                Danger(unauthorizedAccessMessage, true);
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> RemoveProjectScheduleVersion(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // load schedule from db, confirm that it's not null
            ProjectScheduleVersionsRepository schedulesRepo = new ProjectScheduleVersionsRepository();
            var schedule = await schedulesRepo.FindProjectScheduleVersion(id.Value);
            if (schedule == null)
            {
                return HttpNotFound();
            }

            // load project from db, confirm that it's not null
            UserProjectsRepository projectsRepo = new UserProjectsRepository();
            UserProjectViewModel project = await projectsRepo.GetUserProject(schedule.ProjectID);
            if (project == null)
            {
                return HttpNotFound();
            }

            // authorize user
            ApplicationUser user = await ReturnCurrentUserAsync();
            if (project.ApplicationUserID == user.Id)
            {
                if (await schedulesRepo.RemoveProjectScheduleVersion(id.Value))
                {
                    return RedirectToAction("EditScheduleFS", new { id = project.ID });
                }
            }
            Danger("Error: Unable to abandon unsaved changes.");
            return RedirectToAction("Details", new { id = project.ID });
        }

        // POST: UserProjects/EditScheduleFS/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditScheduleFS(UserProjectViewModel viewModel)
        {
            ProjectScheduleVersionsRepository repo = new ProjectScheduleVersionsRepository();
            if (await repo.SaveClonedProjectScheduleVersion(viewModel.ProjectScheduleVersion))
            {
                Success("Project schedule updated successfully.", true);
                return RedirectToAction("Details", new { id = viewModel.ID });
            }
            Warning("Update failed.", true);
            return RedirectToAction("Details", new { id = viewModel.ID });
        }

        // GET: UserProjects/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // load project from db, confirm that it's not null
            UserProject userProject = await db.UserProjects.Where(p => p.ID == id).FirstOrDefaultAsync();
            if (userProject == null)
            {
                return HttpNotFound();
            }

            // authorize user
            ApplicationUser user = await ReturnCurrentUserAsync();
            if (userProject.ApplicationUserID == user.Id)
            {
                UserProjectViewModel viewModel = new UserProjectViewModel();
                viewModel.InjectFrom(userProject);
                return View(viewModel);
            }
            Danger(unauthorizedAccessMessage, true);
            return RedirectToAction("Index");
        }

        // POST: UserProjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UserProject userProject = await db.UserProjects.FindAsync(id);

            // authorize user
            ApplicationUser user = await ReturnCurrentUserAsync();
            if (userProject.ApplicationUserID == user.Id)
            {
                db.Projects.Remove(userProject);
                await db.SaveChangesAsync();
                Success("Project deleted successfully.");
                return RedirectToAction("Index");
            }

            Danger(unauthorizedAccessMessage, true);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> RestoreProjectScheduleVersionToCurrent(int id)
        {
            // load ProjectScheduleVersion, confirm it's not null
            var targetScheduleVersion = await db.ProjectScheduleVersions.FindAsync(id);
            if (targetScheduleVersion == null)
            {
                return HttpNotFound();
            }

            // authorize user
            ApplicationUser user = await ReturnCurrentUserAsync();
            var parentUserProject = await db.UserProjects.FindAsync(targetScheduleVersion.ProjectID);
            if (parentUserProject.ApplicationUserID == user.Id)
            {
                ProjectScheduleVersionsRepository repo = new ProjectScheduleVersionsRepository();
                if (await repo.RestoreArchivedProjectScheduleVersionToCurrent(parentUserProject.ID, targetScheduleVersion.VersionNum))
                {
                    Success("Archived Project Schedule has been restored as Current Project Schedule.", true);
                    return RedirectToAction("Details", new { id = parentUserProject.ID });
                }
            }

            Warning("Update failed.", true);
            return RedirectToAction("Details", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
