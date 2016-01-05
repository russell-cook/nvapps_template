using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        // navigation properties
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

        // navigation properties
        public virtual ICollection<Dept> Depts { get; set; }
        public virtual ICollection<Div> Divs { get; set; }
    }

    public class Dept
    {
        public decimal ID { get; set; }
        public decimal BudgetPeriodID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

        // navigation properties
        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }
        public virtual ICollection<Div> Divs { get; set; }
        public virtual BudgetPeriod BudgetPeriod { get; set; }

        // calculated properties
        [Display(Name = "Dept")]
        public string CompositeDeptName
        {
            get
            {
                if (Code != null && Description != null)
                {
                    return Code + " - " + Description;
                }
                else
                {
                    return null;
                }
            }
        }
    }

    public class Div
    {
        public decimal ID { get; set; }
        public decimal BudgetPeriodID { get; set; }
        public decimal? DeptID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal SortOrder { get; set; }

        // navigation properties
        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }
        public virtual Dept Dept { get; set; }
        public virtual BudgetPeriod BudgetPeriod { get; set; }

        // calculated properties
        [Display(Name = "Div")]
        public string CompositeDivName
        {
            get
            {
                if (Code != null && Description != null)
                {
                    return Code + " - " + Description;
                }
                else
                {
                    return null;
                }
            }
        }

    }

}