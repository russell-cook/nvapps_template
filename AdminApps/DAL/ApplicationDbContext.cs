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
using NVApps.Areas.CIP.Models;

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
        public DbSet<Dept> Depts { get; set; }
        public DbSet<Div> Divs { get; set; }

        public DbSet<CIPApplication> CIPApplications { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // disable auto database generation of GlobalSettings ID value; this table should only have a single record that's initialized from the Seed() method (Migrations => Configuration)
            modelBuilder.Entity<AppGlobalSetting>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // configure user preferences for GlobalSettingsModels
            modelBuilder.Entity<ApplicationUser>().HasRequired(f => f.DefaultAppModule).WithMany(f => f.UsersWithDefaultAppModule).HasForeignKey(f => f.AppModuleID).WillCascadeOnDelete(false);
            modelBuilder.Entity<ApplicationRole>().HasRequired(f => f.AppModule).WithMany(f => f.ApplicationRoles).HasForeignKey(f => f.AppModuleID).WillCascadeOnDelete(false);

            // configure NebsModels:

            // disable auto database generation of ID values for NebsModels to allow for managed concurrency with remote NEBS database
            // specify (18, 0) decimal precision for compatibility with NEBS Oracle DB decimal keys
            modelBuilder.Entity<BudgetPeriod>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None).HasPrecision(18, 0);
            modelBuilder.Entity<Dept>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None).HasPrecision(18, 0);
            modelBuilder.Entity<Div>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None).HasPrecision(18, 0);

            // configure explicit foreign keys, disable automatic control of referential integrity (i.e. cascade deletes) to allow for managed concurrency with remote NEBS database
            modelBuilder.Entity<Dept>().HasRequired(f => f.BudgetPeriod).WithMany(f => f.Depts).HasForeignKey(f => f.BudgetPeriodID).WillCascadeOnDelete(false);
            modelBuilder.Entity<Div>().HasRequired(f => f.BudgetPeriod).WithMany(f => f.Divs).HasForeignKey(f => f.BudgetPeriodID).WillCascadeOnDelete(false);
            modelBuilder.Entity<Div>().HasOptional(f => f.Dept).WithMany(f => f.Divs).HasForeignKey(f => f.DeptID).WillCascadeOnDelete(false);


        }


    }
}