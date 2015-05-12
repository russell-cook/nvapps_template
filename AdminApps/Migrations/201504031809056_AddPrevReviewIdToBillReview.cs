namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPrevReviewIdToBillReview : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BillReviews", "PrevReviewID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BillReviews", "PrevReviewID");
        }
    }
}
