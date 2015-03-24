namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotesAndGovOfficeDeliveryDateToBillsAlsrReport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BillsAlsrReports", "GovOfficeDeliveryDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.BillsAlsrReports", "Notes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BillsAlsrReports", "Notes");
            DropColumn("dbo.BillsAlsrReports", "GovOfficeDeliveryDate");
        }
    }
}
