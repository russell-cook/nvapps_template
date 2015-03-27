namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRevisionStatusBoolsToBillReview : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BillReviews", "IsUnrevisedDupOfPrevReview", c => c.Boolean(nullable: false));
            AddColumn("dbo.BillReviews", "IsRevisionOfPrevReview", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BillReviews", "IsRevisionOfPrevReview");
            DropColumn("dbo.BillReviews", "IsUnrevisedDupOfPrevReview");
        }
    }
}
