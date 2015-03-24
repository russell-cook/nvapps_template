namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FileStreamConfigurationForAlsrReport : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BillsAlsrReports",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            Sql("alter table [dbo].[BillsAlsrReports] add [RowId] uniqueidentifier rowguidcol not null");
            Sql("alter table [dbo].[BillsAlsrReports] add constraint [UQ_BillsAlsrReports_RowId] UNIQUE NONCLUSTERED ([RowId])");
            Sql("alter table [dbo].[BillsAlsrReports] add constraint [DF_BillsAlsrReports_RowId] default (newid()) for [RowId]");

            Sql("alter table [dbo].[BillsAlsrReports] add [Pdf] varbinary(max) FILESTREAM not null");
            Sql("alter table [dbo].[BillsAlsrReports] add constraint [DF_BillsAlsrReports_Data] default(0x) for [Pdf]");
        }
        
        public override void Down()
        {
            Sql("alter table [dbo].[BillsAlsrReports] drop constraint [DF_BillsAlsrReports_Data]");
            Sql("alter table [dbo].[BillsAlsrReports] drop column [Pdf]");

            Sql("alter table [dbo].[BillsAlsrReports] drop constraint [UQ_BillsAlsrReports_RowId]");
            Sql("alter table [dbo].[BillsAlsrReports] drop constraint [DF_BillsAlsrReports_RowId]");
            Sql("alter table [dbo].[BillsAlsrReports] drop column [RowId]");

            DropTable("dbo.BillsAlsrReports");
        }
    }
}
