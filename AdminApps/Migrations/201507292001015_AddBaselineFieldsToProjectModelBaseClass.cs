namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBaselineFieldsToProjectModelBaseClass : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GanttTasks", "PlannedStartDate", c => c.DateTime());
            AddColumn("dbo.GanttTasks", "PlannedEndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.GanttTasks", "PlannedEndDate");
            DropColumn("dbo.GanttTasks", "PlannedStartDate");
        }
    }
}
