namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRowVersionToBillReview : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BillReviews", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            AddColumn("dbo.BillReviews", "CapturedFromRowVersion", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BillReviews", "CapturedFromRowVersion");
            DropColumn("dbo.BillReviews", "RowVersion");
        }
    }
}
