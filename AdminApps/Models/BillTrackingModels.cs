using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AdminApps.Models
{
    public class Bill
    {
        public int ID { get; set; }

        // NEBS BDR Module properties; these are populated when a record is copied from the NEBS BDR Module
        public decimal? NebsBdrID { get; set; }
        public decimal BudgetPeriodID { get; set; }
        public decimal BudgetSessionID { get; set; }
        [Display(Name = "NEBS BDR #")]
        public string NebsBdrNumber { get; set; }
        [ForeignKey("NebsDept")]
        public decimal? NebsDeptID { get; set; }
        [ForeignKey("NebsDiv")]
        public decimal? NebsDivID { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "NEBS BDR Approval Date")]
        public DateTime? NebsBdrApprovedDate { get; set; }
        [Display(Name = "NEBS BDR Title")]
        public string NebsBdrTitle { get; set; }
        [Display(Name = "NEBS BDR Description")]
        public string NebsBdrDescription { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "NEBS BDR Transmittal Date")]
        public DateTime? NebsBdrTransmittalDate { get; set; }
        [Display(Name = "NEBS BDR Agenda Name")]
        public string NebsBdrAgendaName { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "NEBS BDR Agenda Date")]
        public DateTime? NebsBdrAgendaDate { get; set; }

        // AdminApps Bill Tracking properties
        [Display(Name = "Bill Prefix")]
        public int? BillPrefixID { get; set; }
        [Display(Name = "Bill Suffix")]
        public int? Suffix { get; set; }
        [Display(Name = "BDR Prefix")]
        public string BdrPrefix { get; set; }
        [Display(Name = "BDR Suffix")]
        public int? BdrSuffix { get; set; }
        public decimal? DeptID { get; set; }
        public decimal? DivID { get; set; }
        [Required]
        public string Title { get; set; }
        public string Digest { get; set; }
        [Required]
        public string Summary { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateIntroduced { get; set; }
        [Display(Name = "Legislative Status")]
        public int? LegStatusID { get; set; }
        [ForeignKey("FirstHouseCommittee")]
        [Display(Name = "First House Committee")]
        public int? FirstHouseCommitteeID { get; set; }
        [ForeignKey("SecondHouseCommittee")]
        [Display(Name = "Second House Committee")]
        public int? SecondHouseCommitteeID { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DatePassedFirstHouse { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DatePassedSecondHouse { get; set; }
        [Display(Name = "Governor's Action")]
        public int? GovActionID { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Date of Gov Action")]
        public DateTime? DateGovAction { get; set; }
        [Display(Name = "Chapter Number")]
        public int? ChapterNum { get; set; }
        public bool Exempt { get; set; }
        //[DataType(DataType.Url)]
        //public string NelisHyperlink { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        //NELIS properties
        public int? NelisID { get; set; }

        // Identity properties
        [ForeignKey("CreatedByUser")]
        public string ApplicationUserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public virtual ICollection<ApplicationUser> InterestedUsers { get; set; }

        // navigation properties
        public virtual BudgetPeriod BudgetPeriod { get; set; }
        public virtual BudgetSession BudgetSession { get; set; }
        public virtual Dept NebsDept { get; set; }
        public virtual Div NebsDiv { get; set; }
        public virtual Dept Dept { get; set; }
        public virtual Div Div { get; set; }
        public virtual BillPrefix BillPrefix { get; set; }
        public virtual LegStatus LegStatus { get; set; }
        [InverseProperty("FirstHouseBills")]
        public virtual LegCommittee FirstHouseCommittee { get; set; }
        [InverseProperty("SecondHouseBills")]
        public virtual LegCommittee SecondHouseCommittee { get; set; }
        public virtual GovAction GovAction { get; set; }
        public virtual ICollection<Hearing> Hearings { get; set; }
        public virtual ICollection<BillReview> Reviews { get; set; }
        public virtual ICollection<BillRecordRequest> BillRecordRequests { get; set; }
        public virtual ICollection<BillVersion> BillVersions { get; set; }

        // calculated properties
        [Display(Name = "Bill #")]
        public string CompositeBillNumber
        {
            get
            {
                if (BillPrefix != null && Suffix != null)
                {
                    return BillPrefix.Prefix + Suffix.ToString();
                }
                else
                {
                    return string.Format("BDR {0}", CompositeNelisBdrNumber);
                }
            }
        }

        public string CompositeBillName
        {
            get
            {
                return (CompositeBillNumber + ": " + Summary);
            }
        }

        [Display(Name = "NELIS BDR #")]
        public string CompositeNelisBdrNumber
        {
            get
            {
                if (!String.IsNullOrEmpty(BdrPrefix) && BdrSuffix != null)
                {
                    return BdrPrefix + "-" + BdrSuffix.ToString();
                }
                else
                {
                    return null;
                }
            }
        }

        [Display(Name = "Created by")]
        public string CreatedByName
        {
            get
            {
                if (CreatedByUser != null)
                {
                    return CreatedByUser.Email;
                }
                else
                {
                    return "Unknown";
                }
            }
        }

        [DataType(DataType.Url)]
        [UIHint("OpenInNewWindow")]
        public string calculatedHyperlink
        {
            get
            {
                if (NelisID != null)
                {
                    return string.Format("https://www.leg.state.nv.us/App/NELIS/REL/78th2015/Bill/{0}/Overview", NelisID);
                }
                else
                {
                    return null;
                }
            }
        }


    }

    public class BillVersion
    {
        public int ID { get; set; }
        public int BillID { get; set; }
        public int VersionNum { get; set; }
        public string Sponsor { get; set; }
        [Display(Name = "Reprint?")]
        public bool IsReprint { get; set; }
        public int? ReprintNum { get; set; }
        [Display(Name = "Reprint Date")]
        [DataType(DataType.Date)]
        public DateTime? ReprintDate { get; set; }
        public int? Amendment { get; set; }
        public bool IsEnrolled { get; set; }

        // navigation properties
        public virtual Bill Bill { get; set; }
        public virtual ICollection<BillReview> BillReviews { get; set; }

        // calculated properties
        [Display(Name = "Version")]
        public string VersionDescription
        {
            get
            {
                if (VersionNum == 0)
                {
                    return "As Introduced";
                }
                else if(IsReprint)
                {
                    return string.Format("Reprint {0}, {1}", ReprintNum, ReprintDate);
                }
                else if(IsEnrolled)
                {
                    return "As Enrolled";
                }
                return null;
            }
        }
    }

    public class BillReview
    {
        public int ID { get; set; }
        public int BillID { get; set; }
        [Index]
        public int BillVersionID { get; set; }
        [Display(Name = "Recommendation")]
        public int BillReviewRecommendationID { get; set; }
        [ForeignKey("CreatedByUser")]
        public string ApplicationUserID { get; set; }
        [ForeignKey("CreatedByUserInDept")]
        public decimal DeptID { get; set; }
        [ForeignKey("CreatedByUserInDiv")]
        public decimal DivID { get; set; }
        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }
        [UIHint("BooleanButton")]
        [Display(Name = "Testimony Required?")]
        public bool RequiresTestimony { get; set; }
        [UIHint("BooleanButton")]
        [Display(Name = "Information/testimony to be provided?")]
        public bool InformationToBeProvided { get; set; }
        [UIHint("BooleanButton")]
        [Display(Name = "Actively Tracking?")]
        public bool ActivelyTracking { get; set; }
        [UIHint("BooleanButton")]
        [Display(Name = "Policy impact on agency?")]
        public bool? PolicyImpact { get; set; }
        [Display(Name = "Fiscal Impact Yr 1")]
        [Required]
        [DataType(DataType.Currency)]
        public int FiscalImpactYr1 { get; set; }
        [Display(Name = "Fiscal Impact Yr 2")]
        [DataType(DataType.Currency)]
        [Required]
        public int FiscalImpactYr2 { get; set; }
        [Display(Name = "Impact on Future Biennia")]
        [DataType(DataType.Currency)]
        [Required]
        public int FiscalImpactFuture { get; set; }
        [UIHint("BooleanButton")]
        [Display(Name = "Fiscal Note submitted?")]
        public bool FiscalNoteSubmitted { get; set; }
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        public int CreatedAtApprovalLevel { get; set; }
        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual Bill Bill { get; set; }
        public virtual BillVersion BillVersion { get; set; }
        public virtual BillReviewRecommendation Recommendation { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        [Display(Name = "Dept")]
        public virtual Dept CreatedByUserInDept { get; set; }
        [Display(Name = "Div")]
        public virtual Div CreatedByUserInDiv { get; set; }

        public virtual ICollection<BillReviewNotification> Notifications { get; set; }
        public virtual ICollection<BillReviewApproval> Approvals { get; set; }

        [Display(Name = "Reviewed by")]
        public string CreatedByName
        {
            get
            {
                return CreatedByUser.FullName;
            }
        }

        [Display(Name = "Biennium Fiscal Impact")]
        [DataType(DataType.Currency)]
        public int? FiscalImpactBiennium
        {
            get
            {
                return FiscalImpactYr1 + FiscalImpactYr2;
            }
        }

        [Display(Name = "Total Fiscal Impact")]
        [DataType(DataType.Currency)]
        public int? FiscalImpactTotal
        {
            get
            {
                return FiscalImpactYr1 + FiscalImpactYr2 + FiscalImpactFuture;
            }
        }

    }

    public class AlsrBillReviewSnapshot : BillReview
    {
        public int BillsAlsrReportID { get; set; }
        public DateTime Timestamp { get; set; }
        public int CapturedFromBillReviewID { get; set; }
        public byte[] CapturedFromRowVersion { get; set; }
        public int? SupercedesPreviousSnapshotID { get; set; }

        public virtual BillsAlsrReport BillsAlsrReport { get; set; }
    }

    public class BillsAlsrReport
    {
        public BillsAlsrReport()
        {
            this.AlsrBillReviewSnapshots = new List<AlsrBillReviewSnapshot>();
        }

        public int ID { get; set; }
        public decimal BudgetPeriodID { get; set; }
        public decimal BudgetSessionID { get; set; }
        public decimal DeptID { get; set; }
        public decimal DivID { get; set; }
        public byte[] Pdf { get; set; }
        public string Filename { get; set; }
        public DateTime CreatedAt { get; set; }
        [DataType(DataType.Date)]
        public DateTime GovOfficeDeliveryDate { get; set; }
        [ForeignKey("CreatedByUser")]
        public string ApplicationUserID { get; set; }
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        public virtual Dept Dept { get; set; }
        public virtual Div Div { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public virtual List<AlsrBillReviewSnapshot> AlsrBillReviewSnapshots { get; set; }
    }

    public class BillReviewApproval
    {
        public int ID { get; set; }
        [Index]
        [ForeignKey("ApprovedBy")]
        public string ApplicationUserID { get; set; }
        public int BillReviewID { get; set; }
        public int ApprovalLevel { get; set; }
        public DateTime ApprovedAt { get; set; }

        public virtual ApplicationUser ApprovedBy { get; set; }
        public virtual BillReview BillReview { get; set; }
    }

    public class BillReviewNotification
    {
        public int ID { get; set; }
        [Index]
        [ForeignKey("ReadBy")]
        public string ApplicationUserID { get; set; }
        public int BillReviewID { get; set; }
        public int ApprovalLevel { get; set; }
        [Index]
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        public virtual ApplicationUser ReadBy { get; set; }
        public virtual BillReview BillReview { get; set; }
    }

    public class BillReviewRequest
    {
        public int ID { get; set; }
        [Index]
        [ForeignKey("RequestedByUser")]
        public string RequestedByUserID { get; set; }
        [Index]
        [ForeignKey("RequestedToUser")]
        public string RequestedToUserID { get; set; }
        public int BillID { get; set; }
        public DateTime RequestedAt { get; set; }
        public int RequestedAtApprovalLevel { get; set; }
        public bool Fulfilled { get; set; }
        public DateTime? FulfilledAt { get; set; }
        public int? BillReviewID { get; set; }

        [InverseProperty("BillReviewRequestsBy")]
        public virtual ApplicationUser RequestedByUser { get; set; }
        [InverseProperty("BillReviewRequestsTo")]
        public virtual ApplicationUser RequestedToUser { get; set; }
        public virtual Bill Bill { get; set; }
        public virtual BillReview BillReview { get; set; }
    }

    public class BillRecordRequest
    {
        public int ID { get; set; }
        public decimal BudgetPeriodID { get; set; }
        [ForeignKey("RequestedByUser")]
        public string RequestedByUserID { get; set; }
        public DateTime RequestedAt { get; set; }
        public bool Fulfilled { get; set; }
        [ForeignKey("FullfilledByUser")]
        public string FulfilledByUserID { get; set; }
        public DateTime? FulfilledAt { get; set; }
        // resultant Bill record
        public int? BillID { get; set; }
        [Display(Name = "Bill Prefix")]
        public int? BillPrefixID { get; set; }
        [Display(Name = "Bill Suffix")]
        public int? Suffix { get; set; }
        [Display(Name = "BDR Prefix")]
        public string BdrPrefix { get; set; }
        [Display(Name = "BDR Suffix")]
        public int? BdrSuffix { get; set; }
        public string Summary { get; set; }
        public string Title { get; set; }
        [Display(Name = "NELIS Link")]
        [DataType(DataType.Url)]
        public string NelisHyperlink { get; set; }

        // navigation properties
        public virtual BudgetPeriod BudgetPeriod { get; set; }
        [InverseProperty("BillRecordRequestsBy")]
        public virtual ApplicationUser RequestedByUser { get; set; }
        [InverseProperty("BillRecordRequestsFulfilledBy")]
        public virtual ApplicationUser FullfilledByUser { get; set; }
        public virtual Bill Bill { get; set; }
        public virtual BillPrefix BillPrefix { get; set; }
    }

    public class BillsUserAccountRequest : UserAccountRequest
    {
        public int RequestedApprovalLevel { get; set; }
    }

    // This model is used by NebsContext to retrieve records from the NEBS BDR Module (Oracle DB).
    // Schema mapping for Oracle DB connection is specified in DAL => NebsContext
    public class NebsBdr
    {
        [Key]
        public decimal NebsBdrID { get; set; }
        public decimal BudgetPeriodID { get; set; }
        public decimal BudgetSessionID { get; set; }
        public string BudgetSessionTitle { get; set; }
        public string NebsBdrNumber { get; set; }
        public decimal? NebsDeptID { get; set; }
        public decimal? NebsDivID { get; set; }
        public DateTime NebsBdrApprovedDate { get; set; }
        public string NebsBdrTitle { get; set; }
        public string NebsBdrDescription { get; set; }
        public DateTime NebsBdrTransmittalDate { get; set; }
        public string NebsBdrAgendaName { get; set; }
        public DateTime NebsBdrAgendaDate { get; set; }

    }

    public class BillPrefix
    {
        public int ID { get; set; }
        public string Prefix { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Bill> Bills { get; set; }
    }

    public class GovAction
    {
        public int ID { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Bill> Bills { get; set; }
    }

    public class Hearing
    {
        public int ID { get; set; }
        public int BillID { get; set; }
        public int LegCommitteeID { get; set; }
        public DateTime HearingDate { get; set; }
        public string Location { get; set; }
        public string Action { get; set; }

        public virtual Bill Bill { get; set; }
        public virtual LegCommittee Committee { get; set; }

    }

    public enum House
    {
        Assembly, Senate
    }

    public class LegCommittee
    {
        public int ID { get; set; }
        public decimal BudgetSessionID { get; set; }
        public House House { get; set; }
        public string CommitteeName { get; set; }

        public virtual ICollection<Bill> FirstHouseBills { get; set; }
        public virtual ICollection<Bill> SecondHouseBills { get; set; }

        [Display(Name = "Legislative Committee")]
        public string CombinedName
        {
            get
            {
                return House.ToString() + CommitteeName;
            }
        }
    }

    public class LegStatus
    {
        public int ID { get; set; }
        [Display(Name = "Leg Status")]
        public string Description { get; set; }

        public virtual ICollection<Bill> Bills { get; set; }
    }

    public class BillReviewRecommendation
    {
        public int ID { get; set; }
        [Display(Name = "Recommendation")]
        public string Description { get; set; }

        public virtual ICollection<BillReview> BillReviews { get; set; }
    }

    public class FileStreamRowData
    {
        public string Path { get; set; }
        public byte[] Transaction { get; set; }
    }



}