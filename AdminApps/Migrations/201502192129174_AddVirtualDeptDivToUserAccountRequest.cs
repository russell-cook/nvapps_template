namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVirtualDeptDivToUserAccountRequest : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserAccountRequests", "DivID", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.UserAccountRequests", "DeptID", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            CreateIndex("dbo.UserAccountRequests", "DivID");
            CreateIndex("dbo.UserAccountRequests", "DeptID");
            AddForeignKey("dbo.UserAccountRequests", "DeptID", "dbo.Depts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.UserAccountRequests", "DivID", "dbo.Divs", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserAccountRequests", "DivID", "dbo.Divs");
            DropForeignKey("dbo.UserAccountRequests", "DeptID", "dbo.Depts");
            DropIndex("dbo.UserAccountRequests", new[] { "DeptID" });
            DropIndex("dbo.UserAccountRequests", new[] { "DivID" });
            AlterColumn("dbo.UserAccountRequests", "DeptID", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.UserAccountRequests", "DivID", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
