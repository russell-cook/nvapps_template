namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOpenToGanttTask : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GanttTasks", "Open", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GanttTasks", "Open");
        }
    }
}
