namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InstallProjectMgmtModuleV1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ProjectStatusID = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        Comments = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        ModifiedAt = c.DateTime(nullable: false),
                        ApplicationUserID = c.String(maxLength: 128),
                        Goals = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserID)
                .ForeignKey("dbo.ProjectStatus", t => t.ProjectStatusID, cascadeDelete: true)
                .Index(t => t.ProjectStatusID)
                .Index(t => t.ApplicationUserID);
            
            CreateTable(
                "dbo.GanttLinks",
                c => new
                    {
                        GanttLinkId = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        ProjectScheduleVersionID = c.Int(nullable: false),
                        Type = c.String(maxLength: 1),
                        SourceTaskId = c.Int(nullable: false),
                        TargetTaskId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.GanttLinkId)
                .ForeignKey("dbo.ProjectScheduleVersions", t => t.ProjectScheduleVersionID, cascadeDelete: true)
                .ForeignKey("dbo.Projects", t => t.ProjectID)
                .Index(t => t.ProjectID)
                .Index(t => t.ProjectScheduleVersionID);
            
            CreateTable(
                "dbo.GanttTasks",
                c => new
                    {
                        GanttTaskId = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        ProjectScheduleVersionID = c.Int(nullable: false),
                        Text = c.String(maxLength: 255),
                        StartDate = c.DateTime(nullable: false),
                        Duration = c.Int(nullable: false),
                        Progress = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SortOrder = c.Int(nullable: false),
                        Type = c.String(),
                        ParentId = c.Int(),
                        Open = c.Boolean(nullable: false),
                        PlannedStartDate = c.DateTime(),
                        PlannedEndDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.GanttTaskId)
                .ForeignKey("dbo.Projects", t => t.ProjectID)
                .ForeignKey("dbo.ProjectScheduleVersions", t => t.ProjectScheduleVersionID, cascadeDelete: true)
                .Index(t => t.ProjectID)
                .Index(t => t.ProjectScheduleVersionID);
            
            CreateTable(
                "dbo.ProjectScheduleVersions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        VersionNum = c.Int(nullable: false),
                        Comments = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        SavedAt = c.DateTime(nullable: false),
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GanttLinks", "ProjectID", "dbo.Projects");
            DropForeignKey("dbo.Projects", "ProjectStatusID", "dbo.ProjectStatus");
            DropForeignKey("dbo.ProjectScheduleVersions", "ProjectID", "dbo.Projects");
            DropForeignKey("dbo.GanttTasks", "ProjectScheduleVersionID", "dbo.ProjectScheduleVersions");
            DropForeignKey("dbo.GanttLinks", "ProjectScheduleVersionID", "dbo.ProjectScheduleVersions");
            DropForeignKey("dbo.GanttTasks", "ProjectID", "dbo.Projects");
            DropForeignKey("dbo.Projects", "ApplicationUserID", "dbo.AspNetUsers");
            DropIndex("dbo.ProjectScheduleVersions", new[] { "ProjectID" });
            DropIndex("dbo.GanttTasks", new[] { "ProjectScheduleVersionID" });
            DropIndex("dbo.GanttTasks", new[] { "ProjectID" });
            DropIndex("dbo.GanttLinks", new[] { "ProjectScheduleVersionID" });
            DropIndex("dbo.GanttLinks", new[] { "ProjectID" });
            DropIndex("dbo.Projects", new[] { "ApplicationUserID" });
            DropIndex("dbo.Projects", new[] { "ProjectStatusID" });
            DropTable("dbo.ProjectStatus");
            DropTable("dbo.ProjectScheduleVersions");
            DropTable("dbo.GanttTasks");
            DropTable("dbo.GanttLinks");
            DropTable("dbo.Projects");
        }
    }
}
