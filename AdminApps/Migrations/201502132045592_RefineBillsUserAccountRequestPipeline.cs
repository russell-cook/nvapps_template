namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefineBillsUserAccountRequestPipeline : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAccountRequests", "FirstName", c => c.String(nullable: false));
            AddColumn("dbo.UserAccountRequests", "Email", c => c.String(nullable: false));
            AddColumn("dbo.UserAccountRequests", "RequestedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.UserAccountRequests", "Fulfilled", c => c.Boolean(nullable: false));
            AlterColumn("dbo.UserAccountRequests", "LastName", c => c.String(nullable: false));
            AlterColumn("dbo.UserAccountRequests", "Title", c => c.String(nullable: false));
            CreateIndex("dbo.UserAccountRequests", "Fulfilled");
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserAccountRequests", new[] { "Fulfilled" });
            AlterColumn("dbo.UserAccountRequests", "Title", c => c.String());
            AlterColumn("dbo.UserAccountRequests", "LastName", c => c.String());
            DropColumn("dbo.UserAccountRequests", "Fulfilled");
            DropColumn("dbo.UserAccountRequests", "RequestedAt");
            DropColumn("dbo.UserAccountRequests", "Email");
            DropColumn("dbo.UserAccountRequests", "FirstName");
        }
    }
}
