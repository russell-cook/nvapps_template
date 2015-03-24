using AdminApps.DAL;
using AdminApps.Models;
using AdminApps.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AdminApps.Controllers
{
    [Authorize(Roles = "GlobalAdmin")]
    public class NebsConcurrencyController : Controller
    {

        // GET: NebsConcurrency
        public ActionResult Index()
        {
            var viewModel = new NebsConcurrencyIndexViewModel();

            using (var nebsdb = new NebsContext())
            {
                // all NEBS BudgetPeriod/Dept/Div queries specify a BudgetPeriod greater than 5 because there were invalid Dept/Div records
                // attached to some of the older BudgetPeriods in the NEBS database
                viewModel.NebsBudgetPeriods = nebsdb.NebsBudgetPeriods.Where(b => b.ID > 5).ToList();
                viewModel.NebsDepts = nebsdb.NebsDepts.Where(b => b.NebsBudgetPeriodID > 5).ToList();
                viewModel.NebsDivs = nebsdb.NebsDivs.Where(b => b.NebsBudgetPeriodID > 5).ToList();
            }

            using (var localdb = new ApplicationDbContext())
            {
                viewModel.LocalBudgetPeriods = localdb.BudgetPeriods.ToList();
                viewModel.LocalDepts = localdb.Depts.ToList();
                viewModel.LocalDivs = localdb.Divs.ToList();
            }

            return View(viewModel);
        }

        public ActionResult SyncBudgetPeriods()
        {
            var nebsdb = new NebsContext();
            var nebsBudgetPeriods = nebsdb.NebsBudgetPeriods.Where(b => b.ID > 5).ToList();
            nebsdb.Dispose();

            var localdb = new ApplicationDbContext();
            var localBudgetPeriods = localdb.BudgetPeriods.ToList();
            localdb.Dispose();

            var localBudgetPeriodsHS = new HashSet<decimal>(localBudgetPeriods.Select(d => d.ID));
            using (var context = new ApplicationDbContext())
            {
                foreach (var budgetPeriod in nebsBudgetPeriods)
                {
                    if (!localBudgetPeriodsHS.Contains(budgetPeriod.ID))
                    {
                        context.BudgetPeriods.Add(
                            new BudgetPeriod
                            {
                                ID = budgetPeriod.ID,
                                Description = budgetPeriod.Description,
                                ActualYear = budgetPeriod.ActualYear,
                                WorkProgramYear = budgetPeriod.WorkProgramYear,
                                BeginYear = budgetPeriod.BeginYear,
                                EndYear = budgetPeriod.EndYear
                            }
                        );
                        context.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult SyncDepts()
        {
            //var viewModel = new NebsConcurrencyIndexViewModel();
            var nebsdb = new NebsContext();
            var nebsDepts = nebsdb.NebsDepts.Where(b => b.NebsBudgetPeriodID > 5).ToList();
            nebsdb.Dispose();

            var localdb = new ApplicationDbContext();
            var localDepts = localdb.Depts.ToList();
            localdb.Dispose();

            var localDeptsHS = new HashSet<decimal>(localDepts.Select(d => d.ID));
            using (var context = new ApplicationDbContext())
            {
                foreach (var dept in nebsDepts)
                {
                    if (!localDeptsHS.Contains(dept.ID))
                    {
                        context.Depts.Add(
                            new Dept
                            {
                                ID = dept.ID,
                                BudgetPeriodID = dept.NebsBudgetPeriodID,
                                Code = dept.Code,
                                Description = dept.Description
                            }
                        );
                        context.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult SyncDivs()
        {
            //var viewModel = new NebsConcurrencyIndexViewModel();
            var nebsdb = new NebsContext();
            var nebsDivs = nebsdb.NebsDivs.Where(b => b.NebsBudgetPeriodID > 5).ToList();
            nebsdb.Dispose();

            var localdb = new ApplicationDbContext();
            var localDivs = localdb.Divs.ToList();
            localdb.Dispose();

            var localDeptsHS = new HashSet<decimal>(localDivs.Select(d => d.ID));
            using (var context = new ApplicationDbContext())
            {
                foreach (var div in nebsDivs)
                {
                    if (!localDeptsHS.Contains(div.ID))
                    {
                        context.Divs.Add(
                            new Div
                            {
                                ID = div.ID,
                                BudgetPeriodID = div.NebsBudgetPeriodID,
                                DeptID = div.NebsDeptID,
                                Code = div.Code,
                                Description = div.Description,
                                SortOrder = div.SortOrder
                            }
                        );
                        context.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Index");
        }
    }
}