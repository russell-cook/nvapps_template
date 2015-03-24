using AdminApps.DAL;
using AdminApps.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AdminApps.Controllers
{
    public class TrafficController : BaseController
    {
        public async Task<ActionResult> DefaultModule()
        {
            if (User.Identity.IsAuthenticated)
            {
                ApplicationUser user = await ReturnCurrentUserAsync();
                if (user.AutoPwdReplaced)
                {
                    return RedirectToAction(user.DefaultAppModule.DefaultAction, user.DefaultAppModule.DefaultController);
                }
                else
                {
                    return RedirectToAction("ChangePassword", "Manage", new { replaceAutoPwd = true });
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

    }
}