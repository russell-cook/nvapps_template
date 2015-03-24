namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSponsorToBillVersion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BillVersions", "Sponsor", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BillVersions", "Sponsor");
        }
    }
}
