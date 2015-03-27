using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AdminApps.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        // ApplicationUser properties are extended per http://typecastexception.com/post/2014/06/22/ASPNET-Identity-20-Customizing-Users-and-Roles.aspx
        // Extended properties must also be added to:
        // RegisterViewModel (AccountViewModels.cs) and Register View (Views => Accounts => Register.cshtml)
        // Register Method (AccountController)
        // UsersAdmin Create View (Views => UsersAdmin => Create)
        // EditUserViewModel (AdminViewModel.cs) and Edit View (Views => UsersAdmin => Edit.cshtml)
        // UsersAdmin Index, Delete and Details Views (Views => UsersAdmin)
        // Create Method, Edit Method (including [Post] override and white list parameters) in (UserAdminController)

        // Add extended properties here:
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public decimal DivID { get; set; }
        public decimal DeptID { get; set; }
        public bool IsActive { get; set; }
        public bool AutoPwdReplaced { get; set; }

        // user preference properties
        [ForeignKey("DefaultAppModule")]
        public int AppModuleID { get; set; }

        // Add extended navigation properties here:
        public virtual Div Div { get; set; }
        public virtual Dept Dept { get; set; }
        public virtual AppModule DefaultAppModule { get; set; }

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

        [Display(Name = "Name")]
        public string FullNameLastFirst
        {
            get
            {
                if (FirstName != null && LastName != null)
                {
                    return string.Format("{0}, {1}", this.LastName, this.FirstName);
                }
                else
                {
                    return null;
                }
            }
        }


        // Bill Tracking Module navigation properties
        public virtual ICollection<Bill> CreatedBills { get; set; }
        public virtual ICollection<BillReview> CreatedBillReviews { get; set; }
        public virtual ICollection<BillReviewNotification> BillReviewNotificationsRead { get; set; }
        public virtual ICollection<BillReviewApproval> BillReviewApprovals { get; set; }
        public virtual ICollection<BillReviewRequest> BillReviewRequestsBy { get; set; }
        public virtual ICollection<BillReviewRequest> BillReviewRequestsTo { get; set; }
        public virtual ICollection<BillRecordRequest> BillRecordRequestsBy { get; set; }
        public virtual ICollection<BillRecordRequest> BillRecordRequestsFulfilledBy { get; set; }
        public virtual ICollection<UserAccountRequest> UserAccountRequestsBy { get; set; }
        public virtual ICollection<UserAccountRequest> UserAccountRequestsFulfilledBy { get; set; }
        public virtual ICollection<Bill> BillsOfInterest { get; set; }
        public virtual ICollection<BillsAlsrReport> BillsAlsrReports { get; set; }
    }

    // Custom class for extensible User Role management per http://typecastexception.com/post/2014/06/22/ASPNET-Identity-20-Customizing-Users-and-Roles.aspx
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string name) : base(name) { }

        // Extended properties must also be added to:
        // RoleViewModel (AdminViewModels.cs) and the Create, Edit, Index views (Views => RolesAdmin)
        // Create, Edit methods (RolesAdminController)

        // Add extended properties here:
        public string Description { get; set; }
        public int AppModuleID { get; set; }
        public int AppModuleApprovalLevel { get; set; }
        public string AppModuleApprovalTitle { get; set; }

        // navigation properties
        public virtual AppModule AppModule { get; set; }
    }

    public class UserAccountRequest
    {
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
        [Display(Name = "Requested By")]
        public string RequestedByUserID { get; set; }
        public DateTime RequestedAt { get; set; }
        [Index]
        public bool Fulfilled { get; set; }
        [ForeignKey("FulfilledByUser")]
        public string FullfilledByUserID { get; set; }
        public DateTime? FullfilledAt { get; set; }

        // navigation properties
        [InverseProperty("UserAccountRequestsBy")]
        public virtual ApplicationUser RequestedByUser { get; set; }
        [InverseProperty("UserAccountRequestsFulfilledBy")]
        public virtual ApplicationUser FulfilledByUser { get; set; }
        public virtual Dept Dept { get; set; }
        public virtual Div Div { get; set; }

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

}