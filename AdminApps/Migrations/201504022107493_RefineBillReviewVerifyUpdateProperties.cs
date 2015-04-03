namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefineBillReviewVerifyUpdateProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BillReviews", "IsVerifiedDupOfPrevReview", c => c.Boolean(nullable: false));
            DropColumn("dbo.BillReviews", "IsUnrevisedDupOfPrevReview");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BillReviews", "IsUnrevisedDupOfPrevReview", c => c.Boolean(nullable: false));
            DropColumn("dbo.BillReviews", "IsVerifiedDupOfPrevReview");
        }
    }
}
