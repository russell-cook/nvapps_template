using AdminApps.DAL;
using AdminApps.Models;
using AdminApps.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AdminApps.Controllers
{
    [Authorize(Roles = "BillsClerc, BillsDept, BillsDiv, BillsAgency")]
    public class BillReviewsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BillReviews
        public async Task<ActionResult> Index()
        {
            ApplicationUser user = await ReturnCurrentUserAsync();
            var billReviews = db.BillReviews.
                Where(b => b.ApplicationUserID == user.Id).
                Include(b => b.Bill).
                Include(b => b.Recommendation);
            return View(await billReviews.ToListAsync());
        }

        // GET: BillReviews/Unread
        [Authorize(Roles = "BillsDept, BillsDiv")]
        public async Task<ActionResult> Unread()
        {
            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);
            var billReviews = new List<BillReview>();
            
            switch (userModuleApprovalLevel)
            {
                case 0:
                case 1:
                    billReviews = await db.BillReviews.
                        Where(r =>
                            r.Notifications.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Any() &&
                            r.Notifications.Where(a => a.ApprovalLevel == userModuleApprovalLevel).FirstOrDefault().IsRead == false &&
                            r.CreatedByUserInDept.ID == user.DeptID).
                        Include(r => r.Bill).
                        Include(r => r.Recommendation).
                        OrderBy(r => r.CreatedAt).
                        ToListAsync();
                    ViewBag.SubmittedByRole = "Division";
                    ViewBag.SubmittedForAgency = string.Format("Department ({0}):", user.Dept.CompositeDeptName);
                    break;
                case 2:
                    billReviews = await db.BillReviews.
                        Where(r =>
                            r.Notifications.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Any() &&
                            r.Notifications.Where(a => a.ApprovalLevel == userModuleApprovalLevel).FirstOrDefault().IsRead == false &&
                            r.CreatedByUserInDiv.ID == user.DivID).
                        Include(r => r.Bill).
                        Include(r => r.Recommendation).
                        OrderBy(r => r.CreatedAt).
                        ToListAsync();
                    ViewBag.SubmittedByRole = "Agency";
                    ViewBag.SubmittedForAgency = string.Format("Division ({0}):", user.Div.CompositeDivName);
                    break;
                default:
                    break;

            }

            return View(billReviews);
        }

        // GET: BillReviews/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BillReview review = await db.BillReviews
                .Where(r => r.ID == id).
                Include(b => b.Bill).
                Include(v => v.BillVersion).
                Include(n => n.Notifications).
                FirstOrDefaultAsync();

            if (review == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            BillReviewViewModel viewModel = new BillReviewViewModel();
            viewModel.InjectFrom(review);

            // for Dept and Div users, if review was sent up from lower approval level, calculate whether BillReviewNotification has been read.
            // otherwise, set DisplayAsRead to true (this hides the 'mark as read' button)
            if (userModuleApprovalLevel < review.CreatedAtApprovalLevel && userModuleApprovalLevel != 0)
            {
                viewModel.DisplayAsRead = review.Notifications.Where(n => n.ApprovalLevel == userModuleApprovalLevel).Single().IsRead;
            }
            else
            {
                viewModel.DisplayAsRead = true;
            }

            // if user created this BillReview, calculate whether or not user can edit. 
            if (user.Id == review.ApplicationUserID)
            {
                // if user is not Dept level user, determine whether the Review's notificaiton has been read
                if (userModuleApprovalLevel != 1)
                {
                    var notification = review.Notifications.Where(n => n.ApprovalLevel == userModuleApprovalLevel - 1).FirstOrDefault();
                    if (notification != null)
                    {
                        if (!notification.IsRead)
                        {
                            viewModel.UserCanEdit = true;
                        }
                        else
                        {
                            switch (userModuleApprovalLevel)
                            {
                                case 2:
                                    ViewBag.EditMessage = string.Format("This review has already been read at the Department level and can no longer be edited.");
                                    break;
                                case 3:
                                    ViewBag.EditMessage = string.Format("This review has already been read at the Division level and can no longer be edited.");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        viewModel.UserCanEdit = true;
                    }
                }
                // otherwise, Dept level users have no restrictions on editing their own Reviews
                else
                {
                    viewModel.UserCanEdit = true;
                }
            }

            // determine whether user can approve
            var approvedReview = new BillReview();
            switch (userModuleApprovalLevel)
            {
                // BillsClerc users cannot approve BillReviews
                case 0:
                    approvedReview = review.Bill.Reviews
                        .Where(r => 
                            r.Approvals.Where(a => a.ApprovalLevel == 1).Any() && 
                            r.CreatedByUserInDept.ID == user.DeptID)
                        .FirstOrDefault();
                    if (approvedReview != null)
                    {
                        if (approvedReview.ID == review.ID)
                        {
                            viewModel.ApprovedReviewMessage = string.Format("This is the Approved Review for {0}.", approvedReview.CreatedByUserInDept.CompositeDeptName);
                        }
                    }
                    viewModel.UserCanApprove = false;
                    break;
                // BillsDept users can approve a BillReview if the current BillReview is not already the approved review for its Bill
                case 1:
                    approvedReview = review.Bill.Reviews
                        .Where(r => r.Approvals.Where(a => a.ApprovalLevel == 1).Any() && r.CreatedByUserInDept.ID == user.DeptID)
                        .FirstOrDefault();
                    if (approvedReview != null)
                    {
                        if (approvedReview.ID == review.ID)
                        {
                            viewModel.ApprovedReviewMessage = string.Format("This is the Approved Review for {0}.", approvedReview.CreatedByUserInDept.CompositeDeptName);
                            viewModel.UserCanApprove = false;
                        }
                        else
                        {
                            viewModel.UserCanApprove = true;
                        }
                    }
                    else
                    {
                        viewModel.UserCanApprove = true;
                    }
                    break;
                // BillsDiv users can approve a BillReview if the current BillReview is not already the approved review,
                // but only if the existing (alternate) Approved Review has not been MarkedAsRead at the Dept level
                case 2:
                    approvedReview = review.Bill.Reviews
                        .Where(r => r.Approvals.Where(a => a.ApprovalLevel == 2).Any() && r.CreatedByUserInDiv.ID == user.DivID)
                        .FirstOrDefault();
                    if (approvedReview != null)
                    {
                        if (approvedReview.ID == review.ID)
                        {
                            viewModel.ApprovedReviewMessage = string.Format("This is the Approved Review for {0}.", approvedReview.CreatedByUserInDiv.CompositeDivName);
                            viewModel.UserCanApprove = false;
                        }
                        else if(approvedReview.Notifications.Where(n => n.ApprovalLevel == 1).FirstOrDefault().IsRead)
                        {
                            viewModel.UserCanApprove = false;
                        }
                        else
                        {
                            viewModel.UserCanApprove = true;
                        }
                    }
                    else
                    {
                        viewModel.UserCanApprove = true;
                    }
                    break;
                // BillsAgency users cannot approve BillReviews
                case 3:
                    viewModel.UserCanApprove = false;
                    break;
                default:
                    break;
            }


            return View(viewModel);
        }

        // GET: BillReviews/Create/5
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        public async Task<ActionResult> Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Bill parentBill = await db.Bills.
                                        Where(b => b.ID == id).
                                        Include(b => b.BillVersions).
                                        FirstOrDefaultAsync();
            ViewBag.ParentID = parentBill.CompositeBillName;
            ViewBag.ParentNelisLink = parentBill.calculatedHyperlink;

            BillReview review = new BillReview();
            review.BillID = id.Value;
            review.BillVersion = parentBill.BillVersions.OrderByDescending(v => v.VersionNum).FirstOrDefault();
            review.BillVersionID = review.BillVersion.ID;
            ViewBag.BillReviewRecommendationID = new SelectList(await db.BillReviewRecommendations.ToListAsync(), "ID", "Description");
            return View(review);
        }

        // POST: BillReviews/Create/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,BillID,BillVersionID,BillReviewRecommendationID,Comments,RequiresTestimony,InformationToBeProvided,ActivelyTracking,PolicyImpact,FiscalImpactYr1,FiscalImpactYr2,FiscalImpactFuture,FiscalNoteSubmitted,Notes")] BillReview review)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await ReturnCurrentUserAsync();
                var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

                review.ApplicationUserID = user.Id;
                review.DeptID = user.DeptID;
                review.DivID = user.DivID;
                review.CreatedAtApprovalLevel = userModuleApprovalLevel;
                review.CreatedAt = DateTime.Now;
                db.BillReviews.Add(review);
                await db.SaveChangesAsync();

                // for Agency users, create Notification for Div level, and check to see if Review was created in response to a Request
                if (userModuleApprovalLevel == 3)
                {
                    BillReviewNotification notification = new BillReviewNotification();
                    notification.BillReviewID = review.ID;
                    notification.ApprovalLevel = userModuleApprovalLevel - 1;
                    notification.IsRead = false;
                    db.BillReviewNotifications.Add(notification);

                    // if review was created in response to a Request, mark the Request as fulfilled
                    var request = await db.BillReviewRequests.Where(r => r.BillID == review.BillID && r.RequestedToUserID == user.Id).FirstOrDefaultAsync();
                    if (request != null)
                    {
                        request.Fulfilled = true;
                    }

                    await db.SaveChangesAsync();
                }

                Success("Bill Review submitted successfully.");
                return RedirectToAction("Details", "Bills", new { id = review.BillID } );
            }

            ViewBag.BillID = new SelectList(db.Bills, "ID", "NebsBdrNumber", review.BillID);
            ViewBag.BillReviewRecommendationID = new SelectList(db.BillReviewRecommendations, "ID", "Description", review.BillReviewRecommendationID);
            return View(review);
        }

        // GET: BillReviews/Edit/5
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillReview review = await db.BillReviews.Where(r => r.ID == id).Include(r => r.BillVersion).FirstOrDefaultAsync();
            if (review == null)
            {
                return HttpNotFound();
            }
            ViewBag.BillReviewRecommendationID = new SelectList(db.BillReviewRecommendations, "ID", "Description", review.BillReviewRecommendationID);
            return View(review);
        }

        // POST: BillReviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,BillID,BillVersionID,BillReviewRecommendationID,Comments,RequiresTestimony,InformationToBeProvided,ActivelyTracking,PolicyImpact,FiscalImpactYr1,FiscalImpactYr2,FiscalImpactFuture,FiscalNoteSubmitted,Notes,CreatedAtApprovalLevel,CreatedAt,ApplicationUserID,DeptID,DivID,RowVersion")] BillReview review)
        {
            if (ModelState.IsValid)
            {
                db.Entry(review).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Details", new { id = review.ID });
            }
            ViewBag.BillReviewRecommendationID = new SelectList(db.BillReviewRecommendations, "ID", "Description", review.BillReviewRecommendationID);
            return View(review);
        }

        // GET: BillReviews/Delete/5
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillReview review = await db.BillReviews.FindAsync(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // POST: BillReviews/Delete/5
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            BillReview review = await db.BillReviews.FindAsync(id);
            db.BillReviews.Remove(review);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: BillReviews/Approve/5
        [Authorize(Roles = "BillsDept, BillsDiv")]
        public async Task<ActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillReview review = await db.BillReviews.FindAsync(id);
            if (review == null)
            {
                return HttpNotFound();
            }

            BillReviewApprovalViewModel viewModel = new BillReviewApprovalViewModel();
            viewModel.ReviewToApprove = review;

            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            // check for previously approved Review
            var approvedReview = new BillReview();
            switch (userModuleApprovalLevel)
            {
                case 1:
                    approvedReview = review.Bill.Reviews
                        .Where(r => r.Approvals.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Count() > 0 && r.CreatedByUserInDept.ID == user.DeptID)
                        .FirstOrDefault();
                    ViewBag.ApprovedReviewAgency = user.Dept.CompositeDeptName;
                    break;
                case 2:
                    approvedReview = review.Bill.Reviews
                        .Where(r => r.Approvals.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Count() > 0 && r.CreatedByUserInDiv.ID == user.DivID)
                        .FirstOrDefault();
                    ViewBag.ApprovedReviewAgency = user.Div.CompositeDivName;
                    break;
                default:
                    break;
            }

            if (approvedReview != null)
            {
                viewModel.ApprovedReview = approvedReview;
                BillReviewApproval approval = approvedReview.Approvals
                    .Where(r => r.ApprovalLevel == userModuleApprovalLevel)
                    .FirstOrDefault();
                viewModel.ApprovedByUser = approval.ApprovedBy;
                viewModel.ApprovedAt = approval.ApprovedAt;
            }

            return View(viewModel);
        }

        // POST: BillReviews/Approve/5
        [Authorize(Roles = "BillsDept, BillsDiv")]
        [HttpPost, ActionName("Approve")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApproveConfirmed(int id)
        {
            BillReview review = await db.BillReviews
                                        .Where(r => r.ID == id)
                                        .Include(r => r.Bill)
                                        .Include(r => r.CreatedByUserInDept)
                                        .Include(r => r.CreatedByUserInDiv)
                                        .FirstOrDefaultAsync();
            if (review == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            // if the review had not been previously marked as read, mark it read
            var previousNotification = review.Notifications.Where(n => n.ApprovalLevel == userModuleApprovalLevel).FirstOrDefault();
            if (previousNotification != null)
            {
                previousNotification.IsRead = true;
            }

            // check if the Bill already had an approved Review.
            var approvedReview = new BillReview();
            switch (userModuleApprovalLevel)
            {
                case 1:
                    approvedReview = review.Bill.Reviews
                        .Where(r => r.Approvals.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Count() > 0 && r.CreatedByUserInDept.ID == user.DeptID)
                        .FirstOrDefault();
                    //ViewBag.ApprovedReviewAgency = user.Dept.CompositeDeptName;
                    break;
                case 2:
                    approvedReview = review.Bill.Reviews
                        .Where(r => r.Approvals.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Count() > 0 && r.CreatedByUserInDiv.ID == user.DivID)
                        .FirstOrDefault();
                    //ViewBag.ApprovedReviewAgency = user.Div.CompositeDivName;
                    break;
                default:
                    break;
            }

            // if an approved Review existed, remove the previous approval
            if (approvedReview != null)
            {
                BillReviewApproval approvalToRemove = approvedReview.Approvals.Where(a => a.ApprovalLevel == userModuleApprovalLevel).FirstOrDefault();
                db.BillReviewApprovals.Remove(approvalToRemove);
            }

            // create new approval for Review
            BillReviewApproval approval = new BillReviewApproval();
            approval.ApplicationUserID = user.Id;
            approval.BillReviewID = review.ID;
            approval.ApprovalLevel = userModuleApprovalLevel;
            approval.ApprovedAt = DateTime.Now;
            db.BillReviewApprovals.Add(approval);

            // check if this Review was created in response to a Request. If so, mark all Request from that user's Division as fulfilled 
            var requests = await db.BillReviewRequests.Where(r => r.BillID == review.BillID && r.RequestedToUser.DivID == user.DivID).ToListAsync();
            if (requests.Any())
            {
                foreach (BillReviewRequest r in requests)
                {
                    r.Fulfilled = true;
                    r.FulfilledAt = DateTime.Now;
                    r.BillReviewID = review.ID;
                }
            }

            // when a Div user approves a Review at the Div level, create a notification for the Dept level
            string body = ""; //initialize string for email body
            if (userModuleApprovalLevel == 2)
            {
                BillReviewNotification notification = new BillReviewNotification();
                notification.BillReviewID = review.ID;
                notification.ApprovalLevel = userModuleApprovalLevel - 1;
                notification.IsRead = false;
                db.BillReviewNotifications.Add(notification);

                // create email notification
                using (var sr = new StreamReader(Server.MapPath("\\Templates\\") + "BillReviewDivApprovalNotification.html"))
                {
                    body = await sr.ReadToEndAsync();
                }
                string messageSubject = string.Format("AdminApps Bill Tracking: A Review of {0} has been submitted on behalf of {1}", review.Bill.CompositeBillNumber, review.CreatedByUserInDiv.CompositeDivName);
                string domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                string viewReviewUrl = string.Format("{0}/BillReviews/Details/{1}", domain, review.ID);

                var targetUsers = GetUsersInRoleAndDept("BillsDept", review.DeptID);
                foreach (ApplicationUser u in targetUsers)
                {
                    string messageBody = string.Format(body, u.FirstName, review.Bill.calculatedHyperlink, review.Bill.CompositeBillNumber, review.CreatedByName, review.CreatedByUserInDiv.CompositeDivName, viewReviewUrl);
                    await UserManager.SendEmailAsync(u.Id, messageSubject, messageBody);
                }
            }

            // save changes
            await db.SaveChangesAsync();

            Success(string.Format("Bill Review successfully submitted to {0}", review.CreatedByUserInDept.CompositeDeptName));
            return RedirectToAction("Details", "Bills", new { id = approval.BillReview.BillID });

        }

        // GET: BillReviews/MarkAsRead/5
        [Authorize(Roles = "BillsDept, BillsDiv")]
        public async Task<ActionResult> MarkAsRead(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillReview review = await db.BillReviews.FindAsync(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // POST: BillReviews/MarkAsRead/5
        [Authorize(Roles = "BillsDept, BillsDiv")]
        [HttpPost, ActionName("MarkAsRead")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAsReadConfirmed(int id)
        {
            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            BillReviewNotification notification = await db.BillReviewNotifications.Where(n => n.BillReviewID == id && n.ApprovalLevel == userModuleApprovalLevel).SingleAsync();
            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;
            notification.ApplicationUserID = user.Id;

            db.Entry(notification).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Details", "Bills", new { id = notification.BillReview.BillID });

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // This function was intended to get Ann Wilkinson caught up on email notifications, which had not been implemented yet as of 3/18/15
        // public async Task<ActionResult> Catchup()
        // {
        //     var approvals = await db.BillReviewApprovals
        //                             .Where(a => a.ApprovalLevel == 2)
        //                             .Include("BillReview.Bill")
        //                             .Include("BillReview.CreatedByUserInDiv")
        //                             .ToListAsync();
        //     string body = "";
        //     using (var sr = new StreamReader(Server.MapPath("\\Templates\\") + "BillReviewDivApprovalNotification.html"))
        //     {
        //         body = await sr.ReadToEndAsync();
        //     }
        //     var targetUsers = GetUsersInRole("BillsDept");
        //     foreach (ApplicationUser u in targetUsers)
        //     {
        //         foreach (BillReviewApproval a in approvals)
        //         {
        //             string messageSubject = string.Format("AdminApps Bill Tracking: A Review of {0} has been submitted on behalf of {1}", a.BillReview.Bill.CompositeBillNumber, a.BillReview.CreatedByUserInDiv.CompositeDivName);
        //             string viewReviewUrl = string.Format("http://adminapps.nv.gov/BillReviews/Details/{0}", a.BillReview.ID);
        //             string messageBody = string.Format(body, u.FirstName, a.BillReview.Bill.NelisHyperlink, a.BillReview.Bill.CompositeBillNumber, a.BillReview.CreatedByName, a.BillReview.CreatedByUserInDiv.CompositeDivName, viewReviewUrl);
        //             await UserManager.SendEmailAsync(u.Id, messageSubject, messageBody);
        //         }
        //     }
        //     Success(string.Format("{0} email notifications sent", approvals.Count()));
        //     return RedirectToAction("Index");
        //}


    }
}
