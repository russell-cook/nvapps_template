namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSavedAtDateAndCommentsToProjectScheduleVersion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectScheduleVersions", "Comments", c => c.String());
            AddColumn("dbo.ProjectScheduleVersions", "SavedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProjectScheduleVersions", "SavedAt");
            DropColumn("dbo.ProjectScheduleVersions", "Comments");
        }
    }
}
