namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendBillsAlsrReportProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BillReviews", "BillsAlsrReportID", c => c.Int());
            AddColumn("dbo.BillReviews", "Timestamp", c => c.DateTime());
            AddColumn("dbo.BillReviews", "CapturedFromBillReviewID", c => c.Int());
            AddColumn("dbo.BillReviews", "SupercedesPreviousSnapshotID", c => c.Int());
            AddColumn("dbo.BillReviews", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.BillsAlsrReports", "BudgetPeriodID", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.BillsAlsrReports", "BudgetSessionID", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.BillsAlsrReports", "DeptID", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.BillsAlsrReports", "DivID", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.BillsAlsrReports", "Filename", c => c.String());
            CreateIndex("dbo.BillReviews", "BillsAlsrReportID");
            AddForeignKey("dbo.BillReviews", "BillsAlsrReportID", "dbo.BillsAlsrReports", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BillReviews", "BillsAlsrReportID", "dbo.BillsAlsrReports");
            DropIndex("dbo.BillReviews", new[] { "BillsAlsrReportID" });
            DropColumn("dbo.BillsAlsrReports", "Filename");
            DropColumn("dbo.BillsAlsrReports", "DivID");
            DropColumn("dbo.BillsAlsrReports", "DeptID");
            DropColumn("dbo.BillsAlsrReports", "BudgetSessionID");
            DropColumn("dbo.BillsAlsrReports", "BudgetPeriodID");
            DropColumn("dbo.BillReviews", "Discriminator");
            DropColumn("dbo.BillReviews", "SupercedesPreviousSnapshotID");
            DropColumn("dbo.BillReviews", "CapturedFromBillReviewID");
            DropColumn("dbo.BillReviews", "Timestamp");
            DropColumn("dbo.BillReviews", "BillsAlsrReportID");
        }
    }
}
