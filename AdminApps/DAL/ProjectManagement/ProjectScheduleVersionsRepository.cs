using AdminApps.Models;
using AdminApps.ViewModels;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
//using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AdminApps.DAL.ProjectManagement
{
    public class ProjectScheduleVersionsRepository
    {
        const int ProjectScheduleVersionBackupCount = 10;

        internal async Task<ProjectScheduleVersion> FindProjectScheduleVersion(int id)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var projectScheduleVersion = await db.ProjectScheduleVersions.FindAsync(id);
                return projectScheduleVersion;
            }
        }

        internal async Task<int> GetProjectScheduleVersionId(int projectId, int versionNum)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var schedule = await db.ProjectScheduleVersions.Where(s => s.ProjectID == projectId && s.VersionNum == versionNum).FirstOrDefaultAsync();
                return schedule.ID;
            }
        }

        internal async Task<Tuple<ProjectScheduleVersion, bool>> CloneProjectScheduleVersionForEdit(int projectId)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                // check db for existing 'cloned' records that have been abandoned
                var abandonedSchedule = await db.ProjectScheduleVersions.Where(s => s.ProjectID == projectId && s.VersionNum == 0).FirstOrDefaultAsync();
                if (abandonedSchedule != null)
                {
                    return Tuple.Create(abandonedSchedule, true); // set AbandonedSchedule flag to true
                }

                // create new ProjectScheduleVersion for use during editing
                // user will have the option to discard edits, which will delete the new records
                ProjectScheduleVersion newSchedule = new ProjectScheduleVersion()
                    {
                        ProjectID = projectId,
                        VersionNum = 0,  // VersionNum = 0 denotes temporary 'clone' for editing
                        CreatedAt = DateTime.Now,
                        SavedAt = DateTime.Now
                    };
                db.ProjectScheduleVersions.Add(newSchedule);
                await db.SaveChangesAsync();

                // initialize dictionary, used to crosswalk ParentId, SourceGanttTask, and TargetGanttTask values after cloning
                Dictionary<int, int> taskIdDictionary = new Dictionary<int, int>();

                // identify current ProjectScheduleVersion (VersionNum = 1), then clone GanttLinks and GanttTasks
                int currentScheduleId = await GetProjectScheduleVersionId(projectId, 1);
                var existingGanttTasks = await db.GanttTasks.Where(t => t.ProjectScheduleVersionID == currentScheduleId).ToListAsync();
                foreach (GanttTask t in existingGanttTasks)
                {
                    int oldId = t.GanttTaskId;
                    GanttTask newTask = new GanttTask();
                    newTask.InjectFrom(t);
                    db.GanttTasks.Add(newTask);                    
                    newTask.ProjectScheduleVersionID = newSchedule.ID;
                    await db.SaveChangesAsync(); // must save record to generate new ID
                    taskIdDictionary.Add(oldId, newTask.GanttTaskId); // log ID's in dictionary for crosswalk
                }
                var ganttLinks = await db.GanttLinks.Where(l => l.ProjectScheduleVersionID == currentScheduleId).ToListAsync();
                foreach (GanttLink l in ganttLinks)
                {
                    GanttLink newLink = new GanttLink();
                    newLink.InjectFrom(l);
                    db.GanttLinks.Add(newLink);
                    newLink.ProjectScheduleVersionID = newSchedule.ID;
                }
                await db.SaveChangesAsync();

                // crosswalk ParentID values for GanttTasks
                var newGanttTasks = await db.GanttTasks.Where(t => t.ProjectScheduleVersionID == newSchedule.ID).ToListAsync();
                foreach (GanttTask t in newGanttTasks)
                {
                    if (t.ParentId != null)
                    {
                        int newId = taskIdDictionary[t.ParentId.Value];
                        t.ParentId = newId;
                        db.Entry(t).State = EntityState.Modified;
                    }
                }

                // crosswalk SourceTaskId and TargetTaskId values for GanttLinks
                var newGanttLinks = await db.GanttLinks.Where(l => l.ProjectScheduleVersionID == newSchedule.ID).ToListAsync();
                foreach (GanttLink l in newGanttLinks)
                {
                    int newSourceId = taskIdDictionary[l.SourceTaskId];
                    l.SourceTaskId = newSourceId;
                    int newTargetId = taskIdDictionary[l.TargetTaskId];
                    l.TargetTaskId = newTargetId;
                    db.Entry(l).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();

                return Tuple.Create(newSchedule, false); // set AbandonedSchedule flag to false
            }
        }

        internal async Task<bool> RemoveProjectScheduleVersion(int id)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                // confirm ProjectScheduleVersion's existence before removal
                var abandonedSchedule = await db.ProjectScheduleVersions.FindAsync(id);
                if (abandonedSchedule != null)
                {
                    db.ProjectScheduleVersions.Remove(abandonedSchedule);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
        }

        internal async Task<bool> SaveClonedProjectScheduleVersion(ProjectScheduleVersion model)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                // remove oldest backup ProjectScheduleVersion
                // foreach loop is used to allow for changes to ProjectScheduleVersionBackupCount, i.e. multiple records might be returned from query
                var backupVersionsToDelete = await db.ProjectScheduleVersions.Where(v => v.ProjectID == model.ProjectID && v.VersionNum >= ProjectScheduleVersionBackupCount).ToListAsync();
                foreach (ProjectScheduleVersion v in backupVersionsToDelete)
                {
                    db.ProjectScheduleVersions.Remove(v);
                }
                await db.SaveChangesAsync();

                // increment VersionNum of exsting ProjectScheduleVersions
                var existingVersionsToIncrement = await db.ProjectScheduleVersions.Where(v => v.ProjectID == model.ProjectID).OrderBy(v => v.VersionNum).ToListAsync();
                existingVersionsToIncrement.First().SavedAt = DateTime.Now;
                existingVersionsToIncrement.First().Comments = model.Comments;
                foreach (ProjectScheduleVersion v in existingVersionsToIncrement)
                {
                    v.VersionNum++;
                    db.Entry(v).State = EntityState.Modified;
                }
                await db.SaveChangesAsync();

                return true;
            }
        }

        internal async Task<bool> RestoreArchivedProjectScheduleVersionToCurrent(int projectId, int scheduleVersion)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                //load ProjectScheduleVersions whose VersionNum is lower than the selected version
                var existingVersionsToIncrement = await db.ProjectScheduleVersions.Where(v => v.ProjectID == projectId && v.VersionNum <= scheduleVersion).OrderBy(v => v.VersionNum).ToListAsync();
                
                // increment VersionNum
                foreach (ProjectScheduleVersion v in existingVersionsToIncrement)
                {
                    v.VersionNum++;
                    db.Entry(v).State = EntityState.Modified;
                }
                existingVersionsToIncrement.Last().VersionNum = 1;
                await db.SaveChangesAsync();

                return true;
            }
        }
    }
}