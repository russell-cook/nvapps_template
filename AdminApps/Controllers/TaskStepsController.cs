using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdminApps.Models;

namespace AdminApps.Controllers
{   
    //public class TaskStepsController : Controller
    //{
    //    private readonly IDeskManualTaskRepository deskmanualtaskRepository;
    //    private readonly ITaskStepRepository taskstepRepository;

    //    // If you are using Dependency Injection, you can delete the following constructor
    //    public TaskStepsController() : this(new DeskManualTaskRepository(), new TaskStepRepository())
    //    {
    //    }

    //    public TaskStepsController(IDeskManualTaskRepository deskmanualtaskRepository, ITaskStepRepository taskstepRepository)
    //    {
    //        this.deskmanualtaskRepository = deskmanualtaskRepository;
    //        this.taskstepRepository = taskstepRepository;
    //    }

    //    //
    //    // GET: /TaskSteps/

    //    public ViewResult Index()
    //    {
    //        return View(taskstepRepository.AllIncluding(taskstep => taskstep.DeskManualTask));
    //    }

    //    //
    //    // GET: /TaskSteps/Details/5

    //    public ViewResult Details(int id)
    //    {
    //        return View(taskstepRepository.Find(id));
    //    }

    //    //
    //    // GET: /TaskSteps/Create

    //    public ActionResult Create()
    //    {
    //        ViewBag.PossibleDeskManualTasks = deskmanualtaskRepository.All;
    //        return View();
    //    } 

    //    //
    //    // POST: /TaskSteps/Create

    //    [HttpPost]
    //    public ActionResult Create(TaskStep taskstep)
    //    {
    //        if (ModelState.IsValid) {
    //            taskstepRepository.InsertOrUpdate(taskstep);
    //            taskstepRepository.Save();
    //            return RedirectToAction("Index");
    //        } else {
    //            ViewBag.PossibleDeskManualTasks = deskmanualtaskRepository.All;
    //            return View();
    //        }
    //    }
        
    //    //
    //    // GET: /TaskSteps/Edit/5
 
    //    public ActionResult Edit(int id)
    //    {
    //        ViewBag.PossibleDeskManualTasks = deskmanualtaskRepository.All;
    //         return View(taskstepRepository.Find(id));
    //    }

    //    //
    //    // POST: /TaskSteps/Edit/5

    //    [HttpPost]
    //    public ActionResult Edit(TaskStep taskstep)
    //    {
    //        if (ModelState.IsValid) {
    //            taskstepRepository.InsertOrUpdate(taskstep);
    //            taskstepRepository.Save();
    //            return RedirectToAction("Index");
    //        } else {
    //            ViewBag.PossibleDeskManualTasks = deskmanualtaskRepository.All;
    //            return View();
    //        }
    //    }

    //    //
    //    // GET: /TaskSteps/Delete/5
 
    //    public ActionResult Delete(int id)
    //    {
    //        return View(taskstepRepository.Find(id));
    //    }

    //    //
    //    // POST: /TaskSteps/Delete/5

    //    [HttpPost, ActionName("Delete")]
    //    public ActionResult DeleteConfirmed(int id)
    //    {
    //        taskstepRepository.Delete(id);
    //        taskstepRepository.Save();

    //        return RedirectToAction("Index");
    //    }

    //    protected override void Dispose(bool disposing)
    //    {
    //        if (disposing) {
    //            deskmanualtaskRepository.Dispose();
    //            taskstepRepository.Dispose();
    //        }
    //        base.Dispose(disposing);
    //    }
    //}
}

