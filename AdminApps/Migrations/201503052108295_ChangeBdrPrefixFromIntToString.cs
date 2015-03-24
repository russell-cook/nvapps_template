namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeBdrPrefixFromIntToString : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Bills", "BdrPrefix", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Bills", "BdrPrefix", c => c.Int());
        }
    }
}
