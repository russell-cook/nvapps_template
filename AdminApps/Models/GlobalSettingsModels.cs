using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NVApps.Models
{
    public class AppGlobalSetting
    {
        public int ID { get; set; }
        public decimal BudgetPeriodID { get; set; }
    }

    public class AppModule
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string DefaultController { get; set; }
        public string DefaultAction { get; set; }

        public virtual ICollection<ApplicationRole> ApplicationRoles { get; set; }
        public virtual ICollection<ApplicationUser> UsersWithDefaultAppModule { get; set; }
    }

    public class BudgetPeriod
    {
        public decimal ID { get; set; }
        public string Description { get; set; }
        public string ActualYear { get; set; }
        public string WorkProgramYear { get; set; }
        public string BeginYear { get; set; }
        public string EndYear { get; set; }
    }


}