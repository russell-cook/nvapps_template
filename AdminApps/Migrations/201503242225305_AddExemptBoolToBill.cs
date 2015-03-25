namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExemptBoolToBill : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bills", "Exempt", c => c.Boolean(nullable: false));
            AlterColumn("dbo.BillsAlsrReports", "DeptID", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.BillsAlsrReports", "DivID", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            CreateIndex("dbo.BillsAlsrReports", "DeptID");
            CreateIndex("dbo.BillsAlsrReports", "DivID");
            AddForeignKey("dbo.BillsAlsrReports", "DeptID", "dbo.Depts", "ID");
            AddForeignKey("dbo.BillsAlsrReports", "DivID", "dbo.Divs", "ID");
            DropColumn("dbo.Bills", "NelisHyperlink");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Bills", "NelisHyperlink", c => c.String());
            DropForeignKey("dbo.BillsAlsrReports", "DivID", "dbo.Divs");
            DropForeignKey("dbo.BillsAlsrReports", "DeptID", "dbo.Depts");
            DropIndex("dbo.BillsAlsrReports", new[] { "DivID" });
            DropIndex("dbo.BillsAlsrReports", new[] { "DeptID" });
            AlterColumn("dbo.BillsAlsrReports", "DivID", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.BillsAlsrReports", "DeptID", c => c.Decimal(precision: 18, scale: 2));
            DropColumn("dbo.Bills", "Exempt");
        }
    }
}
