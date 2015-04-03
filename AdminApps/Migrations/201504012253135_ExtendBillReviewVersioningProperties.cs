namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendBillReviewVersioningProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BillReviews", "IsOverrideRevision", c => c.Boolean(nullable: false));
            AddColumn("dbo.BillReviews", "OverrideRevisionCreatedByUserID", c => c.String(maxLength: 128));
            AddColumn("dbo.BillReviews", "OverrideOfBillReviewID", c => c.Int(nullable: false));
            CreateIndex("dbo.BillReviews", "OverrideRevisionCreatedByUserID");
            AddForeignKey("dbo.BillReviews", "OverrideRevisionCreatedByUserID", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BillReviews", "OverrideRevisionCreatedByUserID", "dbo.AspNetUsers");
            DropIndex("dbo.BillReviews", new[] { "OverrideRevisionCreatedByUserID" });
            DropColumn("dbo.BillReviews", "OverrideOfBillReviewID");
            DropColumn("dbo.BillReviews", "OverrideRevisionCreatedByUserID");
            DropColumn("dbo.BillReviews", "IsOverrideRevision");
        }
    }
}
