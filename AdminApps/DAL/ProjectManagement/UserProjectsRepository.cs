using AdminApps.Models;
using AdminApps.ViewModels;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AdminApps.DAL.ProjectManagement
{
    public class UserProjectsRepository
    {
        internal async Task<List<UserProjectViewModel>> GetUserProjectList(string applicationUserID)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var userProjects = await db.UserProjects.Where(p => p.CreatedByUser.Id == applicationUserID).Include(p => p.ProjectStatus).ToListAsync();
                List<UserProjectViewModel> viewModel = new List<UserProjectViewModel>();
                foreach (UserProject p in userProjects)
                {
                    UserProjectViewModel v = new UserProjectViewModel();
                    v.InjectFrom(p);
                    viewModel.Add(v);
                }
                return viewModel;
            }
        }

        internal async Task<UserProjectViewModel> GetUserProject(int? id)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                // load user project and populate view model
                var userProject = await db.UserProjects.Where(p => p.ID == id).Include(p => p.ProjectStatus).FirstOrDefaultAsync();
                UserProjectViewModel viewModel = new UserProjectViewModel();
                viewModel.InjectFrom(userProject);

                return viewModel;
            }
        }

        internal async Task<UserProjectViewModel> GetUserProjectWithSchedule(int? id, int versionNum)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                // load user project and populate view model
                var userProject = await db.UserProjects.Where(p => p.ID == id).Include(p => p.ProjectStatus).FirstOrDefaultAsync();
                UserProjectViewModel viewModel = new UserProjectViewModel();
                viewModel.InjectFrom(userProject);

                // load project schedule and populate view model
                var projectSchedule = await db.ProjectScheduleVersions.Where(s => s.ProjectID == id && s.VersionNum == versionNum).FirstOrDefaultAsync();
                viewModel.ProjectScheduleVersion = projectSchedule;

                return viewModel;
            }
        }

        internal async Task<int> CreateNewProject(CreateUserProjectViewModel viewModel)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                // creat new UserProject record
                UserProject userProject = new UserProject();
                userProject.InjectFrom(viewModel);
                userProject.CreatedAt = DateTime.Now;
                userProject.ModifiedAt = DateTime.Now;
                db.UserProjects.Add(userProject);
                await db.SaveChangesAsync();

                // initialize blank ProjectScheduleVersion
                ProjectScheduleVersion schedule = new ProjectScheduleVersion();
                schedule.ProjectID = userProject.ID;
                schedule.VersionNum = 1;
                schedule.CreatedAt = userProject.CreatedAt;
                schedule.SavedAt = userProject.CreatedAt;
                db.ProjectScheduleVersions.Add(schedule);
                await db.SaveChangesAsync();

                // initialize 'project' GanttTask based on user-specified ProjectStartDate
                GanttTask initProject = new GanttTask()
                {
                    ProjectID = userProject.ID,
                    ProjectScheduleVersionID = schedule.ID,
                    Text = viewModel.Name,
                    StartDate = viewModel.ProjectStartDate,
                    Duration = 1,
                    Progress = 0m,
                    SortOrder = 0,
                    Type = "project",
                    Open = true
                };
                db.GanttTasks.Add(initProject);
                await db.SaveChangesAsync();

                // initialize 'task' GanttTask for proper rendering of start/end dates in grid
                GanttTask initTask = new GanttTask()
                {
                    ProjectID = userProject.ID,
                    ProjectScheduleVersionID = schedule.ID,
                    Text = "New Task",
                    StartDate = viewModel.ProjectStartDate,
                    Duration = 1,
                    Progress = 0m,
                    SortOrder = 1,
                    Type = "task",
                    ParentId = initProject.GanttTaskId
                };
                db.GanttTasks.Add(initTask);
                await db.SaveChangesAsync();

                return userProject.ID;
            }
        }
    }
}