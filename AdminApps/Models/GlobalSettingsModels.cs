using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminApps.Models
{
    public class AppGlobalSetting
    {
        public int ID { get; set; }
        public decimal BudgetPeriodID { get; set; }
        public decimal BudgetSessionID { get; set; }

        public virtual BudgetPeriod BudgetPeriod { get; set; }
        public virtual BudgetSession BudgetSession { get; set; }
    }

    public  class AppModule
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string DefaultController { get; set; }
        public string DefaultAction { get; set; }

        public virtual ICollection<ApplicationRole> ApplicationRoles { get; set; }
        public virtual ICollection<ApplicationUser> UsersWithDefaultAppModule { get; set; }
    }
}