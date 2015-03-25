using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdminApps.Models
{
    public class BudgetPeriod
    {
        public decimal ID { get; set; }
        public string Description { get; set; }
        public string ActualYear { get; set; }
        public string WorkProgramYear { get; set; }
        public string BeginYear { get; set; }
        public string EndYear { get; set; }

        public virtual ICollection<Bill> Bills { get; set; }
        public virtual ICollection<Dept> Depts { get; set; }
        public virtual ICollection<Div> Divs { get; set; }
    }

    // 2014-11-19: Temporary model--need access to NEBS.BUDGET_SESSION table
    public class BudgetSession
    {
        public decimal ID { get; set; }
        public string Title { get; set; }
    }

    public class Dept
    {
        public decimal ID { get; set; }
        public decimal BudgetPeriodID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

        // Bill Tracking module navigation properties
        public virtual ICollection<Bill> Bills { get; set; }
        public virtual BudgetPeriod BudgetPeriod { get; set; }
        public virtual ICollection<Div> Divs { get; set; }
        public virtual ICollection<BillsAlsrReport> BillsAlsrReports { get; set; }

        // Identity navigation properties
        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }

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

        public virtual ICollection<Bill> Bills { get; set; }
        public virtual BudgetPeriod BudgetPeriod { get; set; }
        public virtual Dept Dept { get; set; }
        public virtual ICollection<BillsAlsrReport> BillsAlsrReports { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }

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

    public class NebsBudgetPeriod
    {
        public decimal ID { get; set; }
        public string Description { get; set; }
        public string ActualYear { get; set; }
        public string WorkProgramYear { get; set; }
        public string BeginYear { get; set; }
        public string EndYear { get; set; }
    }

    // 2014-11-19: Temporary model--need access to NEBS.BUDGET_SESSION table
    public class NebsBudgetSession
    {
        public decimal ID { get; set; }
        public string Title { get; set; }
    }

    public class NebsDept
    {
        public decimal ID { get; set; }
        public decimal NebsBudgetPeriodID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class NebsDiv
    {
        public decimal ID { get; set; }
        public decimal NebsBudgetPeriodID { get; set; }
        public decimal? NebsDeptID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal SortOrder { get; set; }
    }

}