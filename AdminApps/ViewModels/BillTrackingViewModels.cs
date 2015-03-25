using AdminApps.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Text;

namespace AdminApps.ViewModels
{
    public class BillsHomeViewModel
    {
        public List<BillReview> RecentBillReviews { get; set; }
    }

    public class BillsIndexViewModel
    {
        public int? AdminAppsID { get; set; }
        public decimal? NebsBdrID { get; set; }
        [Display(Name = "Bill Prefix")]
        public int? BillPrefixID { get; set; }
        [Display(Name = "Bill Suffix")]
        public int? Suffix { get; set; }
        public string Location { get; set; }
        [Display(Name = "Bill #")]
        public string CompositeBillNumber { get; set; }
        [Display(Name = "NEBS BDR #")]
        public string NebsBdrNumber { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        [Display(Name = "Reviews")]
        public int? NumReviews { get; set; }
        public bool HasApprovedReview { get; set; }

        public string TruncSummary
        {
            get
            {
                if (Summary.Length > 60)
                {
                    return Summary.Substring(0, 60) + "...";
                }
                else
                {
                    return Summary;
                }
            }
        }
    }

    public class BillEditViewModel
    {
        public BillEditViewModel()
        {
            //this.ApprovedReview = new BillReviewViewModel();
            this.ReviewRequestedFromUsers = new List<BillReviewRequestIndividualUserViewModel>();
            this.Reviews = new List<BillReviewViewModel>();
            this.BillVersions = new List<BillVersion>();
        }

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
        [DataType(DataType.Url)]
        public string NelisHyperlink { get; set; }

        //NELIS properties
        public int? NelisID { get; set; }

        // Identity properties
        [ForeignKey("CreatedByUser")]
        public string ApplicationUserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public virtual ICollection<ApplicationUser> InterestedUsers { get; set; }

        // properties for views
        public BillVersion CurrentVersion { get; set; }
        public bool UserCanReview { get; set; }
        //public bool UserCanApprove { get; set; }
        public BillReviewViewModel ApprovedReview { get; set; }
        public DateTime ApprovedAt { get; set; }
        public ApplicationUser ApprovedByUser { get; set; }
        public List<BillReviewRequestIndividualUserViewModel> ReviewRequestedFromUsers { get; set; }
        public int? BillRecordRequestID { get; set; }

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
        public virtual List<Hearing> Hearings { get; set; }
        public virtual List<BillReviewViewModel> Reviews { get; set; }
        public virtual List<BillVersion> BillVersions { get; set; }


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


        public System.Web.Mvc.SelectList BillPrefixesList { get; set; }
        public System.Web.Mvc.SelectList DeptsList { get; set; }
        public System.Web.Mvc.SelectList DivsList { get; set; }
        public System.Web.Mvc.SelectList LegStatusesList { get; set; }
        public System.Web.Mvc.SelectList CommitteesList { get; set; }
        public System.Web.Mvc.SelectList GovActionsList { get; set; }
    }

    public class AddBillVersionViewModel
    {
        public AddBillVersionViewModel()
        {
            this.Bill = new BillEditViewModel();
        }
        //public int ID { get; set; }
        public int BillID { get; set; }
        public int VersionNum { get; set; }
        [Display(Name = "Reprint?")]
        public bool IsReprint { get; set; }
        public int? ReprintNum { get; set; }
        [Display(Name = "Reprint Date")]
        [DataType(DataType.Date)]
        public DateTime? ReprintDate { get; set; }
        public int? Amendment { get; set; }
        public bool IsEnrolled { get; set; }

