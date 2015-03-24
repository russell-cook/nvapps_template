namespace AdminApps.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitializeDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AppGlobalSettings",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        BudgetPeriodID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        BudgetSessionID = c.Decimal(nullable: false, precision: 18, scale: 0),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BudgetPeriods", t => t.BudgetPeriodID, cascadeDelete: true)
                .ForeignKey("dbo.BudgetSessions", t => t.BudgetSessionID, cascadeDelete: true)
                .Index(t => t.BudgetPeriodID)
                .Index(t => t.BudgetSessionID);
            
            CreateTable(
                "dbo.BudgetPeriods",
                c => new
                    {
                        ID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        Description = c.String(),
                        ActualYear = c.String(),
                        WorkProgramYear = c.String(),
                        BeginYear = c.String(),
                        EndYear = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Bills",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        NebsBdrID = c.Decimal(precision: 18, scale: 2),
                        BudgetPeriodID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        BudgetSessionID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        NebsBdrNumber = c.String(),
                        NebsDeptID = c.Decimal(precision: 18, scale: 0),
                        NebsDivID = c.Decimal(precision: 18, scale: 0),
                        NebsBdrApprovedDate = c.DateTime(),
                        NebsBdrTitle = c.String(),
                        NebsBdrDescription = c.String(),
                        NebsBdrTransmittalDate = c.DateTime(),
                        NebsBdrAgendaName = c.String(),
                        NebsBdrAgendaDate = c.DateTime(),
                        BillPrefixID = c.Int(),
                        Suffix = c.Int(),
                        BdrPrefix = c.Int(),
                        BdrSuffix = c.Int(),
                        DeptID = c.Decimal(precision: 18, scale: 0),
                        DivID = c.Decimal(precision: 18, scale: 0),
                        Title = c.String(nullable: false),
                        Digest = c.String(),
                        Summary = c.String(nullable: false),
                        DateIntroduced = c.DateTime(),
                        LegStatusID = c.Int(),
                        FirstHouseCommitteeID = c.Int(),
                        SecondHouseCommitteeID = c.Int(),
                        DatePassedFirstHouse = c.DateTime(),
                        DatePassedSecondHouse = c.DateTime(),
                        GovActionID = c.Int(),
                        DateGovAction = c.DateTime(),
                        ChapterNum = c.Int(),
                        NelisHyperlink = c.String(),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        NelisID = c.Int(),
                        ApplicationUserID = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        Dept_ID = c.Decimal(precision: 18, scale: 0),
                        Div_ID = c.Decimal(precision: 18, scale: 0),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BillPrefixes", t => t.BillPrefixID)
                .ForeignKey("dbo.Depts", t => t.Dept_ID)
                .ForeignKey("dbo.Divs", t => t.Div_ID)
                .ForeignKey("dbo.BudgetPeriods", t => t.BudgetPeriodID, cascadeDelete: true)
                .ForeignKey("dbo.BudgetSessions", t => t.BudgetSessionID, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserID)
                .ForeignKey("dbo.Depts", t => t.DeptID)
                .ForeignKey("dbo.Divs", t => t.DivID)
                .ForeignKey("dbo.LegCommittees", t => t.FirstHouseCommitteeID)
                .ForeignKey("dbo.GovActions", t => t.GovActionID)
                .ForeignKey("dbo.LegStatus", t => t.LegStatusID)
                .ForeignKey("dbo.Depts", t => t.NebsDeptID)
                .ForeignKey("dbo.Divs", t => t.NebsDivID)
                .ForeignKey("dbo.LegCommittees", t => t.SecondHouseCommitteeID)
                .Index(t => t.BudgetPeriodID)
                .Index(t => t.BudgetSessionID)
                .Index(t => t.NebsDeptID)
                .Index(t => t.NebsDivID)
                .Index(t => t.BillPrefixID)
                .Index(t => t.DeptID)
                .Index(t => t.DivID)
                .Index(t => t.LegStatusID)
                .Index(t => t.FirstHouseCommitteeID)
                .Index(t => t.SecondHouseCommitteeID)
                .Index(t => t.GovActionID)
                .Index(t => t.ApplicationUserID)
                .Index(t => t.Dept_ID)
                .Index(t => t.Div_ID);
            
            CreateTable(
                "dbo.BillPrefixes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Prefix = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.BillRecordRequests",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        BudgetPeriodID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        RequestedByUserID = c.String(maxLength: 128),
                        RequestedAt = c.DateTime(nullable: false),
                        Fulfilled = c.Boolean(nullable: false),
                        FulfilledByUserID = c.String(maxLength: 128),
                        FulfilledAt = c.DateTime(),
                        BillID = c.Int(),
                        BillPrefixID = c.Int(),
                        Suffix = c.Int(),
                        Summary = c.String(),
                        Title = c.String(),
                        NelisHyperlink = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Bills", t => t.BillID)
                .ForeignKey("dbo.BillPrefixes", t => t.BillPrefixID)
                .ForeignKey("dbo.BudgetPeriods", t => t.BudgetPeriodID, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.FulfilledByUserID)
                .ForeignKey("dbo.AspNetUsers", t => t.RequestedByUserID)
                .Index(t => t.BudgetPeriodID)
                .Index(t => t.RequestedByUserID)
                .Index(t => t.FulfilledByUserID)
                .Index(t => t.BillID)
                .Index(t => t.BillPrefixID);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Title = c.String(),
                        DivID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        DeptID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        IsActive = c.Boolean(nullable: false),
                        AutoPwdReplaced = c.Boolean(nullable: false),
                        AppModuleID = c.Int(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Depts", t => t.DeptID, cascadeDelete: true)
                .ForeignKey("dbo.Divs", t => t.DivID, cascadeDelete: true)
                .ForeignKey("dbo.AppModules", t => t.AppModuleID)
                .Index(t => t.DivID)
                .Index(t => t.DeptID)
                .Index(t => t.AppModuleID)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.BillReviewApprovals",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ApplicationUserID = c.String(maxLength: 128),
                        BillReviewID = c.Int(nullable: false),
                        ApprovalLevel = c.Int(nullable: false),
                        ApprovedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserID)
                .ForeignKey("dbo.BillReviews", t => t.BillReviewID, cascadeDelete: true)
                .Index(t => t.ApplicationUserID)
                .Index(t => t.BillReviewID);
            
            CreateTable(
                "dbo.BillReviews",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        BillID = c.Int(nullable: false),
                        BillVersionID = c.Int(nullable: false),
                        BillReviewRecommendationID = c.Int(nullable: false),
                        ApplicationUserID = c.String(maxLength: 128),
                        DeptID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        DivID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        Comments = c.String(),
                        RequiresTestimony = c.Boolean(nullable: false),
                        InformationToBeProvided = c.Boolean(nullable: false),
                        ActivelyTracking = c.Boolean(nullable: false),
                        PolicyImpact = c.Boolean(),
                        FiscalImpactYr1 = c.Int(nullable: false),
                        FiscalImpactYr2 = c.Int(nullable: false),
                        FiscalImpactFuture = c.Int(nullable: false),
                        FiscalNoteSubmitted = c.Boolean(nullable: false),
                        Notes = c.String(),
                        CreatedAtApprovalLevel = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Bills", t => t.BillID, cascadeDelete: true)
                .ForeignKey("dbo.BillVersions", t => t.BillVersionID, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserID)
                .ForeignKey("dbo.Depts", t => t.DeptID, cascadeDelete: true)
                .ForeignKey("dbo.Divs", t => t.DivID, cascadeDelete: true)
                .ForeignKey("dbo.BillReviewRecommendations", t => t.BillReviewRecommendationID, cascadeDelete: true)
                .Index(t => t.BillID)
                .Index(t => t.BillVersionID)
                .Index(t => t.BillReviewRecommendationID)
                .Index(t => t.ApplicationUserID)
                .Index(t => t.DeptID)
                .Index(t => t.DivID);
            
            CreateTable(
                "dbo.BillVersions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        BillID = c.Int(nullable: false),
                        VersionNum = c.Int(nullable: false),
                        IsReprint = c.Boolean(nullable: false),
                        ReprintNum = c.Int(),
                        ReprintDate = c.DateTime(),
                        Amendment = c.Int(),
                        IsEnrolled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Bills", t => t.BillID)
                .Index(t => t.BillID);
            
            CreateTable(
                "dbo.Depts",
                c => new
                    {
                        ID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        BudgetPeriodID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        Code = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BudgetPeriods", t => t.BudgetPeriodID)
                .Index(t => t.BudgetPeriodID);
            
            CreateTable(
                "dbo.Divs",
                c => new
                    {
                        ID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        BudgetPeriodID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        DeptID = c.Decimal(precision: 18, scale: 0),
                        Code = c.String(),
                        Description = c.String(),
                        SortOrder = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BudgetPeriods", t => t.BudgetPeriodID)
                .ForeignKey("dbo.Depts", t => t.DeptID)
                .Index(t => t.BudgetPeriodID)
                .Index(t => t.DeptID);
            
            CreateTable(
                "dbo.BillReviewNotifications",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ApplicationUserID = c.String(maxLength: 128),
                        BillReviewID = c.Int(nullable: false),
                        ApprovalLevel = c.Int(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        ReadAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BillReviews", t => t.BillReviewID, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserID)
                .Index(t => t.ApplicationUserID)
                .Index(t => t.BillReviewID)
                .Index(t => t.IsRead);
            
            CreateTable(
                "dbo.BillReviewRecommendations",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.BillReviewRequests",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        RequestedByUserID = c.String(maxLength: 128),
                        RequestedToUserID = c.String(maxLength: 128),
                        BillID = c.Int(nullable: false),
                        RequestedAt = c.DateTime(nullable: false),
                        RequestedAtApprovalLevel = c.Int(nullable: false),
                        Fulfilled = c.Boolean(nullable: false),
                        FulfilledAt = c.DateTime(),
                        BillReviewID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Bills", t => t.BillID, cascadeDelete: true)
                .ForeignKey("dbo.BillReviews", t => t.BillReviewID)
                .ForeignKey("dbo.AspNetUsers", t => t.RequestedByUserID)
                .ForeignKey("dbo.AspNetUsers", t => t.RequestedToUserID)
                .Index(t => t.RequestedByUserID)
                .Index(t => t.RequestedToUserID)
                .Index(t => t.BillID)
                .Index(t => t.BillReviewID);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AppModules",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        DefaultController = c.String(),
                        DefaultAction = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        Description = c.String(),
                        AppModuleID = c.Int(),
                        AppModuleApprovalLevel = c.Int(),
                        AppModuleApprovalTitle = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AppModules", t => t.AppModuleID)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex")
                .Index(t => t.AppModuleID);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserAccountRequests",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        DivID = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DeptID = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LastName = c.String(),
                        Title = c.String(),
                        RequestedByUserID = c.String(maxLength: 128),
                        FullfilledByUserID = c.String(maxLength: 128),
                        FullfilledAt = c.DateTime(),
                        RequestedApprovalLevel = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.AspNetUsers", t => t.FullfilledByUserID)
                .ForeignKey("dbo.AspNetUsers", t => t.RequestedByUserID)
                .Index(t => t.RequestedByUserID)
                .Index(t => t.FullfilledByUserID);
            
            CreateTable(
                "dbo.BudgetSessions",
                c => new
                    {
                        ID = c.Decimal(nullable: false, precision: 18, scale: 0),
                        Title = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.LegCommittees",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        BudgetSessionID = c.Decimal(nullable: false, precision: 18, scale: 2),
                        House = c.Int(nullable: false),
                        CommitteeName = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.GovActions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Hearings",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        BillID = c.Int(nullable: false),
                        LegCommitteeID = c.Int(nullable: false),
                        HearingDate = c.DateTime(nullable: false),
                        Location = c.String(),
                        Action = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Bills", t => t.BillID, cascadeDelete: true)
                .ForeignKey("dbo.LegCommittees", t => t.LegCommitteeID, cascadeDelete: true)
                .Index(t => t.BillID)
                .Index(t => t.LegCommitteeID);
            
            CreateTable(
                "dbo.LegStatus",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.UserBillsOfInterest",
                c => new
                    {
                        BillID = c.Int(nullable: false),
                        ApplicationUserID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.BillID, t.ApplicationUserID })
                .ForeignKey("dbo.Bills", t => t.BillID, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserID, cascadeDelete: true)
                .Index(t => t.BillID)
                .Index(t => t.ApplicationUserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AppGlobalSettings", "BudgetSessionID", "dbo.BudgetSessions");
            DropForeignKey("dbo.AppGlobalSettings", "BudgetPeriodID", "dbo.BudgetPeriods");
            DropForeignKey("dbo.Bills", "SecondHouseCommitteeID", "dbo.LegCommittees");
            DropForeignKey("dbo.Bills", "NebsDivID", "dbo.Divs");
            DropForeignKey("dbo.Bills", "NebsDeptID", "dbo.Depts");
            DropForeignKey("dbo.Bills", "LegStatusID", "dbo.LegStatus");
            DropForeignKey("dbo.UserBillsOfInterest", "ApplicationUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserBillsOfInterest", "BillID", "dbo.Bills");
            DropForeignKey("dbo.Hearings", "LegCommitteeID", "dbo.LegCommittees");
            DropForeignKey("dbo.Hearings", "BillID", "dbo.Bills");
            DropForeignKey("dbo.Bills", "GovActionID", "dbo.GovActions");
            DropForeignKey("dbo.Bills", "FirstHouseCommitteeID", "dbo.LegCommittees");
            DropForeignKey("dbo.Bills", "DivID", "dbo.Divs");
            DropForeignKey("dbo.Bills", "DeptID", "dbo.Depts");
            DropForeignKey("dbo.Bills", "ApplicationUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.Bills", "BudgetSessionID", "dbo.BudgetSessions");
            DropForeignKey("dbo.Bills", "BudgetPeriodID", "dbo.BudgetPeriods");
            DropForeignKey("dbo.BillRecordRequests", "RequestedByUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.BillRecordRequests", "FulfilledByUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserAccountRequests", "RequestedByUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserAccountRequests", "FullfilledByUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "AppModuleID", "dbo.AppModules");
            DropForeignKey("dbo.AspNetRoles", "AppModuleID", "dbo.AppModules");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BillReviewRequests", "RequestedToUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.BillReviewRequests", "RequestedByUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.BillReviewRequests", "BillReviewID", "dbo.BillReviews");
            DropForeignKey("dbo.BillReviewRequests", "BillID", "dbo.Bills");
            DropForeignKey("dbo.BillReviews", "BillReviewRecommendationID", "dbo.BillReviewRecommendations");
            DropForeignKey("dbo.BillReviewNotifications", "ApplicationUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.BillReviewNotifications", "BillReviewID", "dbo.BillReviews");
            DropForeignKey("dbo.BillReviews", "DivID", "dbo.Divs");
            DropForeignKey("dbo.BillReviews", "DeptID", "dbo.Depts");
            DropForeignKey("dbo.Divs", "DeptID", "dbo.Depts");
            DropForeignKey("dbo.Divs", "BudgetPeriodID", "dbo.BudgetPeriods");
            DropForeignKey("dbo.Bills", "Div_ID", "dbo.Divs");
            DropForeignKey("dbo.AspNetUsers", "DivID", "dbo.Divs");
            DropForeignKey("dbo.Depts", "BudgetPeriodID", "dbo.BudgetPeriods");
            DropForeignKey("dbo.Bills", "Dept_ID", "dbo.Depts");
            DropForeignKey("dbo.AspNetUsers", "DeptID", "dbo.Depts");
            DropForeignKey("dbo.BillReviews", "ApplicationUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.BillReviews", "BillVersionID", "dbo.BillVersions");
            DropForeignKey("dbo.BillVersions", "BillID", "dbo.Bills");
            DropForeignKey("dbo.BillReviews", "BillID", "dbo.Bills");
            DropForeignKey("dbo.BillReviewApprovals", "BillReviewID", "dbo.BillReviews");
            DropForeignKey("dbo.BillReviewApprovals", "ApplicationUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.BillRecordRequests", "BudgetPeriodID", "dbo.BudgetPeriods");
            DropForeignKey("dbo.BillRecordRequests", "BillPrefixID", "dbo.BillPrefixes");
            DropForeignKey("dbo.BillRecordRequests", "BillID", "dbo.Bills");
            DropForeignKey("dbo.Bills", "BillPrefixID", "dbo.BillPrefixes");
            DropIndex("dbo.UserBillsOfInterest", new[] { "ApplicationUserID" });
            DropIndex("dbo.UserBillsOfInterest", new[] { "BillID" });
            DropIndex("dbo.Hearings", new[] { "LegCommitteeID" });
            DropIndex("dbo.Hearings", new[] { "BillID" });
            DropIndex("dbo.UserAccountRequests", new[] { "FullfilledByUserID" });
            DropIndex("dbo.UserAccountRequests", new[] { "RequestedByUserID" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", new[] { "AppModuleID" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.BillReviewRequests", new[] { "BillReviewID" });
            DropIndex("dbo.BillReviewRequests", new[] { "BillID" });
            DropIndex("dbo.BillReviewRequests", new[] { "RequestedToUserID" });
            DropIndex("dbo.BillReviewRequests", new[] { "RequestedByUserID" });
            DropIndex("dbo.BillReviewNotifications", new[] { "IsRead" });
            DropIndex("dbo.BillReviewNotifications", new[] { "BillReviewID" });
            DropIndex("dbo.BillReviewNotifications", new[] { "ApplicationUserID" });
            DropIndex("dbo.Divs", new[] { "DeptID" });
            DropIndex("dbo.Divs", new[] { "BudgetPeriodID" });
            DropIndex("dbo.Depts", new[] { "BudgetPeriodID" });
            DropIndex("dbo.BillVersions", new[] { "BillID" });
            DropIndex("dbo.BillReviews", new[] { "DivID" });
            DropIndex("dbo.BillReviews", new[] { "DeptID" });
            DropIndex("dbo.BillReviews", new[] { "ApplicationUserID" });
            DropIndex("dbo.BillReviews", new[] { "BillReviewRecommendationID" });
            DropIndex("dbo.BillReviews", new[] { "BillVersionID" });
            DropIndex("dbo.BillReviews", new[] { "BillID" });
            DropIndex("dbo.BillReviewApprovals", new[] { "BillReviewID" });
            DropIndex("dbo.BillReviewApprovals", new[] { "ApplicationUserID" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "AppModuleID" });
            DropIndex("dbo.AspNetUsers", new[] { "DeptID" });
            DropIndex("dbo.AspNetUsers", new[] { "DivID" });
            DropIndex("dbo.BillRecordRequests", new[] { "BillPrefixID" });
            DropIndex("dbo.BillRecordRequests", new[] { "BillID" });
            DropIndex("dbo.BillRecordRequests", new[] { "FulfilledByUserID" });
            DropIndex("dbo.BillRecordRequests", new[] { "RequestedByUserID" });
            DropIndex("dbo.BillRecordRequests", new[] { "BudgetPeriodID" });
            DropIndex("dbo.Bills", new[] { "Div_ID" });
            DropIndex("dbo.Bills", new[] { "Dept_ID" });
            DropIndex("dbo.Bills", new[] { "ApplicationUserID" });
            DropIndex("dbo.Bills", new[] { "GovActionID" });
            DropIndex("dbo.Bills", new[] { "SecondHouseCommitteeID" });
            DropIndex("dbo.Bills", new[] { "FirstHouseCommitteeID" });
            DropIndex("dbo.Bills", new[] { "LegStatusID" });
            DropIndex("dbo.Bills", new[] { "DivID" });
            DropIndex("dbo.Bills", new[] { "DeptID" });
            DropIndex("dbo.Bills", new[] { "BillPrefixID" });
            DropIndex("dbo.Bills", new[] { "NebsDivID" });
            DropIndex("dbo.Bills", new[] { "NebsDeptID" });
            DropIndex("dbo.Bills", new[] { "BudgetSessionID" });
            DropIndex("dbo.Bills", new[] { "BudgetPeriodID" });
            DropIndex("dbo.AppGlobalSettings", new[] { "BudgetSessionID" });
            DropIndex("dbo.AppGlobalSettings", new[] { "BudgetPeriodID" });
            DropTable("dbo.UserBillsOfInterest");
            DropTable("dbo.LegStatus");
            DropTable("dbo.Hearings");
            DropTable("dbo.GovActions");
            DropTable("dbo.LegCommittees");
            DropTable("dbo.BudgetSessions");
            DropTable("dbo.UserAccountRequests");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AppModules");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.BillReviewRequests");
            DropTable("dbo.BillReviewRecommendations");
            DropTable("dbo.BillReviewNotifications");
            DropTable("dbo.Divs");
            DropTable("dbo.Depts");
            DropTable("dbo.BillVersions");
            DropTable("dbo.BillReviews");
            DropTable("dbo.BillReviewApprovals");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.BillRecordRequests");
            DropTable("dbo.BillPrefixes");
            DropTable("dbo.Bills");
            DropTable("dbo.BudgetPeriods");
            DropTable("dbo.AppGlobalSettings");
        }
    }
}
