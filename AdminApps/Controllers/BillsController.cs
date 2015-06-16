using AdminApps.DAL;
using AdminApps.Models;
using AdminApps.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
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
using System.IO;
using PagedList;

namespace AdminApps.Controllers
{
    [Authorize(Roles = "BillsClerc, BillsDept, BillsDiv, BillsAgency")]
    public class BillsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private NebsContext nebsDb = new NebsContext();

        public async Task GetUserNotifications(ApplicationUser user, int userModuleApprovalLevel, decimal appGlobalBudgetPeriod)
        {
            var recordRequests = new List<BillRecordRequest>();
            switch (userModuleApprovalLevel)
            {
                case 0:
                    recordRequests = await db.BillRecordRequests
                        .Where(r => 
                            r.BudgetPeriodID == appGlobalBudgetPeriod && 
                            !r.Fulfilled)
                        .ToListAsync();
                    if (recordRequests.Any())
                    {
                        Danger(String.Format(recordRequests.Count() + " outstanding <a href='BillRecordRequests' class='panel-title'>Bill Record Requests</a>."));
                    }
                    break;
                case 2:
                case 3:
                    recordRequests = await db.BillRecordRequests
                        .Where(r => 
                            r.BudgetPeriodID == appGlobalBudgetPeriod && 
                            r.RequestedByUserID == user.Id && 
                            r.Fulfilled && 
                            !r.Bill.Reviews.Where(x => x.CreatedByUser.Id == user.Id).Any())
                        .ToListAsync();
                    if (recordRequests.Any())
                    {
                        Success(String.Format(recordRequests.Count() + " <a href='BillRecordRequests' class='panel-title'>Bill Record Requests</a> have been fulfilled and are ready for review."));
                    }
                    break;
                default:
                    break;
            }

            var reviewRequests = await db.BillReviewRequests.Where(r => r.RequestedToUserID == user.Id && !r.Fulfilled).ToListAsync();
            if (reviewRequests.Any())
            {
                Danger("You have " + reviewRequests.Count() + " outstanding <a href='BillReviewRequests' class='panel-title'>Bill Review Requests</a>.");
            }

            // count Unread BillReviews
            BillReviewsRepository billReviews = new BillReviewsRepository();
            int unreadReviews = await billReviews.CountUnreadAsync(user, userModuleApprovalLevel);
            if (unreadReviews > 0)
            {
                Warning("You have " + unreadReviews + " <a href='../BillReviews/Unread' class='panel-title'>unread Bill Reviews</a>.");
            }

        }

        // GET: Bills/Home
        public async Task<ActionResult> Home()
        {
            var viewModel = new BillsHomeViewModel();
            var appGlobalBudgetPeriod = (await db.AppGlobalSettings.FirstOrDefaultAsync()).BudgetPeriodID;

            var user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            // get user notifications for display at top of Home page
            await GetUserNotifications(user, userModuleApprovalLevel, appGlobalBudgetPeriod);


            // load top 5 recent Bill Reviews
            //viewModel.RecentBillReviews = await db.BillReviews
            //                                        .Where(r => r.CreatedByUser.Id == user.Id)
            //                                        .OrderByDescending(r => r.CreatedAt)
            //                                        .Take(5)
            //                                        .ToListAsync();
            var userBillReviews = await db.BillReviews
                                    .Where(r => r.CreatedByUser.Id == user.Id
                                            && r.Bill.BudgetPeriodID == appGlobalBudgetPeriod)
                                    .Include(r => r.BillVersion)
                                    .GroupBy(r => r.BillID)
                                    .ToListAsync();
            foreach (IGrouping<int, BillReview> group in userBillReviews)
            {
                var mostRecentReview = group.OrderByDescending(g => g.BillVersion.VersionNum).FirstOrDefault();
                viewModel.RecentBillReviews.Add(mostRecentReview);
            }
            viewModel.RecentBillReviews = viewModel.RecentBillReviews.OrderByDescending(r => r.CreatedAt).Take(5).ToList();

            return View(viewModel);
        }

        // GET: Bills/Help
        public ActionResult Help()
        {
            return View();
        }

        // GET: Bills
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


            var billsList = new List<BillsIndexViewModel>();
            var appGlobalBudgetPeriod = (await db.AppGlobalSettings.FirstOrDefaultAsync()).BudgetPeriodID;

            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            var billsQry = db.Bills.Where(b => b.BudgetPeriodID == appGlobalBudgetPeriod)
                .Include(b => b.Reviews)
                .Include(b => b.BillVersions);

            switch (sortOrder)
            {
                case "num_desc":
                    billsQry = billsQry.OrderByDescending(b => b.BillPrefixID).ThenByDescending(b => b.Suffix);
                    break;
                case "Summary":
                    billsQry = billsQry.OrderBy(s => s.Summary);
                    break;
                case "summary_desc":
                    billsQry = billsQry.OrderByDescending(s => s.Summary);
                    break;
                default:
                    billsQry = billsQry.OrderBy(b => b.BillPrefixID).ThenBy(b => b.Suffix);
                    break;
            }

            var bills = await billsQry.ToListAsync();

            // for each Bill, count number of reviews for user's Dept/Div (depending on user's approval level), 
            // and calculate whether or not the Bill has an approved Review at the user's approval level
            foreach (Bill bill in bills)
            {
                var numReviews = new int();
                //var currentVersion = bill.BillVersions.OrderByDescending(v => v.VersionNum).FirstOrDefault();
                BillReview approvedReview = new BillReview();
                var hasApprovedReview = false;
                switch (userModuleApprovalLevel)
                {
                    case 0:
                    case 1:
                        // for Dept users, count all approved Reviews from the Div level, plus all reviews created at the Dept level
                        numReviews = bill.Reviews
                            .Where(r =>
                                !(r is AlsrBillReviewSnapshot) &&
                                r.CreatedByUserInDept.ID == user.DeptID &&
                                (r.Approvals.Where(a => a.ApprovalLevel == 2).Any() || r.CreatedAtApprovalLevel == 1))
                            .GroupBy(r => r.CreatedByUser.Id)
                            .Count();
                        //int counter = 0;
                        //foreach (IGrouping<string, BillReview> group in reviewsList)
                        //{
                        //    counter++;
                        //}
                        //numReviews = counter;
                        approvedReview = bill.Reviews
                            .Where(r =>
                                r.Approvals.Where(a => a.ApprovalLevel == 1).Any() && 
                                r.CreatedByUserInDept.ID == user.DeptID)
                            .FirstOrDefault();
                        if (approvedReview != null)
                        {
                            hasApprovedReview = true;
                        }
                        break;
                    case 2:
                        // for Div or Agency users, count all Reviews created within that Div 
                        numReviews = bill.Reviews
                            .Where(r =>
                                !(r is AlsrBillReviewSnapshot) &&
                                r.CreatedAtApprovalLevel > 1 &&
                                r.CreatedByUserInDiv.ID == user.DivID)
                            .GroupBy(r => r.CreatedByUser.Id)
                            .Count();
                        approvedReview = bill.Reviews
                            .Where(r =>
                                r.Approvals.Where(a => a.ApprovalLevel == 2).Any() && 
                                r.CreatedByUserInDiv.ID == user.DivID)
                            .FirstOrDefault();
                        if (approvedReview != null)
                        {
                            hasApprovedReview = true;
                        }
                        break;
                    case 3:
                        // for Agency users, display whether or not they have reviewed the bill
                        approvedReview = bill.Reviews
                            .Where(r => !(r is AlsrBillReviewSnapshot) &&  r.ApplicationUserID == user.Id)
                            .FirstOrDefault();
                        if (approvedReview != null)
                        {
                            hasApprovedReview = true;
                        }
                        break;
                    default:
                        break;
                }

                // add bill to ViewModel, including number of reviews
                billsList.Add(
                    new BillsIndexViewModel
                    {
                        AdminAppsID = bill.ID,
                        NebsBdrID = bill.NebsBdrID,
                        BillPrefixID = bill.BillPrefixID,
                        Suffix = bill.Suffix,
                        Location = "AdminApps",
                        CompositeBillNumber = bill.CompositeBillNumber,
                        NebsBdrNumber = bill.NebsBdrNumber,
                        Summary = bill.Summary,
                        NumReviews = numReviews,
                        HasApprovedReview = hasApprovedReview
                    }
                );
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                string capSearchString = searchString.ToUpper();
                foreach (BillsIndexViewModel b in billsList.ToList())
                {
                    if (!b.CompositeBillNumber.Contains(capSearchString))
                    {
                        billsList.Remove(b);
                    }
                }
            }
            
