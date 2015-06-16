using AdminApps.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AdminApps.DAL
{
    public class BillsRepository
    {
    }

    public class BillReviewsRepository
    {
        public async Task<int> CountUnreadAsync(ApplicationUser user, int userModuleApprovalLevel)
        {
            using(ApplicationDbContext db = new ApplicationDbContext())
            {
                // load all unread BillReviews from current BudgetPeriod for user's Dept/Div (depending on role)
                // this list of BillReviews will be filtered below to exclude records that have been superseded by a more recent review
                var appGlobalBudgetPeriodId = (await db.AppGlobalSettings.FirstOrDefaultAsync()).BudgetPeriodID;
                var unreadBillReviews = new List<BillReview>();
                switch (userModuleApprovalLevel)
                {
                    case 0:
                        return 0;
                    case 1:
                        unreadBillReviews = await db.BillReviews
                            .Where(r => r.Bill.BudgetPeriodID == appGlobalBudgetPeriodId
                                && r.CreatedByUserInDept.ID == user.DeptID
                                && r.Notifications.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Any()
                                && r.Notifications.Where(a => a.ApprovalLevel == userModuleApprovalLevel).FirstOrDefault().IsRead == false)
                            .ToListAsync();
                        break;
                    case 2:
                        unreadBillReviews = await db.BillReviews
                            .Where(r => r.Bill.BudgetPeriodID == appGlobalBudgetPeriodId
                                && r.CreatedByUserInDiv.ID == user.DivID
                                && r.Notifications.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Any()
                                && r.Notifications.Where(a => a.ApprovalLevel == userModuleApprovalLevel).FirstOrDefault().IsRead == false)
                            .ToListAsync();
                        break;
                    case 3:
                        return 0;
                    default:
                        break;
                }

                // remove any Bill Reviews which aren't the most recent version
                var billReviewsGrouping = unreadBillReviews.GroupBy(r => r.BillID).ToList();
                var billReviewsList = new List<BillReview>();
                foreach (IGrouping<int, BillReview> group in billReviewsGrouping)
                {
                    var mostRecentUnreadReview = group.OrderByDescending(g => g.BillVersion.VersionNum).FirstOrDefault();
                    var mostRecentReviewID = (await db.BillReviews.Where(r => r.BillID == mostRecentUnreadReview.BillID && r.ApplicationUserID == mostRecentUnreadReview.ApplicationUserID).OrderByDescending(r => r.BillVersion.VersionNum).FirstOrDefaultAsync()).ID;
                    if (mostRecentReviewID == mostRecentUnreadReview.ID)
                    {
                        billReviewsList.Add(mostRecentUnreadReview);
                    }
                }
            return billReviewsList.Count;
            }
        }

    }
}