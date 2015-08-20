namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProjectScheduleVersion : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GanttLinks", "ProjectID", "dbo.Projects");
            DropForeignKey("dbo.GanttTasks", "ProjectID", "dbo.Projects");
            CreateTable(
                "dbo.ProjectScheduleVersions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        VersionNum = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Projects", t => t.ProjectID, cascadeDelete: true)
                .Index(t => t.ProjectID);
            
            CreateTable(
                "dbo.ProjectStatus",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.Projects", "ProjectStatusID", c => c.Int(nullable: false));
            AddColumn("dbo.GanttLinks", "ProjectScheduleVersionID", c => c.Int(nullable: false));
            AddColumn("dbo.GanttTasks", "ProjectScheduleVersionID", c => c.Int(nullable: false));
            CreateIndex("dbo.Projects", "ProjectStatusID");
            CreateIndex("dbo.GanttLinks", "ProjectScheduleVersionID");
            CreateIndex("dbo.GanttTasks", "ProjectScheduleVersionID");
            AddForeignKey("dbo.GanttLinks", "ProjectScheduleVersionID", "dbo.ProjectScheduleVersions", "ID", cascadeDelete: true);
            AddForeignKey("dbo.GanttTasks", "ProjectScheduleVersionID", "dbo.ProjectScheduleVersions", "ID", cascadeDelete: true);
            AddForeignKey("dbo.Projects", "ProjectStatusID", "dbo.ProjectStatus", "ID", cascadeDelete: true);
            AddForeignKey("dbo.GanttLinks", "ProjectID", "dbo.Projects", "ID");
            AddForeignKey("dbo.GanttTasks", "ProjectID", "dbo.Projects", "ID");
            DropColumn("dbo.Projects", "ProjectStatus");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Projects", "ProjectStatus", c => c.Int(nullable: false));
            DropForeignKey("dbo.GanttTasks", "ProjectID", "dbo.Projects");
            DropForeignKey("dbo.GanttLinks", "ProjectID", "dbo.Projects");
            DropForeignKey("dbo.Projects", "ProjectStatusID", "dbo.ProjectStatus");
            DropForeignKey("dbo.ProjectScheduleVersions", "ProjectID", "dbo.Projects");
            DropForeignKey("dbo.GanttTasks", "ProjectScheduleVersionID", "dbo.ProjectScheduleVersions");
            DropForeignKey("dbo.GanttLinks", "ProjectScheduleVersionID", "dbo.ProjectScheduleVersions");
            DropIndex("dbo.ProjectScheduleVersions", new[] { "ProjectID" });
            DropIndex("dbo.GanttTasks", new[] { "ProjectScheduleVersionID" });
            DropIndex("dbo.GanttLinks", new[] { "ProjectScheduleVersionID" });
            DropIndex("dbo.Projects", new[] { "ProjectStatusID" });
            DropColumn("dbo.GanttTasks", "ProjectScheduleVersionID");
            DropColumn("dbo.GanttLinks", "ProjectScheduleVersionID");
            DropColumn("dbo.Projects", "ProjectStatusID");
            DropTable("dbo.ProjectStatus");
            DropTable("dbo.ProjectScheduleVersions");
            AddForeignKey("dbo.GanttTasks", "ProjectID", "dbo.Projects", "ID", cascadeDelete: true);
            AddForeignKey("dbo.GanttLinks", "ProjectID", "dbo.Projects", "ID", cascadeDelete: true);
        }
    }
}