        // navigation properties
        public virtual BillEditViewModel Bill { get; set; }
        //public virtual ICollection<BillReview> BillReviews { get; set; }

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
                else if (IsReprint)
                {
                    return string.Format("Reprint {0}, {1}", ReprintNum, ReprintDate);
                }
                else if (IsEnrolled)
                {
                    return "As Enrolled";
                }
                return null;
            }
        }

    }

    public class BillReviewViewModel
    {
        public int ID { get; set; }
        public int BillID { get; set; }
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
        [DataType(DataType.Currency)]
        public int FiscalImpactYr1 { get; set; }
        [Display(Name = "Fiscal Impact Yr 2")]
        [DataType(DataType.Currency)]
        public int FiscalImpactYr2 { get; set; }
        [Display(Name = "Future Impact")]
        [DataType(DataType.Currency)]
        public int FiscalImpactFuture { get; set; }
        [UIHint("BooleanButton")]
        [Display(Name = "Fiscal Note submitted?")]
        public bool FiscalNoteSubmitted { get; set; }
        public string Notes { get; set; }

        public int CreatedAtApprovalLevel { get; set; }
        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; }

        // properties for view
        public bool DisplayAsRead { get; set; }
        public bool UserCanEdit { get; set; }
        public bool UserCanApprove { get; set; }
        [UIHint("DangerText")]
        public string ApprovedReviewMessage { get; set; }
        public bool IsApproved { get; set; }
        public bool UpToDate { get; set; }
        public string StatusMessage { get; set; }

        // navigation properties
        public virtual Bill Bill { get; set; }
        public virtual BillVersion BillVersion { get; set; }
        public virtual BillReviewRecommendation Recommendation { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        [Display(Name = "Dept")]
        public virtual Dept CreatedByUserInDept { get; set; }
        [Display(Name = "Div")]
        public virtual Div CreatedByUserInDiv { get; set; }

        // calculated properties
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

    public class BillReviewApprovalViewModel
    {
        public BillReview ReviewToApprove { get; set; }
        public BillReview ApprovedReview { get; set; }
        public ApplicationUser ApprovedByUser { get; set; }
        public DateTime ApprovedAt { get; set; }
    }

    public class BillReviewRequestIndividualUserViewModel
    {
        public bool Selected { get; set; }
        public bool PreviouslyRequested { get; set; }
        public DateTime? RequestedAt { get; set; }
        public bool Fulfilled { get; set; }
        public DateTime? FulfilledAt { get; set; }
        public string ApplicationUserID { get; set; }
        public string FullName { get; set; }
        public string CompositeDivName { get; set; }
    }

    public class BillReviewRequestSelectUsersViewModel
    {
        public int BillID { get; set; }
        public string BillName { get; set; }
        public List<BillReviewRequestIndividualUserViewModel> Users { get; set; }
        public BillReviewRequestSelectUsersViewModel()
        {
            this.Users = new List<BillReviewRequestIndividualUserViewModel>();
        }

        public IEnumerable<string> getSelectedIds()
        {
            return (from u in this.Users where u.Selected select u.ApplicationUserID).ToList();
        }

        public virtual Bill Bill { get; set; }
    }

    public class BillReviewRequestViewModel
    {
        public int BillID { get; set; }
        public string CompositeBillName { get; set; }
        public string RequestedByUserName { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; }
    }

    public class BillRecordRequestViewModel
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
        // resultant Bill
        public int? BillID { get; set; }
        // resultant BillReview
        public int? BillReviewID { get; set; }
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

        // view properties
        public bool UserReviewed { get; set; }

        // select lists
        public System.Web.Mvc.SelectList BillPrefixesList { get; set; }

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

    }

    public class AttachBillRecordRequestViewModel
    {
        public int RequestID { get; set; }
        public int BillID { get; set; }
        public string RequestedByUserName { get; set; }
        public string CompositeBillNumber { get; set; }
        public string Summary { get; set; }
    }

    public class BillsUserAccountRequestViewModel
    {
        // UserAccountRequestProperties
        public int ID { get; set; }
        public decimal DivID { get; set; }
        public decimal DeptID { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string Title { get; set; }
        [ForeignKey("RequestedByUser")]
        public string RequestedByUserID { get; set; }
        public DateTime RequestedAt { get; set; }
        [Index]
        public bool Fulfilled { get; set; }
        [ForeignKey("FulfilledByUser")]
        public string FullfilledByUserID { get; set; }
        public DateTime? FullfilledAt { get; set; }

        // BillsUserAccountRequest properties
        [Required]
        public int RequestedApprovalLevel { get; set; }

        // view properties
        public string AgencyName { get; set; }
        [Display(Name = "Requested Role")]
        public string RoleName { get; set; }


        // select lists
        [Display(Name = "Requested Role")]
        public System.Web.Mvc.SelectList RolesList { get; set; }
        // navigation properties
        [InverseProperty("UserAccountRequestsBy")]
        public virtual ApplicationUser RequestedByUser { get; set; }
        [InverseProperty("UserAccountRequestsFulfilledBy")]
        public virtual ApplicationUser FulfilledByUser { get; set; }

        // calculated properties
        [Display(Name = "Name")]
        public string FullName
        {
            get
            {
                if (FirstName != null && LastName != null)
                {
                    return FirstName + " " + LastName;
                }
                else
                {
                    return null;
                }
            }
        }

    }

    public class AlsrReportSelectBillReviewsViewModel
    {
        public AlsrReportSelectBillReviewsViewModel()
        {
            this.BillReviews = new List<AlsrReportIndividualBillReviewViewModel>();
        }
        public List<AlsrReportIndividualBillReviewViewModel> BillReviews { get; set; }

        public IEnumerable<int> getSelectedIds()
        {
            return (from r in this.BillReviews where r.Selected select r.ID).ToList();
        }

        [Display(Name = "Governor's Office Delivery Date")]
        [DataType(DataType.Date)]
        public DateTime GovOfficeDeliveryDate { get; set; }

    }

    public class AlsrReportIndividualBillReviewViewModel
    {
        public int ID { get; set; }
        public bool Selected { get; set; }
        public string CompositeBillNumber { get; set; }
        public int VersionNum { get; set; }
        public string CreatedByUserFullName { get; set; }
        public string Recommendation { get; set; }
    }

}