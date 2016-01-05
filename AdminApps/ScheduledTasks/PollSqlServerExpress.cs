using NVApps.DAL;
using Quartz;
using System.Data.Entity;

namespace NVApps.ScheduledTasks
{
    public class PollSqlServerExpress : IJob
    {
        public async void Execute(IJobExecutionContext context)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var appGlobalBudgetPeriod = (await db.AppGlobalSettings.FirstOrDefaultAsync()).BudgetPeriodID;
            }
        }
    }
}