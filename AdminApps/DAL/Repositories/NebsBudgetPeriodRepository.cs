using Newtonsoft.Json;
using NVApps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace NVApps.DAL.Repositories
{
    public interface INebsBudgetPeriodRepository : IDisposable
    {
        // remote calls to consume AdminApps API
        Task<IEnumerable<BudgetPeriod>> GetAllNebsBudgetPeriodsAsync();
    }

    public class NebsBudgetPeriodRepository : NebsRepositoryBase, INebsBudgetPeriodRepository
    {
        public async Task<IEnumerable<BudgetPeriod>> GetAllNebsBudgetPeriodsAsync()
        {
            HttpResponseMessage response = await client.GetAsync("BudgetPeriods");
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<BudgetPeriod>>(data.Result);
            }
            else
            {
                return null;
            }
        }

    }
}