namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGanttModelsForProjMgmtDemo : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.UserProjects", newName: "Projects");
            CreateTable(
                "dbo.GanttLinks",
                c => new
                    {
                        GanttLinkId = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        Type = c.String(maxLength: 1),
                        SourceTaskId = c.Int(nullable: false),
                        TargetTaskId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.GanttLinkId)
                .ForeignKey("dbo.Projects", t => t.ProjectID, cascadeDelete: true)
                .Index(t => t.ProjectID);
            
            CreateTable(
                "dbo.GanttTasks",
                c => new
                    {
                        GanttTaskId = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        Text = c.String(maxLength: 255),
                        StartDate = c.DateTime(nullable: false),
                        Duration = c.Int(nullable: false),
                        Progress = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SortOrder = c.Int(nullable: false),
                        Type = c.String(),
                        ParentId = c.Int(),
                    })
                .PrimaryKey(t => t.GanttTaskId)
                .ForeignKey("dbo.Projects", t => t.ProjectID, cascadeDelete: true)
                .Index(t => t.ProjectID);
            
            AddColumn("dbo.Projects", "ProjectStatus", c => c.Int(nullable: false));
            AddColumn("dbo.Projects", "Goals", c => c.String());
            AddColumn("dbo.Projects", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.Projects", "ProjectStatusID");
            DropColumn("dbo.Projects", "ProjectStatus_Value");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Projects", "ProjectStatus_Value", c => c.String());
            AddColumn("dbo.Projects", "ProjectStatusID", c => c.Int(nullable: false));
            DropForeignKey("dbo.GanttTasks", "ProjectID", "dbo.Projects");
            DropForeignKey("dbo.GanttLinks", "ProjectID", "dbo.Projects");
            DropIndex("dbo.GanttTasks", new[] { "ProjectID" });
            DropIndex("dbo.GanttLinks", new[] { "ProjectID" });
            DropColumn("dbo.Projects", "Discriminator");
            DropColumn("dbo.Projects", "Goals");
            DropColumn("dbo.Projects", "ProjectStatus");
            DropTable("dbo.GanttTasks");
            DropTable("dbo.GanttLinks");
            RenameTable(name: "dbo.Projects", newName: "UserProjects");
        }
    }
}
