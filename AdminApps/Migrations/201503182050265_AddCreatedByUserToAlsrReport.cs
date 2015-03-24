namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCreatedByUserToAlsrReport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BillsAlsrReports", "ApplicationUserID", c => c.String(maxLength: 128));
            CreateIndex("dbo.BillsAlsrReports", "ApplicationUserID");
            AddForeignKey("dbo.BillsAlsrReports", "ApplicationUserID", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BillsAlsrReports", "ApplicationUserID", "dbo.AspNetUsers");
            DropIndex("dbo.BillsAlsrReports", new[] { "ApplicationUserID" });
            DropColumn("dbo.BillsAlsrReports", "ApplicationUserID");
        }
    }
}
