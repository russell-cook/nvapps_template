using NVApps.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace NVApps.DAL
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("NVApps_SQLServerExpress", throwIfV1Schema: false)
        {
        }

        // The ApplicationDbInitializer is currently disabled to preven the database from being dropped unintentionally.
        // Seed data for Identiy initialization has been relocated to the Migrations => Configuration file.

        //static ApplicationDbContext()
        //{
        //    // Set the database intializer which is run once during application start
        //    // This seeds the database with admin user credentials and admin role
        //    Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
        //}

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        // configure global models
        public DbSet<AppGlobalSetting> AppGlobalSettings { get; set; }
        public DbSet<AppModule> AppModules { get; set; }

        // configure local versions of NebsModels
        public DbSet<BudgetPeriod> BudgetPeriods { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // disable auto database generation of GlobalSettings ID value; this table should only have a single record that's initialized from the Seed() method (Migrations => Configuration)
            modelBuilder.Entity<AppGlobalSetting>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // configure user preferences for GlobalSettingsModels
            modelBuilder.Entity<ApplicationUser>().HasRequired(f => f.DefaultAppModule).WithMany(f => f.UsersWithDefaultAppModule).HasForeignKey(f => f.AppModuleID).WillCascadeOnDelete(false);
            modelBuilder.Entity<ApplicationRole>().HasRequired(f => f.AppModule).WithMany(f => f.ApplicationRoles).HasForeignKey(f => f.AppModuleID).WillCascadeOnDelete(false);


        }


    }
}