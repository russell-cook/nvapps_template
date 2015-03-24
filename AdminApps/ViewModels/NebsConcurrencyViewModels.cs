using AdminApps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminApps.ViewModels
{
    public class NebsConcurrencyIndexViewModel
    {
        public ICollection<BudgetPeriod> LocalBudgetPeriods { get; set; }
        public ICollection<NebsBudgetPeriod> NebsBudgetPeriods { get; set; }
        public ICollection<Div> LocalDivs { get; set; }
        public ICollection<NebsDiv> NebsDivs { get; set; }
        public ICollection<Dept> LocalDepts { get; set; }
        public ICollection<NebsDept> NebsDepts { get; set; }
    }
}