using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AdminApps.Models
{
    public partial class ApplicationUser : IdentityUser
    {
        // Project Management module navigation properties
        public virtual ICollection<UserProject> UserProjects { get; set; }
    }

    public class UserProject : Project
    {
        [ForeignKey("CreatedByUser")]
        public string ApplicationUserID { get; set; }

        // navigation properties
        [InverseProperty("UserProjects")]
        public virtual ApplicationUser CreatedByUser { get; set; }
    }

}