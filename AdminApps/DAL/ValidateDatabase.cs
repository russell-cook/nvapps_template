using System;
using System.Configuration;
using System.Data.Entity;

namespace NVApps.DAL
{

    // This class is meant to prevent the entity framework from issuing "create" commands to external databases (i.e. the NEBS Oracle db, etc.).
    // The class is called from the respective DbContext constructor (i.e. NEBS_Context). Code was derived from: http://coding.abel.nu/2012/03/prevent-ef-migrations-from-creating-or-changing-the-database/

    public class ValidateDatabase<TContext> : IDatabaseInitializer<TContext>
      where TContext : DbContext
        {
            public void InitializeDatabase(TContext context)
            {
                if (!context.Database.Exists())
                {
                    throw new ConfigurationErrorsException(
                      "Database does not exist");
                }                
                // 2014-11-14: This logic will throw an exception when run on external databases without EF migration metadata, therefore it's commented out for now 
                // Without this logic the function simply validates the existence of the target database

                //else
                //{
                //    if (!context.Database.CompatibleWithModel(true))
                //    {
                //        throw new InvalidOperationException("The database is not compatible with the entity model.");
                //    }
                //}

            }
        }
}