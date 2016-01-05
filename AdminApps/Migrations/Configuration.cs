namespace NVApps.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using NVApps.DAL;
    using NVApps.Models;
    using Omu.ValueInjecter;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Web;


    internal sealed class Configuration : DbMigrationsConfiguration<NVApps.DAL.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }


        protected override void Seed(ApplicationDbContext context)
        {
            // 2014-11-19: Initialize NEBS testing values, starting at the top of the dependency hierarchy. These values are meant for development purposes; 
            // they will be replaced by populated values from NEBS DB using functions in NebsConcurrencyController.
            context.BudgetPeriods.AddOrUpdate(i => i.ID,
                    new BudgetPeriod { ID = 7, Description = "2015-2017 Biennium (FY16-17)", ActualYear = "2014", WorkProgramYear = "2015", BeginYear = "2016", EndYear = "2017" },
                    new BudgetPeriod { ID = 8, Description = "2017-2019 Biennium (FY18-19)", ActualYear = "2016", WorkProgramYear = "2017", BeginYear = "2018", EndYear = "2019" }
                );

            context.Depts.AddOrUpdate(i => i.ID,
                    new Dept { ID = 212516, BudgetPeriodID = 7, Code = "08", Description = "DEPARTMENT OF ADMINISTRATION" }
                );

            context.Divs.AddOrUpdate(i => i.ID,
                    new Div { ID = 212568, BudgetPeriodID = 7, DeptID = 212516, Code = "080", Description = "ADMIN - DIRECTOR'S OFFICE", SortOrder = 0 }
                );

            // Initialize AppGlobalSettings values. There should never be more than a single record in this table.
            // In the production app this can be configured via the GlobalSettingsController.
            context.AppGlobalSettings.AddOrUpdate(i => i.ID,
                    new AppGlobalSetting { ID = 1, BudgetPeriodID = 8 }
                );

            // Initialize list of AppModules
            context.AppModules.AddOrUpdate(i => i.ID,
                    new AppModule { ID = 1, Title = "Manage User Account", DefaultController = "Manage", DefaultAction = "Index" },
                    new AppModule { ID = 2, Title = "Global App Settings", DefaultController = "GlobalSettings", DefaultAction = "Index" },
                    new AppModule { ID = 3, Title = "Role Administration", DefaultController = "RolesAdmin", DefaultAction = "Index" },
                    new AppModule { ID = 4, Title = "User Administration", DefaultController = "UsersAdmin", DefaultAction = "Index" },
                    new AppModule { ID = 5, Title = "CIP Application", DefaultController = "CIP/Home", DefaultAction = "Index" }
                );

            // Initialize Identity data. This routine has been adapted from ApplicationDbInitializer; it was relocated here so that it would run as part of the update-database migration command.

            // These variables use the extensible ApplicationUser class and the custom/extensible ApplicationRole class per the tutorial at: http://typecastexception.com/post/2014/06/22/ASPNET-Identity-20-Customizing-Users-and-Roles.aspx
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(userStore);
            var roleStore = new RoleStore<ApplicationRole>(context);
            var roleManager = new ApplicationRoleManager(roleStore);

            // Initialize ApplicationRoles in db--these roles are used to control access to AdminApp's various modules via the [Authorize] attribute in the module's controller, as well as for showing/hiding access to these modules in the UI via conditional logic in the "Shared => _Layout.cshtml" View: "@if (Request.IsAuthenticated && User.IsInRole("RoleName"))"
            List<ApplicationRole> initRoles = new List<ApplicationRole> {
                // first role in list is default Administration Role for default user, used by function below when selecting initRoles[0].Name
                new ApplicationRole(){Name = "GlobalAdmin", AppModuleID = 2, AppModuleApprovalLevel = 1, AppModuleApprovalTitle = "GlobalAdmin", Description = "Highest level of administration; allows configuration of Global App Settings" },
                new ApplicationRole(){Name = "RolesAdmin",  AppModuleID = 3, AppModuleApprovalLevel = 1, AppModuleApprovalTitle = "RolesAdmin", Description = "Allows for User/Role administration" },
                new ApplicationRole(){Name = "UsersAdmin",  AppModuleID = 4, AppModuleApprovalLevel = 1, AppModuleApprovalTitle = "UsersAdmin", Description = "Allows for User administration, but not Role administration"},
                new ApplicationRole(){Name = "CIPAdmin",  AppModuleID = 5, AppModuleApprovalLevel = 1, AppModuleApprovalTitle = "CIPAdmin", Description = "Allows for CIP Application administration"},
                new ApplicationRole(){Name = "CIPUser",  AppModuleID = 5, AppModuleApprovalLevel = 1, AppModuleApprovalTitle = "CIPUser", Description = "End-user role for CIP; allows for submission of CIP Application"}
            };

            //Create ApplicationRole for each roleNames if it does not exist
            foreach (ApplicationRole initRole in initRoles)
            {
                var role = roleManager.FindByName(initRole.Name);
                if (role == null)
                {
                    var roleresult = roleManager.Create(initRole);
                }
            }

            // set username and password for default admin user
            const string name = "admin@nvapps.nv.gov";
            const string password = "Admin@1234";

            // Create default admin@example.com user if it does not exist
            var user = userManager.FindByName(name);
            if (user == null)
            {
                user = new ApplicationUser { UserName = name, Email = name, FirstName = "Russell", LastName = "Cook", AppModuleID = 1, DeptID = 212516, DivID = 212568, IsActive = true, AutoPwdReplaced = true };
                var result = userManager.Create(user, password);
                result = userManager.SetLockoutEnabled(user.Id, false);
            }

            // Add user admin to default Administration Role if not already added.
            var rolesForUser = userManager.GetRoles(user.Id);
            if (!rolesForUser.Contains(initRoles[0].Name))
            {
                // adds default admin user to first role in list above
                var result = userManager.AddToRole(user.Id, initRoles[0].Name);
            }

        }
    }
}
