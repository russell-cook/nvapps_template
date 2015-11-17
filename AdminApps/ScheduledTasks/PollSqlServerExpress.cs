using NVApps.DAL;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

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