            switch (sortOrder)
            {
                case "Reviews":
                    billsList = billsList.OrderByDescending(b => b.NumReviews).ThenBy(b => b.BillPrefixID).ThenBy(b => b.Suffix).ToList();
                    break;
                case "reviews_desc":
                    billsList = billsList.OrderBy(b => b.NumReviews).ThenBy(b => b.BillPrefixID).ThenBy(b => b.Suffix).ToList();
                    break;
                default:
                    break;
            }

            int pageNumber = (page ?? 1);
            PagedList<BillsIndexViewModel> viewModel = new PagedList<BillsIndexViewModel>(billsList, pageNumber, pageSize);

            return View(viewModel);
        }

        // GET: Bills/NEBS_BDRs
        [Authorize(Roles = "GlobalAdmin, BillsClerc, BillsDept")]
        public async Task<ActionResult> NEBS_BDRs()
        {
            var viewModel = new List<BillsIndexViewModel>();
            var appGlobalBudgetPeriod = (await db.AppGlobalSettings.FirstOrDefaultAsync()).BudgetPeriodID;

            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            var bills = await db.Bills.Where(b => b.BudgetPeriodID == appGlobalBudgetPeriod)
                .Include(b => b.Reviews)
                .ToListAsync();

            // initialize HashSet that will be used to compare remote DB records (i.e. NEBS) with local DB records (AdminApps)
            var localNebsBdrIdsHS = new HashSet<decimal>();
            foreach (Bill bill in bills)
            {
                // if the record contains a NebsBdrID then it originated in NEBS, and the corresponding NEBS record (which is duplicative) 
                // should not be included in the view model. This routine adds any non-null NebsBdrID's to the HashSet
                if (bill.NebsBdrID != null)
                {
                    localNebsBdrIdsHS.Add(bill.NebsBdrID.Value);
                }
            }

            // query all BDRs from NEBS BDR module
            // 2014-12-03: Currently this query targets a View in the NEBS database (NEBS.TRANSMITTED_BDRS),
            // but direct access to the BDR_SESSION and BDR_HEADER tables might be preferable for long-term maintainability,
            // and also for allowing us to specify the inclusion criteria (i.e. matching BUDGET_PERIOD, non-null TRANSMITTAL_DATE, etc.) which is currently hard-coded in to the view

            // 2014-12-04: added Where clause for AgendaName contains("Leg") to filter duplicate BDR records from the NEBS.TRANSMITTED_BDRS view.
            // Duplicate NebsBdrID values occur for each transmission, i.e. Gov's Office, Leg, etc.
            // We need policy decision on which transmissions make the cut for the Bill Tracking module

            var nebsBdrs = await nebsDb.NebsBdrs.Where(b => b.BudgetPeriodID == appGlobalBudgetPeriod && b.NebsBdrAgendaName.Contains("Leg")).ToListAsync();

            // iterate over NEBS query results. if a record's NebsBdrID is not contained in the HashSet (above) then the record is added to the ViewModel
            foreach (NebsBdr nebsBdr in nebsBdrs)
            {
                if (!localNebsBdrIdsHS.Contains(nebsBdr.NebsBdrID))
                {
                    viewModel.Add(
                        new BillsIndexViewModel
                        {
                            AdminAppsID = null,
                            NebsBdrID = nebsBdr.NebsBdrID,
                            Location = "NEBS",
                            CompositeBillNumber = "(BDR)",
                            NebsBdrNumber = nebsBdr.NebsBdrNumber,
                            Title = nebsBdr.NebsBdrTitle,
                            NumReviews = null
                        }
                    );
                }
            }

            return View(viewModel);
        }

        // GET: Bills/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Bill bill = await db.Bills
                                .Where(b => b.ID == id)
                                .Include(b => b.BillVersions)
                                .FirstOrDefaultAsync();

            if (bill == null)
            {
                return HttpNotFound();
            }

            // convert db model to view model
            BillEditViewModel viewModel = new BillEditViewModel();
            viewModel.InjectFrom(bill);

            // get user's approval level
            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            // get current BillVersion and attach to ViewModel
            BillVersion currentVersion = bill.BillVersions.
                                            OrderByDescending(v => v.VersionNum).
                                            FirstOrDefault();
            viewModel.CurrentVersion = currentVersion;

            // locate approved review (if one exists) for user's Dept/Div, based on user's approval level; set title of user's agency for display in view
            var approvedReview = new BillReview();
            switch (userModuleApprovalLevel)
            {
                case 0:
                case 1:
                    approvedReview = bill.Reviews
                        .Where(r =>
                            //r.BillID == bill.ID &&
                            r.Approvals.Where(a => a.ApprovalLevel == 1).Any() && 
                            r.CreatedByUserInDept.ID == user.DeptID)
                        .OrderByDescending(r => r.BillVersion.VersionNum)
                        .FirstOrDefault();
                    ViewBag.ApprovedReviewAgency = user.Dept.CompositeDeptName;
                    break;
                case 2:
                    approvedReview = bill.Reviews
                        .Where(r =>
                            //r.BillID == bill.ID &&
                            r.Approvals.Where(a => a.ApprovalLevel == 2).Any() && 
                            r.CreatedByUserInDiv.ID == user.DivID)
                        .OrderByDescending(r => r.BillVersion.VersionNum)
                        .FirstOrDefault();
                    ViewBag.ApprovedReviewAgency = user.Div.CompositeDivName;
                    break;
                case 3:
                    // Agency users should only see their own reviews. This case loads their review
                    // (if one exists) into the approvedReview variable,
                    // and then removes all other BillReviews from the viewModel
                    approvedReview = bill.Reviews
                        .Where(r =>
                            //r.BillID == bill.ID &&
                            r.ApplicationUserID == user.Id)
                        .OrderByDescending(r => r.BillVersion.VersionNum)
                        .FirstOrDefault();

                    if (approvedReview == null)
                    {
                        viewModel.UserCanReview = true;
                    }
                    else
                    {
                        viewModel.ApprovedReview.InjectFrom(approvedReview);
                        viewModel.UserCanReview = false;
                        viewModel.ApprovedAt = approvedReview.CreatedAt;
                        ViewBag.ReviewMessage = "You have already submitted a review for this Bill. If your Review has not yet been read at the Division level then you can modify your submission by clicking 'View' (above), then clicking 'Edit' on the following screen.";
                    }

                    // remove all reviews from viewModel to prevent unauthorized viewing
                    foreach (BillReviewViewModel review in viewModel.Reviews.ToList())
                    {
                        viewModel.Reviews.Remove(review);
                    }

                    // return user to view and skip over the following functions
                    return View(viewModel);
                default:
                    break;
            }

            // separate approved BillReview (if one exists) from non-approved BillReviews
            if (approvedReview != null)
            {
                viewModel.ApprovedReview = new BillReviewViewModel();
                viewModel.ApprovedReview.InjectFrom(approvedReview);
                viewModel.ApprovedReview.IsApproved = true;

                CalcBillReviewStatus(currentVersion, approvedReview, viewModel.ApprovedReview, user);

                var targetLevel = (userModuleApprovalLevel == 0 ? 1 : userModuleApprovalLevel);
                BillReviewApproval approval = approvedReview.Approvals
                    .Where(r => r.ApprovalLevel == targetLevel)
                    .FirstOrDefault();
                viewModel.ApprovedByUser = approval.ApprovedBy;
                viewModel.ApprovedAt = approval.ApprovedAt;

                // if user's review is the approved review then set the message accordingly and prevent them from creating an additional review
                if (approval.BillReview.CreatedByUser.Id == user.Id)
                {
                    viewModel.UserCanReview = false;
                    ViewBag.ReviewMessage = "Your review is the Approved Review for this Bill (above)";
                }
                else if (user.CreatedBillReviews.Where(r => r.BillID == bill.ID).Any())
                {
                    viewModel.UserCanReview = false;
                    ViewBag.ReviewMessage = "You have already reviewed this Bill. To modify your review clicking 'View' (below), then clicking 'Edit' on the following screen.";
                }
                // otherwise, if user is not at approval level 0 they can create a Review
                else if (userModuleApprovalLevel > 0)
                {
                    viewModel.UserCanReview = true;
                }

                // load non-approved reviews
                switch (userModuleApprovalLevel)
                {
                    case 0:
                    case 1:
                        // load approved reviews from Div level plus all reviews created at the Dept level, except for the approved review; restrict reviews to user's Dept
                        var x = await db.BillReviews
                            .Where(r =>
                                !(r is AlsrBillReviewSnapshot) &&
                                r.BillID == bill.ID &&
                                r.CreatedByUserInDept.ID == user.DeptID &&
                                r.ApplicationUserID != approvedReview.ApplicationUserID &&
                                (r.Approvals.Where(a => a.ApprovalLevel == 2).Any() 
                                    || r.CreatedAtApprovalLevel == 1 && r.ID != approvedReview.ID))
                            .Include(r => r.BillVersion)
                            .GroupBy(r => r.CreatedByUser.Id)
                            .ToListAsync();
                        foreach (IGrouping<string, BillReview> group in x)
                        {
                            var viewModelNonApprovedReview = new BillReviewViewModel();
                            var latestReview = group.OrderByDescending(g => g.BillVersion.VersionNum).FirstOrDefault();
                            viewModelNonApprovedReview.InjectFrom(latestReview);
                            viewModelNonApprovedReview.IsApproved = false;

                            CalcBillReviewStatus(currentVersion, latestReview, viewModelNonApprovedReview, user);

                            viewModel.Reviews.Add(viewModelNonApprovedReview);
                        }
                        // Dept level users have no restrictions on BillReviewApprovals
                        ViewBag.UserCanApprove = (userModuleApprovalLevel == 0 ? false : true);
                        break;
                    case 2:
                        // load reviews created at the Div or Agency level, except for the approved review; restrict reviews to user's Div
                        var y = await db.BillReviews
                            .Where(r =>
                                !(r is AlsrBillReviewSnapshot) &&
                                r.BillID == bill.ID &&
                                r.CreatedByUserInDiv.ID == user.DivID && 
                                r.ApplicationUserID != approvedReview.ApplicationUserID &&
                                r.CreatedAtApprovalLevel >= userModuleApprovalLevel)
                            .Include(r => r.BillVersion)
                            .GroupBy(r => r.CreatedByUser.Id)
                            .ToListAsync();
                        foreach (IGrouping<string, BillReview> group in y)
                        {
                            var viewModelNonApprovedReview = new BillReviewViewModel();
                            var latestReview = group.OrderByDescending(g => g.BillVersion.VersionNum).FirstOrDefault();
                            viewModelNonApprovedReview.InjectFrom(latestReview);
                            viewModelNonApprovedReview.IsApproved = false;

                            CalcBillReviewStatus(currentVersion, latestReview, viewModelNonApprovedReview, user);

                            viewModel.Reviews.Add(viewModelNonApprovedReview);
                        }
                        // if ApprovedReview has been read at Dept level, set UserCanApprove to false, otherwise set it to true
                        if (approvedReview.Notifications.Where(n => n.ApprovalLevel == userModuleApprovalLevel - 1).Single().IsRead)
                        {
                            ViewBag.UserCanApprove = false;
                        }
                        else
                        {
                            ViewBag.UserCanApprove = true;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // since no Approved Review exists, set UserCanApprove to true
                ViewBag.UserCanApprove = true;

                // if user already reviewed this bill then set the message accordingly and prevent them from creating an additional review
                if (user.CreatedBillReviews.Where(r => r.BillID == bill.ID).Any())
                {
                    ViewBag.ReviewMessage = "You have already reviewed this Bill. To modify your review click 'View' (below), then click 'Edit' on the following screen.";
                    viewModel.UserCanReview = false;
                }
                else if (userModuleApprovalLevel > 0)
                {
                    viewModel.UserCanReview = true;
                }

                // load non-approved Reviews
                switch (userModuleApprovalLevel)
                {
                    case 0:
                    case 1:
                        // load approved reviews from Div level plus all reviews created at the Dept level; restrict reviews to user's Dept
                        var x = await db.BillReviews
                            .Where(r =>
                                !(r is AlsrBillReviewSnapshot) &&
                                r.BillID == bill.ID &&
                                ((r.CreatedByUserInDept.ID == user.DeptID && r.Approvals.Where(a => a.ApprovalLevel == 2).Any())
                                || (r.CreatedAtApprovalLevel == 1)))
                            .Include(r => r.BillVersion)
                            .GroupBy(r => r.CreatedByUser.Id)
                            .ToListAsync();
                        foreach (IGrouping<string, BillReview> group in x)
                        {
                            var viewModelNonApprovedReview = new BillReviewViewModel();
                            var latestReview = group.OrderByDescending(g => g.BillVersion.VersionNum).FirstOrDefault();
                            viewModelNonApprovedReview.InjectFrom(latestReview);
                            viewModelNonApprovedReview.IsApproved = false;

                            CalcBillReviewStatus(currentVersion, latestReview, viewModelNonApprovedReview, user);

                            viewModel.Reviews.Add(viewModelNonApprovedReview);
                        }
                        break;
                    case 2:
                        // load all reviews created at the Div or Agency level; restrict reviews to user's Div
                        var y = await db.BillReviews
                            .Where(r =>
                                !(r is AlsrBillReviewSnapshot) &&
                                r.BillID == bill.ID &&
                                r.CreatedByUserInDiv.ID == user.DivID && 
                                r.CreatedAtApprovalLevel >= userModuleApprovalLevel)
                            .Include(r => r.BillVersion)
                            .GroupBy(r => r.CreatedByUser.Id)
                            .ToListAsync();
                        foreach (IGrouping<string, BillReview> group in y)
                        {
                            var viewModelNonApprovedReview = new BillReviewViewModel();
                            var latestReview = group.OrderByDescending(g => g.BillVersion.VersionNum).FirstOrDefault();
                            viewModelNonApprovedReview.InjectFrom(latestReview);
                            viewModelNonApprovedReview.IsApproved = false;

                            CalcBillReviewStatus(currentVersion, latestReview, viewModelNonApprovedReview, user);

                            viewModel.Reviews.Add(viewModelNonApprovedReview);
                        }
                        break;
                    default:
                        break;
                }
            }


            // load BillReviewRequests 
            var requests = new List<BillReviewRequest>();
            switch (userModuleApprovalLevel)
            {
                case 0:
                case 1:
                    requests = await db.BillReviewRequests
                        .Where(r => 
                            r.BillID == bill.ID &&
                            r.RequestedToUser.Dept.ID == user.DeptID && 
                            r.RequestedAtApprovalLevel == 1)
                        .Include(r => r.RequestedToUser.Div)
                        .OrderBy(r => r.RequestedToUser.DivID)
                        .ToListAsync();
                    break;
                case 2:
                    requests = await db.BillReviewRequests
                        .Where(r => 
                            r.BillID == bill.ID &&
                            r.RequestedToUser.Div.ID == user.DivID && 
                            r.RequestedAtApprovalLevel == userModuleApprovalLevel)
                        .Include(r => r.RequestedToUser.Div)
                        .OrderBy(r => r.RequestedToUser.DivID)
                        .ToListAsync();
                    break;
            }

            // parse BillReviewRequests (if any) for viewModel
            if (requests != null)
            {
                foreach (BillReviewRequest r in requests)
                {
                    var editorViewModel = new BillReviewRequestIndividualUserViewModel()
                    {
                        ApplicationUserID = r.RequestedToUserID,
                        FullName = r.RequestedToUser.FullName,
                        CompositeDivName = r.RequestedToUser.Div.CompositeDivName,
                        Selected = false,
                        PreviouslyRequested = true,
                        RequestedAt = r.RequestedAt
                    };

                    if (r.Fulfilled)
                    {
                        editorViewModel.Fulfilled = true;
                        editorViewModel.FulfilledAt = r.FulfilledAt;
                    }
                    else
                    {
                        editorViewModel.Fulfilled = false;
                    }

                    viewModel.ReviewRequestedFromUsers.Add(editorViewModel);
                }
            }

            return View(viewModel);
        }

        private static void CalcBillReviewStatus(BillVersion currentVersion, BillReview review, BillReviewViewModel viewModel, ApplicationUser user)
        {
            if (review.BillVersion.VersionNum == currentVersion.VersionNum)
            {
                viewModel.UpToDate = true;
                viewModel.StatusMessage = "Up to date";
            }
            else
            {
                viewModel.UpToDate = false;
                viewModel.StatusMessage = "Out of date";
                if (review.CreatedByUser.Id == user.Id)
                {
                    viewModel.UserCanUpdate = true;
                }
            }
        }

        // GET: Bills/Create
        [Authorize(Roles = "BillsClerc, BillsDept")]
        public async Task<ActionResult> Create(decimal? nebsBdrId, int? billRecordRequestId)
        {
            BillEditViewModel viewModel = new BillEditViewModel();
            viewModel = await PopulateBillEditViewModelLists(viewModel);

            if (nebsBdrId.HasValue)
            {
                Information(string.Format("<B>NOTE:</B> You are about to copy a BDR record from the NEBS BDR Module to the AdminApps Bill Tracking Module."
                + "The 'Title', 'Dept' and 'Div' values below have been pre-populated with information from the NEBS BDR Module. However, you can override these values if you wish."
                + "The original values from the NEBS BDR Module will be retained in the AdminApps database, so overriding the pre-populated values will not result in data loss."));

                // 2014-12-04: added Where clause for AgendaName contains("Leg") to filter duplicate BDR records from the NEBS.TRANSMITTED_BDRS view.
                // Duplicate NebsBdrID values occur for each transmission, i.e. Gov's Office, Leg, etc.
                // We need policy decision on which transmissions make the cut for the Bill Tracking module
                NebsBdr sourceBdr = await nebsDb.NebsBdrs.Where(b => b.NebsBdrID == nebsBdrId && b.NebsBdrAgendaName.Contains("Leg")).FirstAsync();
                viewModel.InjectFrom(sourceBdr);

                // pre-populate Title and selected Dept/Div
                viewModel.Title = sourceBdr.NebsBdrTitle;
                viewModel.DeptID = sourceBdr.NebsDeptID;
                viewModel.DivID = sourceBdr.NebsDivID;
            }

            if (billRecordRequestId.HasValue)
            {
                var request = await db.BillRecordRequests.
                    FindAsync(billRecordRequestId);

                // check for pre-existing Bill record with matching prefix and suffix
                if (request.BillPrefixID != null && request.Suffix != null)
                {
                    var potentialTarget = await db.Bills.Where(b => b.BillPrefixID == request.BillPrefixID && b.Suffix == request.Suffix).FirstOrDefaultAsync();
                    if (potentialTarget != null)
                    {
                        return RedirectToAction("AttachBillRecordRequest", new { id = potentialTarget.ID, requestID = request.ID });
                    }
                }

                viewModel.InjectFrom(request);
                viewModel.BillRecordRequestID = billRecordRequestId;
                Warning(string.Format("This record is being created in response to a Bill Record Request. Please confirm Bill Prefix/Suffix, BDR Prefix/Suffix, Summary, and Title."));
            }

            return View(viewModel);
        }

        // POST: Bills/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "BillsClerc, BillsDept")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,NelisID,NebsBdrID,NebsBdrNumber,NebsDeptID,NebsDivID,NebsBdrApprovedDate,NebsBdrTitle,NebsBdrDescription,NebsBdrTransmittalDate,NebsBdrAgendaName,NebsBdrAgendaDate,BillPrefixID,Suffix,BdrPrefix,BdrSuffix,DeptID,DivID,Title,Digest,Summary,DateIntroduced,LegStatusID,FirstHouseCommitteeID,SecondHouseCommitteeID,DatePassedFirstHouse,DatePassedSecondHouse,GovActionID,DateGovAction,ChapterNum,NelisHyperlink,BillRecordRequestID")] BillEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                Bill bill = new Bill();
                bill.InjectFrom(viewModel);

                // add CreatedByUser
                ApplicationUser user = await ReturnCurrentUserAsync();
                bill.ApplicationUserID = user.Id;

                // add CreatedAt timestamp
                bill.CreatedAt = DateTime.Now;

                // attach global BudgetPeriodID and BudgetSessionID
                AppGlobalSetting globalSettings = await db.AppGlobalSettings.FirstOrDefaultAsync();
                bill.InjectFrom(globalSettings);

                db.Bills.Add(bill);
                await db.SaveChangesAsync();

                // create BillVersionNum 0 to initialize versioning process
                BillVersion version = new BillVersion();
                version.BillID = bill.ID;
                version.VersionNum = 0;
                version.IsReprint = false;
                version.IsEnrolled = false;
                db.BillVersions.Add(version);
                await db.SaveChangesAsync();

                if (viewModel.BillRecordRequestID.HasValue)
                {
                    // mark request fulfilled
                    var request = await db.BillRecordRequests
                                                .Where(r => r.ID == viewModel.BillRecordRequestID)
                                                .Include(r => r.RequestedByUser)
                                                .FirstOrDefaultAsync();

                    request.Fulfilled = true;
                    request.FulfilledAt = DateTime.Now;
                    request.FulfilledByUserID = user.Id;
                    request.BillID = bill.ID;
                    await db.SaveChangesAsync();

                    // send confirmation email
                    string body;
                    using (var sr = new StreamReader(Server.MapPath("\\Templates\\") + "BillRecordRequestFulfilledConfirmation.html"))
                    {
                        body = await sr.ReadToEndAsync();
                    }
                    string domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                    string listAllBillsUrl = string.Format(domain + "/Bills");
                    string createBillReviewUrl = string.Format(domain + "/BillReviews/Create/" + bill.ID);
                    string messageBody = string.Format(body, request.RequestedByUser.FirstName, bill.CompositeBillNumber, listAllBillsUrl, createBillReviewUrl);
                    await UserManager.SendEmailAsync(request.RequestedByUserID, "Your AdminApps Bill Record Request has been fulfilled", messageBody);

                    // generate success message for BillsClerc user
                    Success(string.Format("Bill Record Request from " + request.RequestedByUser.FullName + " fulfilled. User has been notified."));
                }
                return RedirectToAction("Index");
            }

            Danger("One or more of the errors listed below prevented the record from being created");
            viewModel = await PopulateBillEditViewModelLists(viewModel);

            return View(viewModel);
        }

        // GET: Bills/Edit/5
        [Authorize(Roles = "BillsClerc, BillsDept")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bill bill = await db.Bills.FindAsync(id);
            if (bill == null)
            {
                return HttpNotFound();
            }

            BillEditViewModel viewModel = new BillEditViewModel();
            viewModel.InjectFrom(bill);
            viewModel = await PopulateBillEditViewModelLists(viewModel);

            return View(viewModel);
        }

        // POST: Bills/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "BillsClerc, BillsDept")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bill bill = await db.Bills.FindAsync(id);

            if (TryUpdateModel(bill, "",
                new string[] { "DeptID", "DivID", "BillPrefixID", "NelisID", "Suffix", "BdrPrefix", "BdrSuffix", "Title", "Digest",
                    "Summary", "DateIntroduced", "LegStatusID", "FirstHouseCommitteeID", "SecondHouseCommitteeID", 
                    "DatePassedFirstHouse", "DatePassedSecondHouse", "GovACtionID", "DateGovAction", "ChapterNum" }))
            {
                try
                {
                    db.Entry(bill).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (DataException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            BillEditViewModel viewModel = new BillEditViewModel();
            viewModel = await PopulateBillEditViewModelLists(viewModel);

            return View(viewModel);
        }

        private async Task<BillEditViewModel> PopulateBillEditViewModelLists(BillEditViewModel viewModel)
        {
            var globalBudgetPeriod = (await db.AppGlobalSettings.FirstOrDefaultAsync()).BudgetPeriodID;

            // populate DropDown lists
            viewModel.BillPrefixesList = new SelectList(await db.BillPrefixes.ToListAsync(), "ID", "Prefix");
            viewModel.DeptsList = new SelectList(await db.Depts.Where(b => b.BudgetPeriodID == globalBudgetPeriod).OrderBy(c => c.Code).ToListAsync(), "ID", "CompositeDeptName");
            viewModel.DivsList = new SelectList(await db.Divs.Where(b => b.BudgetPeriodID == globalBudgetPeriod).OrderBy(c => c.Code).ToListAsync(), "ID", "CompositeDivName");
            viewModel.LegStatusesList = new SelectList(await db.LegStatuses.ToListAsync(), "ID", "Description");
            viewModel.CommitteesList = new SelectList(await db.LegCommittees.ToListAsync(), "ID", "CommitteeName");
            viewModel.GovActionsList = new SelectList(await db.GovActions.ToListAsync(), "ID", "Description");

            return viewModel;
        }

        // GET: Bills/Delete/5
        [Authorize(Roles = "BillsClerc, BillsDept")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bill bill = await db.Bills.FindAsync(id);
            if (bill == null)
            {
                return HttpNotFound();
            }
            return View(bill);
        }

        // POST: Bills/Delete/5
        [Authorize(Roles = "BillsClerc, BillsDept")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Bill bill = await db.Bills.FindAsync(id);
            var billVersions = bill.BillVersions.ToList();
            foreach (BillVersion v in billVersions)
            {
                db.BillVersions.Remove(v);
            }
            await db.SaveChangesAsync();

            db.Bills.Remove(bill);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Bills/AddVersion/5
        [Authorize(Roles = "BillsClerc")]
        public async Task<ActionResult> AddVersion(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var bill = await db.Bills.Where(b => b.ID == id)
                            .Include(b => b.BillVersions)
                            .FirstOrDefaultAsync();

            if (bill == null)
            {
                return HttpNotFound();
            }

            var viewModel = new AddBillVersionViewModel();
            viewModel.Bill.InjectFrom(bill);
            viewModel.BillID = bill.ID;
            foreach (BillVersion v in bill.BillVersions.OrderBy(x => x.VersionNum).ToList())
            {
                viewModel.Bill.BillVersions.Add(v);
            }
            return View(viewModel);
        }

        // POST: Bills/AddVersion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BillsClerc")]
        public async Task<ActionResult> AddVersion(AddBillVersionViewModel model)
        {
            if (ModelState.IsValid)
            {
                // identify users who submitted a BillReview for a previous BillVersion
                var store = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var userManager = new UserManager<ApplicationUser>(store);
                var targetUsers = await userManager.Users.Where(u => u.CreatedBillReviews.Where(r => r.BillID == model.BillID).Any()).ToListAsync();


                // create new BillVersion
                var previousVersion = await db.BillVersions.Where(v => v.BillID == model.BillID).OrderByDescending(v => v.VersionNum).FirstOrDefaultAsync();
                var newVersion = new BillVersion();
                newVersion.InjectFrom(model);
                newVersion.VersionNum = previousVersion.VersionNum + 1;
                db.BillVersions.Add(newVersion);
                await db.SaveChangesAsync();


                // notify users of new version
                if (targetUsers != null && newVersion.IsReprint && newVersion.ReprintDate != null)
                {
                    var newVersionRead = await db.BillVersions
                                            .Where(v => v.ID == newVersion.ID)
                                            .Include(v => v.Bill)
                                            .FirstOrDefaultAsync();
                    string body = "";
                    using (var sr = new StreamReader(Server.MapPath("\\Templates\\") + "NewBillVersionNotification.html"))
                    {
                        body = await sr.ReadToEndAsync();
                    }
                    string messageSubject = string.Format("AdminApps Bill Tracking: Notification of Reprint for {0}", newVersionRead.Bill.CompositeBillNumber);
                    string domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                    foreach (ApplicationUser u in targetUsers)
                    {
                        // compose email notification
                        int userBillReviewId = u.CreatedBillReviews.Where(r => r.BillID == newVersionRead.BillID).OrderByDescending(r => r.BillVersion.VersionNum).FirstOrDefault().ID;
                        string confirmReviseBillReviewUrl = string.Format("{0}/BillReviews/Update/{1}", domain, userBillReviewId);
                        string messageBody = string.Format(body, newVersionRead.Bill.CompositeBillNumber, u.FirstName, newVersionRead.ReprintDate.Value.ToShortDateString(), newVersionRead.Bill.calculatedHyperlink, confirmReviseBillReviewUrl, newVersionRead.VersionDescription);
                        await UserManager.SendEmailAsync(u.Id, messageSubject, messageBody);

                        // create notification record
                    }


                }
                Success("Bill version added successfully");
                return RedirectToAction("Details", new { id = model.BillID });
            }
            Danger("Error adding new Bill Version");
            return RedirectToAction("Index");
        }

        // GET: Bills/RequestReview/5
        [Authorize(Roles = "BillsClerc, BillsDept, BillsDiv")]
        public async Task<ActionResult> RequestReview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Bill bill = await db.Bills.FindAsync(id);
            if (bill == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            ApplicationRole targetRole = new ApplicationRole();
            List<ApplicationUser> targetUsers = new List<ApplicationUser>();
            switch (userModuleApprovalLevel)
            {
                case 0:
                case 1:
                    targetRole = await RoleManager.FindByNameAsync("BillsDiv");
                    targetUsers = await UserManager.Users.Where(u => u.DeptID == user.DeptID && u.Roles.Where(r => r.RoleId == targetRole.Id).Count() > 0).ToListAsync();
                    break;
                case 2:
                    targetRole = await RoleManager.FindByNameAsync("BillsAgency");
                    targetUsers = await UserManager.Users.Where(u => u.DivID == user.DivID && u.Roles.Where(r => r.RoleId == targetRole.Id).Count() > 0).ToListAsync();
                    break;
                default:
                    break;
            }

            var viewModel = new BillReviewRequestSelectUsersViewModel();

            viewModel.BillID = bill.ID;
            viewModel.BillName = bill.CompositeBillName;

            foreach (ApplicationUser u in targetUsers)
            {
                var editorViewModel = new BillReviewRequestIndividualUserViewModel()
                {
                    ApplicationUserID = u.Id,
                    FullName = u.FullName,
                    CompositeDivName = u.Div.CompositeDivName,
                    Selected = false
                };

                // for Dept users, look for Approved Reviews from the Div level.
                // for Div users, look for any Reviews that have been created within the Div
                BillReview existingReview = new BillReview();
                switch (userModuleApprovalLevel)
                {
                    case 0:
                    case 1:
                        existingReview = u.CreatedBillReviews.Where(r => r.BillID == bill.ID && r.Approvals.Where(a => a.ApprovalLevel == 2).Any()).FirstOrDefault();
                        break;
                    case 2:
                        existingReview = u.CreatedBillReviews.Where(r => r.BillID == bill.ID).FirstOrDefault();
                        break;
                    default:
                        break;
                }

                if (existingReview == null)
                {
                    var previousRequest = u.BillReviewRequestsTo.Where(r => r.BillID == bill.ID).FirstOrDefault();
                    if (previousRequest != null)
                    {
                        editorViewModel.PreviouslyRequested = true;
                        if (previousRequest.Fulfilled)
                        {
                            editorViewModel.Fulfilled = true;
                            editorViewModel.FulfilledAt = previousRequest.FulfilledAt;
                        }
                        else
                        {
                            editorViewModel.Fulfilled = false;
                            editorViewModel.RequestedAt = previousRequest.RequestedAt;
                        }
                    }
                    else
                    {
                        editorViewModel.PreviouslyRequested = false;
                        editorViewModel.Fulfilled = false;
                    }
                }
                else
                {
                    editorViewModel.Fulfilled = true;
                }

                viewModel.Users.Add(editorViewModel);
            }

            return View(viewModel);
        }

        // POST: Bills/RequestReview/5
        [HttpPost]
        [Authorize(Roles = "BillsClerc, BillsDept, BillsDiv")]
        public async Task<ActionResult> RequestReview(int id, BillReviewRequestSelectUsersViewModel model)
        {
            Bill bill = await db.Bills.FindAsync(id);

            var selectedIds = model.getSelectedIds();

            if (selectedIds.Count() == 0)
            {
                Danger("No users were selected for requests.");
                return RedirectToAction("RequestReview", new { id = bill.ID });
            }

            var selectedUsers = from x in UserManager.Users.Include(u => u.Div)
                                where selectedIds.Contains(x.Id)
                                select x;

            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            // determine name/role of requesting Agency, for use in email notification below. Set email template according to role.
            string requestedByAgency = "";
            string body = "";
            switch (userModuleApprovalLevel)
            {
                case 0:
                case 1:
                    requestedByAgency = string.Format("Department {0}", user.Dept.CompositeDeptName);
                    using (var sr = new StreamReader(Server.MapPath("\\Templates\\") + "BillReviewRequestNotificationDiv.html"))
                    {
                        body = await sr.ReadToEndAsync();
                    }
                    break;
                case 2:
                    requestedByAgency = string.Format("Division {0}", user.Div.CompositeDivName);
                    using (var sr = new StreamReader(Server.MapPath("\\Templates\\") + "BillReviewRequestNotificationAgency.html"))
                    {
                        body = await sr.ReadToEndAsync();
                    }
                    break;
                default:
                    break;
            }


            // initialize counter for use in Success message, below
            var counter = 0;

            foreach (var u in selectedUsers.ToList())
            {
                // confirm that user has not submitted a review for this Bill
                var previousReview = await db.BillReviews
                    .Where(r => r.BillID == bill.ID && r.ApplicationUserID == u.Id)
                    .FirstOrDefaultAsync();

                // if no previous review exists from this user then create the BillReviewRequest
                if (previousReview == null)
                {
                    // for BillsClerc users, make request on behalf of Department level by setting requestedAtApprovalLevel to 1 
                    // otherwise set to userModuleApprovalLevel
                    int requestedAtApprovalLevel = new int();
                    if (userModuleApprovalLevel == 0)
                    {
                        requestedAtApprovalLevel = 1;
                    }
                    else
                    {
                        requestedAtApprovalLevel = userModuleApprovalLevel;
                    }

                    db.BillReviewRequests.Add(new BillReviewRequest()
                        {
                            RequestedByUserID = user.Id,
                            RequestedToUserID = u.Id,
                            BillID = bill.ID,
                            RequestedAt = DateTime.Now,
                            RequestedAtApprovalLevel = requestedAtApprovalLevel,
                            Fulfilled = false
                        });

                    // send email notification to user
                    string messageSubject = string.Format("AdminApps Bill Tracking: A Review of {0} has been requested on behalf of {1}", bill.CompositeBillNumber, requestedByAgency);
                    string domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                    string createReviewUrl = string.Format("{0}/BillReviews/Create/{1}", domain, bill.ID);
                    string messageBody = string.Format(body, u.FirstName, bill.calculatedHyperlink, bill.CompositeBillNumber, requestedByAgency, createReviewUrl, u.Div.CompositeDivName);
                    await UserManager.SendEmailAsync(u.Id, messageSubject, messageBody);

                    counter++;
                }


            }

            await db.SaveChangesAsync();
            Success("A review for " + bill.CompositeBillName + " was requested from " + counter + " user(s).");

            return RedirectToAction("Details", new { id = bill.ID });
        }

        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        public async Task<ActionResult> BillReviewRequests()
        {
            ApplicationUser user = await ReturnCurrentUserAsync();

            var requests = await db.BillReviewRequests
                .Where(r => r.RequestedToUserID == user.Id)
                .ToListAsync();

            var viewModel = new List<BillReviewRequestViewModel>();

            foreach (BillReviewRequest r in requests)
            {
                var viewModelRequest = new BillReviewRequestViewModel()
                    {
                        BillID = r.BillID,
                        CompositeBillName = r.Bill.CompositeBillName,
                        RequestedByUserName = r.RequestedByUser.FullName,
                        RequestedAt = r.RequestedAt
                    };
                if (r.Fulfilled)
                {
                    viewModelRequest.Status = "Fulfilled " + r.FulfilledAt;
                }
                else if (r.Bill.Reviews.Where(x => x.CreatedByUser.Id == user.Id).Any())
                {
                    viewModelRequest.Status = "Reviewed, but no Approved Review submitted";
                }
                else
                {
                    viewModelRequest.Status = "Pending";
                }

                viewModel.Add(viewModelRequest);
            }

            return View(viewModel);
        }

        // GET: Bills/RequestRecord/5
        [Authorize(Roles = "BillsDiv, BillsAgency")]
        public async Task<ActionResult> RequestRecord()
        {
            var viewModel = new BillRecordRequestViewModel();
            viewModel.BillPrefixesList = new SelectList(await db.BillPrefixes.ToListAsync(), "ID", "Prefix");

            return View(viewModel);
        }

        // POST: Bills/RequestRecord/5
        [HttpPost]
        [Authorize(Roles = "BillsDiv, BillsAgency")]
        public async Task<ActionResult> RequestRecord([Bind(Include = "BillPrefixID,Suffix,BdrPrefix,BdrSuffix,Summary,Title,NelisHyperlink")] BillRecordRequestViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // capitalize BDR Prefix
                if (viewModel.BdrPrefix != null)
                {
                    viewModel.BdrPrefix = viewModel.BdrPrefix.ToUpper();
                }

                // check for matching, pre-existing Bill Record in database
                if (viewModel.BillPrefixID != null && viewModel.Suffix != null)
                {
                    var existingBill = await db.Bills.Where(b => b.BillPrefixID == viewModel.BillPrefixID && b.Suffix == viewModel.Suffix).FirstOrDefaultAsync();
                    if (existingBill != null)
                    {
                            viewModel.BillPrefixesList = new SelectList(await db.BillPrefixes.ToListAsync(), "ID", "Prefix");
                            Warning(string.Format("Bill {0} already exists in the Bill Tracking module's database. To submit a Bill Review for {0} please locate the Bill on the 'List All Bills' page, click the 'View' button, then click the 'Submit Bill Review' button.", existingBill.CompositeBillNumber));
                            return View(viewModel);
                    }
                }
                // check for matching, pre-existing BDR record
                else if (viewModel.BdrPrefix != null && viewModel.BdrSuffix != null)
                {
                    var existingBdr = await db.Bills.Where(b => b.BdrPrefix == viewModel.BdrPrefix && b.BdrSuffix == viewModel.BdrSuffix).FirstOrDefaultAsync();
                    if (existingBdr != null)
                    {
                        viewModel.BillPrefixesList = new SelectList(await db.BillPrefixes.ToListAsync(), "ID", "Prefix");
                        Warning(string.Format("BDR {0} already exists in the Bill Tracking module's database. To submit a Bill Review for {0} please locate the BDR on the 'List All Bills' page, click the 'View' button, then click the 'Submit Bill Review' button.", existingBdr.CompositeNelisBdrNumber));
                        return View(viewModel);
                    }
                }

                BillRecordRequest request = new BillRecordRequest();
                request.InjectFrom(viewModel);

                // attach global BudgetPeriodID and BudgetSessionID
                AppGlobalSetting globalSettings = await db.AppGlobalSettings.FirstOrDefaultAsync();
                request.InjectFrom(globalSettings);

                // add CreatedByUser
                ApplicationUser user = await ReturnCurrentUserAsync();
                request.RequestedByUserID = user.Id;

                // add RequestedAt timestamp
                request.RequestedAt = DateTime.Now;

                db.BillRecordRequests.Add(request);
                await db.SaveChangesAsync();
                Success("Request for New Bill Record has been submitted");
                return RedirectToAction("BillRecordRequests");
            }

            Danger("One or more of the errors listed below prevented the record from being created");
            viewModel.BillPrefixesList = new SelectList(await db.BillPrefixes.ToListAsync(), "ID", "Prefix");
            return View(viewModel);
        }

        // GET:/Bills/AttachBillRecordRequest/5
        [Authorize(Roles = "BillsClerc")]
        public async Task<ActionResult> AttachBillRecordRequest(int id, int requestID)
        {
            Bill bill = await db.Bills.FindAsync(id);
            BillRecordRequest request = await db.BillRecordRequests
                                                    .Where(r => r.ID == requestID)
                                                    .Include(r => r.RequestedByUser)
                                                    .FirstOrDefaultAsync();
            AttachBillRecordRequestViewModel viewModel = new AttachBillRecordRequestViewModel();

            viewModel.RequestedByUserName = request.RequestedByUser.FullName;
            viewModel.CompositeBillNumber = bill.CompositeBillNumber;
            viewModel.Summary = bill.Summary;
            viewModel.RequestID = requestID;
            viewModel.BillID = id;

            return View(viewModel);
        }

        // POST:/Bills/AttachBillRecordRequest/5
        [HttpPost]
        [Authorize(Roles = "BillsClerc")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AttachBillRecordRequest(AttachBillRecordRequestViewModel model)
        {
            BillRecordRequest request = await db.BillRecordRequests.FindAsync(model.RequestID);

            if (request == null)
            {
                Warning("Fail!");
                return View();
            }

            ApplicationUser user = await ReturnCurrentUserAsync();
            request.Fulfilled = true;
            request.FulfilledAt = DateTime.Now;
            request.FulfilledByUserID = user.Id;
            request.BillID = model.BillID;

            await db.SaveChangesAsync();


            // send confirmation email
            var bill = await db.Bills.FindAsync(model.BillID);
            string body;
            using (var sr = new StreamReader(Server.MapPath("\\Templates\\") + "BillRecordRequestFulfilledConfirmation.html"))
            {
                body = await sr.ReadToEndAsync();
            }
            string domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            string listAllBillsUrl = string.Format(domain + "/Bills");
            string createBillReviewUrl = string.Format(domain + "/BillReviews/Create/" + bill.ID);
            string messageBody = string.Format(body, request.RequestedByUser.FirstName, bill.CompositeBillNumber, listAllBillsUrl, createBillReviewUrl);
            await UserManager.SendEmailAsync(request.RequestedByUserID, "Your AdminApps Bill Record Request has been fulfilled", messageBody);

            // generate success message for BillsClerc user
            Success(string.Format("Bill Record Request from " + request.RequestedByUser.FullName + " fulfilled. User has been notified."));

            return RedirectToAction("BillRecordRequests");
        }

        // GET:/Bills/BillRecordRequests
        [Authorize(Roles = "BillsClerc, BillsDiv, BillsAgency")]
        public async Task<ActionResult> BillRecordRequests()
        {
            AppGlobalSetting globalSettings = await db.AppGlobalSettings.FirstOrDefaultAsync();
            ApplicationUser user = await ReturnCurrentUserAsync();
            int userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            var viewModel = new List<BillRecordRequestViewModel>();
            var requests = new List<BillRecordRequest>();
            switch (userModuleApprovalLevel)
            {
                case 0:
                    requests = await db.BillRecordRequests.
                        Where(r => r.BudgetPeriodID == globalSettings.BudgetPeriodID).
                        ToListAsync();
                    foreach (BillRecordRequest r in requests)
                    {
                        var x = new BillRecordRequestViewModel();
                        x.InjectFrom(r);
                        viewModel.Add(x);
                    }
                    break;
                case 2:
                case 3:
                    requests = await db.BillRecordRequests.
                        Where(r => r.BudgetPeriodID == globalSettings.BudgetPeriodID && r.RequestedByUserID == user.Id).
                        ToListAsync();
                    foreach (BillRecordRequest r in requests)
                    {
                        var x = new BillRecordRequestViewModel();
                        x.InjectFrom(r);
                        if (r.BillID != null)
                        {
                            BillReview review = r.Bill.Reviews.Where(y => y.CreatedByUser.Id == user.Id).FirstOrDefault();
                            if (review != null)
                            {
                                x.UserReviewed = true;
                                x.BillReviewID = review.ID;
                            }
                        }
                        viewModel.Add(x);
                    }
                    //ViewBag.RequestCount = requests.Count();
                    break;
                default:
                    break;
            }
            return View(viewModel);
        }

        // GET:/Bills/UserAccountRequests
        [Authorize(Roles = "BillsDiv, BillsAgency")]
        public async Task<ActionResult> BillsUserAccountRequests()
        {
            var user = await ReturnCurrentUserAsync();
            var viewModel = new List<BillsUserAccountRequestViewModel>();

            var requests = await db.BillsUserAccountRequests.
                                    Where(r => r.RequestedByUserID == user.Id).
                                    OrderByDescending(r => r.RequestedAt).
                                    ToListAsync();

            foreach (BillsUserAccountRequest r in requests)
            {
                var m = new BillsUserAccountRequestViewModel();
                m.InjectFrom(r);
                switch (r.RequestedApprovalLevel)
                {
                    case 2:
                        m.RoleName = "Division";
                        break;
                    case 3:
                        m.RoleName = "Agency";
                        break;
                    default:
                        break;
                }
                viewModel.Add(m);
            }

            return View(viewModel);
        }

        // GET:/Bills/PendingUserAccountRequests
        [Authorize(Roles = "GlobalAdmin")]
        public async Task<ActionResult> PendingBillsUserAccountRequests()
        {
            var user = await ReturnCurrentUserAsync();
            var viewModel = new List<BillsUserAccountRequestViewModel>();

            var requests = await db.BillsUserAccountRequests.
                                    Where(r => !r.Fulfilled).
                                    OrderByDescending(r => r.RequestedAt).
                                    ToListAsync();

            foreach (BillsUserAccountRequest r in requests)
            {
                var m = new BillsUserAccountRequestViewModel();
                m.InjectFrom(r);
                switch (r.RequestedApprovalLevel)
                {
                    case 2:
                        m.RoleName = "Division";
                        break;
                    case 3:
                        m.RoleName = "Agency";
                        break;
                    default:
                        break;
                }
                viewModel.Add(m);
            }
            return View(viewModel);
        }

        // GET:/Bills/RequestUserAccount
        [Authorize(Roles = "BillsDiv, BillsAgency")]
        public async Task<ActionResult> RequestBillsUserAccount()
        {
            var user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);
            var viewModel = new BillsUserAccountRequestViewModel();

            viewModel.DivID = user.DivID;
            viewModel.DeptID = user.DeptID;
            viewModel.AgencyName = user.Div.CompositeDivName;

            switch (userModuleApprovalLevel)
            {
                case 2:
                    var roles = await RoleManager.Roles.Where(r => r.AppModuleID == 5 && r.AppModuleApprovalLevel >= userModuleApprovalLevel).ToListAsync();
                    viewModel.RolesList = new SelectList(roles, "AppModuleApprovalLevel", "AppModuleApprovalTitle");
                    break;
                case 3:
                    viewModel.RequestedApprovalLevel = userModuleApprovalLevel;
                    break;
                default:
                    break;
            }

            return View(viewModel);
        }

        // POST:/Bills/RequestUserAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BillsDiv, BillsAgency")]
        public async Task<ActionResult> RequestBillsUserAccount(BillsUserAccountRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await ReturnCurrentUserAsync();
                var request = new BillsUserAccountRequest();
                request.InjectFrom(model);
                request.RequestedByUserID = user.Id;
                request.RequestedAt = DateTime.Now;
                request.Fulfilled = false;
                db.BillsUserAccountRequests.Add(request);
                await db.SaveChangesAsync();
                Success(string.Format("Request submitted for user: {0}", request.FullName));
                return RedirectToAction("Home");
            }
            Danger("Error creating request");
            var u = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(u, 5);
            var viewModel = new BillsUserAccountRequestViewModel();
            viewModel.InjectFrom(model);
            var roles = await RoleManager.Roles.Where(r => r.AppModuleID == 5 && r.AppModuleApprovalLevel >= userModuleApprovalLevel).ToListAsync();
            viewModel.RolesList = new SelectList(roles, "ID", "AppModuleApprovalTitle");
            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                // add nebsDb to Dispose routine
                nebsDb.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
