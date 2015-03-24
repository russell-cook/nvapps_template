// This initializer is disabled to prevent the ApplicationDbContext database from being dropped unintentionally.
// The initializer was originally called from the ApplicationDbContext, but is currently commented out.

using AdminApps.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web;


namespace AdminApps.DAL
{
    // This is useful if you do not want to tear down the database each time you run the application.
    // public class ApplicationDbInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    // This example shows you how to create a new database if the Model changes
    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            InitializeIdentityForEF(context);
            base.Seed(context);
        }

        //Create User=Admin@Admin.com with password=Admin@123456 in the Admin role        
        public static void InitializeIdentityForEF(ApplicationDbContext db)
        {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            const string name = "admin@example.com";
            const string password = "Admin@123456";
            // Initialize ApplicationRoles in db--these roles are used to control access to AdminApp's various modules via the [Authorize] attribute in the module's controller, as well as for showing/hiding access to these modules in the UI via conditional logic in the "Shared => _Layout.cshtml" View: "@if (Request.IsAuthenticated && User.IsInRole("RoleName"))"
            List<ApplicationRole> initRoles = new List<ApplicationRole> {
                // first role in list is default Administration Role for default user, per function below
                new ApplicationRole(){Name = "IdentityManager", Description = "Highest level of administration for User/Role management" },
                new ApplicationRole(){Name = "UserManager", Description = "Allows for User administration, but not Role administration"},
                new ApplicationRole(){Name = "BillsDept", Description = "Bill Tracking module: Department-level access, including all commentary from Divisions within that Department"},
                new ApplicationRole(){Name = "BillsDiv", Description = "Bill Tracking module: Division-level access"}
            };

            //Create ApplicationRole for each roleNames if it does not exist
            foreach (ApplicationRole initRole in initRoles)
            {
                var role = roleManager.FindByName(initRole.Name);
                if (role == null)
                {
                    // Implemented extensible ApplicationRole per http://typecastexception.com/post/2014/06/22/ASPNET-Identity-20-Customizing-Users-and-Roles.aspx
                    // role = new ApplicationRole(initRole);
                    var roleresult = roleManager.Create(initRole);
                }
            }

            // Create default admin@example.com user if it does not exist
            var user = userManager.FindByName(name);
            if (user == null)
            {
                user = new ApplicationUser { UserName = name, Email = name };
                var result = userManager.Create(user, password);
                result = userManager.SetLockoutEnabled(user.Id, false);
            }

            // Add user admin to default Administration Role if not already added.
            var rolesForUser = userManager.GetRoles(user.Id);
            if (!rolesForUser.Contains(initRoles[0].Name))
            {
                var result = userManager.AddToRole(user.Id, initRoles[0].Name);
            }
        }
    }

}