namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBdrPrefixAndSuffixToBillRecordRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BillRecordRequests", "BdrPrefix", c => c.String());
            AddColumn("dbo.BillRecordRequests", "BdrSuffix", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BillRecordRequests", "BdrSuffix");
            DropColumn("dbo.BillRecordRequests", "BdrPrefix");
        }
    }
}
