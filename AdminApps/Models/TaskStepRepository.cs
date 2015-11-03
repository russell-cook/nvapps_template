using AdminApps.DAL;
using System;
//using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace AdminApps.Models
{ 
    //public class TaskStepRepository : ITaskStepRepository
    //{
    //    ApplicationDbContext context = new ApplicationDbContext();

    //    public IQueryable<TaskStep> All
    //    {
    //        get { return context.TaskSteps; }
    //    }

    //    public IQueryable<TaskStep> AllIncluding(params Expression<Func<TaskStep, object>>[] includeProperties)
    //    {
    //        IQueryable<TaskStep> query = context.TaskSteps;
    //        foreach (var includeProperty in includeProperties) {
    //            query = query.Include(includeProperty);
    //        }
    //        return query;
    //    }

    //    public TaskStep Find(int id)
    //    {
    //        return context.TaskSteps.Find(id);
    //    }

    //    public void InsertOrUpdate(TaskStep taskstep)
    //    {
    //        if (taskstep.ID == default(int)) {
    //            // New entity
    //            context.TaskSteps.Add(taskstep);
    //        } else {
    //            // Existing entity
    //            context.Entry(taskstep).State = EntityState.Modified;
    //        }
    //    }

    //    public void Delete(int id)
    //    {
    //        var taskstep = context.TaskSteps.Find(id);
    //        context.TaskSteps.Remove(taskstep);
    //    }

    //    public void Save()
    //    {
    //        context.SaveChanges();
    //    }

    //    public void Dispose() 
    //    {
    //        context.Dispose();
    //    }
    //}

    public interface ITaskStepRepository : IDisposable
    {
        IQueryable<TaskStep> All { get; }
        IQueryable<TaskStep> AllIncluding(params Expression<Func<TaskStep, object>>[] includeProperties);
        TaskStep Find(int id);
        void InsertOrUpdate(TaskStep taskstep);
        void Delete(int id);
        void Save();
    }
}